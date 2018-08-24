using System;
using System.Collections.Generic;
using System.Text;

namespace PongGlobe.Core
{
    /// <summary>
    ///  定义平面和外包盒之间的关系，在盒子前面，在盒子后面，有相交
    /// </summary>
    public enum PlaneIntersectionType
    {
        /// <summary>
        /// There is no intersection, the bounding volume is in the negative half space of the plane.
        /// </summary>
        Front,
        /// <summary>
        /// There is no intersection, the bounding volume is in the positive half space of the plane.
        /// </summary>
        Back,
        /// <summary>
        /// The plane is intersected.
        /// </summary>
        Intersecting
    }
}
