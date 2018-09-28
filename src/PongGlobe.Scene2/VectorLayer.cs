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
namespace PongGlobe.Renders
{
    /// <summary>
    /// 矢量图层类,根据点线面的不同这里还是区分出来
    /// </summary>
    public class VectorLayerRender : IRender
    {
        private IEnumerable<IShapefileFeature> _features;
        private Scene.Scene _scene;
        private List<BaseFeatureRender> renders = new List<BaseFeatureRender>();
        private List<BaseFeatureRender> selectedRenders = new List<BaseFeatureRender>();
       
        //构建一个四叉树的来存储其索引
        private NetTopologySuite.Index.Quadtree.Quadtree<IShapefileFeature> _quadTree = new Quadtree<IShapefileFeature>();
        private VectorStyle SelectedStyle=new VectorStyle();
        private VectorStyle Style = new VectorStyle();
        /// <summary>
        /// 当前选中的要素
        /// </summary>
        private List<IShapefileFeature> _selectedFeatures=new List<IShapefileFeature>();
        /// <summary>
        /// 当前选中的要素
        /// </summary>
        public List<IShapefileFeature> SelectedFeatures { get { return _selectedFeatures; } }

        /// <summary>
        /// 通过射线来判定第一个相交的要素
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool GetSelectedFeatureByRayCasting(Ray ray)
        {
            //计算Ray于地球的交点
            bool isIntersect =_scene.Ellipsoid.Intersections(ray,out Geodetic2D result);
            ImGui.Text(string.Format("Latitude:{0},logitude:{1}",result.Latitude,result.Longitude));
            SelectedFeatures.Clear();
            var geoFactory = new NetTopologySuite.Geometries.GeometryFactory();
            if (isIntersect)
            {
                //是否考虑根据当前精度生成一个合适的Enve
                //这里一律采用0.001经纬度的精度
                var env = new GeoAPI.Geometries.Envelope(new Coordinate(result.Longitude*180/Math.PI - 0.001, result.Latitude * 180 / Math.PI - 0.001), new Coordinate(result.Longitude * 180 / Math.PI + 0.001, result.Latitude * 180 / Math.PI + 0.001));
                var selectedff= _quadTree.Query(env);
                foreach (var item in selectedff)
                {
                    if (item.Geometry.Intersects(geoFactory.ToGeometry(env)))
                    {
                        SelectedFeatures.Add(item);
                    }                    
                }              
                return true;
            }
            return false;
        }


        /// <summary>
        /// 这里统一使用弧度制作为参数
        /// </summary>
        /// <param name="shpPath"></param>
        /// <param name="scene"></param>
        public VectorLayerRender(string shpPath,Scene.Scene scene)
        {
            var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeDataReader(shpPath);
            var fea= shpReader.ReadByMBRFilter(shpReader.ShapefileBounds);
            _features = fea;
            //构建索引
            foreach (var item in _features)
            {
                if (item.Geometry is IPolygon)
                {
                    var env = item.Geometry.EnvelopeInternal;                 
                    _quadTree.Insert(item.Geometry.EnvelopeInternal, item);
                }
                else if (item.Geometry is IMultiPolygon)
                {
                    foreach (var child in item.Geometry as IMultiPolygon )
                    {
                        _quadTree.Insert(child.EnvelopeInternal, item);
                    }
                }
            }
            _scene = scene;
            //设置其选中的样式
            SelectedStyle.PolygonStyle.FillColor = RgbaFloat.Orange;
        }

        /// <summary>
        /// 测试一个feature一个render对象
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="factory"></param>
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            if (_features == null) return;
            foreach (var item in _features)
            {
                var featureRender = new BaseFeatureRender(item, _scene);
                renders.Add(featureRender);
                featureRender.CreateDeviceResources(gd, factory);
            }
        }

        public void Dispose()
        {
            foreach (var item in renders)
            {
                item.Dispose();
            }
        }

        public void Draw(CommandList _cl)
        {
            foreach (var item in renders)
            {
                item.Draw(_cl);
            }
        }

        public void Update()
        {
            //获取当前的鼠标点位
            var pos = InputTracker.MousePosition;
            var camera = (MyCameraController2)_scene.Camera;
            var rayVector3 = camera.Unproject(new Vector3(pos.X, pos.Y, 0.01f), camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            var rayVector = camera.Unproject(new Vector3(pos.X,  pos.Y, 5), camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            var rayVector2 = camera.Unproject(new Vector3(pos.X, pos.Y, 10), camera.ProjectionMatrix, camera.ViewMatrix, Matrix4x4.Identity);
            //此rayVector为近裁剪面的世界坐标，与eye相减得到ray向量，ray向量与地球求交即可得到结果
            //计算是否与地球有交
            var rayDir =  rayVector - camera.Position;
            var ray = new Ray(camera.Position, rayDir);
            //构造一个射线
            GetSelectedFeatureByRayCasting(ray);

            //根据当前的选中状态调整render的样式信息
            foreach (var item in renders)
            {
                item.VectorStyle = Style;
                item.Update();
                foreach (var child in SelectedFeatures)
                {
                    if (child.FeatureId == item.Feature.FeatureId)
                    {
                        item.VectorStyle = SelectedStyle;
                        item.Update();
                    }
                }
            }          
        }
    }

    /// <summary>
    /// 将VectorStyle转换成结构体UBO
    /// </summary>
    public static class VectorStyleExtension
    {
        public static PolygonVectorStyleUBO ToUBO(this PolygonVectorStyle style)
        {
            return new PolygonVectorStyleUBO(style.FillColor);
        }
        public static LineVectorStyleUBO ToUBO(this LineVectorStyle style)
        {
            return new LineVectorStyleUBO(style.LineColor, style.LineWidth);
        }
    }



    /// <summary>
    /// 对单个要素进行渲染,独立渲染每个要素，当要使用动态水渲染要素时，使用DynamicWaterRender,由FeatureRenderFactory根据参数生成适当的Render对象。
    /// 测试模拟鼠标移动的高亮，首先实现CPU RayCasting Picking。这适用于polygon对象，可能并不适合其它对象。
    /// 这便需要建立四叉树对场景中所有的对象进行管理。然后使用ray去于对象进行求交判断
    /// </summary>
    public class BaseFeatureRender : IRender
    {
        /// <summary>
        /// feature转换成的mesh对象
        /// </summary>
        private List<Mesh<Vector3>> _mesh = null;
        private Mesh<Vector3> _meshLine = null;
        private List<DeviceBuffer> _vertexBuffer=new List<DeviceBuffer>();
        private List<DeviceBuffer> _indexBuffer=new List<DeviceBuffer>();
        private List<DeviceBuffer> _boundingboxvertexBuffer = new List<DeviceBuffer>();
        private List<DeviceBuffer> _boundingBoxindiceBuffer = new List<DeviceBuffer>();
        private DeviceBuffer _lineVertexBuffer;
        private DeviceBuffer _lineIndicesBuffer; 
        private DeviceBuffer _polygonstyleBuffer;
        private DeviceBuffer _polylinetyleBuffer;
        private ResourceSet _styleResourceSet;
        private bool ShowBoundingBox = true;
        private GraphicsDevice _gd;
        //可能用到的纹理
        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;
        //private CommandList _cl;
        //渲染管线
        private Pipeline _pipeline;
        /// <summary>
        /// 绘制一个立方体居然用到了两个渲染管线
        /// </summary>
        private Pipeline _boundingBoxPipeLine;
        private Pipeline _linePipeLine;
        private Ellipsoid _shape = Ellipsoid.ScaledWgs842;
        /// <summary>
        /// 当前需要渲染的矢量对象
        /// </summary>
        private IShapefileFeature _feature;
        /// <summary>
        /// 使用_fea构造FeatureRFender对象
        /// </summary>
        /// <param name="_fea"></param>
        public BaseFeatureRender(IShapefileFeature _fea,Scene.Scene _scene)
        {
            _feature = _fea;
            _shape = _scene.Ellipsoid;
            VectorStyle = new VectorStyle();
        }
        /// <summary>
        /// 显示轮廓
        /// </summary>
        /// <returns></returns>
        public bool ShowWireFrame { get; set; }
        public IShapefileFeature Feature => _feature;
       
        public Styles.VectorStyle VectorStyle { get; set; }
        /// <summary>
        /// 创建相关资源
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="factory"></param>
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            _gd = gd;
            //计算每个mesh的boundbox                      
            _mesh= FeatureTrianglator.FeatureToMesh(this._feature, this._shape);
            //提取所有点绘制其线
            _meshLine = FeatureTrianglator.FeatureToLineStripAdjacency(this._feature, this._shape);
            ///创建并更新资源
            var result = _meshLine.CreateGraphicResource(gd, factory);
            _lineVertexBuffer = result.Item1;
            _lineIndicesBuffer = result.Item2;

            foreach (var item in _mesh)
            {
                var typle = item.CreateGraphicResource(gd, factory);
                var box= BoundingBox.CreateFromPoints(item.Positions);
                var ttt = box.CreateResource(gd, factory);
                _vertexBuffer.Add(typle.Item1);
                _indexBuffer.Add(typle.Item2);               
            }
            //三角细分,细分精度为1度
            //_mesh = TriangleMeshSubdivision.Compute(posClearUp, indices.ToArray(), Math.PI / 180);

            //创建一个PolygonStyle的Buffer并更新
            _polygonstyleBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer|BufferUsage.Dynamic));
            gd.UpdateBuffer(_polygonstyleBuffer, 0,VectorStyle.PolygonStyle.ToUBO());

            //创建一个LineStyle的Buffer
            _polylinetyleBuffer = _polygonstyleBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            gd.UpdateBuffer(_polylinetyleBuffer, 0, VectorStyle.LineStyle.ToUBO());

            //创建一个stylelayout
            ResourceLayout styleLayout = factory.CreateResourceLayout(
               new ResourceLayoutDescription(
                   new ResourceLayoutElementDescription("LineStyle", ResourceKind.UniformBuffer,  ShaderStages.Fragment| ShaderStages.Geometry),
                   new ResourceLayoutElementDescription("PolygonStyle", ResourceKind.UniformBuffer, ShaderStages.Fragment)
                   ));
            var curAss = this.GetType().Assembly;
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"PolygonVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"PolygonFS.spv",gd,curAss)
                });
            ShaderSetDescription shaderSetBoundingBox = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
                },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"LineVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"LineFS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Geometry,"LineGS.spv",gd,curAss)
                });


            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                shaderSet,
                //共享View和prj的buffer
                new ResourceLayout[] { ShareResource.ProjectionResourceLoyout, styleLayout },
                gd.MainSwapchain.Framebuffer.OutputDescription));

            //创建一个渲染boundingBox的渲染管线
            var rasterizer = RasterizerStateDescription.Default;
            rasterizer.FillMode = PolygonFillMode.Solid;
            rasterizer.FrontFace = FrontFace.Clockwise;
            //gpu的lineWidth实际绘制的效果并不好仍然需要GeometryShader来实现更好的效果
            //rasterizer.LineWidth = 8.0f;
            _boundingBoxPipeLine = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rasterizer,
                _meshLine.PrimitiveTopology,
                shaderSetBoundingBox,
                //共享View和prj的buffer
                new ResourceLayout[] { ShareResource.ProjectionResourceLoyout, styleLayout },
                gd.MainSwapchain.Framebuffer.OutputDescription));
          

            //创建一个StyleresourceSet,0是线样式1是面样式
            _styleResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
               styleLayout,
               _polylinetyleBuffer,_polygonstyleBuffer
               ));
            //创建一个ResourceSet
            //_cl = factory.CreateCommandList();
        }
        /// <summary>
        /// 释放相关资源
        /// </summary>
        public void Dispose()
        {
            
        }

        public void Draw(CommandList _cl)
        {
            //_cl.SetPipeline(_pipeline);          
            //_cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
            //_cl.SetGraphicsResourceSet(1, _styleResourceSet);
            //for (int i = 0; i < _mesh.Count; i++)
            //{
            //    _cl.SetVertexBuffer(0, _vertexBuffer[i]);
            //    _cl.SetIndexBuffer(_indexBuffer[i], IndexFormat.UInt16);
            //    _cl.DrawIndexed((uint)_mesh[i].Indices.Length, 1, 0, 0, 0);                
            //}
            //_gd.MainSwapchain.Framebuffer.
            _cl.SetPipeline(_boundingBoxPipeLine);
            _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
            _cl.SetGraphicsResourceSet(1, _styleResourceSet);
            _cl.SetVertexBuffer(0, _lineVertexBuffer);
            _cl.SetIndexBuffer( _lineIndicesBuffer, IndexFormat.UInt16);
            _cl.DrawIndexed((uint)_meshLine.Indices.Length,1,0,0,0);
        }

        public void Update()
        {
            _gd.UpdateBuffer(_polygonstyleBuffer, 0, VectorStyle.PolygonStyle.ToUBO());
            _gd.UpdateBuffer(_polylinetyleBuffer, 0, VectorStyle.LineStyle.ToUBO());
        }
    }


    //public class PolygonRender


    /// <summary>
    /// 样式信息的结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PolygonVectorStyleUBO
    {
        /// <summary>
        /// 多边形的颜色,结构类似vector4
        /// </summary>
        public RgbaFloat FillColor;      

        public PolygonVectorStyleUBO(RgbaFloat fillColor)
        {
            FillColor = fillColor;
            
        }
    }

    /// <summary>
    /// 样式信息的结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LineVectorStyleUBO
    {
        /// <summary>
        /// 多边形的颜色,结构类似vector4
        /// </summary>
        public RgbaFloat LineColor;
        public float LineWidth;
        //补位信息
        public Vector3 spa1;

        public LineVectorStyleUBO(RgbaFloat lineColor, float lineWidth=1.0f)
        {
            spa1 = new Vector3();
            LineColor = lineColor;
            LineWidth = lineWidth;

        }
    }


    ///// <summary>
    ///// 面图层矢量类
    ///// </summary>
    //public class PolygonVectorLayer : IRender
    //{
    //    Ellipsoid _shape = Ellipsoid.ScaledWgs842;
    //    /// <summary>
    //    /// 将要素转换成Mesh对象
    //    /// </summary>
    //    private void FeaturesToMesh()
    //    {
    //        //读取shapefile的内容
    //        string path = "";
    //        var rader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeDataReader( path);
    //        var result= rader.ReadByMBRFilter(rader.ShapefileBounds);
    //        //读取其中的点信息，转换成mesh对象
    //        var mesh = new PongGlobe.Core.Mesh<Vector3>();
    //        mesh.PrimitiveType = PrimitiveType.TriangleStrip;
    //        //在转换成
    //        List<Vector3> positions = new List<Vector3>();
    //        //记录其indices
    //        List<ushort> indics = new List<ushort>();
    //        //填充顶点跟索引
    //        //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标
    //        foreach (var item in result)
    //        {
    //            var geo = item.Geometry.Coordinates;
    //            //暂未考虑multipolygon的情况，也即是Hole polygon.
    //            foreach (var coord in geo)
    //            {
    //                //将其转换成弧度制
    //                positions.Add(_shape.ToVector3(new Geodetic3D(MathExtension.ToRadius(coord.X),MathExtension.ToRadius(coord.Y))));
    //            }               
    //        }
    //        //简化几何，如去除重复点，重置indices等操作

    //    }

    //    public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
    //    {
    //        //throw new NotImplementedException();
    //    }

    //    public void Draw()
    //    {
    //        //throw new NotImplementedException();
    //    }

    //    public void Update()
    //    {
    //        //throw new NotImplementedException();
    //    }

    //    public void Dispose()
    //    {
    //        //throw new NotImplementedException();
    //    }
    //}
}
