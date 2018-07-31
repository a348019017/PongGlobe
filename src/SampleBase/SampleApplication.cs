using AssetPrimitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Veldrid;
using ImGuiNET;

namespace SampleBase
{
    //sample中默认显示帧率
    public abstract class SampleApplication
    {
        private readonly Dictionary<Type, BinaryAssetSerializer> _serializers = DefaultSerializers.Get();

        protected Camera _camera;

        public ApplicationWindow Window { get; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ResourceFactory ResourceFactory { get; private set; }
        public Swapchain MainSwapchain { get; private set; }

        private float _ticks;
        protected ImGuiController _controller = null;
        private static FrameTimeAverager _fta = new FrameTimeAverager(0.666);
        protected CommandList _clMain = null;

        public SampleApplication(ApplicationWindow window)
        {
            Window = window;
            Window.Resized += HandleWindowResize;
            Window.GraphicsDeviceCreated += OnGraphicsDeviceCreated;
            Window.GraphicsDeviceDestroyed += OnDeviceDestroyed;
            Window.Rendering += PreDraw;
            Window.Rendering += Draw;
            Window.KeyPressed += OnKeyDown;

            _camera = new Camera(Window.Width, Window.Height);
        }

        public void OnGraphicsDeviceCreated(GraphicsDevice gd, ResourceFactory factory, Swapchain sc)
        {
            GraphicsDevice = gd;
            ResourceFactory = factory;
            MainSwapchain = sc;
            CreateResources(factory);
            CreateSwapchainResources(factory);

            //为帧率的绘制创建一个_cl,同时创建一个
            _clMain = factory.CreateCommandList();
            _controller = new ImGuiController(this.GraphicsDevice, this.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, (int)this.Window.Width, (int)this.Window.Height);
        }

        protected virtual void OnDeviceDestroyed()
        {
            GraphicsDevice = null;
            ResourceFactory = null;
            MainSwapchain = null;
        }

        protected virtual string GetTitle() => GetType().Name;

        protected abstract void CreateResources(ResourceFactory factory);

        protected virtual void CreateSwapchainResources(ResourceFactory factory) { }

        private void PreDraw(float deltaSeconds)
        {
            _camera.Update(deltaSeconds);

            _controller.Update(1f / 60f, InputTracker.FrameSnapshot);
            _fta.AddTime(deltaSeconds);
            SubmitUI();
            _ticks += deltaSeconds * 1000f;
            //_cl.Begin();
            //_cl.SetFramebuffer(MainSwapchain.Framebuffer);
            ////_cl.ClearColorTarget(0, RgbaFloat.Black);
            ////_cl.ClearDepthStencil(1f);           
            //_controller.Render(GraphicsDevice, _cl);
            //_cl.End();
            //GraphicsDevice.SubmitCommands(_cl);
            //GraphicsDevice.SwapBuffers(MainSwapchain);
            //GraphicsDevice.WaitForIdle();
        }

        //显示imgui
        private  void SubmitUI()
        {
            {
                //显示帧率
                ImGui.Text(_fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));
                //ImGui.BeginWindow()
                //ImGui.Text("Hello, world!");                                        // Display some text (you can use a format string too)                
            }
        }

        protected abstract void Draw(float deltaSeconds);

        protected virtual void HandleWindowResize()
        {
            _camera.WindowResized(Window.Width, Window.Height);
        }

        protected virtual void OnKeyDown(KeyEvent ke) { }

        public Stream OpenEmbeddedAssetStream(string name) => GetType().Assembly.GetManifestResourceStream(name);

        public Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
        {
            string name = $"{set}-{stage.ToString().ToLower()}.{GetExtension(factory.BackendType)}";
            return factory.CreateShader(new ShaderDescription(stage, ReadEmbeddedAssetBytes(name), entryPoint));
        }

        public byte[] ReadEmbeddedAssetBytes(string name)
        {
            using (Stream stream = OpenEmbeddedAssetStream(name))
            {
                byte[] bytes = new byte[stream.Length];
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    stream.CopyTo(ms);
                    return bytes;
                }
            }
        }

        private static string GetExtension(GraphicsBackend backendType)
        {
			bool isMacOS = RuntimeInformation.OSDescription.Contains("Darwin");

            return (backendType == GraphicsBackend.Direct3D11)
                ? "hlsl.bytes"
                : (backendType == GraphicsBackend.Vulkan)
                    ? "450.glsl.spv"
                    : (backendType == GraphicsBackend.Metal)
					    ? isMacOS ? "metallib" : "ios.metallib"
                        : (backendType == GraphicsBackend.OpenGL)
                            ? "330.glsl"
                            : "300.glsles";
        }

        public T LoadEmbeddedAsset<T>(string name)
        {
            if (!_serializers.TryGetValue(typeof(T), out BinaryAssetSerializer serializer))
            {
                throw new InvalidOperationException("No serializer registered for type " + typeof(T).Name);
            }

            using (Stream stream = GetType().Assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("No embedded asset with the name " + name);
                }

                BinaryReader reader = new BinaryReader(stream);
                return (T)serializer.Read(reader);
            }
        }
    }
}
