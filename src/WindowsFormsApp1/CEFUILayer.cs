using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;
namespace PongGlobe.Scene
{
    /// <summary>
    /// cef离线渲染的图层，暂时仅在winform中实现
    /// </summary>
    public class CEFUILayer : IRender
    {

        /// <summary>
        /// 测试Url
        /// </summary>
        private const string TestUrl = "https://www.baidu.com/";
        //private ChromiumWebBrowser _osrWebBrowser;
        private GraphicsDevice _gd;

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {          
            //创建一个cef
            
            //var browserSettings = new BrowserSettings(TestUrl);
            //Reduce rendering speed to one frame per second so it's easier to take screen shots
            //browserSettings.WindowlessFrameRate = 1;
            //var requestContextSettings = new RequestContextSettings { CachePath = cachePath };

             //_osrWebBrowser = new ChromiumWebBrowser(TestUrl)


        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Draw(CommandList _cl)
        {
            //throw new NotImplementedException();
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
