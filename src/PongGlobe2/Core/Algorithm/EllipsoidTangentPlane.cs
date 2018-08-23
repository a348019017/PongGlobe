﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;



namespace PongGlobe.Core
{
    public class EllipsoidTangentPlane
    {
        public EllipsoidTangentPlane(Ellipsoid ellipsoid, IEnumerable<Vector3> positions)
        {
            if (ellipsoid == null)
            {
                throw new ArgumentNullException("ellipsoid");
            }

            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (!CollectionAlgorithms.EnumerableCountGreaterThanOrEqual(positions, 1))
            {
                throw new ArgumentOutOfRangeException("positions", "At least one position is required.");
            }

            AxisAlignedBoundingBox box = new AxisAlignedBoundingBox(positions);

            _origin = ellipsoid.ScaleToGeodeticSurface(box.Center);
            _normal = ellipsoid.GeodeticSurfaceNormal(_origin);
            _d = -_origin.Dot(_origin);
            _yAxis = _origin.Cross(_origin.MostOrthogonalAxis).Normalize();
            _xAxis = _yAxis.Cross(_origin).Normalize();
        }

        public ICollection<Vector2> ComputePositionsOnPlane(IEnumerable<Vector3D> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            IList<Vector2> positionsOnPlane = new List<Vector2>(CollectionAlgorithms.EnumerableCount(positions));

            foreach (Vector3 position in positions)
            {
                Vector3 intersectionPoint;

                if (IntersectionTests.TryRayPlane(Vector3.Zero, position.Normalize(), _normal, _d, out intersectionPoint))
                {
                    Vector3D v = intersectionPoint - _origin;
                    positionsOnPlane.Add(new Vector2D(_xAxis.Dot(v), _yAxis.Dot(v)));
                }
                else
                {
                    // Ray does not intersect plane
                }
            }

            return positionsOnPlane;
        }

        public Vector3D Origin { get { return _origin; } }
        public Vector3D Normal { get { return _normal; } }
        public double D { get { return _d; } }
        public Vector3D XAxis { get { return _xAxis; } }
        public Vector3D YAxis { get { return _yAxis; } }

        private Vector3D _origin;
        private Vector3D _normal;
        private double _d;
        private Vector3D _xAxis;
        private Vector3D _yAxis;
    }
}
