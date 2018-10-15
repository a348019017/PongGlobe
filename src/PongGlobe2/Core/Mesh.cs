#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace PongGlobe.Core
{



    /// <summary>
    /// 自定义Mesh结构便于通用化处理,暂不包含UV，normal等信息
    /// </summary>
    public class Mesh
    {
        public Mesh()
        {
            //_attributes = new VertexAttributeCollection();
        }

        public VertexPosition[] Positions { get; set; }

        public ushort[] Indices { get; set; }

        //mesh的实体类型
        public PrimitiveTopology PrimitiveTopology { get; set; }
        //public WindingOrder FrontFaceWindingOrder { get; set; }

        //private VertexAttributeCollection _attributes;
    }

    /// <summary>
    /// 泛型Mesh对象，自定义position,uv,normal的生成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Mesh<T> where T:struct
    {
        public Mesh()
        {
            PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        public T[] Positions { get; set; }

        public ushort[] Indices { get; set; }

        //mesh的实体类型
        public PrimitiveTopology PrimitiveTopology { get; set; }

        /// <summary>
        /// 创建相应的顶点和indices的资源，同时也更新部分资源,资源的释放由使用者处理
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>返回顶点缓存和索引缓存</returns>
        public System.Tuple<DeviceBuffer,DeviceBuffer> CreateGraphicResource(GraphicsDevice gd,ResourceFactory factory)
        {
            var _vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(12 * this.Positions.Length), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(_vertexBuffer, 0, this.Positions);
            var _indexBuffer = factory.CreateBuffer(new BufferDescription((uint)(sizeof(ushort) * this.Indices.Length), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(_indexBuffer, 0, this.Indices);
            return new System.Tuple<DeviceBuffer, DeviceBuffer>(_vertexBuffer,_indexBuffer);
        }
    }

    /// <summary>
    /// 仅记录顶点坐标
    /// </summary>
    public struct VertexPosition
    {
        public const uint SizeInBytes = 12;

        public float PosX;
        public float PosY;
        public float PosZ;

       

        public VertexPosition(Vector3 pos)
        {
            PosX = pos.X;
            PosY = pos.Y;
            PosZ = pos.Z;
            //TexU = uv.X;
            //TexV = uv.Y;
            //FloatBlue = color.Z;
            //FloatRed = color.X;
            //FloatGreen = color.Y;
        }
        public VertexPosition(float posX,float posY,float posZ)
        {
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            
        }
    }

    /// <summary>
    /// 顶点法线贴图
    /// </summary>
    public struct VertexPositionUVNormal
    {
        public float PosX;
        public float PosY;
        public float PosZ;

        //TexU = uv.X;
        //TexV = uv.Y;
        //FloatBlue = color.Z;
        //FloatRed = color.X;
        //FloatGreen = color.Y;

        public VertexPositionUVNormal(Vector3 pos,Vector3 uv)
        {
            PosX = pos.X;
            PosY = pos.Y;
            PosZ = pos.Z;

        }
    }

}
