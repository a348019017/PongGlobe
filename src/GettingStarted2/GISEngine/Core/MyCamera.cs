using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using PongGlobe.Core;
using SampleBase;

namespace PongGlobe.Core
{
    /// <summary>
    /// 自定义的一个相机类主要处理Geodetic3D参考系
    /// </summary>
    public class MyCamera
    {
        //将世界坐标系变换到当前摄像机所在参考系的四元组
        //取椭球体上一点，X轴切于纬线，Y轴切于经线，Z轴垂直椭球体表面向外，Camera的position.height实际上是此参考系下的Z坐标
        private Quaternion quaternion_cameraLocalFrame;
        private Camera _camera = null;
        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 100f;
        private const float _minDistance = 0.01f;
        //地球当前的椭球体，参与部分参数的计算
        private PongGlobe.Core.Ellipsoid _shape;
        //当前的摄像机定位信息
        private CameraInfo _cameraInfo;
        //前一次的鼠标点
        Vector2 _previousMousePos;

        public MyCamera(float width, float height)
        {           
            //默认为此椭球体
            _shape = Ellipsoid.ScaledWgs842;
            _camera = new Camera(width, height);                   
        }

        public Ellipsoid Shape { get { return _shape; }set { _shape = value; } }


        public Camera Camera => _camera;

        public MyCamera(float width, float height, Ellipsoid shape)
        {         
            _shape = shape;
            _camera = new Camera(width, height);
           
        }

        public CameraInfo CameraInfo { get { return _cameraInfo; } set { _cameraInfo = value; UpdateCamera(); } }
      
        /// <summary>
        /// 更新camera参数
        /// </summary>
        private void UpdateCamera()
        {                 
            //计算相机参考于椭球体参考系下的坐标体系
            var rotate = Quaternion.CreateFromYawPitchRoll((float)_cameraInfo.Postion.Longitude, (float)_cameraInfo.Postion.Latitude , 0);          
            quaternion_cameraLocalFrame = rotate;
            //此时相机在此参考系下的坐标为（0，0，height）,未考虑平移

            //再次计算Tile和heading情况下的旋转偏量           
            var rotation = Quaternion.CreateFromYawPitchRoll(0, (float)_cameraInfo.Tilt, (float)_cameraInfo.Heading);
            var totalRotation = rotate* rotation ;           
            var tt = Vector3.Transform(-Vector3.UnitZ, totalRotation);
            var realPosition = _shape.ToVector3(_cameraInfo.Postion);
            var newCameraInfo2 = new Geodetic3D(_cameraInfo.Postion.Longitude, _cameraInfo.Postion.Latitude);
            var vectT = _shape.ToVector3(newCameraInfo2);
            Vector3 aa = new Vector3(0,0,(float)_cameraInfo.Postion.Height);
            //设置相机定位的三个重要参数
            this.Camera.Position = vectT+ Vector3.Transform(aa, totalRotation);
            this.Camera.Up = Vector3.Transform(Vector3.UnitY, totalRotation);
            this.Camera.Target = this.Camera.Position+tt;
            //不重要但可能需要
            this.Camera.Right= Vector3.Transform(Vector3.UnitX, totalRotation);
        }

        public void Update(float deltaSeconds)
        {
            ////控制鼠标中键,进行缩放视图
            var delta = InputTracker.GetMouseWheelDelta();

            if (delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var de = delta * (_cameraInfo.Postion.Height) * 0.1;
                _cameraInfo.Postion.Height += de;
                //pos.Height += de;
                //更新相机位置后更新View矩阵
                UpdateCamera();
            }
            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;
            var isMouseWhellClick = InputTracker.GetMouseButton(MouseButton.Middle);
            if ((mouseDelta.Y != 0 || mouseDelta.X != 0) && isMouseWhellClick)
            {
                var deltaAngleY = (float)Math.PI * 0.002f * mouseDelta.Y;
                var deltaAngleX = (float)Math.PI * 2 * 0.001f * mouseDelta.X;
                _cameraInfo.Tilt += deltaAngleY;
                _cameraInfo.Heading += (float)deltaAngleX;
                if (_cameraInfo.Heading > Math.PI * 2) _cameraInfo.Heading = (float)Math.PI * 2;
                if (_cameraInfo.Tilt < 0) _cameraInfo.Tilt = 0;
                if (_cameraInfo.Tilt > Math.PI / 2) _cameraInfo.Tilt = (float)Math.PI / 2;

                //获取相机坐标系的Right系
                var rotation = Quaternion.CreateFromYawPitchRoll(0, -deltaAngleY, deltaAngleX);
                _camera.Position = Vector3.Transform(_camera.Position, rotation);
                //将世界坐标系下的


                //_cameraInfo.Postion= _cameraInfo.
                //UpdateCamera();
            }

            ////鼠标左右即是旋转地球，改变相机的position
            //if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
            //{
            //    ////左右旋转
            //    //if (mouseDelta.X != 0)
            //    //{
            //    //   
            //    //    Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll((float)deltaAngle, 0, 0);
            //    //    _position = Vector3.Transform(_position, lookRotation);
            //    //    UpdateViewMatrix();
            //    //}               
            //}
        }










    }



    
}
