using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
namespace PongGlobe.Core.Algorithm
{
    /// <summary>
    /// Hit到哪个对象且距离对象有多远
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RayCastHit<T>
    {
        public readonly T Item;
        public readonly Vector3 Location;
        public readonly float Distance;

        public RayCastHit(T item, Vector3 location, float distance)
        {
            Item = item;
            Location = location;
            Distance = distance;
        }
    }
}
