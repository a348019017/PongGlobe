using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using Xilium.CefGlue;
using PongGlobe.Scene.cef;
using Xilium.CefGlue.Wrapper;
using System.Drawing;
using PongGlobe.Core.Util;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 渲染和更新主界面UI
    /// </summary>
    public class MainUIRender : IRender
    {
        /// <summary>
        /// 默认显示的网页
        /// </summary>
        //private string url = "http://www.baidu.com";
       // private string url = "file:///E:/chromeDownLoad/cef-mixer-master/cef-mixer-master/resource/hud.html";
        private string url = "http://192.168.0.133:6767/index.html";
        //隐藏进度条
        private bool hideScrollbars = true;
        private bool shouldQuit = false;
        private CefOSRClient cefClient;
        private CefBrowserHost _host;
        private CefBrowser _browser;
        private Texture _texture;
        private TextureView _textureView;
        private ShaderSetDescription _shaderset;
        private Pipeline _screenPipeline;
        private ResourceSet _screenResourceSet;
        private ResourceLayout _screenResourceLayout;
        private CommandList _cl;
        private GraphicsDevice _gd;

        //进程锁，在修改texture时使用
        public readonly object BitmapLock = new object();

        /// <summary>
        /// 子界面的大小
        /// </summary>
        private uint _windowsWidth;
        private uint _windowsHeight;


        /// <summary>
        /// 加载Cef相关的资源
        /// </summary>
        public MainUIRender(GraphicsDevice gd,IntPtr handle,uint windwosWidth,uint windowsHeight)
        {
            _gd = gd;
            _windowsHeight = windowsHeight;
            _windowsWidth = windwosWidth;
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(handle, true);

            CefBrowserSettings cefBrowserSettings = new CefBrowserSettings()
            {
                BackgroundColor = new CefColor(0, 60, 85, 115),
                JavaScript = CefState.Enabled,
                JavaScriptAccessClipboard = CefState.Disabled,
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptDomPaste = CefState.Disabled,
                //JavaScriptOpenWindows = CefState.Disabled,
                LocalStorage = CefState.Disabled
            };

            // Initialize some of the custom interactions with the browser process.
            this.cefClient = new CefOSRClient(this);
           
            // Start up the browser instance.
            CefBrowserHost.CreateBrowser(cefWindowInfo, this.cefClient, cefBrowserSettings, string.IsNullOrEmpty(this.url) ? "http://www.google.com" : this.url);
            
        }


        //调整其子界面的大小
        public void ResizeWindow(uint width, uint height)
        {
            lock (BitmapLock)
            {
                //触发host的resize
                _windowsHeight = height;
                _windowsWidth = width;
                if (_host != null)
                    _host.WasResized();
                //同时重建渲染管线
                if (_screenPipeline != null)
                    _screenPipeline.Dispose();
                if (_textureView != null)
                    _textureView.Dispose();
                if (_texture != null)
                    _texture.Dispose();
                if (_screenResourceSet != null)
                    _screenResourceSet.Dispose();

                
                _texture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)_windowsWidth, (uint)_windowsHeight, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled));
                _textureView = _gd.ResourceFactory.CreateTextureView(_texture);

                //重建resourceSet
                _screenResourceSet = _gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
           _screenResourceLayout,
           _textureView,
           _gd.Aniso4xSampler
           ));

                var rasterizer = RasterizerStateDescription.Default;
                rasterizer.FillMode = PolygonFillMode.Solid;
                rasterizer.FrontFace = FrontFace.CounterClockwise;
                rasterizer.CullMode = FaceCullMode.Front;
                _screenPipeline = _gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                   BlendStateDescription.SingleOverrideBlend,
                   DepthStencilStateDescription.DepthOnlyLessEqual,
                   rasterizer,
                   PrimitiveTopology.TriangleList,
                   _shaderset,
                   //共享View和prj的buffer
                   new ResourceLayout[] { _screenResourceLayout },
                   _gd.MainSwapchain.Framebuffer.OutputDescription));


            }

           
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            ///创建一个默认的commandlist
            _cl = factory.CreateCommandList();

            //创建一个当前窗体大小的屏幕纹理
            _texture = factory.CreateTexture(TextureDescription.Texture2D((uint)_windowsWidth, (uint)_windowsHeight, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled));
            //创建textureView
            _textureView = factory.CreateTextureView(_texture);

            //Create resourceLayout
            _screenResourceLayout = factory.CreateResourceLayout(
              new ResourceLayoutDescription(                
                  new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                  new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)                
                  ));

            //create resourceview
           _screenResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
           _screenResourceLayout,
           _textureView,
           gd.Aniso4xSampler           
           ));

            var curAss = this.GetType().Assembly;
            //无顶点布局
            _shaderset = new ShaderSetDescription(
                new VertexLayoutDescription[] { },
                new[]
                {
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Vertex,"FullScreenVS.spv",gd,curAss),
                   ResourceHelper.LoadEmbbedShader(ShaderStages.Fragment,"FullScreenFS.spv",gd,curAss),                  
                });

            var rasterizer = RasterizerStateDescription.Default;
            rasterizer.FillMode = PolygonFillMode.Solid;
            rasterizer.FrontFace = FrontFace.CounterClockwise;
            rasterizer.CullMode = FaceCullMode.Front;
            //创建渲染管道
            _screenPipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
               BlendStateDescription.SingleOverrideBlend,
               DepthStencilStateDescription.DepthOnlyLessEqual,
               rasterizer,
               PrimitiveTopology.TriangleList,
               _shaderset,
               //共享View和prj的buffer
               new ResourceLayout[] { _screenResourceLayout },
               gd.MainSwapchain.Framebuffer.OutputDescription));


           

        }

        public void Dispose()
        {
            if (_host != null)
            {
                _host.CloseBrowser();
                _host = null;
            }
            if (_browser != null)
            {
                _browser.Dispose();
                _browser = null;
            }
            CefRuntime.Shutdown();
        }

        public void Draw(CommandList _cl)
        {
            //throw new NotImplementedException();
            _cl.SetPipeline(_screenPipeline);
            _cl.SetGraphicsResourceSet(0, _screenResourceSet);
            //_cl.SetGraphicsResourceSet(1, _pointStyleRSet);
            //_cl.SetVertexBuffer(0, _VertexBuffer);
            //_cl.SetIndexBuffer(_IndicesBuffer, IndexFormat.UInt16);
            //_systemEventUbo.MousePosition = PongGlobe.Scene.InputTracker.MousePosition;
            //_cl.UpdateBuffer(_eventBuffer, 0, _systemEventUbo);
            _cl.Draw(3, 1, 0, 0);
        }

        /// <summary>
        /// 仅调用Update来更新主界面即可
        /// </summary>
        public void Update()
        {
            
        }

        #region all cef handler


        /// <summary>
        /// 创建之后的事件回调
        /// </summary>
        /// <param name="browser"></param>
        public void HandleAfterCreated(CefBrowser browser)
        {
            this._host = browser.GetHost();
            CefInputTracker.Host = _host;
        }


        /// <summary>
        /// 处理获取当前ViewRect的事件
        /// </summary>
        public void HandleGetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = (int)_windowsWidth;
            rect.Height = (int)_windowsHeight;          
        }


        /// <summary>
        /// 处理重绘操作
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="type"></param>
        /// <param name="dirtyRects"></param>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        //此方法是多线程进行的，因此-texture在多线程中被使用
        public void HandleOnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (_texture != null)
            {
                lock (BitmapLock)
                {
                    //当前分辨率不一致
                    if (width != _windowsWidth && height != _windowsHeight)
                    {
                        return;
                    }
                    else {
                        _gd.UpdateTexture(_texture, buffer, (uint)(_windowsWidth * _windowsHeight * 4), 0, 0, 0, _windowsWidth, _windowsHeight, 1, 0, 0);
                        //_gd.WaitForIdle();
                    }                                            
                    foreach (var item in dirtyRects)
                    {
                        //_gd.UpdateTexture                        
                    }
                }                           
            }
        }
        


        #endregion








    }
}
