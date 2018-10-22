using System;
using System.Collections.Generic;
using System.Text;
using Xilium.CefGlue;
using System.Drawing;
using Veldrid;
namespace PongGlobe.Scene.cef
{
    /// <summary>
    /// 
    /// </summary>
    internal class CefOSRClient:CefClient
    {
        private readonly CefLoadHandler _loadHandler;
        private readonly CefOSRRenderHandle _renderHandler;

        private static readonly object sPixelLock = new object();
        
        public Texture MainTexture { get { return _renderHandler.Texture; } }

        public CefBrowserHost sHost;

        public CefOSRClient(int windowWidth,int windowHeight,GraphicsDevice gd, bool hideScrollbars = false)
        {
            this._loadHandler = new CefOSRLoadHandler (this, hideScrollbars);
            this._renderHandler = new CefOSRRenderHandle(windowWidth, windowHeight, this,gd);
            //this.sPixelBuffer = new byte[windowSize.Width * windowSize.Height * 4];
           // Debug.Log("Constructed Offscreen Client");
        }

        //public void UpdateTexture(Texture2D pTexture)
        //{
        //    if (this.sHost != null)
        //    {
        //        lock (sPixelLock)
        //        {
        //            if (this.sHost != null)
        //            {
        //                pTexture.LoadRawTextureData(this.sPixelBuffer);
        //                pTexture.Apply(false);
        //            }
        //        }
        //    }
        //}

        public void Shutdown()
        {
            if (this.sHost != null)
            {
                //Debug.Log("Host Cleanup");
                this.sHost.CloseBrowser(true);
                this.sHost.Dispose();
                this.sHost = null;
            }
        }

        #region Interface

        protected override CefRenderHandler GetRenderHandler()
        {
            return this._renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            return this._loadHandler;
        }
    }

     
}
