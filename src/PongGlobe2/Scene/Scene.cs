using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 场景类，EarthControl.Scene 描述地球场景等信息，类似于Map，Earth等对象
    /// </summary>
    public class Scene
    {
        private float _width = 0;
        private float _height = 0;
        /// <summary>
        /// 当前场景中的椭球体参数
        /// </summary>
        private Ellipsoid _ellipsoid = Ellipsoid.ScaledWgs842;
        ///当前场景中的camera对象
        private ICameraController _camera;

        public Scene(float width,float height)
        {
            _width = width;
            _height = height;
            _camera = new MyCameraController2(width, height);
        }
       
        public ICameraController Camera { get { return _camera; } }

        /// <summary>
        /// 返回当前地球的椭球体参数
        /// </summary>
        public Ellipsoid Ellipsoid { get { return _ellipsoid; } }


       

    }
}
