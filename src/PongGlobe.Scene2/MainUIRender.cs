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
        private string url = "http://www.baidu.com";
        //private string url = "file:///E:/chromeDownLoad/cef-mixer-master/cef-mixer-master/resource/hud.html";
        //隐藏进度条
        private bool hideScrollbars = true;
        private bool shouldQuit = false;
        private CefOSRClient cefClient;
        private Texture _texture;
        private TextureView _textureView;
        private ShaderSetDescription _shaderset;
        private Pipeline _screenPipeline;
        private ResourceSet _screenResourceSet;
        private ResourceLayout _screenResourceLayout;


        public CefBrowserHost BrowHost { get { return cefClient.sHost; } }


        /// <summary>
        /// 加载Cef相关的资源
        /// </summary>
        public MainUIRender(GraphicsDevice gd,IntPtr handle,int windwosWidth,int windowsHeight)
        {
            
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
            this.cefClient = new CefOSRClient(windwosWidth, windowsHeight, gd, this.hideScrollbars);

            this._texture = this.cefClient.MainTexture;
            // Start up the browser instance.
            CefBrowserHost.CreateBrowser(cefWindowInfo, this.cefClient, cefBrowserSettings, string.IsNullOrEmpty(this.url) ? "http://www.google.com" : this.url);

            //参照cefwpsosr的例子添加更多消息管理等操作，再测试其实际的帧率。

            //CefBrowserHost


            // this.cefClient
            //CefRuntime.do
        }

        private void Quit()
        {
            this.shouldQuit = true;
            //this.StopAllCoroutines();
            this.cefClient.Shutdown();
            CefRuntime.Shutdown();
        }

        private static void ResizeWindow(IntPtr handle, int width, int height)
        {
            //if (handle != IntPtr.Zero)
            //{
            //    NativeMethods.SetWindowPos(handle, IntPtr.Zero,
            //        0, 0, width, height,
            //        SetWindowPosFlags.NoMove | SetWindowPosFlags.NoZOrder
            //        );
            //}
        }

        //private IEnumerator MessagePump()
        //{
        //    while (!this.shouldQuit)
        //    {
        //        CefRuntime.DoMessageLoopWork();
        //        if (!this.shouldQuit)
        //            this.cefClient.UpdateTexture(this.BrowserTexture);
        //        yield return null;
        //    }
        //}


        /// <summary>
        /// 调整屏幕分辨率
        /// </summary>
        public void Resize()
        {
            
        }


        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            if (_texture == null) throw new Exception("Not Initial texture;");
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
            this.shouldQuit = true;
            //this.StopAllCoroutines();
            this.cefClient.Shutdown();
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
    }
}
