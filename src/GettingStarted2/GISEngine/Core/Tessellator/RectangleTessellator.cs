using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Drawing;
namespace PongGlobe.Core
{
    /// <summary>
    /// 对reactange进行细分三角网
    /// </summary>
    public static class RectangleTessellator
    {
        public static Mesh<Vector2> Compute(RectangleF rectangle, int numberOfPartitionsX, int numberOfPartitionsY)
        {
            if (numberOfPartitionsX < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsX");
            }

            if (numberOfPartitionsY < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsY");
            }
            //创建一个二维顶点对象，即其坐标为二维坐标
            Mesh<Vector2> mesh = new Mesh<Vector2>();
            mesh.PrimitiveType = PrimitiveType.Triangles;          
            int numberOfPositions = (numberOfPartitionsX + 1) * (numberOfPartitionsY + 1);          
            int numberOfIndices = (numberOfPartitionsX * numberOfPartitionsY) * 6;
            List<ushort> indices = new List<ushort>(numberOfIndices);
            var positions = new List<Vector2>();

            //
            // Positions
            //
            //左下角
            Vector2 lowerLeft = new Vector2( rectangle.Left,rectangle.Bottom);
            //长宽
            Vector2 toUpperRight = new Vector2(rectangle.Width, rectangle.Height);

            for (int y = 0; y <= numberOfPartitionsY; ++y)
            {
                double deltaY = y / (double)numberOfPartitionsY;
                double currentY = lowerLeft.Y + (deltaY * toUpperRight.Y);

                for (int x = 0; x <= numberOfPartitionsX; ++x)
                {
                    double deltaX = x / (double)numberOfPartitionsX;
                    double currentX = lowerLeft.X + (deltaX * toUpperRight.X);
                    positions.Add(new Vector2((float)currentX, (float)currentY));
                }
            }

            //
            // Indices暂时用ushort来保存16位，至多可以保存65,535个Index，差不多2w多个三角形，这已经足够使用了
            //
            int rowDelta = numberOfPartitionsX + 1;
            int i = 0;
            for (int y = 0; y < numberOfPartitionsY; ++y)
            {
                for (int x = 0; x < numberOfPartitionsX; ++x)
                {
                    indices.Add((ushort)i);
                    indices.Add((ushort)(i + 1));
                    indices.Add((ushort)(rowDelta + (i + 1)));

                    indices.Add((ushort)i);
                    indices.Add((ushort)(rowDelta + i + 1));
                    indices.Add((ushort)(rowDelta + i));                    
                    i += 1;
                }
                i += 1;
            }

            mesh.Indices = indices.ToArray();

            return mesh;
        }
    }
}
