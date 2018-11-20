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
        void Draw(CommandList _cl);
        void UpdateQueueOp();
        void Update();
        void CreateDeviceResources(GraphicsDevice gd,
            ResourceFactory factory);
    }


    /// <summary>
    /// 泛型渲染类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRender<T> : IRender
    {

    }


    

}
