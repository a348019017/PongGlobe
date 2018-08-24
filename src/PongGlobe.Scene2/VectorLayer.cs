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
        /// <summary>
        /// 构件一个最大范围为-1-1-1，111的八叉树，最大子节点为100
        /// </summary>
        private Octree<IShapefileFeature> _quadTree = new Octree<IShapefileFeature>(new BoundingBox(new Vector3(-1,-1,-1),new Vector3(1,1,1)),100);
        public VectorLayerRender(string shpPath,Scene.Scene scene)
        {
            var shpReader = new NetTopologySuite.IO.ShapeFile.Extended.ShapeDataReader(shpPath);
            var fea= shpReader.ReadByMBRFilter(shpReader.ShapefileBounds);
            _features = fea;
            _scene = scene;
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
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将VectorStyle转换成结构体UBO
    /// </summary>
    public static class VectorStyleExtension
    {
        public static VectorStyleUBO ToUBO(this VectorStyle style)
        {
            return new VectorStyleUBO(style.FillColor,style.LineColor);
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
        List<Pipeline> _pipeLines;        
        private List<DeviceBuffer> _vertexBuffer=new List<DeviceBuffer>();
        private List<DeviceBuffer> _indexBuffer=new List<DeviceBuffer>();
        private List<DeviceBuffer> _boundingboxvertexBuffer = new List<DeviceBuffer>();
        private List<DeviceBuffer> _boundingBoxindiceBuffer = new List<DeviceBuffer>();
        private DeviceBuffer _styleBuffer;
        private ResourceSet _styleResourceSet;
        private bool ShowBoundingBox = true;
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
        private Pipeline _boundingBoxPipeLine2;

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
            Style = new VectorStyle();
        }
        /// <summary>
        /// 显示轮廓
        /// </summary>
        /// <returns></returns>
        public bool ShowWireFrame { get; set; }  
        /// <summary>
        /// 图层渲染的样式信息
        /// </summary>
        public Styles.VectorStyle Style { get; set; }
        /// <summary>
        /// 创建相关资源
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="factory"></param>
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //计算每个mesh的boundbox                      
            _mesh= FeatureTrianglator.FeatureToMesh(this._feature, this._shape);
            
            foreach (var item in _mesh)
            {
                var typle = item.CreateGraphicResource(gd, factory);
                var box= BoundingBox.CreateFromPoints(item.Positions);
                _vertexBuffer.Add(typle.Item1);
                _indexBuffer.Add(typle.Item2);
            }
            //三角细分,细分精度为1度
            //_mesh = TriangleMeshSubdivision.Compute(posClearUp, indices.ToArray(), Math.PI / 180);
           
            //创建一个Color的Buffer并更新
            _styleBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer|BufferUsage.Dynamic));
            gd.UpdateBuffer(_styleBuffer,0,Style.ToUBO());

            //创建一个stylelayout
            ResourceLayout styleLayout = factory.CreateResourceLayout(
               new ResourceLayoutDescription(
                   new ResourceLayoutElementDescription("Style", ResourceKind.UniformBuffer,  ShaderStages.Fragment)

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
            rasterizer.FillMode = PolygonFillMode.Wireframe;
            _boundingBoxPipeLine = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rasterizer,
                PrimitiveTopology.LineStrip,
                shaderSet,
                //共享View和prj的buffer
                new ResourceLayout[] { ShareResource.ProjectionResourceLoyout },
                gd.MainSwapchain.Framebuffer.OutputDescription));
          
            _boundingBoxPipeLine2 = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rasterizer,
                PrimitiveTopology.LineList,
                shaderSet,
                //共享View和prj的buffer
                new ResourceLayout[] { ShareResource.ProjectionResourceLoyout },
                gd.MainSwapchain.Framebuffer.OutputDescription));


            //创建一个StyleresourceSet
            _styleResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
               styleLayout,
               _styleBuffer
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
            _cl.SetPipeline(_pipeline);          
            _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
            _cl.SetGraphicsResourceSet(1, _styleResourceSet);
            for (int i = 0; i < _mesh.Count; i++)
            {
                _cl.SetVertexBuffer(0, _vertexBuffer[i]);
                _cl.SetIndexBuffer(_indexBuffer[i], IndexFormat.UInt16);
                _cl.DrawIndexed((uint)_mesh[i].Indices.Length, 1, 0, 0, 0);                
            }
           
            for (int i = 0; i < _mesh.Count; i++)
            {
                //同时显示每个polygonMesh的外包盒子
                _cl.SetPipeline(_boundingBoxPipeLine);
                //_boundingBoxPipeLine.
                _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
                _cl.SetVertexBuffer(0, _vertexBuffer[i]);
                _cl.SetIndexBuffer(_indexBuffer[i], IndexFormat.UInt16);
                //绘制上面
                _cl.DrawIndexed(5, 1, 0, 0, 0);
                //绘制下面
                _cl.DrawIndexed(5, 1, 0, 0, 0);
                //切换到渲染管线2
                _cl.SetPipeline(_boundingBoxPipeLine2);
                //绘制四条边线
                _cl.DrawIndexed((uint)_mesh[i].Indices.Length, 1, 0, 0, 0);
            }
        }

        public void Update()
        {
            
        }
    }


    //public class PolygonRender


    /// <summary>
    /// 样式信息的结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VectorStyleUBO
    {
        /// <summary>
        /// 多边形的颜色,结构类似vector4
        /// </summary>
        public RgbaFloat FillColor;
        public RgbaFloat LineColor;

        public VectorStyleUBO(RgbaFloat fillColor,RgbaFloat lineColor)
        {
            FillColor = fillColor;
            LineColor = lineColor;
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
