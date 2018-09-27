using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Primitive
{
    /// <summary>
    /// 渲染cyliner
    /// </summary>
    public class CylinderRender : IRender
    {
        private Scene.Scene _scene;
        private Cylinder _cylinder;


        public CylinderRender(Scene.Scene scene,Cylinder cylinder)
        {
            _scene = scene;
        }

        public void CreateDeviceResources(GraphicsDevice gd, ResourceFactory factory)
        {
            
        }

        public void Dispose()
        {
           
        }

        public void Draw(CommandList _cl)
        {
           
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
