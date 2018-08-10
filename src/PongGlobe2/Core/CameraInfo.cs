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
        //相机所看的中心点
        public double Latitude;

        public double Longitude;

        public double Altitude;

        //public Geodetic3D Postion;
        /// <summary>
        /// 范围0-90度
        /// </summary>
        public double Tilt;

        /// <summary>
        /// 范围0-360度
        /// </summary>
        public double Heading;

        public CameraInfo(double latitude,double longitude,double altitude,double tile,double heading)
        {
            Altitude = altitude;
            Longitude = longitude;
            Latitude = latitude;
            Tilt = tile;
            Heading = heading;
        }

    }
}
