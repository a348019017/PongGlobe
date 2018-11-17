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
    /// 几何图元的可变参数，样式
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
    }
}
