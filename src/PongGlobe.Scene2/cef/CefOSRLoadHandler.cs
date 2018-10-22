using System;
using System.Collections.Generic;
using System.Text;
using Xilium.CefGlue;

namespace PongGlobe.Scene.cef
{
    internal sealed class CefOSRLoadHandler:CefLoadHandler
    {
        private CefOSRClient client;
        private bool hideScrollbars;

        public CefOSRLoadHandler(CefOSRClient client, bool hideScrollbars)
        {
            this.client = client;
            this.hideScrollbars = hideScrollbars;
        }

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            if (browser != null)
                this.client.sHost = browser.GetHost();

            //if (frame.IsMain)
               // Debug.LogFormat("START: {0}", browser.GetMainFrame().Url);
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (frame.IsMain)
            {
                ///Debug.LogFormat("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode.ToString());

                if (this.hideScrollbars)
                    this.HideScrollbars(frame);
            }
        }

        /// <summary>
        /// 隐藏掉滚动条
        /// </summary>
        /// <param name="frame"></param>
        private void HideScrollbars(CefFrame frame)
        {
            string jsScript = "var head = document.head;" +
                              "var style = document.createElement('style');" +
                              "style.type = 'text/css';" +
                              "style.appendChild(document.createTextNode('::-webkit-scrollbar { visibility: hidden; }'));" +
                              "head.appendChild(style);";
            frame.ExecuteJavaScript(jsScript, string.Empty, 107);
        }
    }
}
