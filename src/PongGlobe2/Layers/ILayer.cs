using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
    public interface ILayer
    {
        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MinVisible { get; set; }

        /// <summary>
        /// Minimum visible zoom level
        /// </summary>
        double MaxVisible { get; set; }

        /// <summary>
        /// 显示隐藏
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// 可见范围单位
        /// </summary>
        //Styles.VisibilityUnits VisibilityUnits { get; set; }

        /// <summary>
        /// Specifies whether this layer should be rendered or not
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Name of layer
        /// </summary>
        string LayerName { get; set; }

        /// <summary>
        /// Optional title of layer. It will be used for services like WMS where both Name and Title are supported.
        /// </summary>
        string LayerTitle { get; set; }

        /// <summary>
        /// Gets the boundingbox of the entire layer
        /// </summary>
        object Envelope { get; }

        /// <summary>
        /// 图层定义的参考系，当数据源SRID与此SRID不一致时，动态变换
        /// </summary>
        int SRID { get; set; }       
    }



}
