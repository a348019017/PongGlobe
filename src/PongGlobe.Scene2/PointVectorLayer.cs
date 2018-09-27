using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using PongGlobe.Core;
using PongGlobe.Core.Algorithm;
using PongGlobe.Core.Util;
using PongGlobe.Scene;
using PongGlobe.Styles;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using PongGlobe.Core.Extension;
using NetTopologySuite.Index.Quadtree;
using GeoAPI.Geometries;
using System.Collections.ObjectModel;
using System;
using ImGuiNET;
using Veldrid.ImageSharp;
using PongGlobe.Core.Render;
using System.Linq;


namespace PongGlobe.Scene
{
    /// <summary>
    /// 点矢量图层渲染类
    /// </summary>
    public class PointVectorLayerRender : IRender
    {       
        private Mesh<Vector3> _meshPoints;
        private Scene _scene;
        private PointStyleUBO _pointStyleUBO;
        private SystemEventUBO _systemEventUbo;
        private DeviceBuffer _VertexBuffer;
        private DeviceBuffer _IndicesBuffer;
        private DeviceBuffer _pointStyle;
        private DeviceBuffer _eventBuffer;
        //点的渲染管道，默认使用矢量点的渲染模式,非billBoard
        private Pipeline _pointPipeLine;
        ShaderSetDescription shaderSet;
        private ResourceSet _pointStyleRSet;
        private ResourceLayout _pointstyleLayout;
        private TextureView _pointTextureView;
        private Texture _pointTexture;
        private BasicFeatureRenderStrategy _renderStrategy = new BasicFeatureRenderStrategy();
        private IEnumerable<FeatureRenderableObject> _allRenderableObjects = null;
        private IEnumerable<FeatureRenderableObject> _renderStrategyResult = null;
        private GraphicsDevice _gd = null;


        public PointVectorLayerRender(string shpPath, Scene scene)
        {
            _scene = scene;
            var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeDataReader(shpPath);
            var fea = shpReader.ReadByMBRFilter(shpReader.ShapefileBounds);
            _meshPoints = FeatureTrianglator.PointFeatureToPoints(fea, _scene.Ellipsoid);
            //这里可以强制释放相关资源了
            shpReader.Dispose();
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            _gd = gd;
            var inPath = @"E:\swyy\Lib\PongGlobe\PongGlobe\assets\icon\vaves.png";
            ImageSharpTexture inputImage = new ImageSharpTexture(inPath, false);
            _pointTexture= inputImage.CreateDeviceTexture(gd, factory);
            _pointTextureView = factory.CreateTextureView(_pointTexture);

            var result = _meshPoints.CreateGraphicResource(gd, factory);
            _allRenderableObjects = MeshToRenderableObject(_meshPoints);
            _VertexBuffer = result.Item1;
            _IndicesBuffer = result.Item2;
            ///Shader布局
            var curAss = this.GetType().Assembly;
            shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3)
                        ,
                        //这里在顶点中传入了Id的相关信息
                        new VertexElementDescription("Id", VertexElementSemantic.Position, VertexElementFormat.UInt1))
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"PointVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"PointFS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Geometry,"PointGS.spv",gd,curAss)
                });

            //创建一个pointStyle的UBO,一共32字节，8个浮点值
            _pointStyle = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //传一个系统事件buffer
            _eventBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //创建一个Style的ResourceLayout
             _pointstyleLayout = factory.CreateResourceLayout(
               new ResourceLayoutDescription(
                   new ResourceLayoutElementDescription("Style", ResourceKind.UniformBuffer, ShaderStages.Fragment|ShaderStages.Geometry),
                   new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                   new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                   new ResourceLayoutElementDescription("SystemEvent", ResourceKind.UniformBuffer, ShaderStages.Geometry)
                   ));
            _pointStyleUBO = new PointStyleUBO(RgbaFloat.Red);
            //更新变量等
            gd.UpdateBuffer(_pointStyle, 0, _pointStyleUBO);

            //创建一个StyleresourceSet
            _pointStyleRSet = factory.CreateResourceSet(new ResourceSetDescription(
               _pointstyleLayout,
               _pointStyle,
               _pointTextureView,
               gd.Aniso4xSampler,
               _eventBuffer
               ));

            var rasterizer = RasterizerStateDescription.Default;
            rasterizer.FillMode = PolygonFillMode.Solid;
            rasterizer.FrontFace = FrontFace.Clockwise;

            //创建渲染管道
            _pointPipeLine = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
               BlendStateDescription.SingleOverrideBlend,
               DepthStencilStateDescription.DepthOnlyLessEqual,
               rasterizer,
               _meshPoints.PrimitiveTopology,
               shaderSet,
               //共享View和prj的buffer
               new ResourceLayout[] { ShareResource.ProjectionResourceLoyout, _pointstyleLayout },
               gd.MainSwapchain.Framebuffer.OutputDescription));
        }

        public void Dispose()
        {
            //释放渲染管线
            _pointPipeLine.Dispose();
            //释放resource等
        }

        public void Draw(CommandList _cl)
        {
            _cl.SetPipeline(_pointPipeLine);
            _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
            _cl.SetGraphicsResourceSet(1, _pointStyleRSet);
            _cl.SetVertexBuffer(0, _VertexBuffer);
            _cl.SetIndexBuffer(_IndicesBuffer, IndexFormat.UInt16);
            _systemEventUbo.MousePosition = PongGlobe.Scene.InputTracker.MousePosition;
            _cl.UpdateBuffer(_eventBuffer, 0, _systemEventUbo);
            if (_renderStrategyResult != null)
            {              
                _cl.DrawIndexed((uint)_renderStrategyResult.Count(), 1, 0, 0, 0);
            }
            else
            {
                _cl.DrawIndexed((uint)_meshPoints.Indices.Length, 1, 0, 0, 0);
            }           
        }

        /// <summary>
        /// 将Mesh对象转换为可渲染对象
        /// </summary>
        private List<FeatureRenderableObject> MeshToRenderableObject(Mesh<Vector3> _mesh)
        {
            List<FeatureRenderableObject> renders = new List<FeatureRenderableObject>();
            for (ushort i = 0; i < _mesh.Positions.Length; i++)
            {
                var item = _mesh.Positions[i];
                var featureRenderableObject = new FeatureRenderableObject();
                featureRenderableObject.PointVector = item;
                featureRenderableObject.Indice = i;
                renders.Add(featureRenderableObject);
            }
            return renders;
        }

        public  void Update()
        {
            //使用渲染策略更新相关数据，这里顶点数据不必更新，仅更新indicesbuffer即可，相当高效，当然在视椎体裁切时可能仍然需要充值顶点数据
            _renderStrategyResult= _renderStrategy.Apply(_scene, _allRenderableObjects);
            //读取GPU计算的Id信息
            var result= _gd.Map<SystemEventUBO>(_eventBuffer,MapMode.Read);
            var ubo = result[0];
            _gd.Unmap(_eventBuffer);
            //显示其中的值
            ImGui.Text(string.Format("Slected PointId:{0}", ubo.FeatureId);
        }
    }

   
    [StructLayout(LayoutKind.Sequential)]
    public struct PointStyleUBO
    {    
        /// <summary>
        /// 点的颜色
        /// </summary>
        public RgbaFloat PointColor;
        /// <summary>
        /// 点的尺寸
        /// </summary>
        public float PointSize;
        /// <summary>
        /// 打断符，凑整而已
        /// </summary>
        public Vector3 split;

        public PointStyleUBO(RgbaFloat pointColor, float pointSize=5)
        {
            PointSize = pointSize;
            PointColor = pointColor;
            split = new Vector3();
        }
    }

    /// <summary>
    /// 系统事件UBO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemEventUBO
    {
        /// <summary>
        /// 鼠标所在屏幕坐标
        /// </summary>
        public Vector2 MousePosition;
        /// <summary>
        /// 记录传出的Id
        /// </summary>
        public UInt32 FeatureId;

        public float spa1;
    }

}
