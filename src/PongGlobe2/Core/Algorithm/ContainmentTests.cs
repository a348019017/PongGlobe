#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion
using System.Numerics;


namespace PongGlobe.Core
{
    public static class ContainmentTests
    {
        //public static bool PointInsideTriangle(Vector2 point, Vector2 p0, Vector2 p1, Vector2 p2)
        //{
        //    //
        //    // Implementation based on http://www.blackpawn.com/texts/pointinpoly/default.html.
        //    //
        //    Vector2 v0 = (p1 - p0);
        //    Vector2 v1 = (p2 - p0);
        //    Vector2 v2 = (point - p0);

        //    double dot00 =  v0.Dot(v0);
        //    double dot01 = v0.Dot(v1);
        //    double dot02 = v0.Dot(v2);
        //    double dot11 = v1.Dot(v1);
        //    double dot12 = v1.Dot(v2);

        //    double q = 1.0 / (dot00 * dot11 - dot01 * dot01);
        //    double u = (dot11 * dot02 - dot01 * dot12) * q;
        //    double v = (dot00 * dot12 - dot01 * dot02) * q;

        //    return (u > 0) && (v > 0) && (u + v < 1);
        //}

        /// <summary>
        /// The pyramid's base points should be in counterclockwise winding order.
        /// </summary>
        public static bool PointInsideThreeSidedInfinitePyramid(
            Vector3 point,
            Vector3 pyramidApex,
            Vector3 pyramidBase0,
            Vector3 pyramidBase1,
            Vector3 pyramidBase2)
        {
            Vector3 v0 = pyramidBase0 - pyramidApex;
            Vector3 v1 = pyramidBase1 - pyramidApex;
            Vector3 v2 = pyramidBase2 - pyramidApex;

            //
            // Face normals
            //
            Vector3 n0 = Vector3.Cross(v1, v0);
            Vector3 n1 = Vector3.Cross(v2, v1);
            //v2.Cross(v1);
            Vector3 n2 = Vector3.Cross(v0, v2);
            //v0.Cross(v2);

            Vector3 planeToPoint = point - pyramidApex;

            return ((Vector3.Dot(planeToPoint,n0) < 0) && (Vector3.Dot(planeToPoint, n1) < 0) && Vector3.Dot(planeToPoint, n2) < 0);
        }
    }
}