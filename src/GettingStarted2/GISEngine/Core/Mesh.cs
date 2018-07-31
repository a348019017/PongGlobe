﻿#region License
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
    public enum PrimitiveType
    {
        Points,
        Lines,
        LineLoop,
        LineStrip,
        Triangles,
        TriangleStrip,
        TriangleFan,
        LinesAdjacency,
        LineStripAdjacency,
        TrianglesAdjacency,
        TriangleStripAdjacency
    }


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
        public PrimitiveType PrimitiveType { get; set; }
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
            PrimitiveType = PrimitiveType.Triangles;
        }

        public T[] Positions { get; set; }

        public ushort[] Indices { get; set; }

        //mesh的实体类型
        public PrimitiveType PrimitiveType { get; set; }

        public bool CreateGraphicResource(ResourceFactory factory)
        {
            return true;
        }
    }

    //public class MeshResources


    /// <summary>
    /// 定点坐标
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
}
