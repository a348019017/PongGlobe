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
        private float _near = 0.001f;
        private float _far = 3.0f;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        /// <summary>
        /// 视口的变换矩阵，将二维矩阵变换为屏幕的坐标体系，windows的坐标体系是左上角为0，0
        /// </summary>
        private Matrix4x4 _viewportMaxtrix;


        private Vector3 _positon;

        private float _windowWidth;
        private float _windowHeight;

        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 10f;
        private const float _minDistance = 0.001f;
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
            //更新视口矩阵
            UpdateViewPortTransformMaxtrix();
            //UpdateViewMatrix();
            this.LookAtInfo = new LookAt(0, 0, 0, 0, 0, 1);
            UpdateCamera();            
        }

        //通过Speed参数实现动画效果，如FlyTo等其它操作
        private void FlyTo(LookAt lookAt, double lastTime)
        {

        }

        /// <summary>
        /// 将屏幕坐标转换成世界坐标,这种转换是非线性的。
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
              world*view*projection , out matrix
           );           
            //计算view
            source.X = (((source.X) / ((float)_windowWidth)) * 2f) - 1.0f;
            source.Y = -((((source.Y) / ((float)_windowHeight)) * 2f) - 1.0f);
            //这里仍需要考虑传入二维坐标的深度，以计算其在场景中的准确位置，此处也可以判断View坐标范围是0-1，既然非线性
            source.Z = (source.Z - this._near)  / (this._far - this._near);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (
                ((source.X * matrix.M14) + (source.Y * matrix.M24)) +
                (source.Z * matrix.M34)
            ) + matrix.M44;
            //如果a比1要大的话再次向量化，相当于齐次化坐标
            if (!(Math.Abs(a - 1.0f) < float.Epsilon))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            return vector;
        }

        /// <summary>
        /// vulkan默认深度范围是0-1，进地面是0，远裁剪面是1，刚好与NDC Space的 Z/W相对应。由于depth并非线性，所有还是需要RayCasting与平面求交计算真实的深度值，或直接通过当前像素的深度值反向计算
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to project.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public Vector3 Project(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Matrix4x4 matrix = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            //此处的计算可以判断Z非线性，且范围为0-1，这里Z的计算暂无意义
            vector.X = (((vector.X + 1f) * 0.5f) * this._windowWidth) + 0;
            vector.Y = (((-vector.Y + 1f) * 0.5f) * this._windowHeight) + 0;
            vector.Z = (vector.Z * (this._far - this._near)) + this._near;
            return vector;
        }
        private static bool WithinEpsilon(float a, float b)
        {
            float num = a - b;
            return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
        }


        /// <summary>
        /// 返回视口的大小
        /// </summary>
        public Vector2 ViewPort => new Vector2(_windowWidth, _windowHeight);
        

       
        /// <summary>
        /// 测试视口矩阵
        /// </summary>
        public void TestViewportMaxtrix()
        {
            //因此屏幕坐标的Z值可以定义为距离视点的距离
            var source = new Vector3(380f, 140f,0.5f);
            //使用矩阵计算
            bool inverted = Matrix4x4.Invert(_viewportMaxtrix, out Matrix4x4 invert);

            //projection转换后的坐标范围为-1到1，包括Z也是-1到1，定义-1位近裁剪面，1为远裁剪面。
            var result = Vector3.Transform(source,invert);


            source.X = (((source.X) / ((float)_windowWidth)) * 2f) - 1.0f;
            source.Y = -((((source.Y) / ((float)_windowHeight)) * 2f) - 1.0f);
            //这里仍需要考虑传入二维坐标的深度，此Z非线性
            source.Z = source.Z;

        }


        /// <summary>
        /// 将世界坐标转换成屏幕坐标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public Vector3 Project(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world,Matrix4x4 viewport)
        {
            //其结果可能没有归一化
            return Vector3.Transform(source, world * view * projection*viewport);
        }

        public Vector3 Project(Vector3 source)
        {
            Matrix4x4 matrix = Matrix4x4.Multiply(Matrix4x4.Multiply(Matrix4x4.Identity, this.ViewMatrix), this.ProjectionMatrix);
            Vector3 vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X = vector.X / a;
                vector.Y = vector.Y / a;
                vector.Z = vector.Z / a;
            }
            //此处的计算可以判断Z非线性，且范围为0-1，这里Z的计算暂无意义
            vector.X = (((vector.X + 1f) * 0.5f) * this._windowWidth) + 0;
            vector.Y = (((-vector.Y + 1f) * 0.5f) * this._windowHeight) + 0;
            vector.Z = (vector.Z * (this._far - this._near)) + this._near;
            return vector;
        }





        public Ellipsoid Shape { get { return _shape; } set { _shape = value; } }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Matrix4x4 ViewportMaxtrix => _viewportMaxtrix;


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
        public void UpdateCamera()
        {
            //首先计算LookAt点所在世界坐标系
            var centerLookAt = _shape.ToVector3(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //描述一个基于基本参考系的旋转平移之后的参考系
            var transform = Shape.GeographicToCartesianTransform2(new Geodetic3D(_lookAtInfo.Longitude, _lookAtInfo.Latitude, _lookAtInfo.Altitude));
            //对这个参考系再进行旋转
            //var transformLocal = Matrix4x4.CreateFromYawPitchRoll(0,(float)_lookAtInfo.Tilt, -(float)_lookAtInfo.Heading);
            //v
            var localTransformheading = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, (float)_lookAtInfo.Heading);
            //
            var localTransformTilt = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (float)_lookAtInfo.Tilt);

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


        public float WindowsHeight => _windowHeight;
        public float WindowsWidth => _windowWidth;



        /// <summary>
        /// 屏幕坐标转换成椭球体经纬度坐标
        /// </summary>
        private void ScreenPostionToGeodeticPostion()
        {

        }


        public bool testwindowsCoordOnEllipse(Vector2 pos, out Geodetic2D geo)
        {
            var rayVector = Unproject(new Vector3(pos.X, this._windowHeight-pos.Y, 0), this.ProjectionMatrix, this.ViewMatrix, Matrix4x4.Identity);
            //此rayVector为近裁剪面的世界坐标，与eye相减得到ray向量，ray向量与地球求交即可得到结果
            //计算是否与地球有交
            var rayDir = rayVector - _positon;
            var result = Shape.Intersections(_positon, rayDir);
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

                _lookAtInfo.Latitude += delatLat;
                if (_lookAtInfo.Latitude >= Math.PI / 2) _lookAtInfo.Latitude = Math.PI / 2;
                if (_lookAtInfo.Latitude <= -Math.PI / 2) _lookAtInfo.Latitude = -Math.PI / 2;
                //重新计算相关参数
                UpdateCamera();
            }

            var isMouseWhellClick = InputTracker.GetMouseButton(MouseButton.Middle);
            if ((mouseDelta.Y != 0 || mouseDelta.X != 0) && isMouseWhellClick)
            {
                var deltaAngleY = (float)Math.PI * 0.002f * mouseDelta.Y;
                var deltaAngleX = (float)Math.PI * 2 * 0.0005f * mouseDelta.X;
                _lookAtInfo.Tilt += -deltaAngleY;
                _lookAtInfo.Heading += -(float)deltaAngleX;
                if (_lookAtInfo.Heading > Math.PI * 2) _lookAtInfo.Heading = (float)Math.PI * 2;
                if (_lookAtInfo.Tilt < 0) _lookAtInfo.Tilt = 0;
                if (_lookAtInfo.Tilt > Math.PI / 2) _lookAtInfo.Tilt = (float)Math.PI / 2;
                UpdateCamera();
            }
            _previousMousePos = InputTracker.MousePosition;
            UpdateCamera();

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

        //计算视口矩阵
        private void UpdateViewPortTransformMaxtrix()
        {
            float halfWidth = this._windowWidth * 0.5f;
            float halfHeight = this._windowHeight * 0.5f;
            float halfDepth = (_far - _near) * 0.5f;
            //
            // Bottom and top swapped:  MS -> OpenGL
            //
            //vulkan定义的深度是0-1，perspctive matrix ,深度是非线性关系的，透视矩阵是线性的
            //这里默认为左上角为00，且视口的左上角也是00，当需要多视口时，便需要加上视口实际的内容
            var maxtrix =new Matrix4x4(
                halfWidth, 0.0f, 0.0f, 0 + halfWidth,
                0.0f, -halfHeight, 0.0f, 0 + halfHeight,
                0.0f, 0.0f, 1.0f, 0,
                0.0f, 0.0f, 0.0f, 1.0f);
            _viewportMaxtrix = Matrix4x4.Transpose(maxtrix);
            //这里需要作一个转置，因为numberic是行许，而opengl为列序，一个左乘一个右乘
        }


        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;
        public Vector3 Position => _positon;
    }
}
