using System;
using System.Collections.Generic;
using System.Text;
using Xilium.CefGlue;
using Veldrid;
namespace PongGlobe.Scene.cef
{
    /// <summary>
    /// 
    /// </summary>
    public class CefOSRRenderHandle : CefRenderHandler
    {
        private MainUIRender _renderOwner = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        /// <param name="client"></param>
        public CefOSRRenderHandle(MainUIRender client)
        {           
            this._renderOwner = client;         
        }


        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            return GetViewRect(browser, ref rect);
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            //rect.X = 0;
            //rect.Y = 0;
            //rect.Width = client.WindowsWidth;
            //rect.Height = client.WindowsHeight;
            this._renderOwner.HandleGetViewRect(browser,ref rect);
            return true;
        }

        //[SecurityCritical]
        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            this._renderOwner.HandleOnPaint(browser, type, dirtyRects, buffer, width, height);
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }

        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
        {
        }

        protected override CefAccessibilityHandler GetAccessibilityHandler()
        {
            return null;
           //return  base.GetAccessibilityHandler();
            //throw new NotImplementedException();
        }
    }
}
