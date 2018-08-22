using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using PongGlobe.Core;
using Veldrid;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 自定义的一个相机类控制器类来处理相机相关参数以及改变
    /// </summary>
    public class MyCameraController2 : ICameraController
    {
        /// <summary>
        /// 当前的动画效果类
        /// </summary>
        //private IDynamicOperator _curDynamicOper;
        private float _fov = 1f;
        private float _near = 0.01f;
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
            //UpdateViewMatrix();
            this.LookAtInfo = new LookAt(0, 0, 0, 0, 0, 1);
        }

        //通过Speed参数实现动画效果，如FlyTo等其它操作
        private void FlyTo(LookAt lookAt, double lastTime)
        {

        }

        /// <summary>
        /// 二维投影到三维视图
        /// </summary>
        /// <param name="source"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Matrix4x4 matrix;
            Matrix4x4.Invert(
               Matrix4x4.Multiply(
                   Matrix4x4.Multiply(world, view),
                   projection
               ), out matrix
           );
            source.X = (((source.X) / ((float)_windowWidth)) * 2f) - 1.0f;
            source.Y = ((((source.Y) / ((float)_windowHeight)) * 2f) - 1f);
            source.Z = 0;
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (
                ((source.X * matrix.M14) + (source.Y * matrix.M24)) +
                (source.Z * matrix.M34)
            ) + matrix.M44;
            //如果a比1要大的话再次向量化
            if (!(Math.Abs(a - 1.0f) < float.Epsilon))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            return vector;
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

        //计算其视椎体对象，以此作为获取数据的范围。具体影响加载的参数有，视椎体，相机距离地面的高度等，具体的参数可在ViewPort中定义并传出，所有可渲染对象的Draw操作均传入ViewPort和GraphicDevice至少这两个对象
        //private Get

        public LookAt LookAtInfo { get { return _lookAtInfo; } set { _lookAtInfo = value; UpdateCamera(); } }

        public CameraInfo CameraInfo { get { return _cameraInfo; } set { _cameraInfo = value; UpdateCameraInfo(); } }
     
        /// <summary>
        /// 更新camera参数
        /// </summary>
        private void UpdateCamera()
        {
            //首先计算LookAt点所在世界坐标系
            var centerLookAt = _shape.ToVector3(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //描述一个基于基本参考系的旋转平移之后的参考系
            var transform = Shape.GeographicToCartesianTransform2(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //对这个参考系再进行旋转
            var transformLocal = Matrix4x4.CreateFromYawPitchRoll(0, (float)_lookAtInfo.Tilt, -(float)_lookAtInfo.Heading);
            //v
            var localTransformheading = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, (float)_lookAtInfo.Heading);
            //
            var localTransformTilt = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, -(float)_lookAtInfo.Tilt);

            //再创建平移矩阵
            var transTransform = Matrix4x4.CreateTranslation(0, 0, (float)_lookAtInfo.Range);
            //var localTransformheading2=  Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)_lookAtInfo.Heading);
            //var result1 = Matrix4x4.Transform(transform, localTransformheading2);
            //var result2 = transform * localTransformheading;

            //var transformAll2 = transTransform*(localTransformTilt*(localTransformheading*(localTransformheading*transform)));
            //var tttt = transform * localTransformheading * localTransformTilt;
            //* transTransform;
            var tttt = transTransform * localTransformTilt * localTransformheading * transform;
            //求其转置矩阵,
            _positon = tttt.Translation;
            var unitY = Vector3.Transform(Vector3.UnitY, Quaternion.CreateFromRotationMatrix(tttt));
            // _viewMatrix = tttt.InvertOrthonormal();
            //_positon = _viewMatrix.ExtractEyePosition();
            // _positon = new Vector3(_positon.X,-_positon.Y,_positon.Z);
            var target = Vector3.Transform(-Vector3.UnitZ, Quaternion.CreateFromRotationMatrix(tttt));
            _viewMatrix = Matrix4x4.CreateLookAt(_positon, _positon + target, unitY);
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
            //将X，Y坐标
        }


        /// <summary>
        /// 将LookAt转换为CameraInfo的参数信息
        /// </summary>
        private void LookAtToCameraInfo()
        {

        }


        

        /// <summary>
        /// 屏幕坐标转换成椭球体经纬度坐标
        /// </summary>
        private void ScreenPostionToGeodeticPostion()
        {

        }


        private bool testwindowsCoordOnEllipse(Vector2 pos, out Geodetic2D geo)
        {
            var rayVector = Unproject(new Vector3(pos.X, pos.Y, 0), this.ProjectionMatrix, this.ViewMatrix, Matrix4x4.Identity);
            //此rayVector为近裁剪面的世界坐标，与eye相减得到ray向量，ray向量与地球求交即可得到结果
            //计算是否与地球有交
            var rayDir = rayVector - _positon;
            var result = Shape.Intersections(_positon, rayVector - _positon);
            //计算第一个相交点的世界坐标
            if (result.Length != 0)
            {
                Vector3 deta = Vector3.Normalize(rayDir) * (float)result[0];
                Vector3 position = _positon + deta;
                geo = Shape.ToGeodetic2D(position);
                return true;
            }
            geo = new Geodetic2D();
            return false;
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


            //如果前一个点和当前点均位于地球上，则计算两者的经纬度之差，
            var isMouseleftClick = InputTracker.GetMouseButton(MouseButton.Left);
            if (isMouseleftClick && testwindowsCoordOnEllipse(_previousMousePos, out Geodetic2D preCoord) && testwindowsCoordOnEllipse(InputTracker.MousePosition, out Geodetic2D curPos))
            {
                var delatLat = curPos.Latitude - preCoord.Latitude;
                var deltaLon = curPos.Longitude - preCoord.Longitude;
                //设置lookAt的偏移量       
                _lookAtInfo.Longitude -= deltaLon;
                if (_lookAtInfo.Longitude >= 2 * Math.PI) _lookAtInfo.Longitude -= 2 * Math.PI;
                if (_lookAtInfo.Longitude <= 0) _lookAtInfo.Longitude += 2 * Math.PI;

                _lookAtInfo.Latitude -= delatLat;
                if (_lookAtInfo.Latitude >= Math.PI / 2) _lookAtInfo.Latitude = Math.PI / 2;
                if (_lookAtInfo.Latitude <= -Math.PI / 2) _lookAtInfo.Latitude = -Math.PI / 2;
                //重新计算相关参数
                UpdateCamera();
            }

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
            _previousMousePos = InputTracker.MousePosition;


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
