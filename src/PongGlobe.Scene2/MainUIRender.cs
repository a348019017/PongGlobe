using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 渲染和更新主界面UI
    /// </summary>
    public class MainUIRender : IRender
    {
        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
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
