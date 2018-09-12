using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Veldrid;
using Veldrid.Utilities;
using Veldrid.StartupUtilities;
using System.Diagnostics;
using PongGlobe.Core;

using PongGlobe;
using PongGlobe.Scene;
using PongGlobe.Renders;

namespace WindowsFormsApp1
{
    public partial class UserControl1 : UserControl
    {

        private GraphicsDevice _gd;
        private DisposeCollectorResourceFactory _factory;

        public event Action<float> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
       // public event Action Resized;


        private CommandList _cl;

        /// <summary>
        /// 当前地球的场景对象
        /// </summary>
        protected PongGlobe.Scene.Scene _scene;
        /// <summary>
        /// 当前的各个子渲染对象
        /// </summary>
        private List<IRender> renders = new List<IRender>();
          
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ResourceFactory ResourceFactory { get; private set; }
        public Swapchain MainSwapchain { get; private set; }

        private float _ticks;
        //protected ImGuiController _controller = null;
        //protected static FrameTimeAverager _fta = new FrameTimeAverager(0.666);

       


        //创建一个计时器和记录之前的时间
        Stopwatch sw;
        double previousElapsed;


        public UserControl1()
        {
           

            try
            {
                InitializeComponent();

                if (this.DesignMode) return;
                //释放组件
                Disposed += OnDispose;
                


                GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                   debug: false,
                   swapchainDepthFormat: PixelFormat.R16_UNorm,
                   syncToVerticalBlank: true,
                   resourceBindingModel: ResourceBindingModel.Improved);
#if DEBUG
                options.Debug = true;
#endif
                //获取当前窗体的Hwnd
                var hwnd = this.Handle;
                //获取运行进程的handle
                var instance = Process.GetCurrentProcess().Handle;
                _gd = VeldridStartup.CreateVulkanGraphicsDeviceForWin32(options, hwnd, instance, this.Width, this.Height);
                _factory = new DisposeCollectorResourceFactory(_gd.ResourceFactory);

                _scene = new PongGlobe.Scene.Scene(this.Width, this.Height);
                var globeRender = new RayCastedGlobe(_scene);               
                var shareRender = new ShareRender(_scene);
                this.renders.Add(shareRender);
                this.renders.Add(globeRender);

                //创建完相关对象后注册事件
                this.GraphicsDeviceCreated += OnGraphicsDeviceCreated;
                this.GraphicsDeviceDestroyed += OnDeviceDestroyed;
                this.Rendering += PreDraw;
                this.Rendering += Draw;


                GraphicsDeviceCreated?.Invoke(_gd, _factory, _gd.MainSwapchain);

                //创建一个计时器
                 sw = Stopwatch.StartNew();
                 previousElapsed = sw.Elapsed.TotalSeconds;                    
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
        protected virtual void OnDeviceDestroyed()
        {
            GraphicsDevice = null;
            ResourceFactory = null;
            MainSwapchain = null;
        }

        /// <summary>
        /// 在设备创建完成后初始化和创建资源
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="factory"></param>
        /// <param name="sc"></param>
        public void OnGraphicsDeviceCreated(GraphicsDevice gd, ResourceFactory factory, Swapchain sc)
        {
            GraphicsDevice = gd;
            ResourceFactory = factory;
            MainSwapchain = sc;
            CreateResources(factory);
            CreateSwapchainResources(factory);
            //_controller = new ImGuiController(this.GraphicsDevice, this.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, (int)this.Window.Width, (int)this.Window.Height);
        }

        protected virtual void CreateSwapchainResources(ResourceFactory factory) { }


        protected virtual unsafe void CreateResources(ResourceFactory factory)
        {
            //创建一个公共资源
            _cl = factory.CreateCommandList();
            foreach (var item in renders)
            {
                item.CreateDeviceResources(GraphicsDevice, factory);
            }
        }

        //在onpait中渲染
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            double newElapsed = sw.Elapsed.TotalSeconds;
            float deltaSeconds = (float)(newElapsed - previousElapsed);

            //这里不再使用通用的事件处理程序
            //InputSnapshot inputSnapshot = _window.PumpEvents();
            //InputTracker.UpdateFrameInput(inputSnapshot);
            //           
            previousElapsed = newElapsed;                
            Rendering?.Invoke(deltaSeconds);
            
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.DesignMode) return;
            if (_gd != null)
            {
                //尺寸改变时，修改相机的相关参数               
                _gd.ResizeMainWindow((uint)this.Width, (uint)this.Height);
            }
            if (_scene != null)
            {
                _scene.Camera.WindowResized(this.Width, this.Height);
            }         
            //_controller.WindowResized((int)this.Width, (int)this.Height);
        }

        /// <summary>
        /// 目前暂时先使用单个CommandList，在单线程的情况下提交渲染对象
        /// </summary>
        /// <param name="deltaSeconds"></param>
        protected virtual void Draw(float deltaSeconds)
        {
            _cl.Begin();
            _cl.SetFramebuffer(GraphicsDevice.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            foreach (var item in renders)
            {
                //if (item is RayCastedGlobe) continue;
                item.Draw(_cl);
            }
            //最后渲染IMGUI
            //_controller.Render(GraphicsDevice, _cl);
            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);
            GraphicsDevice.SwapBuffers(GraphicsDevice.MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }


        /// <summary>
        /// 渲染之前更新相关参数或者数据
        /// </summary>
        /// <param name="deltaSeconds"></param>
        protected virtual void PreDraw(float deltaSeconds)
        {
           // _controller.Update(1f / 60f, InputTracker.FrameSnapshot);
            //更新相机
            _scene.Camera.Update(deltaSeconds);
            //_fta.AddTime(deltaSeconds);
            //SubmitUI();
            _ticks += deltaSeconds * 1000f;
            foreach (var item in renders)
            {
                item.Update();
            }
        }


        //控件释放时释放相关资源
        private void OnDispose(object sender, EventArgs e)
        {
            // do stuff on dispose
            _gd.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _gd.Dispose();
        }



    }
}
