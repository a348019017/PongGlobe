using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using PongGlobe.Core;


namespace PongGlobe.Core.Extension
{
    public static class BoundingBoxExtension
    {
        public static Tuple<DeviceBuffer, DeviceBuffer> CreateResource(this BoundingBox bounding ,GraphicsDevice gd,ResourceFactory factory)
        {
            var position = bounding.GetCorners();
            var _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(12 * position.Length), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vertexBuffer, 0, position);
            //共24个indices,使用line模式绘制
            var indices = new ushort[] { 0,1,1,2,2,3,3,0,4,5,5,6,6,7,7,4,0,4,1,5,2,6,3,7};
            var _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * indices.Length), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_indexBuffer, 0, indices);
            return new System.Tuple<DeviceBuffer, DeviceBuffer>(_vertexBuffer, _indexBuffer);
        }
    }
}
