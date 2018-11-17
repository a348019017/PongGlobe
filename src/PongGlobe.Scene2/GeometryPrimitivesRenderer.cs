using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using PongGlobe.Graphics.GeometricPrimitive;
using Veldrid;
using System.Collections.Concurrent;

namespace PongGlobe.Scene
{
    /// <summary>
    /// GeometryPrimitive渲染器
    /// </summary>
    class GeometryPrimitivesRenderer : IRender<GeometricPrimitive>
    {
        //线程安全的集合，可能阻塞渲染进程
        private ConcurrentBag<GeometricPrimitive> _geo=new ConcurrentBag<GeometricPrimitive>();

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

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
