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
    public class CefOSRClient:CefClient
    {
        private readonly CefLoadHandler _loadHandler;
        private readonly CefOSRRenderHandle _renderHandler;
        private MainUIRender _renderOwner;

        private static readonly object sPixelLock = new object();     

        public CefOSRClient(MainUIRender owner)
        {
            _renderOwner = owner;
            this._loadHandler = new CefOSRLoadHandler (this._renderOwner);
            this._renderHandler = new CefOSRRenderHandle(this._renderOwner);          
        }

       

        public void Shutdown()
        {
             
        }

        

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
