using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace SampleBase
{

    //一个基本的基于世界坐标系的Camera
    public class Camera
    {
        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 1000f;
       

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        //相机的初始位置位于世界坐标系的Z轴
        private Vector3 _position = new Vector3(0, 0, 3.0f);
        //朝向默认为原点
        private Vector3 _target = new Vector3(0, 0, 0);
        //相机的Y朝向,默认为世界坐标系Y轴
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;
        private float _moveSpeed = 10.0f;
    
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
        public Vector3 Target
        {
            get { return _target; }
            set { _target = value;UpdateViewMatrix(); }
        }
        public Vector3 Right {
            get { return _right; }
            set { _right = value; }
        }
        public Vector3 Up
        { get { return _up; } set { _up = value;UpdateViewMatrix(); } }
        
        public float FarDistance { get => _far; set { _far = value; UpdatePerspectiveMatrix(); } }
        public float FieldOfView => _fov;
        public float NearDistance { get => _near; set { _near = value; UpdatePerspectiveMatrix(); } }

        public float AspectRatio => _windowWidth / _windowHeight;

       

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        

        public void Update(float deltaSeconds)
        {
           
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
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _target, _up);
            ViewChanged?.Invoke(_viewMatrix);
        }

       

        
    }




    




   


  
}
