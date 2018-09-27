using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;



namespace PongGlobe.Primitive
{
    /// <summary>
    /// 简单表示一个圆柱体的基元
    /// </summary>
    public class Cylinder
    {
        /// <summary>
        /// 中心线起止点，半径来表示一个圆柱
        /// </summary>
        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }
        public float Radius { get; set; }      
        public Cylinder(Vector3 point1,Vector3 point2,  float radius)
        {
            StartPoint = point1;
            EndPoint = point2;
            Radius = radius;         
        }
        /// <summary>
        /// 高度,均已世界坐标系为准
        /// </summary>
        public float Height { get { return (StartPoint - EndPoint).Length(); } }
        /// <summary>
        /// 周长
        /// </summary>
        public double Circumference
        {
            get { return  Math.PI*2 * Radius; }
        }
        /// <summary>
        /// 直径
        /// </summary>
        public double Diameter
        {
            get { return 2 * Radius; }
        }
        /// <summary>
        /// 体积
        /// </summary>
        public double Volume
        {
            get { return Math.PI * Radius * Radius * Height; }
        }
    }
}
