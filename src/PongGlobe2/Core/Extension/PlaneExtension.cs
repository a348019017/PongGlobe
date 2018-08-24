using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core.Extension
{
    /// <summary>
    /// 临时的扩展，如果与system.numberic.vector有较大的出入则考虑彻底替换为xna的库
    /// </summary>
    public static class PlaneExtension
    {
        public static PlaneIntersectionType Intersects(this Plane plane,ref Vector3 point)
        {
            float distance;
            plane.DotCoordinate(ref point, out distance);

            if (distance > 0)
                return PlaneIntersectionType.Front;

            if (distance < 0)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        public static void DotCoordinate(this Plane plane, ref Vector3 value, out float result)
        {
            result = (((plane.Normal.X * value.X) + (plane.Normal.Y * value.Y)) + (plane.Normal.Z * value.Z)) + plane.D;
        }

    }
}
