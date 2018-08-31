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
using PongGlobe.Core;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 点矢量图层类
    /// </summary>
    public class PointVectorLayer : IRender
    {       
        private Mesh<Vector3> _meshPoints;
        private Scene _scene;
        private PointStyleUBO _pointStyleUBO;
        private DeviceBuffer _VertexBuffer;
        private DeviceBuffer _IndicesBuffer;
        private DeviceBuffer _pointStyle;
        //点的渲染管道，默认使用矢量点的渲染模式,非billBoard
        private Pipeline _pointPipeLine;
        ShaderSetDescription shaderSet;
        private ResourceSet _pointStyleRSet;
        private ResourceLayout _pointstyleLayout;

        public PointVectorLayer(string shpPath, Scene scene)
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
           
            var result = _meshPoints.CreateGraphicResource(gd, factory);
            _VertexBuffer = result.Item1;
            _IndicesBuffer = result.Item2;
            ///Shader布局
            var curAss = this.GetType().Assembly;
            shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"PointVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"PointFS.spv",gd,curAss)
                });

            //创建一个pointStyle的UBO,一共32字节，8个浮点值
            _pointStyle = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //创建一个Style的ResourceLayout
             _pointstyleLayout = factory.CreateResourceLayout(
               new ResourceLayoutDescription(
                   new ResourceLayoutElementDescription("Style", ResourceKind.UniformBuffer, ShaderStages.Fragment|ShaderStages.Vertex)

                   ));
            _pointStyleUBO = new PointStyleUBO(RgbaFloat.Red);
            //更新变量等
            gd.UpdateBuffer(_pointStyle, 0, _pointStyleUBO);

            //创建一个StyleresourceSet
            _pointStyleRSet = factory.CreateResourceSet(new ResourceSetDescription(
               _pointstyleLayout,
               _pointStyle
               ));

            //创建渲染管道
            _pointPipeLine = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
               BlendStateDescription.SingleOverrideBlend,
               DepthStencilStateDescription.DepthOnlyLessEqual,
               RasterizerStateDescription.Default,
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
            _cl.DrawIndexed((uint)_meshPoints.Indices.Length, 1, 0, 0, 0);
        }

        public void Update()
        {
            //暂未更新必要
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
}
