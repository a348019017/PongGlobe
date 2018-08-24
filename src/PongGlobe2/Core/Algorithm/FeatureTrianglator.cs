using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using System.Numerics;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using System.Collections;
using PongGlobe.Core.Extension;
using System.Linq;

namespace PongGlobe.Core.Algorithm
{
    /// <summary>
    /// 对矢量要素进行mesh化的算法
    /// </summary>
    public class FeatureTrianglator
    {
        /// <summary>
        /// 思路1，对经纬度进行平面程度的三角网化，同时进行sub,一直到当前视图的精度
        /// 然后将经纬度坐标转换成世界坐标，以实现贴椭球体的mesh划分,z这样的实现在地球背面端任然有多边形显示，可以考虑裁剪掉（globeCull或者faceCull的）或者在shader中处理，
        /// </summary>
        public static List<Mesh<Vector3>> FeatureToMesh(IShapefileFeature _feature,Ellipsoid _shape)
        {
            if (_feature == null) return null;
            List<Mesh<Vector3>> meshes = new List<Mesh<Vector3>>();
            if (_feature.Geometry is GeoAPI.Geometries.IMultiPolygon)
            {
                foreach (var item in ((GeoAPI.Geometries.IMultiPolygon)_feature.Geometry).Geometries)
                {
                    var mesh = PolygonToMesh(item as GeoAPI.Geometries.IPolygon, _shape);
                    if (mesh != null) meshes.Add(mesh);
                }
                //判断Hole的顺序是否和外边框相同
                //var geo = _feature.Geometry as NetTopologySuite.Geometries.po               
                //throw new Exception("Not Support Multipolygon");
            }
            if (_feature.Geometry is GeoAPI.Geometries.IPolygon)
            {
                var mesh = PolygonToMesh(_feature.Geometry as GeoAPI.Geometries.IPolygon, _shape);
                if (mesh != null) meshes.Add(mesh);
            }
            return meshes;
        }

        public static Mesh<Vector3> PolygonToMesh(GeoAPI.Geometries.IPolygon _polygon, Ellipsoid _shape)
        {
            if (_polygon == null) throw new Exception("_polygon is Null");
            List<Vector2> positions = new List<Vector2>();
            //填充顶点跟索引
            //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标         
            foreach (var coord in _polygon.Coordinates)
            {
                //将其转换成弧度制,自动贴地
                positions.Add(new Vector2(MathExtension.ToRadius(coord.X), MathExtension.ToRadius(coord.Y)));
            }
            //去除重复的数据
            var posClearUp = SimplePolygonAlgorithms.Cleanup<Vector2>(positions);
            //如果不是顺时针，强制转换成顺时针
            if (SimplePolygonAlgorithms.ComputeWindingOrder(posClearUp) != PolygonWindingOrder.Clockwise)
            {
                posClearUp = posClearUp.Reverse().ToArray();
            }
            var indices = EarClippingOnEllipsoid.Triangulate2D(posClearUp);
            //将vector2转换成vector3 
            List<Vector3> worldPosition = new List<Vector3>();
            foreach (var item in posClearUp)
            {
                var vec = _shape.ToVector3(new Geodetic2D(item.X, item.Y));
                worldPosition.Add(vec);
            }
            var _mesh = new Mesh<Vector3>();
            _mesh.Indices = indices.ToArray();
            _mesh.Positions = worldPosition.ToArray();
            return _mesh;
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
