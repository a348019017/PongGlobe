using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace PongGlobe.Graphics.GeometricPrimitive
{
    /// <summary>
    /// 几何图元的可变参数，样式，使用StyleClass是为了灵活性和节省空间
    /// </summary>
    public class GeometryPrimitiveStyle
    {
        public RgbaFloat Color { get; set; }
        //优先使用Image作为纹理
        public Image<Rgba32> Image { get; set; }

        public GeometryPrimitiveStyle()
        {
            //默认有颜色无纹理
            Color = RgbaFloat.Red;
            Image = null;
        }

        /// <summary>
        /// 转换成结构体参数，以供Shader使用。
        /// </summary>
        /// <returns></returns>
        internal GeometryPrimitiveStyleStruct ToStyleStruct()
        {
            return new GeometryPrimitiveStyleStruct(Color,Image!=null);
        }
    }

    //存储GeometryPrimitive的Color信息，便于动态修改
    internal struct GeometryPrimitiveStyleStruct
    {
        public GeometryPrimitiveStyleStruct(RgbaFloat color,bool isTexture)
        {
            this.Color = color;
            this.IsTexture = isTexture;
            spa1 = 0;
            spa2 = 0;
            spa3 = 0;
        }
        RgbaFloat Color;
        bool IsTexture;
        float spa1;
        float spa2;
        float spa3;
    }
}
