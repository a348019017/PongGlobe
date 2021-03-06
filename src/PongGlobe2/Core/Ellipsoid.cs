﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Globalization;
using System.Collections.Generic;
using Veldrid;
using System.Numerics;
using PongGlobe.Scene;
using PongGlobe.Core.Extension;
namespace PongGlobe.Core
{
    /// <summary>
    /// 描述椭球体,注意世界坐标系的定位这里为上Y，右X，面向为Z
    /// </summary>
    public class Ellipsoid
    {
        public static readonly Ellipsoid Wgs84 = new Ellipsoid(6378137.0f, 6356752.314245f, 6378137.0f );
        public static readonly Ellipsoid ScaledWgs84 = new Ellipsoid(1.0f, 1.0f, 6356752.314245f / 6378137.0f);
        public static readonly Ellipsoid ScaledWgs842 = new Ellipsoid(1.0f, 6356752.314245f / 6378137.0f, 1.0f );
        public static readonly Ellipsoid UnitSphere = new Ellipsoid(1.0f, 1.0f, 1.0f);

        public Ellipsoid(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        public Ellipsoid(Vector3 radii)
        {
            if ((radii.X <= 0.0) || (radii.Y <= 0.0) || (radii.Z <= 0.0))
            {
                throw new ArgumentOutOfRangeException("radii");
            }

            _radii = radii;
            _radiiSquared = new Vector3(
                radii.X * radii.X,
                radii.Y * radii.Y,
                radii.Z * radii.Z);
            _radiiToTheFourth = new Vector3(
                _radiiSquared.X * _radiiSquared.X,
                _radiiSquared.Y * _radiiSquared.Y,
                _radiiSquared.Z * _radiiSquared.Z);
            _oneOverRadiiSquared = new Vector3(
                (float)(1.0 / (radii.X * radii.X)), 
                (float)(1.0 / (radii.Y * radii.Y)), 
                (float)(1.0 / (radii.Z * radii.Z)));
        }

        public static Vector3 CentricSurfaceNormal(Vector3 positionOnEllipsoid)
        {
            return Vector3.Normalize(positionOnEllipsoid);
            //return positionOnEllipsoid.nor();
        }

        public Vector3 GeodeticSurfaceNormal(Vector3 positionOnEllipsoid)
        {            
            return Vector3.Normalize(positionOnEllipsoid*_oneOverRadiiSquared);
        }

        public Vector3 GeodeticSurfaceNormal(Geodetic3D geodetic)
        {
            double cosLatitude = Math.Cos(geodetic.Latitude);

            return new Vector3(                
                (float)(cosLatitude * Math.Sin(geodetic.Longitude)),
                (float)(Math.Sin(geodetic.Latitude)),
                (float)(cosLatitude * Math.Cos(geodetic.Longitude)));
        }

        public Vector3 Radii 
        {
            get { return _radii; }
        }

        public Vector3 RadiiSquared
        {
            get { return _radiiSquared; }
        }

        public Vector3 OneOverRadiiSquared
        {
            get { return _oneOverRadiiSquared; }
        }

        public double MinimumRadius
        {
            get { return Math.Min(_radii.X, Math.Min(_radii.Y, _radii.Z)); }
        }

        public double MaximumRadius
        {
            get { return Math.Max(_radii.X, Math.Max(_radii.Y, _radii.Z)); }
        }

        public double[] Intersections(Vector3 origin, Vector3 direction)
        {
            var dirNormal = Vector3.Normalize(direction);

            // By laborious algebraic manipulation....
            double a = dirNormal.X * dirNormal.X * _oneOverRadiiSquared.X +
                       dirNormal.Y * dirNormal.Y * _oneOverRadiiSquared.Y +
                       dirNormal.Z * dirNormal.Z * _oneOverRadiiSquared.Z;
            double b = 2.0 *
                       (origin.X * dirNormal.X * _oneOverRadiiSquared.X +
                        origin.Y * dirNormal.Y * _oneOverRadiiSquared.Y +
                        origin.Z * dirNormal.Z * _oneOverRadiiSquared.Z);
            double c = origin.X * origin.X * _oneOverRadiiSquared.X +
                       origin.Y * origin.Y * _oneOverRadiiSquared.Y +
                       origin.Z * origin.Z * _oneOverRadiiSquared.Z - 1.0;

            // Solve the quadratic equation: ax^2 + bx + c = 0.
            // Algorithm is from Wikipedia's "Quadratic equation" topic, and Wikipedia credits
            // Numerical Recipes in C, section 5.6: "Quadratic and Cubic Equations"
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0.0)
            {
                // no intersections
                return new double[0]; 
            }
            else if (discriminant == 0.0)
            {
                // one intersection at a tangent point
                return new double[1] { -0.5 * b / a };
            }

            double t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * Math.Sqrt(discriminant));
            double t1 = -0.5 * b / a;
            double deta = -0.5 * Math.Sqrt(discriminant) / a;
            double root1 = t / a;
            double root2 = c / t;
            double root11 = t1 + deta;
            double root22 = t1 - deta;

            // Two intersections - return the smallest first.
            if (root1 < root2)
            {
                return new double[2] { root1, root2 };
            }
            else
            {
                return new double[2] { root2, root1 };
            }
        }

        /// <summary>
        /// 返回两个相交的地理坐标中的靠近eye的一方
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool Intersections(Ray ray,out Geodetic2D geo)
        {
            geo = default(Geodetic2D);
            var result = Intersections(ray.Position, ray.Direction);
            if (result == null||result.Length==0) return false;
            Vector3 position = ray.Position + ((float)result[0] * Vector3.Normalize(ray.Direction));
            geo=ToGeodetic2D(position);
            return true;
        }

        public Vector3 ToVector3(Geodetic2D geodetic)
        {
            return ToVector3(new Geodetic3D(geodetic.Longitude, geodetic.Latitude, 0.0));
        }

        

        public Vector3 ToVector3(Geodetic3D geodetic)
        {
            Vector3 n = GeodeticSurfaceNormal(geodetic);
            Vector3 k = _radiiSquared*n;
            float gamma = (float)Math.Sqrt(
                (k.X * n.X) +
                (k.Y * n.Y) +
                (k.Z * n.Z));

            Vector3 rSurface = k / gamma;
            return rSurface + ((float)geodetic.Height * n);
        }

        public ICollection<Geodetic3D> ToGeodetic3D(IEnumerable<Vector3> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Geodetic3D> geodetics = new List<Geodetic3D>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3 position in positions)
            {
                geodetics.Add(ToGeodetic3D(position));
            }

            return geodetics;
        }

        public ICollection<Geodetic2D> ToGeodetic2D(IEnumerable<Vector3> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Geodetic2D> geodetics = new List<Geodetic2D>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3 position in positions)
            {
                geodetics.Add(ToGeodetic2D(position));
            }
            
            return geodetics;
        }

        public Geodetic2D ToGeodetic2D(Vector3 positionOnEllipsoid)
        {
            Vector3 n = GeodeticSurfaceNormal(positionOnEllipsoid);
            return new Geodetic2D(
                Math.Atan2(n.X,n.Z),
                Math.Asin(n.Y / n.Length()));
        }

        public Geodetic3D ToGeodetic3D(Vector3 position)
        {
            Vector3 p = ScaleToGeodeticSurface(position);
            Vector3 h = position - p;
            double height = Math.Sign(Vector3.Dot( h,position)) * h.Length();
            return new Geodetic3D(ToGeodetic2D(p), height);
        }

        public Vector3 ScaleToGeodeticSurface(Vector3 position)
        {
            float beta = (float)(1.0 / Math.Sqrt(
                (position.X * position.X) * _oneOverRadiiSquared.X +
                (position.Y * position.Y) * _oneOverRadiiSquared.Y +
                (position.Z * position.Z) * _oneOverRadiiSquared.Z));
            double n = new Vector3(
                beta * position.X * _oneOverRadiiSquared.X,
                beta * position.Y * _oneOverRadiiSquared.Y,
                beta * position.Z * _oneOverRadiiSquared.Z).Length();
            double alpha = (1.0 - beta) * (position.Length() / n);

            double x2 = position.X * position.X;
            double y2 = position.Y * position.Y;
            double z2 = position.Z * position.Z;

            double da = 0.0;
            double db = 0.0;
            double dc = 0.0;

            double s = 0.0;
            double dSdA = 1.0;

            do
            {
                alpha -= (s / dSdA);

                da = 1.0 + (alpha * _oneOverRadiiSquared.X);
                db = 1.0 + (alpha * _oneOverRadiiSquared.Y);
                dc = 1.0 + (alpha * _oneOverRadiiSquared.Z);

                double da2 = da * da;
                double db2 = db * db;
                double dc2 = dc * dc;

                double da3 = da * da2;
                double db3 = db * db2;
                double dc3 = dc * dc2;

                s = x2 / (_radiiSquared.X * da2) +
                    y2 / (_radiiSquared.Y * db2) +
                    z2 / (_radiiSquared.Z * dc2) - 1.0;

                dSdA = -2.0 *
                    (x2 / (_radiiToTheFourth.X * da3) +
                     y2 / (_radiiToTheFourth.Y * db3) +
                     z2 / (_radiiToTheFourth.Z * dc3));
            }
            while (Math.Abs(s) > 1e-10);

            return new Vector3(
                (float)(position.X / da),
                (float)(position.Y / db),
                (float)(position.Z / dc));
        }

        public Vector3 ScaleToGeocentricSurface(Vector3 position)
        {
            float beta = (float)(1.0 / Math.Sqrt(
                (position.X * position.X) * _oneOverRadiiSquared.X +
                (position.Y * position.Y) * _oneOverRadiiSquared.Y +
                (position.Z * position.Z) * _oneOverRadiiSquared.Z));

            return beta * position;
        }

        public IList<Vector3> ComputeCurve(
            Vector3 start, 
            Vector3 stop, 
            double granularity)
        {
            if (granularity <= 0.0)
            {
                throw new ArgumentOutOfRangeException("granularity", "Granularity must be greater than zero.");
            }

            Vector3 normal =Vector3.Normalize(Vector3.Cross( start,stop));
            double theta = start.AngleBetween(stop);
            int n = Math.Max((int)(theta / granularity) - 1, 0);
            
            List<Vector3> positions = new List<Vector3>(2 + n);

            positions.Add(start);

            for (int i = 1; i <= n; ++i)
            {
                double phi = (i * granularity);

                positions.Add(ScaleToGeocentricSurface(start.RotateAroundAxis(normal, phi)));
            }

            positions.Add(stop);

            return positions;
        }


        /// <summary>
        /// 根据地理坐标生成,此点的变换矩阵
        /// </summary>
        /// <param name="geoPos"></param>
        /// <returns></returns>
        public Matrix4x4 geographicToCartesianTransform(Geodetic3D geoPos)
        {
            //平移加旋转的矩阵,注意是逆时针和经纬度之间的关系，维度是顺时针也即是负数
            var rotate = Matrix4x4.CreateFromYawPitchRoll((float)geoPos.Longitude,-(float)geoPos.Latitude,0);
            var vecTranslate = this.ToVector3(geoPos);
            //创建一个平移矩阵
            var translate = Matrix4x4.CreateTranslation(vecTranslate);
            var transform = rotate * translate;         
            return transform;
        }

        /// <summary>
        /// 这个变换结果可以认为是准确的，等同于B参考系到A参考系
        /// </summary>
        /// <param name="geoPos"></param>
        /// <returns></returns>
        public Matrix4x4 GeographicToCartesianTransform2(Geodetic3D geoPos)
        {
            //计算此经纬度坐标的世界坐标系
            var vectorCoord = this.ToVector3(geoPos);
            //计算east法向量
            var east = Vector3.Normalize(new Vector3(vectorCoord.Z, 0, -vectorCoord.X));
            //计算此点的法向量,垂直于椭球体切平面
            var normalUp = this.GeodeticSurfaceNormal(vectorCoord);
            //计算north的法向量,使用叉乘,这里似乎是符合右手定则
            var north = Vector3.Cross(normalUp, east);
            //构造一个矩阵
            var transofrmLoacl = new Matrix4x4(east.X, east.Y, east.Z, 0, north.X, north.Y, north.Z, 0, normalUp.X, normalUp.Y, normalUp.Z, 0, vectorCoord.X, vectorCoord.Y, vectorCoord.Z, 1);
            return transofrmLoacl;
        }

        public Matrix4x4 GeographicToCartesianTransform3(Geodetic3D geoPos)
        {
            //计算此经纬度坐标的世界坐标系
            var vectorCoord = this.ToVector3(geoPos);
            //计算east法向量
            var east = Vector3.Normalize(new Vector3(vectorCoord.Z, 0, -vectorCoord.X));
            //计算此点的法向量,垂直于椭球体切平面
            var normalUp = this.GeodeticSurfaceNormal(vectorCoord);
            //计算north的法向量,使用叉乘,这里似乎是符合右手定则
            var north = Vector3.Cross(normalUp, east);
            //构造一个矩阵
            var transofrmLoacl = new Matrix4x4(east.X, north.X, normalUp.X, 0, east.Y, north.Y, normalUp.Y, 0, east.Z, north.Z, normalUp.Z, 0, vectorCoord.X, vectorCoord.Y, vectorCoord.Z, 1);
            return transofrmLoacl;
        }







        private readonly Vector3 _radii;
        private readonly Vector3 _radiiSquared;
        private readonly Vector3 _radiiToTheFourth;
        private readonly Vector3 _oneOverRadiiSquared;
    }



    public class EllipsoidTest
    {
        public static bool TestGeoToCartesianTransform()
        {
            //在经度30度，维度30度，高程为1 的情况下，
            //vector(0,0,1)的实际坐标为
            var geoCooord = new Geodetic3D(Math.PI / 6, Math.PI / 6, 1);
            var shape = Ellipsoid.ScaledWgs842;
            var resultCoord = shape.ToVector3(geoCooord);

            var transfrom = shape.GeographicToCartesianTransform2(new Geodetic3D(Math.PI / 6, Math.PI / 6, 1));
            var transfrom2 = shape.geographicToCartesianTransform(new Geodetic3D(Math.PI / 6, Math.PI / 6, 1));
           

            var result2= Vector3.Transform(new Vector3(1, 1, 0), transfrom);
            //
            var result3 = Vector3.Transform(new Vector3(1, 1, 0), transfrom2);

            //计算次向量的长度
            var length = resultCoord.LengthSquared() + 2.0;
            var length1 = result2.LengthSquared();
            var length2 = result3.LengthSquared();



            
            //var eq
            //此结果略有误差，尝试分析误差来源
            return resultCoord.Equals(result2);
        }
    }

}
