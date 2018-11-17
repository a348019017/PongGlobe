using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace PongGlobe.Core
{

    /// <summary>
    /// 共享的一些基本资源，如View,proj的DeviceBuffer或实际数据
    /// </summary>
    public class ShareResource
    {
        /// <summary>
        /// 投影矩阵对象
        /// </summary>
        public static DeviceBuffer ProjectionBuffer { get; set; }
        /// <summary>
        /// 投影矩阵的布局信息
        /// </summary>
        public static ResourceLayout ProjectionResourceLoyout { get; set; }
        /// <summary>
        /// 共享资源集
        /// </summary>
        public static ResourceSet ProjectionResourceSet { get; set; }
    }


}
