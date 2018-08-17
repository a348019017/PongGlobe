

using System;
using System.Drawing;
using System.Reflection;
using Veldrid.ImageSharp;


namespace PongGlobe.Styles
{
    /// <summary>
    /// ����һ��ʸ�����ݵ���ʽ�࣬�պ���ʹ��skia��ʸ������Ⱦ֮����ʾ��3d������
    /// </summary>
    [Serializable]
    public class VectorStyle : Style, ICloneable
    {
        private static readonly Random _rnd = new Random();
     
        /// <summary>
        /// ��Ҫ��ͼ�����ʽ,Ŀǰ�������һ��ͼ���Ӧһ��ͼ�꣬�Ժ���Ʒ��Ż�ʱ�ÿ��Ƕ���ͼ������
        /// </summary>
        public static readonly ImageSharpTexture DefaultSymbol;

        /// <summary>
        /// Static constructor
        /// </summary>
        static VectorStyle()
        {
            //var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
            //if (rs != null)
             //   DefaultSymbol = Image.FromStream(rs);
        }

        /// <summary>
        /// ��¡һ����ʽ
        /// </summary>
        /// <returns></returns>
        public VectorStyle Clone()
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
        /// �ߵ���ɫ
        /// </summary>
        private Color _lineColor;
        /// <summary>
        /// �߿�
        /// </summary>
        private uint _width;
        /// <summary>
        /// ������ɫ
        /// </summary>
        private Color _fillColor;       
        /// <summary>
        /// ͼ���ImageIndex
        /// </summary>
        private uint imageIndex;
        /// <summary>
        /// �ߵ�ƫ��
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
        public VectorStyle()
        {
            _fillColor = Color.FromArgb(128,128,0,0);
            _lineColor= Color.FromArgb(255, 0, 0, 0);
            //�������ؿ����
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
        /// �ߵ���ɫ
        /// </summary>
        public Color LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; }
        }

        /// <summary>
        ///�߿�ʵ����Ⱦ�Ŀ��
        /// </summary>
        public uint LineWidth
        {
            get { return _width; }
            set { _width = value; }
        }

        
        /// <summary>
        /// Fillstyle for Polygon geometries
        /// </summary>
        public Color FillColor
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