using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using System.Numerics;

namespace PongGlobe.Core
{
    /// <summary>
    /// Mesh Strut for text
    /// </summary>
    public class TextMesh
    {

        private int vtxWritePosition;
        private int idxWritePosition;
        private int currentIdx;

        public TextMesh()
        {
            //_attributes = new VertexAttributeCollection();
        }

        /// <summary>
        /// 默认包含10000个点，仅为了防止GC
        /// </summary>
        public VertexPosition2Color[] Positions { get; set; } = new VertexPosition2Color[10000];


        public ushort[] Indices { get; set; } = new ushort[100000];

        //mesh的实体类型,默认为LineList
        public PrimitiveTopology PrimitiveTopology => PrimitiveTopology.LineList;
    }

    /// <summary>
    /// 点的像素坐标偏移量+color+点的世界坐标
    /// </summary>
    public struct VertexPosition2Color
    {
        //屏幕坐标
        public Vector2 ScreenPos;
        //世界坐标
        public Vector3 Pos;
        //颜色
        public RgbaFloat Color;

        public VertexPosition2Color(ref Vector2 screenPos, ref Vector3 pos, ref RgbaFloat color)
        {
            ScreenPos = screenPos;
            Pos = pos;
            Color = color;
        }
    }

}
