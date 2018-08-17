using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;


namespace PongGlobe.Core.Extension
{
    public static class MathExtension
    {
        /// <summary>
        /// 角度转换成弧度
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static float ToRadius( float r)
        {          
           return (float)(r / 180f * Math.PI);
        }

        /// <summary>
        /// 角度转换成弧度
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static float ToRadius(double r)
        {
            return (float)(r / 180f * Math.PI);
        }


    }
}
