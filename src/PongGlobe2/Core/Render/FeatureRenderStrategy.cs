using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using PongGlobe.Scene;
using System.Drawing;
using System.Numerics;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
namespace PongGlobe.Core.Render
{
    /// <summary>
    /// 继承至RenderStrategy，用于控制要素的渲染,这里是从要素层开始控制，因此有很多额外的转换工作，性能是比较低下的，完全可以在实际数据层也就是mesh+indices层处理
    /// </summary>
    public class BasicFeatureRenderStrategy : IFeatureRenderStrategy
    {
        private List<FeatureRenderableObject> _lstScreenEnv = new List<FeatureRenderableObject>(); 
        /// <summary>
        /// 计算当前场景之下，每个要素的屏幕坐标，屏幕坐标需要矢量样式信息，通常情况下，每个要素最终渲染的范围不一定相同
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="shapes"></param>
        /// <returns></returns>
        public IEnumerable<FeatureRenderableObject> Apply(Scene.Scene dc, IEnumerable<FeatureRenderableObject> shapes)
        {
            var prj = dc.Camera.ProjectionMatrix;
            var view = dc.Camera.ViewMatrix;
            float width = ((MyCameraController2)dc.Camera).WindowsWidth;
            float hight= ((MyCameraController2)dc.Camera).WindowsHeight;
            _lstScreenEnv.Clear();
            foreach (var item in shapes)
            {
                var pointVector = item.PointVector;       
                    //计算出wind坐标系
                    //以左上角为原点                   
                 var windCoord = ((MyCameraController2)dc.Camera).Project(pointVector);
                //这里提前Cull掉场景外的
                if (windCoord.X > width || windCoord.X < 0) continue;
                if (windCoord.Y > hight || windCoord.Y < 0) continue;
                var coordExtent = new Envelope2DZ(windCoord - new Vector3(16, 16, 0), windCoord + new Vector3(16, 16, 0));
                item.WindowsScreenEnvolope = coordExtent;
                //以其为中心，宽度为32/2计算器像素坐标系
                //将其添加到集合中,这里暂时没有使用空间索引   
                bool isIntersect = false;
                for (int i = 0; i < _lstScreenEnv.Count; i++)
                {
                    var env = _lstScreenEnv[i].WindowsScreenEnvolope;
                    //如果找到相交点的box，且更靠近近地面则替换env,0为近裁剪面，1为远裁剪面
                    if (!env.Intersects(coordExtent) && env.Z > coordExtent.Z)
                    {
                        _lstScreenEnv[i] = item;
                        isIntersect = true;                       
                    }
                }
                //如果没有相交则添加
                if (!isIntersect)
                    _lstScreenEnv.Add(item);
            }
            //返回筛选后的shapes
            return _lstScreenEnv;
        }      
    }


    /// <summary>
    /// 要素渲染对象，记录要素的渲染结果
    /// </summary>
    public class FeatureRenderableObject
    {
        public ushort Indice { get; set; }

        public Vector3 PointVector { get; set; }
        /// <summary>
        /// 屏幕坐标的外包矩形，包含Z值，非BoundingBox
        /// </summary>
        public Envelope2DZ WindowsScreenEnvolope { get; set; }
    }
    
}
