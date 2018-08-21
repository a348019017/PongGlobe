using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Scene
{
    public interface ICameraController
    {
        //获取和设置视图矩阵
        Matrix4x4 ViewMatrix { get; }
        //获取和设置投影矩阵
        Matrix4x4 ProjectionMatrix { get; }
        //更新操作
        void Update(float deltaSeconds);
        //视图发送变化时的更新操作
        void WindowResized(float width, float height);
        //返回camera的世界坐标，shader中可能需要
        Vector3 Position { get; }
    }
}
