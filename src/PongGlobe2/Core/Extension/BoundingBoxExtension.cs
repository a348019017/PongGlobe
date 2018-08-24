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
            //前10个点使用linestrip绘制上下两个面，中间4条线使用line绘制四条线,共18个indices
            var indices = new ushort[] { 0,1,2,3,0,4,5,6,7,4,0,4,1,5,2,6,3,7};
            var _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * this.Indices.Length), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_indexBuffer, 0, indices);
            return new System.Tuple<DeviceBuffer, DeviceBuffer>(_vertexBuffer, _indexBuffer);
        }
    }
}
