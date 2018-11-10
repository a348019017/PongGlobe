using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core
{
    /// <summary>
    /// 顶点坐标颜色和纹理
    /// </summary>
    public struct VertexPositionColorTexture
    {
        public const uint SizeInBytes = 32;

        public float PosX;
        public float PosY;
        public float PosZ;

        public float FloatRed;
        public float FloatGreen;
        public float FloatBlue;

        public float TexU;
        public float TexV;

        public VertexPositionColorTexture(Vector3 pos, Vector3 color, Vector2 uv)
        {
            PosX = pos.X;
            PosY = pos.Y;
            PosZ = pos.Z;
            TexU = uv.X;
            TexV = uv.Y;
            FloatBlue = color.Z;
            FloatRed = color.X;
            FloatGreen = color.Y;
        }

        public VertexPositionColorTexture(Vector3 pos, Vector2 uv)
        {
            PosX = pos.X;
            PosY = pos.Y;
            PosZ = pos.Z;
            TexU = uv.X;
            TexV = uv.Y;
            FloatBlue = 0.5f;
            FloatRed = 0.5f;
            FloatGreen = 0.5f;
        }
    }
}
