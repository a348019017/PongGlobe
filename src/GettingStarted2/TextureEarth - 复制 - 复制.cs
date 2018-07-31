using AssetPrimitives;
using SampleBase;
using System.Numerics;
using Veldrid;
using System.Collections.Generic;
using System;
using System.IO;
using Veldrid.ImageSharp;
using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using GettingStarted2.GISEngine;
using BruTile;
using GettingStarted2.GISEngine.Core;
using PongGlobe.Core;
namespace GettingStarted2
{
    public class TexturedEarth : SampleApplication,IEarthView
    {
        private readonly ProcessedTexture _stoneTexData;
        private VertexPosition[] _vertices;
        private  ushort[] _indices;
        private DeviceBuffer _projectionBuffer;
        //private DeviceBuffer _viewBuffer;
        //private DeviceBuffer _worldBuffer;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private CommandList _cl;
        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;
        private Pipeline _pipeline;
        private ResourceSet _projViewSet;
        private ResourceSet _worldTextureSet;
        private float _ticks;
        

        public Extent Extent { get ; set ; }


        /// <summary>
        /// 屏幕坐标转世界坐标系
        /// </summary>
        //private Vector3 ScreenPositionTo


        protected override void HandleWindowResize()
        {
            if (_controller == null) return;
            _controller.WindowResized((int)this.Window.Width, (int)this.Window.Height);
        }


        public TexturedEarth(ApplicationWindow window) : base(window)
        {


            //_stoneTexData = LoadEmbeddedAsset<ProcessedTexture>("Earth.binary");
            //_stoneTexData.MipLevels = 1;
            var mesh = PongGlobe.Core.BoxTessellator.Compute(new Vector3(1, 1, 1));
            _vertices = mesh.Positions;
            _indices = mesh.Indices;
           // _vertices = CreateSphere(1, 32, 32, out _indices);
            //_indices = GetCubeIndices();
            //_indices = GetCubeIndices();
            
        }


       


        private  Shader LoadShader(ShaderStages stage)
        {
            string extension = null;         
            switch (GraphicsDevice.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    extension = "hlsl.bytes";
                    break;
                case GraphicsBackend.Vulkan:
                    extension = "spv";
                    break;
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "metallib";
                    break;
                default: throw new System.InvalidOperationException();
            }
           
            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory, "Shaders", $"{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return GraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }

        protected unsafe override void CreateResources(ResourceFactory factory)
        {

            _camera.Position = new Vector3(10f, 0f, 0f);           
            _camera.NearDistance = 0.001f;
            _camera.FarDistance = 100f;
            _camera.Pitch = 0f;
            _camera.Yaw = (float)Math.PI / 2;
            //_camera.
            //_camera.

           

            _projectionBuffer = factory.CreateBuffer(new BufferDescription(192, BufferUsage.UniformBuffer| BufferUsage.Dynamic));
           // _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            //_worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionColorTexture.SizeInBytes * 100000), BufferUsage.VertexBuffer));
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);

            _indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * 400000, BufferUsage.IndexBuffer));
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices);

            

            var inPath = @"E:\swyy\Lib\veldrid-samples\assets\Earth.png";
            ImageSharpTexture inputImage = new ImageSharpTexture(inPath, false);
            _surfaceTexture = inputImage.CreateDeviceTexture(GraphicsDevice, factory);
            
             _surfaceTextureView = factory.CreateTextureView(_surfaceTexture);
            //_surfaceTexture = _stoneTexData.CreateDeviceTexture(GraphicsDevice, ResourceFactory,TextureUsage.Storage);
            //_surfaceTextureView = factory.CreateTextureView(_surfaceTexture);
           
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3))                      
                },
                new[]
                {
                    LoadShader(ShaderStages.Vertex),
                    LoadShader(ShaderStages.Fragment)
                });

            ResourceLayout projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    ));

            ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(                   
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            var rd = RasterizerStateDescription.Default;
            rd.CullMode = FaceCullMode.Front;
            //rd.c
            //rd.FillMode = PolygonFillMode.Wireframe;
            _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rd,
                PrimitiveTopology.TriangleList,
                shaderSet,
                new[] { projViewLayout, worldTextureLayout },
                MainSwapchain.Framebuffer.OutputDescription));

            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
                projViewLayout,
                _projectionBuffer
                ));
            //var sampler = Veldrid.SamplerDescription.Aniso4x;
            //sampler.AddressModeU=SamplerAddressMode.

            _worldTextureSet = factory.CreateResourceSet(new ResourceSetDescription(
                worldTextureLayout,            
                _surfaceTextureView,
                GraphicsDevice.Aniso4xSampler));

            _cl = factory.CreateCommandList();
        }

        protected override void OnDeviceDestroyed()
        {
            
            base.OnDeviceDestroyed();
        }

        //自转
        protected override void Draw(float deltaSeconds)
        {
            //地球自转的速度为
            var selfrockRate = 0.00007292;
            

            //_controller.Update(1f / 60f, InputTracker.FrameSnapshot);
            //_fta.AddTime(deltaSeconds);
            //SubmitUI();            
            //_ticks += deltaSeconds * 1000f;
            _cl.Begin();

            //投影矩阵
            var prj = _camera.ProjectionMatrix;
            //Matrix4x4.CreatePerspectiveFieldOfView(
            //    1.0f,
            //    (float)Window.Width / Window.Height,
            //    0.5f,
            //    100f);
            var view = 
                _camera.ViewMatrix;
            //Matrix4x4.CreateLookAt(Vector3.UnitZ * 3.0f, Vector3.UnitZ * 3.0f- Vector3.UnitZ*0.01f, Vector3.UnitY);
            //地球沿着Z轴自转
            Matrix4x4 rotation =
                Matrix4x4.CreateFromAxisAngle(-Vector3.UnitY, (float)(_ticks /10* selfrockRate));

            MappedResourceView<Matrix4x4> writeMap = GraphicsDevice.Map<Matrix4x4>(_projectionBuffer, MapMode.Write);
            //writeMap.= new Matrix4x4[3] { prj, view, rotation };
            writeMap[0] = rotation;
            writeMap[1] = view;
            writeMap[2] = prj;
            GraphicsDevice.Unmap(_projectionBuffer);


            //var prjTemp = new UniformBufferObject(prj,view, rotation);
            //获取Camera距离球面的距离,根据距离计算显示网格数量
            var unit = Vector3.Normalize(_camera.Position);
            var distanceOfEarthSuface = (_camera.Position - unit).Length();
            //以10个单位为32间断采样为标准，距离减少百分比提高相应百分比
            var stacks =(uint)( 32 * (2 - distanceOfEarthSuface / 10));
            //_vertices = CreateSphere(1, stacks, stacks,out _indices);

            //更新顶点坐标
            //_cl.UpdateBuffer(_vertexBuffer, 0, _vertices);
            //_cl.UpdateBuffer(_indexBuffer, 0,  _indices);

           // _cl.UpdateBuffer(_projectionBuffer, 0,ref prjTemp);          
            _cl.SetFramebuffer(MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);
            _cl.SetPipeline(_pipeline);
            _cl.SetVertexBuffer(0, _vertexBuffer);
            _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _cl.SetGraphicsResourceSet(0, _projViewSet);
            _cl.SetGraphicsResourceSet(1, _worldTextureSet);
            _cl.DrawIndexed((uint)_indices.Length, 1, 0, 0, 0);
            _controller.Render(GraphicsDevice, _cl);
            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);
            _clMain.Begin();
            //_clMain.SetFramebuffer(MainSwapchain.Framebuffer);
            _controller.Render(GraphicsDevice, _clMain);
            _clMain.End();
            GraphicsDevice.SubmitCommands(_clMain);
            GraphicsDevice.SwapBuffers(MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }



       

        private VertexPositionColorTexture[] CreateSphere(float radius, float slices, float stacks, out ushort[] indicess)
        {
            var indices = new List<ushort>();
            List<VertexPositionColorTexture> vertex = new List<VertexPositionColorTexture>();
            for (int i = 0; i <= stacks; ++i)
            {
                // V texture coordinate
                double V = i / stacks;
                double phi = V  * Math.PI;
                for (int j = 0; j <= slices; ++j)
                {
                    // U texture coordinate
                    double U = j / (double)slices;
                    double theta = U * 2 * Math.PI;
                                 
                    double X = Math.Cos(theta)*Math.Sin(phi);
                    double Y = Math.Cos(phi);
                    double Z = Math.Sin(theta)*Math.Sin(phi);

                    var vpct = new VertexPositionColorTexture(new Vector3((float)X * radius, (float)Y * radius, (float)Z * radius), new Vector3(0.5f, 0.5f, 0.5f), new Vector2((float)U,1-(float)V) );
                    vertex.Add(vpct);
                }
            }

            //计算Index
            for (int i = 0; i < slices * stacks + slices; i++)
            {
                indices.Add((ushort)i);
                indices.Add((ushort)(i + slices + 1));
                indices.Add((ushort)(i + slices));

                indices.Add((ushort)(i + slices + 1));
                indices.Add((ushort)i);
                indices.Add((ushort)(i + 1));
            }
            indicess = indices.ToArray();
            return vertex.ToArray();
        }

        //根据范围以及级别生成多边形格网
        private VertexPositionColorTexture[] CreateSphere2(float radius, float slice, float statcks, object env, out ushort[] indices)        
        {
            //根据范围读取Cache中的Image，依此生成若干网格或三角网，对应好UV之间的关系
            indices = null;
            return null;
        }

        //private VertexPositionColorTexture[] CreateSphere2(float radius, float slices, float stacks, out ushort[] indicess)
        //{
        //    List<VertexPositionColorTexture> vertex = new List<VertexPositionColorTexture>();

        //    float R = 1f;//球的半径

        //    float statckStep = (float)(Math.PI / stacks);//单位角度值

        //    float sliceStep = (float)(Math.PI / slices);//水平圆递增的角度

        //    float r0, r1, x0, x1, y0, y1, z0, z1; //r0、r1为圆心引向两个临近切片部分表面的两条线 (x0,y0,z0)和(x1,y1,z1)为临近两个切面的点。
        //    float alpha0 = 0, alpha1 = 0; //前后两个角度
        //    float beta = 0; //切片平面上的角度
        //    //List<Float> coordsList = new ArrayList<Float>();
        //    //外层循环
        //    for (int i = 0; i < stacks; i++)
        //    {
        //        alpha0 = (float)(-Math.PI / 2 + (i * statckStep));
        //        alpha1 = (float)(-Math.PI / 2 + ((i + 1) * statckStep));
        //        y0 = (float)(R * Math.Sin(alpha0));
        //        r0 = (float)(R * Math.Cos(alpha0));
        //        y1 = (float)(R * Math.Sin(alpha1));
        //        r1 = (float)(R * Math.Cos(alpha1));

        //        //循环每一层圆
        //        for (int j = 0; j <= (slices * 2); j++)
        //        {
        //            beta = j * sliceStep;
        //            x0 = (float)(r0 * Math.Cos(beta));
        //            z0 = -(float)(r0 * Math.Sin(beta));
        //            x1 = (float)(r1 * Math.Cos(beta));
        //            z1 = -(float)(r1 * Math.Sin(beta));

        //            //添加顶点
        //            var vpct = new VertexPositionColorTexture(new Vector3((float)x0, (float)y0, (float)z0), new Vector3(1, 1, 1), new Vector2((float)U, (float)V));
        //            vertex.Add(vpct);
        //        }
        //    }
        //}


        private static VertexPositionColorTexture[] GetCubeVertices()
        {
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[]
            {
                // Top
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1)),
                // Bottom                                                             
                new VertexPositionColorTexture(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1)),
                // Left                                                               
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                // Right                                                              
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
                // Back                                                               
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                // Front                                                              
                new VertexPositionColorTexture(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
            };

            return vertices;
        }

        private static ushort[] GetCubeIndices()
        {
            ushort[] indices =
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            return indices;
        }


      
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferObject
    {
        public Matrix4x4 World;
        public Matrix4x4 View;
        public Matrix4x4 Projection;        
        public UniformBufferObject(Matrix4x4 _projection, Matrix4x4 _view, Matrix4x4 _world)
        {
            this.Projection = _projection;
            this.View = _view;
            this.World = _world;
        }
    }


   
}
