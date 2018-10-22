using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
using Xilium.CefGlue;
using PongGlobe.Scene.cef;
using Xilium.CefGlue.Wrapper;
using System.Drawing;
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
        //隐藏进度条
        private bool hideScrollbars = false;
        private bool shouldQuit = false;
        private CefOSRClient cefClient;
        private Texture _texture;
        private TextureView _textureView;
        /// <summary>
        /// 加载Cef相关的资源
        /// </summary>
        public MainUIRender(GraphicsDevice gd,int windwosWidth,int windowsHeight)
        {
            //加载runtime
            CefRuntime.Load();
            var cefApp = new CefOSRApp();
            var cefMainArgs = new CefMainArgs(new string[] { });

            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
                Console.WriteLine("Could not start the secondary process.");

            var cefSettings = new CefSettings
            {
                //ExternalMessagePump = true,
                MultiThreadedMessageLoop = false,
                //SingleProcess = true,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
                WindowlessRenderingEnabled = true,
                NoSandbox = true,
            };


            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);

            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);


            CefBrowserSettings cefBrowserSettings = new CefBrowserSettings()
            {
                BackgroundColor = new CefColor(255, 60, 85, 115),
                JavaScript = CefState.Enabled,
                JavaScriptAccessClipboard = CefState.Disabled,
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptDomPaste = CefState.Disabled,
                //JavaScriptOpenWindows = CefState.Disabled,
                LocalStorage = CefState.Disabled
            };

            // Initialize some of the custom interactions with the browser process.
            this.cefClient = new CefOSRClient(windwosWidth, windowsHeight,this.hideScrollbars);

            // Start up the browser instance.
            CefBrowserHost.CreateBrowser(cefWindowInfo, this.cefClient, cefBrowserSettings, string.IsNullOrEmpty(this.url) ? "http://www.google.com" : this.url);

           // this.cefClient
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
            //throw new NotImplementedException();

           
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
        }

        /// <summary>
        /// 仅调用Update来更新主界面即可
        /// </summary>
        public void Update()
        {
            
        }
    }
}
