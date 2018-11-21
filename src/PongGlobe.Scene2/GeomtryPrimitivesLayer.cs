using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using PongGlobe.Graphics.GeometricPrimitive;
using System.Collections.ObjectModel;

namespace PongGlobe.Scene
{
    /// <summary>
    /// 存储GeometryPrimitive的图层类
    /// </summary>
    public class GeomtryPrimitivesLayer:Layer
    {
        public GeomtryPrimitivesLayer()
        {
            GeometricPrimitives.CollectionChanged += GeometricPrimitives_CollectionChanged;
        }
        private void GeometricPrimitives_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        { 
                
        }
        /// <summary>
        /// 可观察集合
        /// </summary>
        public ObservableCollection<GeometricPrimitive> GeometricPrimitives { get; set; } = new ObservableCollection<GeometricPrimitive>();


    }
}
