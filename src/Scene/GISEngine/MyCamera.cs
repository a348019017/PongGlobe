using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using PongGlobe.Core;
using SampleBase;
using PongGlobe.Core.Extension;
namespace PongGlobe.Core
{
    /// <summary>
    /// 自定义的一个相机类控制器类来处理相机相关参数以及改变
    /// </summary>
    public class MyCameraController:ICameraController
    {

        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 10f;


        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        private Vector3 _positon;

        private float _windowWidth;
        private float _windowHeight;

        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 100f;
        private const float _minDistance = 0.01f;
        //地球当前的椭球体，参与部分参数的计算
        private PongGlobe.Core.Ellipsoid _shape;
        //当前的摄像机定位信息
        private CameraInfo _cameraInfo;
        //前一次的鼠标点
        Vector2 _previousMousePos;

        public MyCameraController(float width, float height)
        {           
            //默认为此椭球体
            _shape = Ellipsoid.ScaledWgs842;
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
           // UpdateViewMatrix();
        }

        public Ellipsoid Shape { get { return _shape; }set { _shape = value; } }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public MyCameraController(float width, float height, Ellipsoid shape)
        {         
            _shape = shape;
            //_camera = new Camera(width, height); 
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
            //UpdateViewMatrix();
        }

        public CameraInfo CameraInfo { get { return _cameraInfo; } set { _cameraInfo = value; UpdateCamera(); } }
      
        /// <summary>
        /// 更新camera参数
        /// </summary>
        private void UpdateCamera()
        {
            //首先计算LookAt点，也即是相机参考系的原点
            //计算
            var centerEarth= _shape.ToVector3(new Geodetic2D(_cameraInfo.Longitude, _cameraInfo.Latitude));
            var transform = Shape.geographicToCartesianTransform(new Geodetic3D(_cameraInfo.Longitude, _cameraInfo.Latitude, _cameraInfo.Altitude));
            //再次旋转Tile和heading，roll这里先不管
            var transformLocal= Matrix4x4.CreateFromYawPitchRoll(0, -(float)_cameraInfo.Tilt, -(float)_cameraInfo.Heading);
            //这里的transform是经过两次变换，第一次是经纬度的变化，第二次是rollpitch的变化，此时的矩阵还并不是视图矩阵，因为Camera的Z与世界坐标系是相反的，因此我们
            //世界参考系是右手参考系，相机参考系为左手参考系，此transform中的平移量即为相机坐标，相机的三个坐标系可据此进行变换，注意，只需要使用变换矩阵的旋转变量参数（一个四元组）即可
            var transoformAll = transform * transformLocal;
            
            var scale = Matrix4x4.Decompose(transoformAll, out Vector3 scale1, out Quaternion rotation1, out Vector3 translation1);
            var cameraPosition = transoformAll.Translation;
            _positon = transoformAll.Translation;
            var cameraUp = Vector3.Transform(Vector3.UnitY, Quaternion.CreateFromRotationMatrix(transoformAll));
            var cameraZ = Vector3.Transform(-Vector3.UnitZ, Quaternion.CreateFromRotationMatrix(transoformAll));
            var target=  cameraZ;
            
            //相机一直朝向当前相机的中心点，且此夹角不能超过90度
            _viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, centerEarth, cameraUp);
           // _viewMatrix = transformAll;                     
        }

        public void Update(float deltaSeconds)
        {
            ////控制鼠标中键,进行缩放视图
            var delta = InputTracker.GetMouseWheelDelta();

            if (delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var de = delta * (_cameraInfo.Altitude) * 0.1;
                _cameraInfo.Altitude += de;
                //pos.Height += de;
                //更新相机位置后更新View矩阵
                UpdateCamera();
            }
            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;
            var isMouseWhellClick = InputTracker.GetMouseButton(MouseButton.Middle);
            if ((mouseDelta.Y != 0 || mouseDelta.X != 0) && isMouseWhellClick)
            {
                var deltaAngleY = -(float)Math.PI * 0.002f * mouseDelta.Y;
                var deltaAngleX = (float)Math.PI * 2 * 0.001f * mouseDelta.X;
                _cameraInfo.Tilt += deltaAngleY;
                _cameraInfo.Heading += (float)deltaAngleX;
                if (_cameraInfo.Heading > Math.PI * 2) _cameraInfo.Heading = (float)Math.PI * 2;
                if (_cameraInfo.Tilt < 0) _cameraInfo.Tilt = 0;
                if (_cameraInfo.Tilt > Math.PI / 2) _cameraInfo.Tilt = (float)Math.PI / 2;
                UpdateCamera();               
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


        //窗口尺寸的变化
        public void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
        }

        private void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
            //ProjectionChanged?.Invoke(_projectionMatrix);
        }

        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;

        public Vector3 Position => _positon;
    }


    /// <summary>
    /// 自定义的一个相机类控制器类来处理相机相关参数以及改变
    /// </summary>
    public class MyCameraController2 : ICameraController
    {

        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 10f;


        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        private Vector3 _positon;

        private float _windowWidth;
        private float _windowHeight;

        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 100f;
        private const float _minDistance = 0.01f;
        //地球当前的椭球体，参与部分参数的计算
        private PongGlobe.Core.Ellipsoid _shape;
        //当前的摄像机定位信息
        private LookAt _lookAtInfo;
        private CameraInfo _cameraInfo;
        //前一次的鼠标点
        Vector2 _previousMousePos;

        public MyCameraController2(float width, float height)
        {
            //默认为此椭球体
            _shape = Ellipsoid.ScaledWgs842;
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
            // UpdateViewMatrix();
        }

        public Ellipsoid Shape { get { return _shape; } set { _shape = value; } }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public MyCameraController2(float width, float height, Ellipsoid shape)
        {
            _shape = shape;
            //_camera = new Camera(width, height); 
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
            //UpdateViewMatrix();
        }

        public LookAt LookAtInfo { get { return _lookAtInfo; } set { _lookAtInfo = value; UpdateCamera(); } }

        public CameraInfo CameraInfo { get { return _cameraInfo; }set { _cameraInfo = value;UpdateCameraInfo(); } }

        //public CameraInfo {get}

        /// <summary>
        /// 更新camera参数
        /// </summary>
        private void UpdateCamera()
        {
            //首先计算LookAt点所在世界坐标系
             var centerLookAt= _shape.ToVector3(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude,_lookAtInfo.Altitude));
            //描述一个基于基本参考系的旋转平移之后的参考系
            var transform = Shape.GeographicToCartesianTransform2(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //对这个参考系再进行旋转
            var transformLocal = Matrix4x4.CreateFromYawPitchRoll(0, (float)_lookAtInfo.Tilt, -(float)_lookAtInfo.Heading);
            //v
            var localTransformheading = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ,(float)_lookAtInfo.Heading);
            //
            var localTransformTilt = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX,-(float)_lookAtInfo.Tilt);

            //再创建平移矩阵
            var transTransform = Matrix4x4.CreateTranslation(0,0,(float)_lookAtInfo.Range);
            //var localTransformheading2=  Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)_lookAtInfo.Heading);
            //var result1 = Matrix4x4.Transform(transform, localTransformheading2);
            //var result2 = transform * localTransformheading;

            var transformAll2 = transTransform*(localTransformTilt*(localTransformheading*(localTransformheading*transform)));
            var tttt = transform * localTransformheading * localTransformTilt * transTransform;

            //求其转置矩阵,
            //_positon = tttt.Translation;
            //var unitY = Vector3.Transform(Vector3.UnitY, transformAll);
           Matrix4x4.Invert(tttt,out _viewMatrix);
            _positon = _viewMatrix.ExtractEyePosition();
            //_viewMatrix = Matrix4x4.CreateLookAt(_positon, Vector3.Zero, Vector3.UnitY);
        }


        /// <summary>
        /// 
        /// </summary>
        private void UpdateCameraInfo()
        {
            //描述一个基于基本参考系的旋转平移之后的参考系
            var transform = Shape.GeographicToCartesianTransform2(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //对这个参考系再进行旋转
            var transformLocal = Matrix4x4.CreateFromYawPitchRoll(0, -(float)_lookAtInfo.Tilt, -(float)_lookAtInfo.Heading);
            //v
            var localTransformheading = Matrix4x4.CreateRotationZ((float)_lookAtInfo.Heading);
            //
            var localTransformTilt = Matrix4x4.CreateRotationX(-(float)_lookAtInfo.Tilt);
            //再创建平移矩阵
           //将X，Y坐标反制

        }

        /// <summary>
        /// 将Camera参数直接转换成view矩阵
        /// </summary>
        /// <param name="globe"></param>
        /// <param name="camera"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected Matrix4x4 cameraToViewingMatrix(PongGlobe.Core.Ellipsoid shape, Camera camera, Matrix4x4 result)
        {
            // TODO interpret altitude mode other than absolute
            // Transform by the local cartesian transform at the camera's position.
            //globe.geographicToCartesianTransform(camera.latitude, camera.longitude, camera.altitude, result);

            //// Transform by the heading, tilt and roll.
            //result.multiplyByRotation(0, 0, 1, -camera.heading); // rotate clockwise about the Z axis
            //result.multiplyByRotation(1, 0, 0, camera.tilt); // rotate counter-clockwise about the X axis
            //result.multiplyByRotation(0, 0, 1, camera.roll); // rotate counter-clockwise about the Z axis (again)

            //// Make the transform a viewing matrix.
            //result.invertOrthonormal();

            //return result;
            return default(Matrix4x4);
        }


        public void Update(float deltaSeconds)
        {
            ////控制鼠标中键,进行缩放视图
            var delta = InputTracker.GetMouseWheelDelta();

            if (delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var de = delta * (_lookAtInfo.Range) * 0.1;
                _lookAtInfo.Range += de;
                //pos.Height += de;
                //更新相机位置后更新View矩阵
                UpdateCamera();
            }
            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;
            var isMouseWhellClick = InputTracker.GetMouseButton(MouseButton.Middle);
            if ((mouseDelta.Y != 0 || mouseDelta.X != 0) && isMouseWhellClick)
            {
                var deltaAngleY = -(float)Math.PI * 0.002f * mouseDelta.Y;
                var deltaAngleX = (float)Math.PI * 2 * 0.0005f * mouseDelta.X;
                _lookAtInfo.Tilt += deltaAngleY;
                _lookAtInfo.Heading += (float)deltaAngleX;
                if (_lookAtInfo.Heading > Math.PI * 2) _lookAtInfo.Heading = (float)Math.PI * 2;
                if (_lookAtInfo.Tilt < 0) _lookAtInfo.Tilt = 0;
                if (_lookAtInfo.Tilt > Math.PI / 2) _lookAtInfo.Tilt = (float)Math.PI / 2;
                UpdateCamera();
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


        //窗口尺寸的变化
        public void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
        }

        private void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
            //ProjectionChanged?.Invoke(_projectionMatrix);
        }

        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;

        public Vector3 Position => _positon;
    }

}
