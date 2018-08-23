using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using System.Numerics;
namespace PongGlobe.Core.Algorithm
{
    /// <summary>
    /// 对矢量要素进行mesh化的算法
    /// </summary>
    public class FeatureTrianglator
    {
        /// <summary>
        /// 思路1，对经纬度进行平面程度的三角网化，同时进行sub,一直到当前视图的精度
        /// 然后将经纬度坐标转换成世界坐标，以实现贴椭球体的mesh划分
        /// </summary>
        public static Mesh<Vector3> PolygonToMesh1()
        {

            return null;
        }

        /// <summary>
        /// 思路2，对世界坐标系下的坐标进行平面程度的三角网化，同时进行sub,一直到当前视图的精度
        /// 然后将经纬度坐标转换成世界坐标，以实现贴椭球体的mesh划分
        /// </summary>
        public static Mesh<Vector3> PolygonToMesh2()
        {
            return null;
        }



    }
}
