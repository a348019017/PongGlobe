using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using System.Numerics;
using PongGlobe.Renders;
using PongGlobe.Core.Util;


namespace PongGlobe.Scene
{
    //写一个在屏幕近裁剪面绘制线的功能,带UI和相关交互，看如何组织代码
    public class DrawLineTool : IRender
    {
        //当前需要绘制的顶点集合
        private List<Vector3> points = new List<Vector3>();        
        private List<ushort> indices = new List<ushort>();
        private LineVectorStyleUBO _lineStyle;

        private DeviceBuffer _lineVertexBuffer;
        private DeviceBuffer _lineIndicesBuffer;
        private DeviceBuffer _polylinetyleBuffer;
        private ResourceSet _styleResourceSet;
        private GraphicsDevice _gd;
        //线的渲染管线
        private Pipeline _pipeline;
        private Scene _scene;


        public DrawLineTool(Scene scene)
        {
            _scene=scene;
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
             _gd = gd;
            ///至多100个点输入，至多102个indices输入
             _lineVertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(6 * 100), BufferUsage.VertexBuffer));
            //gd.UpdateBuffer(_vertexBuffer, 0, this.Positions);
             _lineIndicesBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * 102), BufferUsage.IndexBuffer));
            //gd.UpdateBuffer(_indexBuffer, 0, this.Indices);
            _polylinetyleBuffer  = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            gd.UpdateBuffer(_polylinetyleBuffer, 0, new LineVectorStyleUBO(RgbaFloat.Red));


            ResourceLayout styleLayout = factory.CreateResourceLayout(
              new ResourceLayoutDescription(
                  new ResourceLayoutElementDescription("LineStyle", ResourceKind.UniformBuffer, ShaderStages.Fragment | ShaderStages.Geometry)                
                  ));


            var curAss = this.GetType().Assembly;


            //这里position的定义极有可能是vec3，因此传入vec2可能出现问题，具体可以参考vk里的源码
            ShaderSetDescription shaderSetBoundingBox = new ShaderSetDescription(
               new[]
               {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))
               },
               new[]
               {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"DrawLineVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"DrawLineFS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Geometry,"DrawLineGS.spv",gd,curAss)
               });


            var rasterizer = RasterizerStateDescription.Default;
            rasterizer.FillMode = PolygonFillMode.Wireframe;
            rasterizer.FrontFace = FrontFace.CounterClockwise;
            //gpu的lineWidth实际绘制的效果并不好仍然需要GeometryShader来实现更好的效果
            //rasterizer.LineWidth = 8.0f;
            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rasterizer,
                PrimitiveTopology.LinesAdjacency,
                shaderSetBoundingBox,
                //共享View和prj的buffer
                new ResourceLayout[] { ShareResource.ProjectionResourceLoyout, styleLayout },
                gd.MainSwapchain.Framebuffer.OutputDescription));


            //创建一个StyleresourceSet,0是线样式1是面样式
            _styleResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
               styleLayout,
               _polylinetyleBuffer
               ));

        }

        public void Dispose()
        {
            
        }

        public void Draw(CommandList _cl)
        {
            if (points.Count >= 2)
            {
                _cl.SetPipeline(_pipeline);
                _cl.SetGraphicsResourceSet(0, ShareResource.ProjectuibResourceSet);
                _cl.SetGraphicsResourceSet(1, _styleResourceSet);
                _cl.SetVertexBuffer(0, _lineVertexBuffer);
                _cl.SetIndexBuffer(_lineIndicesBuffer, IndexFormat.UInt16);
                _cl.DrawIndexed((ushort)indices.Count, 1, 0, 0, 0);
            }         
        }

        public void Update()
        {
            var pos = InputTracker.GetMouseButtonDown(MouseButton.Left);
            if (pos)
            {
                var point = InputTracker.MousePosition;
                points.Add(new Vector3(point,0));
                //添加点之后更新buffer中的数据
                UpdateBuffer();
            }
            //空格键清空
            if (InputTracker.GetKeyDown(Key.Space))
            {
                points.Clear();
            }
        }

        /// <summary>
        /// 更新顶点和indice
        /// </summary>
        public void UpdateBuffer()
        {
            ///至少具有两个点
            if (points.Count >= 2)
            {
                int count = points.Count;
                List<Vector3> meshPoints = new List<Vector3>(count+2);
                //计算第一个ajacy,根据第一个点和第二个点
                //将屏幕坐标转换成NDC坐标
                var viewportMaxtrix= ((MyCameraController2)_scene.Camera).ViewportMaxtrix;
                Matrix4x4 invertViewportMaxtrix;
                     Matrix4x4.Invert(viewportMaxtrix,out invertViewportMaxtrix);
                //将视口坐标转换成NDC坐标
                meshPoints.Add(Vector3.Transform(2 * points[0] - points[1],invertViewportMaxtrix));
                meshPoints.AddRange(points.ConvertAll(p=>Vector3.Transform(p, invertViewportMaxtrix)));
                meshPoints.Add(Vector3.Transform(2 * points[count - 1] - points[count - 2], invertViewportMaxtrix));
                //生成Indices
                indices.Clear();
                for (ushort i = 1; i < count ; i++)
                {
                    indices.Add((ushort)(i-1));
                    indices.Add((ushort)(i));
                    indices.Add((ushort)(i+1));
                    indices.Add((ushort)(i+2));
                }
                //如果updatebuffer是异步的方法，可能需要同步draw方法
                _gd.UpdateBuffer(_lineVertexBuffer, 0, meshPoints.ToArray());
                _gd.UpdateBuffer(_lineIndicesBuffer, 0, this.indices.ToArray());
            }         
        }

    }



    


}
