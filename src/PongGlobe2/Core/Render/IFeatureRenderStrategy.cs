using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;


namespace PongGlobe.Core.Render
{
    /// <summary>
    /// 继承至RenderStrategy，用于控制要素的渲染
    /// </summary>
    public interface IFeatureRenderStrategy
    {
        /// <summary>
        /// 遍历所有要素，然后返回可以渲染的要素
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="shapes"></param>
        IEnumerable<FeatureRenderableObject> Apply(Scene.Scene dc, IEnumerable<FeatureRenderableObject> shapes);
    }
}
