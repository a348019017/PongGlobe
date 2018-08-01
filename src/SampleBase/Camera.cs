using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace SampleBase
{
    public class Camera
    {
        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 1000f;
        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 100f;
        private const float _minDistance = 0.01f;
        //private 

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        //相机的初始位置位于世界坐标系的Z轴
        private Vector3 _position = new Vector3(0, 0, 3.0f);
        //初始方向为Z轴的反方向
        private Vector3 _lookDirection = new Vector3(0, 0f, -1.0f);

        private float _moveSpeed = 10.0f;

        private float _yaw;
        private float _pitch;

        private Vector2 _previousMousePos;
        private float _windowWidth;
        private float _windowHeight;

        public event Action<Matrix4x4> ProjectionChanged;
        public event Action<Matrix4x4> ViewChanged;

        public Camera(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public Vector3 Forward => GetLookDir();

        public void Update(float deltaSeconds)
        {
            float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
                ? 0.1f
                : InputTracker.GetKey(Key.ShiftLeft)
                    ? 2.5f
                    : 1f;
            Vector3 motionDir = Vector3.Zero;
            if (InputTracker.GetKey(Key.A))
            {
                motionDir += -Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.D))
            {
                motionDir += Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.W))
            {
                motionDir += -Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.S))
            {
                motionDir += Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.Q))
            {
                motionDir += -Vector3.UnitY;
            }
            if (InputTracker.GetKey(Key.E))
            {
                motionDir += Vector3.UnitY;
            }
            //控制鼠标中键
            var delta = InputTracker.GetMouseWheelDelta();
            if (delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var de = (new Vector3((float)delta,(float)delta,(float)delta))*(_position - new Vector3(0, 0, 0));
                //计算单位向量
                var unit = Vector3.Normalize(_position) *1.0001f ;
                //计算距离地球表面的向量
                var sufaceV = _position - unit;
                _position = _position - Vector3.Multiply(sufaceV, (float)(0.1*delta));
                UpdateViewMatrix();
            }

            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
                motionDir = Vector3.Transform(motionDir, lookRotation);
                _position += motionDir * MoveSpeed * sprintFactor * deltaSeconds;
                UpdateViewMatrix();
            }

            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;

            if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
            {
                //Yaw锁定在1.57左右实际上在移动相机位置
                //Yaw += -mouseDelta.X * 0.01f;
                //Yaw = 1.57f;
                //相机向量绕z轴转动
                if (mouseDelta.X != 0)
                {
                    var deltaAngle = Math.PI * 2 * 0.01f * mouseDelta.X;
                    Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll((float)deltaAngle, 0, 0);
                    _position= Vector3.Transform(_position, lookRotation);
                    Yaw += (float)deltaAngle;
                }
               

                Pitch += -mouseDelta.Y * 0.001f;
                //计算position到屏幕中心点的最大角度，即为其最大的俯仰角
                var tang =Math.Atan(1f / _position.Length());
                Pitch = Clamp(Pitch, (float)-tang, 0);

                UpdateViewMatrix();
            }
        }

        private float Clamp(float value, float min, float max)
        {
            return value > max
                ? max
                : value < min
                    ? min
                    : value;
        }

        public void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
        }

        private void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
            ProjectionChanged?.Invoke(_projectionMatrix);
        }

        private void UpdateViewMatrix()
        {
            Vector3 lookDir = GetLookDir();
            _lookDirection = lookDir;
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
            ViewChanged?.Invoke(_viewMatrix);
        }

        private Vector3 GetLookDir()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            return lookDir;
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = _lookDirection
        };
    }



    /// <summary>
    /// 参考超图设计一个相机对象，通过相机的经纬度和Tile以及Heading来对应
    /// </summary>
    public class MyCamera
    {
        private float _fov = 1f;
        private float _near = 0.1f;
        private float _far = 100f;
        //相机距离地球表面的最大最小距离
        private const float _maxDistance = 100f;
        private const float _minDistance = 0.01f;      

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        //相机默认的位置是0，3，0 
        private Vector3 _position = new Vector3(0, 0, 3.0f);
        //初始方向为Z轴的反方向
        private Vector3 _lookDirection = new Vector3(0, 0f, -1.0f);

        private float _moveSpeed = 10.0f;

        //Heading角度对应Y轴，以相机Z轴进行旋转范围为0-360度，以弧度制计算
        private float _heading = 0;
        //相机的俯仰角0，90度，即按相机X轴逆时针旋转
        private float _tilt = 0;
        private Vector3 _lookAt = new Vector3();
        private Vector2 _previousMousePos;
        private float _windowWidth;
        private float _windowHeight;

        public event Action<Matrix4x4> ProjectionChanged;
        public event Action<Matrix4x4> ViewChanged;

        public MyCamera(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

        //记录此时的视点即地球上的点
        public Vector3 LookAt { }


        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;

        public float Heading { get => _heading; set { _heading = value; UpdateViewMatrix(); } }
        public float Tilt { get => _tilt; set { _tilt = value; UpdateViewMatrix(); } }

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

        bool isMouseWhellClick = false;

        public void Update(float deltaSeconds)
        {
            //float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
            //    ? 0.1f
            //    : InputTracker.GetKey(Key.ShiftLeft)
            //        ? 2.5f
            //        : 1f;          
            ////控制鼠标中键,进行缩放视图
            var delta = InputTracker.GetMouseWheelDelta();
            

            if (delta != 0)
            {
                //camera的position移动0.1个坐标单位
                var de = (new Vector3((float)delta, (float)delta, (float)delta)) * (_position - new Vector3(0, 0, 0));
                //计算单位向量
                var unit = Vector3.Normalize(_position) * 1.0001f;
                //计算距离地球表面的向量
                var sufaceV = _position - unit;
                _position = _position - Vector3.Multiply(sufaceV, (float)(0.1 * delta));
                UpdateViewMatrix();
            }
            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;

            if ((mouseDelta.Y != 0||mouseDelta.X!=0) && isMouseWhellClick)
            {
                var deltaAngleY = (float)Math.PI * 0.002f * mouseDelta.Y;
                var deltaAngleX = Math.PI * 2 * 0.001f * mouseDelta.X;
               // _tilt += deltaAngleY;
                _heading +=(float) deltaAngleX;
                if (_heading > Math.PI * 2) _heading =(float) Math.PI * 2;
                //if (_tilt < 0) _tilt = 0;
                //if (_tilt > Math.PI / 2) _tilt = (float)Math.PI / 2;

                UpdateViewMatrix();
            }
            isMouseWhellClick = InputTracker.GetMouseButton(MouseButton.Middle);



            //鼠标左右即是旋转地球，改变相机的position
            if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
            {              
                ////左右旋转
                //if (mouseDelta.X != 0)
                //{
                //   
                //    Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll((float)deltaAngle, 0, 0);
                //    _position = Vector3.Transform(_position, lookRotation);
                //    UpdateViewMatrix();
                //}               
            }
        }

        private float Clamp(float value, float min, float max)
        {
            return value > max
                ? max
                : value < min
                    ? min
                    : value;
        }

        public void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
        }

        private void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
            ProjectionChanged?.Invoke(_projectionMatrix);
        }

        private void UpdateViewMatrix()
        {
            var lookRotation = Matrix4x4.CreateFromYawPitchRoll(0,-_tilt,_heading);                           
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);     
            Vector3 upVector= Vector3.Transform(Vector3.UnitY, lookRotation);
            //Vector3 positionNew = Vector3.Transform(_position, lookRotation);
            //根据tile和Heading计算            
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + lookDir, upVector);
            ViewChanged?.Invoke(_viewMatrix);
        }

        

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = _lookDirection
        };
    }

    /// <summary>
    /// 固定点的相机
    /// </summary>
    public class CameraLookAt
    {

    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        private float _padding1;
        public Vector3 CameraLookDirection;
        private float _padding2;
    }
}
