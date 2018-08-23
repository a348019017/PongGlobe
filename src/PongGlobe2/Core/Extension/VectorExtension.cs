using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core.Extension
{
    //对vector等的扩展方法
    public static class VectorExtension
    {
        /// <summary>
        /// 求A向量到b向量之间的角度，不区分前后
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double AngleBetween(this Vector3 t,Vector3 other)
        {
            
           return Math.Acos(Vector3.Dot(Vector3.Normalize(t), Vector3.Normalize(other)));
           //return Math.Acos(Normalize().Dot(other.Normalize()));
        }

        /// <summary>
        /// 二维向量的叉乘
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double Cross(this Vector2 t, Vector2 other)
        {
            return t.X * other.Y - t.Y * other.X;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public static Vector3 RotateAroundAxis(this Vector3 t, Vector3 axis, double theta)
        {
            double u = axis.X;
            double v = axis.Y;
            double w = axis.Z;

            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            double ms = axis.LengthSquared();
            double m = Math.Sqrt(ms);

            return new Vector3(
                 (float)(((u * (u * axis.X + v * axis.Y + w * axis.Z)) +
                (((axis.X * (v * v + w * w)) - (u * (v * axis.Y + w * axis.Z))) * cosTheta) +
                (m * ((-w * axis.Y) + (v * axis.Z)) * sinTheta)) / ms),

                (float)(((v * (u * axis.X + v * axis.Y + w * axis.Z)) +
                (((axis.Y * (u * u + w * w)) - (v * (u * axis.X + w * axis.Z))) * cosTheta) +
                (m * ((w * axis.X) - (u * axis.Z)) * sinTheta)) / ms),

                (float)(((w * (u * axis.X + v * axis.Y + w * axis.Z)) +
                (((axis.Z * (u * u + v * v)) - (w * (u * axis.X + v * axis.Y))) * cosTheta) +
                (m * (-(v * axis.X) + (u * axis.Y)) * sinTheta)) / ms));
        }
    }
}
