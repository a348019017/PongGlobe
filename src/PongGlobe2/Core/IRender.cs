using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace PongGlobe.Core
{
    /// <summary>
    /// 通用接口，对Device的资源创建CreateResource，Draw，Update等操作
    /// </summary>
    public interface IRender:IDisposable
    {
        void Draw();
        void Update();
        void CreateDeviceResources(GraphicsDevice gd,
            ResourceFactory factory);
    }
}
