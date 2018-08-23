using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace PongGlobe.Core.Algorithm
{
    public static class SimplePolygonAlgorithms
    {
        /// <summary>
        /// Cleans up a simple polygon by removing duplicate adjacent positions and making
        /// the first position not equal the last position
        /// 仅仅只是移除邻近重复点
        /// </summary>
        public static IList<T> Cleanup<T>(IEnumerable<T> positions)
        {
            IList<T> positionsList = CollectionAlgorithms.EnumerableToList(positions);

            if (positionsList.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }

            List<T> cleanedPositions = new List<T>(positionsList.Count);

            for (int i0 = positionsList.Count - 1, i1 = 0; i1 < positionsList.Count; i0 = i1++)
            {
                T v0 = positionsList[i0];
                T v1 = positionsList[i1];

                if (!v0.Equals(v1))
                {
                    cleanedPositions.Add(v1);
                }
            }
            cleanedPositions.TrimExcess();
            return cleanedPositions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static double ComputeArea(IEnumerable<Vector2> positions)
        {
            IList<Vector2> positionsList = CollectionAlgorithms.EnumerableToList(positions);

            if (positionsList.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }

            double area = 0.0;

            //
            // Compute the area of the polygon.  The sign of the area determines the winding order.
            //
            for (int i0 = positionsList.Count - 1, i1 = 0; i1 < positionsList.Count; i0 = i1++)
            {
                Vector2 v0 = positionsList[i0];
                Vector2 v1 = positionsList[i1];

                area += (v0.X * v1.Y) - (v1.X * v0.Y);
            }

            return area * 0.5;
        }

        public static PolygonWindingOrder ComputeWindingOrder(IEnumerable<Vector2> positions)
        {
            return (ComputeArea(positions) >= 0.0) ? PolygonWindingOrder.Counterclockwise : PolygonWindingOrder.Clockwise;
        }
    }


    public enum PolygonWindingOrder
    {
        Clockwise,
        Counterclockwise
    }
}
