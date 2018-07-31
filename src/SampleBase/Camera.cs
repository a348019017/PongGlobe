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

        private Vector3 _position = new Vector3(0, 3, 0);
        private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
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



    ///// <summary>
    ///// 参考超图设计一个相机对象，通过相机的经纬度和Tile以及Heading来对应
    ///// </summary>
    //public class MyCamera
    //{
    //    private float _fov = 1f;
    //    private float _near = 0.1f;
    //    private float _far = 100f;
    //    //相机距离地球表面的最大最小距离
    //    private const float _maxDistance = 100f;
    //    private const float _minDistance = 0.01f;
    //    //private 

    //    private Matrix4x4 _viewMatrix;
    //    private Matrix4x4 _projectionMatrix;

    //    //相机默认的位置是0，3，0 
    //    private Vector3 _position = new Vector3(0, 3, 0);
    //    //private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
    //    private float _moveSpeed = 10.0f;

    //    //Heading角度对应Y轴，以相机Z轴进行旋转范围为0-360度，以弧度制计算
    //    private float _heading = 0;
    //    //相机的俯仰角0，90度，即按相机X轴逆时针旋转

    //    private float _tilt = 0;

    //    private Vector2 _previousMousePos;
    //    private float _windowWidth;
    //    private float _windowHeight;

    //    public event Action<Matrix4x4> ProjectionChanged;
    //    public event Action<Matrix4x4> ViewChanged;

    //    public MyCamera(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //        UpdateViewMatrix();
    //    }

    //    public Matrix4x4 ViewMatrix => _viewMatrix;
    //    public Matrix4x4 ProjectionMatrix => _projectionMatrix;

    //    public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }

    //    public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
    //    public float FieldOfView => _fov;
    //    public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

    //    public float AspectRatio => _windowWidth / _windowHeight;

    //    public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
    //    public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

    //    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    //    public Vector3 Forward => GetLookDir();

    //    public void Update(float deltaSeconds)
    //    {
    //        float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
    //            ? 0.1f
    //            : InputTracker.GetKey(Key.ShiftLeft)
    //                ? 2.5f
    //                : 1f;
    //        Vector3 motionDir = Vector3.Zero;
    //        if (InputTracker.GetKey(Key.A))
    //        {
    //            motionDir += -Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.D))
    //        {
    //            motionDir += Vector3.UnitX;
    //        }
    //        if (InputTracker.GetKey(Key.W))
    //        {
    //            motionDir += -Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.S))
    //        {
    //            motionDir += Vector3.UnitZ;
    //        }
    //        if (InputTracker.GetKey(Key.Q))
    //        {
    //            motionDir += -Vector3.UnitY;
    //        }
    //        if (InputTracker.GetKey(Key.E))
    //        {
    //            motionDir += Vector3.UnitY;
    //        }
    //        //控制鼠标中键
    //        var delta = InputTracker.GetMouseWheelDelta();
    //        if (delta != 0)
    //        {
    //            //camera的position移动0.1个坐标单位
    //            var de = (new Vector3((float)delta, (float)delta, (float)delta)) * (_position - new Vector3(0, 0, 0));
    //            //计算单位向量
    //            var unit = Vector3.Normalize(_position) * 1.0001f;
    //            //计算距离地球表面的向量
    //            var sufaceV = _position - unit;
    //            _position = _position - Vector3.Multiply(sufaceV, (float)(0.1 * delta));
    //            UpdateViewMatrix();
    //        }

    //        if (motionDir != Vector3.Zero)
    //        {
    //            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //            motionDir = Vector3.Transform(motionDir, lookRotation);
    //            _position += motionDir * MoveSpeed * sprintFactor * deltaSeconds;
    //            UpdateViewMatrix();
    //        }

    //        Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
    //        _previousMousePos = InputTracker.MousePosition;

    //        if (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right))
    //        {
    //            //Yaw锁定在1.57左右实际上在移动相机位置
    //            //Yaw += -mouseDelta.X * 0.01f;
    //            //Yaw = 1.57f;
    //            //相机向量绕z轴转动
    //            if (mouseDelta.X != 0)
    //            {
    //                var deltaAngle = Math.PI * 2 * 0.01f * mouseDelta.X;
    //                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll((float)deltaAngle, 0, 0);
    //                _position = Vector3.Transform(_position, lookRotation);
    //                Yaw += (float)deltaAngle;
    //            }


    //            Pitch += -mouseDelta.Y * 0.001f;
    //            //计算position到屏幕中心点的最大角度，即为其最大的俯仰角
    //            var tang = Math.Atan(1f / _position.Length());
    //            Pitch = Clamp(Pitch, (float)-tang, 0);

    //            UpdateViewMatrix();
    //        }
    //    }

    //    private float Clamp(float value, float min, float max)
    //    {
    //        return value > max
    //            ? max
    //            : value < min
    //                ? min
    //                : value;
    //    }

    //    public void WindowResized(float width, float height)
    //    {
    //        _windowWidth = width;
    //        _windowHeight = height;
    //        UpdatePerspectiveMatrix();
    //    }

    //    private void UpdatePerspectiveMatrix()
    //    {
    //        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth / _windowHeight, _near, _far);
    //        ProjectionChanged?.Invoke(_projectionMatrix);
    //    }

    //    private void UpdateViewMatrix()
    //    {
    //        Vector3 lookDir = GetLookDir();
    //        _lookDirection = lookDir;
    //        _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
    //        ViewChanged?.Invoke(_viewMatrix);
    //    }

    //    private Vector3 GetLookDir()
    //    {
    //        Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
    //        Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
    //        return lookDir;
    //    }

    //    public CameraInfo GetCameraInfo() => new CameraInfo
    //    {
    //        CameraPosition_WorldSpace = _position,
    //        CameraLookDirection = _lookDirection
    //    };
    //}

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
