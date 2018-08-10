using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
    /// <summary>
    /// 用于相对定位的描述信息
    /// </summary>
    public struct LookAt
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }
      
        public double Range { get; set; }

        //0-360度
        public double Heading { get; set; }

        //0-90度
        public double Tilt { get; set;   }

        public double Roll { get; set;   }

        public LookAt(double latitude, double longitude, double altitude, double tile, double heading,double range)
        {
            Range = range;
            Altitude = altitude;
            Longitude = longitude;
            Latitude = latitude;
            Tilt = tile;
            Heading = heading;
            Roll = 0;
        }
    }
}
