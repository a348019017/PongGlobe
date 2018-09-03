

using System;
using System.Drawing;
using System.Reflection;
using Veldrid.ImageSharp;
using Veldrid;


namespace PongGlobe.Styles
{
    [Serializable]
    public class VectorStyle
    {
        /// <summary>
        /// 面样式
        /// </summary>
        public PolygonVectorStyle PolygonStyle { get; set; }
        /// <summary>
        /// 线样式
        /// </summary>
        public LineVectorStyle LineStyle { get; set; }
        /// <summary>
        /// 点样式
        /// </summary>
        public PointVectorStyle PointStyle { get; set; }
        public VectorStyle()
        {
            PolygonStyle = new PolygonVectorStyle();
            LineStyle = new LineVectorStyle();
            PointStyle = new PointVectorStyle();
        }
    }


    /// <summary>
    /// 定义一个矢量面的样式类
    /// </summary>
    [Serializable]
    public class PolygonVectorStyle : Style, ICloneable
    {
        private static readonly Random _rnd = new Random();  
        /// <summary>
        /// 面要素也可以使用纹理贴图
        /// </summary>
        public static readonly ImageSharpTexture DefaultSymbol;
      
        /// <summary>
        /// 克隆一个样式
        /// </summary>
        /// <returns></returns>
        public PolygonVectorStyle Clone()
        {
            return null;
            //VectorStyle vs;
            //lock (this)
            //{
            //    try
            //    {
            //        vs = (VectorStyle)MemberwiseClone();// new VectorStyle();

            //        if (_fillStyle != null)
            //            vs._fillStyle = _fillStyle.Clone() as Brush;

            //        if (_lineStyle != null)
            //            vs._lineStyle = _lineStyle.Clone() as Pen;

            //        if (_outlineStyle != null)
            //            vs._outlineStyle = _outlineStyle.Clone() as Pen;

            //        if (_pointBrush != null)
            //            vs._pointBrush = _pointBrush.Clone() as Brush;

            //        vs._symbol = (_symbol != null ? _symbol.Clone() as Image : null);
            //        vs._symbolRotation = _symbolRotation;
            //        vs._symbolScale = _symbolScale;
            //        vs.PointSymbolizer = PointSymbolizer;
            //        vs.LineSymbolizer = LineSymbolizer;
            //        vs.PolygonSymbolizer = PolygonSymbolizer;
            //    }
            //    catch (Exception ee)
            //    {
            //        logger.Error("Exception while creating cloned style", ee);
            //        /* if we got an exception, set the style to null and return since we don't know what we got...*/
            //        vs = null;
            //    }
            //}
            //return vs;
        }
        
        object ICloneable.Clone()
        {
            return Clone();
        }

    
        /// <summary>
        /// 面的填充色
        /// </summary>
        private RgbaFloat _fillColor;       
        /// <summary>
        /// 图标的ImageIndex
        /// </summary>
        private uint imageIndex;

    
        /// <summary>
        /// Initializes a new VectorStyle and sets the default values
        /// </summary>
        /// <remarks>
        /// Default style values when initialized:<br/>
        /// *LineStyle: 1px solid black<br/>
        /// *FillStyle: Solid black<br/>
        /// *Outline: No Outline
        /// *Symbol: null-reference
        /// </remarks>
        public PolygonVectorStyle()
        {
            _fillColor =new  RgbaFloat(0,0.5f,0,0.5f);          
        }
             
        /// <summary>
        /// Fillstyle for Polygon geometries
        /// </summary>
        public RgbaFloat FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

       


        /// <summary>
        /// Symbol used for rendering points
        /// </summary>
        public uint ImageIndex
        {
            get { return imageIndex; }
            set { imageIndex = value; }
        }

       
    }

    /// <summary>
    /// 点的矢量样式，由于样式比较多，抽象出点线面的样式出来
    /// </summary>
    [Serializable]
    public class PointVectorStyle
    {

    }

    /// <summary>
    /// 矢量线要素样式
    /// </summary>
    [Serializable]
    public class LineVectorStyle : Style, ICloneable
    {
        private static readonly Random _rnd = new Random();
        /// <summary>
        /// 点要素图标的样式,目前的设计是一个图层对应一种图标，以后设计符号化时得考虑多种图标的情况
        /// </summary>
        public static readonly ImageSharpTexture DefaultSymbol;

        /// <summary>
        /// 克隆一个样式
        /// </summary>
        /// <returns></returns>
        public LineVectorStyle Clone()
        {
            return null;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        /// <summary>
        /// 线的颜色
        /// </summary>
        private RgbaFloat _lineColor;
        /// <summary>
        /// 线宽
        /// </summary>
        private uint _width;     
        /// <summary>
        /// 图标的ImageIndex
        /// </summary>
        private uint imageIndex;
        /// <summary>
        /// 线的偏移
        /// </summary>
        private float _lineOffset;

        /// <summary>
        /// Initializes a new VectorStyle and sets the default values
        /// </summary>
        /// <remarks>
        /// Default style values when initialized:<br/>
        /// *LineStyle: 1px solid black<br/>
        /// *FillStyle: Solid black<br/>
        /// *Outline: No Outline
        /// *Symbol: null-reference
        /// </remarks>
        public LineVectorStyle()
        {         
            _lineColor = new RgbaFloat(0.5f, 0, 0, 1);
            //三个像素宽的线
            _width = 3;
            //Outline = new Pen(Color.Black, 1);
            //Line = new Pen(Color.Black, 1);
            //Fill = new SolidBrush (Color.FromArgb(192, Color.Black));
            //EnableOutline = false;
            //SymbolScale = 1f;
            //PointColor = new SolidBrush(Color.Red);
            //PointSize = 10f;
            //LineOffset = 0;
        }
        /// <summary>
        /// 线的颜色
        /// </summary>
        public RgbaFloat LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        /// <summary>
        ///线宽，实际渲染的宽度
        /// </summary>
        public uint LineWidth
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// 线也可以有自己的纹理
        /// </summary>
        public uint ImageIndex
        {
            get { return imageIndex; }
            set { imageIndex = value; }
        }
        /// <summary>
        /// Gets or sets the offset (in pixel units) by which line will be offset from its original posision (perpendicular).
        /// </summary>
        /// <remarks>
        /// A positive value offsets the line to the right
        /// A negative value offsets to the left
        /// </remarks>
        public float LineOffset
        {
            get { return _lineOffset; }
            set { _lineOffset = value; }
        }

    }
}