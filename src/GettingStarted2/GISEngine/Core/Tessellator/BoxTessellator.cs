using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core
{
    /// <summary>
    /// 创建一个Box的Mesh对象
    /// </summary>
    public static class BoxTessellator
    {
        public static Mesh Compute(Vector3 length)
        {
            if (length.X < 0 || length.Y < 0 || length.Z < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            //mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;
            //VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", 8);
            //mesh.Attributes.Add(positionsAttribute);
            //IndicesUnsignedShort indices = new IndicesUnsignedShort(36);
            //mesh.Indices = indices;
            //
            // 8 corner points
            //
            List<VertexPosition> positions = new List<VertexPosition>(); 

            Vector3 corner = 0.5f * length;
            positions.Add(new VertexPosition(-corner.X, -corner.Y, -corner.Z));
            positions.Add(new VertexPosition(corner.X, -corner.Y, -corner.Z));
            positions.Add(new VertexPosition(corner.X, corner.Y, -corner.Z));
            positions.Add(new VertexPosition(-corner.X, corner.Y, -corner.Z));
            positions.Add(new VertexPosition(-corner.X, -corner.Y, corner.Z));
            positions.Add(new VertexPosition(corner.X, -corner.Y, corner.Z));
            positions.Add(new VertexPosition(corner.X, corner.Y, corner.Z));
            positions.Add(new VertexPosition(-corner.X, corner.Y, corner.Z));
            //
            // 6 faces, 2 triangles each
            //
            ushort[] indices =
            {
                4, 5, 6, 4, 6, 7,
                1, 0, 3, 1, 3, 2,
                1, 6, 5, 1, 2, 6,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7,
                0, 1, 5, 0, 5, 4,
            };
            mesh.Indices = indices;
            mesh.Positions = positions.ToArray();            
            return mesh;
        }
    }
}
