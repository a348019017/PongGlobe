using System;
using System.Collections.Generic;
using System.Text;
using PongGlobe.Core;
using System.Numerics;
using NetTopologySuite.IO.ShapeFile.Extended.Entities;
using System.Collections;
using PongGlobe.Core.Extension;
using System.Linq;
using Veldrid;
using GeoAPI.Geometries;
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
        /// 将一个Feature转换成
        /// </summary>
        /// <param name="_feature"></param>
        /// <param name="_shape"></param>
        /// <returns></returns>
        public static Mesh<Vector3> FeatureToPoints(IShapefileFeature _feature, Ellipsoid _shape)
        {            
            if (_feature == null) return null;
            List<Vector3> points = new List<Vector3>();
            List<ushort> indices = new List<ushort>();
            if (_feature.Geometry is GeoAPI.Geometries.IMultiPolygon)
            {
                foreach (var item in ((GeoAPI.Geometries.IMultiPolygon)_feature.Geometry).Geometries)
                {
                    PolygonToPoints(item as GeoAPI.Geometries.IPolygon, _shape, points,indices);                  
                }             
            }
            if (_feature.Geometry is GeoAPI.Geometries.IPolygon)
            {
                PolygonToPoints(_feature.Geometry as GeoAPI.Geometries.IPolygon, _shape,points,indices);
                //查找其Hole
                var geo = _feature.Geometry as GeoAPI.Geometries.IPolygon;               
            }
            var mesh = new Mesh<Vector3>();
            mesh.PrimitiveTopology = PrimitiveTopology.LineStrip;
            mesh.Positions = points.ToArray();
            mesh.Indices = indices.ToArray();
            return mesh;
        }


        /// <summary>
        /// 将点要素转换为PointListMesh
        /// </summary>
        /// <returns></returns>
        public static Mesh<Vector3> PointFeatureToPoints(IEnumerable<IShapefileFeature> _features, Ellipsoid _shape)
        {
            var points =new Mesh<Vector3>();
            points.PrimitiveTopology = PrimitiveTopology.PointList;
            List<Vector3> vectors = new List<Vector3>();
            List<ushort> indices = new List<ushort>();
            ushort start = 0;
            foreach (var item in _features)
            {
                var geo = item.Geometry;
                ///仅添加点和多点
                if (geo is IPoint)
                {
                    vectors.Add(_shape.ToVector3(new Geodetic2D(geo.Coordinate.X,geo.Coordinate.Y)));
                    indices.Add(start++);
                } else if (geo is IMultiPoint)
                {
                    foreach (var coord in geo as IMultiPoint)
                    {
                        vectors.Add(_shape.ToVector3(new Geodetic2D(geo.Coordinate.X, geo.Coordinate.Y)));
                        indices.Add(start++);
                    }
                }
            }
            points.Positions = vectors.ToArray();
            points.Indices = indices.ToArray();
            return points;
        }




        public static Mesh<Vector3> FeatureToLineStripAdjacency(IShapefileFeature _feature, Ellipsoid _shape)
        {
            if (_feature == null) return null;
            List<Vector2> points = new List<Vector2>();
            List<ushort> indices = new List<ushort>();
            if (_feature.Geometry is GeoAPI.Geometries.IMultiPolygon)
            {
                foreach (var item in ((GeoAPI.Geometries.IMultiPolygon)_feature.Geometry).Geometries)
                {
                    PolygonToLineStripAdjacency(item as GeoAPI.Geometries.IPolygon, _shape, points, indices);
                }
            }
            if (_feature.Geometry is GeoAPI.Geometries.IPolygon)
            {
                PolygonToLineStripAdjacency(_feature.Geometry as GeoAPI.Geometries.IPolygon, _shape, points, indices);          
            }           
            var mesh = new Mesh<Vector3>();
            mesh.PrimitiveTopology = PrimitiveTopology.LineStripAdjacency;            
            mesh.Positions = points.ConvertAll(i => _shape.ToVector3(new Geodetic2D(i.X, i.Y))).ToArray();
            mesh.Indices = indices.ToArray();
            return mesh;
        }


        public static bool PolygonToLineStripAdjacency(GeoAPI.Geometries.IPolygon _polygon, Ellipsoid _shape, List<Vector2> positions, List<ushort> indices)
        {
            if (_polygon == null) throw new Exception("_polygon is Null");
            //先进行二维的处理，在转换成贴于地表的三维坐标
            
            //填充顶点跟索引
            //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标         
            ///0xFFFF/0xFFFFFFFF分别表示16位和32位的indice中断符
            ushort breakupIndice = 0xFFFF;
            var extRing = _polygon.ExteriorRing;
            var interRing = _polygon.InteriorRings;
            //添加外环
            LineStringToLineStripAdjacency(extRing,_shape, positions, indices);
            //完事添加间隔符
            indices.Add(breakupIndice);
            //添加内环
            foreach (var item in interRing)
            {
                LineStringToLineStripAdjacency(item, _shape, positions, indices);
                //完事添加间隔符
                indices.Add(breakupIndice);
            }                     
            return true;
        }

        private static bool LineStringToLineStripAdjacency(GeoAPI.Geometries.ILineString _lineString, Ellipsoid _shape, List<Vector2> positions, List<ushort> indices)
        {
            
            if (_lineString == null)  throw new Exception("_lineString is Null");
            if (_lineString.IsClosed)
            {
                //记录起点
                ushort indicesMax = (ushort)positions.Count();             
                //添加第一个环的最后一个点
                indices.Add((ushort)(indicesMax + _lineString.Coordinates.Length - 2));
                //对于polygon来说第一个点和最后一个点是相同的,因此只添加一次
                for (int i = 0; i < _lineString.Coordinates.Length - 1; i++)
                {
                    var coord = _lineString.Coordinates[i];
                    var geoDetic = new Vector2(MathExtension.ToRadius(coord.X), MathExtension.ToRadius(coord.Y));
                    positions.Add(geoDetic);
                    indices.Add((ushort)(indicesMax+i));
                }
                //添加起点
                indices.Add((ushort)(indicesMax));
                //添加第二个点
                indices.Add((ushort)(indicesMax+1));             
            }
            else
            {
                //非闭合的情况下,首尾特殊处理即可
                throw new Exception("Not Support!");
            }
            return true;
        }


        /// <summary>
        /// 如果两个点的间距低于当前精度，则进一步细分
        /// </summary>
        /// <param name="_polygon"></param>
        /// <param name="_shape"></param>
        /// <returns></returns>
        public static bool PolygonToPoints(GeoAPI.Geometries.IPolygon _polygon, Ellipsoid _shape, List<Vector3> positions,List<ushort> indices)
        {
            if (_polygon == null) throw new Exception("_polygon is Null");
            List<Vector2> positions2D = new List<Vector2>();
            //填充顶点跟索引
            //详细流程，如果是投影坐标，将其转换成wgs84的经纬度坐标，再使用参考系计算出其真实的地理坐标         
            ///0xFFFF/0xFFFFFFFF分别表示16位和32位的indice中断符
            ushort breakupIndice = 0xFFFF;
            var extRing = _polygon.ExteriorRing;
            var interRing = _polygon.InteriorRings;
            var ccc = extRing as GeoAPI.Geometries.ILinearRing;
            ///判断是否为逆时针，可能需要调整其方向
            var isCCW = ccc.IsCCW;
            ///添加外环
            ushort indicesMax = (ushort)positions.Count();
            foreach (var coord in extRing.Coordinates)
            {                
                //将其转换成弧度制,自动贴地
                var geoDetic = new Vector2(MathExtension.ToRadius(coord.X), MathExtension.ToRadius(coord.Y));
                positions2D.Add(geoDetic);
                indices.Add(indicesMax++);
            }
            indices.Add(breakupIndice);
            //添加内环
            foreach (var item in interRing)
            {
                foreach (var coord in item.Coordinates)
                {
                    var geoDetic = new Vector2(MathExtension.ToRadius(coord.X), MathExtension.ToRadius(coord.Y));
                    positions2D.Add(geoDetic);
                    indices.Add(indicesMax++);
                }
                indices.Add(breakupIndice);
            }
            //这里计算的并不准确
            //if (SimplePolygonAlgorithms.ComputeWindingOrder(positions2D) != PolygonWindingOrder.Clockwise)
            //{
            //    positions2D.Reverse();
            //}
            positions.AddRange(positions2D.ConvertAll(i=>_shape.ToVector3(new Geodetic2D(i.X,i.Y))));
            return true;
        }

    }
}
