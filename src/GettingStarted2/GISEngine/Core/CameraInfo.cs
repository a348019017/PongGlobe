using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
    /// <summary>
    /// 定义一个Camera的信息结构体，参照SuperMap的定义，用于描述参考系为椭球体的相机信息
    /// </summary>
    public struct CameraInfo
    {
        public Geodetic3D Postion;

        /// <summary>
        /// 范围0-90度
        /// </summary>
        public double Tilt;

        /// <summary>
        /// 范围0-360度
        /// </summary>
        public double Heading;

        public CameraInfo(Geodetic3D postion,double tile,double heading)
        {
            Postion = postion;
            Tilt = tile;
            Heading = heading;
        }

    }
}
