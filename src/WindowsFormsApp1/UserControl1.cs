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
using Veldrid.Sdl2;



namespace WindowsFormsApp1
{


    /// <summary>
    /// EarthControl的winform封装，这里暂时不使用sld的事件处理，为求跨平台，可能还是需要使用sdl2的库。
    /// //这里需要单独开一个线程来跑渲染进程
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        private GraphicsDevice _gd;
        private DisposeCollectorResourceFactory _factory;
        private Task _renderTask;
        public event Action<float> Rendering;
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;
        public event Action GraphicsDeviceDestroyed;
        // public event Action Resized;
        private Sdl2Window _window;

        private CommandList _cl;
        //是否正在进行渲染，如缩小界面的时候都可以停止渲染。
        private bool isRendering = true;
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
       
       

        //创建一个计时器和记录之前的时间
        Stopwatch sw;
        double previousElapsed;


        public unsafe UserControl1()
        {         
            try
            {
                InitializeComponent();
                if (this.DesignMode) return;
                this.MouseWheel += UserControl1_MouseWheel;

                //释放组件
                Disposed += OnDispose;
                //this.MouseMove += UserControl1_MouseMove    ;
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
                var p = hwnd.ToPointer();
                //这里创建的sld windows消息捕获出现了问题,
               
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
                _window = new Sdl2Window(hwnd, false);
                //创建一个计时器
                sw = Stopwatch.StartNew();
                previousElapsed = sw.Elapsed.TotalSeconds;
                //this.ParentForm.Shown += ParentForm_Shown;
                //开始运行
                //Run();
                //创建一个线程执行run,使用多线程之后便需要考虑不同线程变量同步的问题了，例如isrunning变量的修改需要lock之后再修改，有时可以使用线程安全的集合来处理不同线程的变量交换，
                //c#封装了很多，这些都不是问题_coomadList并不是线程安全的，在主线程创建，在子线程使用，这样问题并不大，当时两个子线程同时使用commandLits便可能出现问题
                //这里的渲染仍然是单线程的，每个不同的render都可以独开线程并提交渲染任务
                _renderTask= Task.Factory.StartNew(()=> { Run(); });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
        }

        //可以直接在winform控件的事件中修改
        private void UserControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            // throw new NotImplementedException();
            if (e.Delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var camera = this._scene.Camera as MyCameraController2;
                var lookat = camera.LookAtInfo;
                var de = e.Delta/100 * (lookat.Range) * 0.1;
                lookat.Range += de;
                camera.LookAtInfo = lookat;
                //pos.Height += de;
                //更新相机位置后更新View矩阵
                camera.UpdateCamera();
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
            //_controller = new ImGuiController(this.GraphicsDevice, this.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, (int)this.Width, (int)this.Height);

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

        
        public void Run()
        {
            while (isRendering)
            {
                //防止界面假死
               // Application.DoEvents();
                double newElapsed = sw.Elapsed.TotalSeconds;
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                //这里不再使用通用的事件处理程序
                InputSnapshot inputSnapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);
                //           
                previousElapsed = newElapsed;
                Rendering?.Invoke(deltaSeconds);
            }          
        }


        //在onpait中渲染
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
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
            //if (_controller != null)
            //{
            //    _controller.WindowResized((int)this.Width, (int)this.Height);
            //}       
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
           //_controller.Update(1f / 60f, InputTracker.FrameSnapshot);
            //更新相机
            _scene.Camera.Update(deltaSeconds);
           
           // SubmitUI();
            _ticks += deltaSeconds * 1000f;
            foreach (var item in renders)
            {
                item.Update();
            }
        }

        private void SubmitUI()
        {
            {
                //显示帧率
                //ImGui.Text(_fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));
            }
        }


        //控件释放时释放相关资源
        private void OnDispose(object sender, EventArgs e)
        {
            // do stuff on dispose
            _gd.WaitForIdle();
            _factory.DisposeCollector.DisposeAll();
            _gd.Dispose();
            //释放掉渲染线程
            if (_renderTask != null) _renderTask.Dispose();
            //置空相关变量
            GraphicsDevice = null;
            ResourceFactory = null;
            MainSwapchain = null;
        }



    }
}
