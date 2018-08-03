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
    /// <summary>
    /// 
    /// </summary>
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
        private BaseUBO _ubo = new BaseUBO();
        private MyCamera myCamera;

        public Extent Extent { get ; set ; }

        public Ellipsoid Shape { get; set; }

      

        protected override void HandleWindowResize()
        {
            if (_controller == null) return;
            _controller.WindowResized((int)this.Window.Width, (int)this.Window.Height);
        }


        public TexturedEarth(ApplicationWindow window) : base(window)
        {
           
            //_stoneTexData = LoadEmbeddedAsset<ProcessedTexture>("Earth.binary");
            //_stoneTexData.MipLevels = 1;
            Shape = Ellipsoid.ScaledWgs842;

            myCamera = new MyCamera(Window.Width, Window.Height,Shape);

            var cameraInfo = new CameraInfo(new Geodetic3D(0, 0, 1), -MathF.PI/4, MathF.PI*25.0/180.0);          
            _camera = myCamera.Camera;
            myCamera.CameraInfo = cameraInfo;
            

            var mesh = PongGlobe.Core.BoxTessellator.Compute(2*Shape.Radii);
            _vertices = mesh.Positions;
            _indices = mesh.Indices;
           // _vertices = CreateSphere(1, 32, 32, out _indices);
            //_indices = GetCubeIndices();
            //_indices = GetCubeIndices();
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="embedName"></param>
        /// <returns></returns>
        private Shader LoadEmbbedShader(ShaderStages stage, string embedName)
        {
            byte[] shaderBytes = ReadEmbeddedAssetBytes(embedName);
            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            return GraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
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
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(144, BufferUsage.UniformBuffer| BufferUsage.Dynamic));
           // _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            //_worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VertexPositionColorTexture.SizeInBytes * 100000), BufferUsage.VertexBuffer));
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, _vertices);

            _indexBuffer = factory.CreateBuffer(new BufferDescription(sizeof(ushort) * 400000, BufferUsage.IndexBuffer));

            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, _indices);
            var DiffuseIntensity = 0.65f;
            var SpecularIntensity = 0.25f;
            var AmbientIntensity = 0.10f;
            var Shininess = 12;
            var lightModel = new Vector4(
                DiffuseIntensity,
                SpecularIntensity,
                AmbientIntensity,
                Shininess);
            _ubo.DiffuseSpecularAmbientShininess = lightModel;
            _ubo.GlobeOneOverRadiiSquared = Shape.OneOverRadiiSquared;            
            //_ubo.UseAverageDepth = false;
            //提前更新参数
            GraphicsDevice.UpdateBuffer(_projectionBuffer, 0, _ubo);


            var inPath = @"E:\swyy\Lib\veldrid-samples\assets\NE2_50M_SR_W_4096.jpg";
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
                   LoadEmbbedShader(ShaderStages.Vertex,"GlobeVS.spv"),
                   LoadEmbbedShader(ShaderStages.Fragment,"GlobeFS.spv")
                });

            ResourceLayout projViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex|ShaderStages.Fragment)

                    ));

            ResourceLayout worldTextureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(                   
                    new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            var rd = RasterizerStateDescription.Default;
            rd.CullMode = FaceCullMode.None;
            rd.FillMode = PolygonFillMode.Solid;
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


        protected override void PreDraw(float deltaSeconds)
        {
            base.PreDraw(deltaSeconds);
            myCamera.Update(deltaSeconds);
        }

        //自转
        protected override void Draw(float deltaSeconds)
        {
            //地球自转的速度为
            var selfrockRate = 0.00007292;
            

            //_controller.Update(1f / 60f, InputTracker.FrameSnapshot);
            //_fta.AddTime(deltaSeconds);
            //SubmitUI();            
            _ticks += deltaSeconds * 1000f;
            _cl.Begin();

            //投影矩阵
            var prj = _camera.ProjectionMatrix;
            
            var view = 
                _camera.ViewMatrix;

            

            Matrix4x4 rotation =
                Matrix4x4.CreateFromAxisAngle(-Vector3.UnitY, (float)(_ticks /10* selfrockRate));
            //这里矩阵的定义和后者是有区别的，numberic中是行列，glsl中是列行，因此这里需要反向计算
            var prjviewWorld = view* prj;
            _ubo.ProjectionViewModel = prjviewWorld;
            _ubo.CameraEye = _camera.Position;
            _ubo.CameraEyeSquared = _camera.Position * _camera.Position;
            _ubo.CameraLightPosition = _camera.Position;
            
            _cl.UpdateBuffer(_projectionBuffer, 0, _ubo);          
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
            
            GraphicsDevice.SwapBuffers(MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }



       

     

       

       
    }




    /// <summary>
    /// 可能变化的相关参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BaseUBO
    {
        //变换矩阵
        public Matrix4x4 ProjectionViewModel;
                
        //相机位置的平方
        public Vector3 CameraEyeSquared;

        public float spa1;

        //相机位置
        public Vector3 CameraEye;

        public float spa2;

        //光线起始位置，暂等于相机位置
        public Vector3 CameraLightPosition;

        public float spa3;

        //关照模型的参数
        public Vector4 DiffuseSpecularAmbientShininess;
        
        //固定常量
        public Vector3 GlobeOneOverRadiiSquared;

        public float spa4;

    }



}
