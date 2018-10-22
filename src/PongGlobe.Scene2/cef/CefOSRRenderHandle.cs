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
    internal class CefOSRRenderHandle : CefRenderHandler
    {
        private CefOSRClient client;
        private Texture _cefTexture;
        private GraphicsDevice _gd;
        private readonly int _windowWidth;
        private readonly int _windowHeight;

        public Texture Texture { get { return _cefTexture; } }

        /// <summary>
        /// 使用GraphicDevice构造
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        /// <param name="client"></param>
        public CefOSRRenderHandle(int windowWidth, int windowHeight, CefOSRClient client,GraphicsDevice gd)
        {
            this._windowWidth = windowWidth;
            this._windowHeight = windowHeight;
            this.client = client;
            this._gd = gd;
            //同时创建一个可更新的Texture
            if (this._gd != null)
            {
                // factory.CreateTexture(TextureDescription.Texture2D(
                //Width, Height, MipLevels, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                _cefTexture = _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)windowWidth,(uint)windowHeight,0,1,PixelFormat.B8_G8_R8_A8_UNorm,TextureUsage.Sampled));
            }
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
            rect.X = 0;
            rect.Y = 0;
            rect.Width = this._windowWidth;
            rect.Height = this._windowHeight;
            return true;
        }

        //[SecurityCritical]
        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (browser != null&&_gd!=null)
            {
                //直接更新全部纹理               
                _gd.UpdateTexture(_cefTexture, buffer, (uint)(4 * _windowWidth * _windowHeight), 0, 0, 0, (uint)_windowWidth, (uint)_windowHeight, 1, 0, 0);            
            }
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
