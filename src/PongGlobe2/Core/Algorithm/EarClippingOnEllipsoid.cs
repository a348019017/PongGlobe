#region License
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
using PongGlobe.Core.Extension;

namespace PongGlobe.Core
{
    /// <summary>
    /// 多边形转换成三角网的EarClipping算法
    /// </summary>
    public static class EarClippingOnEllipsoid
    {
        /// <summary>
        /// 传入坐标串，返回索引串
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static IEnumerable<ushort> Triangulate(IEnumerable<Vector3> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }
            //
            // Doubly linked list.  This would be a tad cleaner if it were also circular.
            //
            LinkedList<IndexedVector<Vector3>> remainingPositions = new LinkedList<IndexedVector<Vector3>>(); ;

            int index = 0;
            foreach (Vector3 position in positions)
            {
                remainingPositions.AddLast(new IndexedVector<Vector3>(position, index++));
            }

            if (remainingPositions.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }

            List<ushort> indices = new List<ushort>(3 * (remainingPositions.Count - 2));

            ///////////////////////////////////////////////////////////////////

            LinkedListNode<IndexedVector<Vector3>> previousNode = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector3>> node = previousNode.Next;
            LinkedListNode<IndexedVector<Vector3>> nextNode = node.Next;

            int bailCount = remainingPositions.Count * remainingPositions.Count;

            while (remainingPositions.Count > 3)
            {
                Vector3 p0 = previousNode.Value.Vector;
                Vector3 p1 = node.Value.Vector;
                Vector3 p2 = nextNode.Value.Vector;

                if (IsTipConvex(p0, p1, p2))
                {
                    bool isEar = true;
                    for (LinkedListNode<IndexedVector<Vector3>> n = ((nextNode.Next != null) ? nextNode.Next : remainingPositions.First);
                        n != previousNode;
                        n = ((n.Next != null) ? n.Next : remainingPositions.First))
                    {
                        if (ContainmentTests.PointInsideThreeSidedInfinitePyramid(n.Value.Vector, Vector3.Zero, p0, p1, p2))
                        {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar)
                    {
                        ///
                        indices.Add((ushort)previousNode.Value.Index);
                        indices.Add((ushort)node.Value.Index);
                        indices.Add((ushort)nextNode.Value.Index);
                        //indices.AddTriangle(new TriangleIndicesUnsignedInt(previousNode.Value.Index, node.Value.Index, nextNode.Value.Index));
                        remainingPositions.Remove(node);

                        node = nextNode;
                        nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;
                        continue;
                    }
                }

                previousNode = (previousNode.Next != null) ? previousNode.Next : remainingPositions.First;
                node = (node.Next != null) ? node.Next : remainingPositions.First;
                nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;

                if (--bailCount == 0)
                {
                    break;
                }
            }

            LinkedListNode<IndexedVector<Vector3>> n0 = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector3>> n1 = n0.Next;
            LinkedListNode<IndexedVector<Vector3>> n2 = n1.Next;
            //indices.AddTriangle(new TriangleIndicesUnsignedInt(n0.Value.Index, n1.Value.Index, n2.Value.Index));
            indices.Add((ushort)n0.Value.Index);
            indices.Add((ushort)n1.Value.Index);
            indices.Add((ushort)n2.Value.Index);
            return indices;
        }


        public static IEnumerable<ushort> Triangulate2D(IEnumerable<Vector2> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }
            //
            // Doubly linked list.  This would be a tad cleaner if it were also circular.
            //
            LinkedList<IndexedVector<Vector2>> remainingPositions = new LinkedList<IndexedVector<Vector2>>(); ;

            int index = 0;
            foreach (Vector2 position in positions)
            {
                remainingPositions.AddLast(new IndexedVector<Vector2>(position, index++));
            }

            if (remainingPositions.Count < 3)
            {
                throw new ArgumentOutOfRangeException("positions", "At least three positions are required.");
            }

            List<ushort> indices = new List<ushort>(3 * (remainingPositions.Count - 2));

            ///////////////////////////////////////////////////////////////////

            LinkedListNode<IndexedVector<Vector2>> previousNode = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector2>> node = previousNode.Next;
            LinkedListNode<IndexedVector<Vector2>> nextNode = node.Next;

            int bailCount = remainingPositions.Count * remainingPositions.Count;

            while (remainingPositions.Count > 3)
            {
                Vector2 p0 = previousNode.Value.Vector;
                Vector2 p1 = node.Value.Vector;
                Vector2 p2 = nextNode.Value.Vector;

                if (IsTipConvex(p0, p1, p2))
                {
                    bool isEar = true;
                    for (LinkedListNode<IndexedVector<Vector2>> n = ((nextNode.Next != null) ? nextNode.Next : remainingPositions.First);
                        n != previousNode;
                        n = ((n.Next != null) ? n.Next : remainingPositions.First))
                    {
                        if (TrangleContainPoint( p0, p1, p2,n.Value.Vector))
                        {
                            isEar = false;
                            break;
                        }
                    }
                    if (isEar)
                    {
                        ///
                        indices.Add((ushort)previousNode.Value.Index);
                        indices.Add((ushort)node.Value.Index);
                        indices.Add((ushort)nextNode.Value.Index);
                        //indices.AddTriangle(new TriangleIndicesUnsignedInt(previousNode.Value.Index, node.Value.Index, nextNode.Value.Index));
                        remainingPositions.Remove(node);

                        node = nextNode;
                        nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;
                        continue;
                    }
                }

                previousNode = (previousNode.Next != null) ? previousNode.Next : remainingPositions.First;
                node = (node.Next != null) ? node.Next : remainingPositions.First;
                nextNode = (nextNode.Next != null) ? nextNode.Next : remainingPositions.First;

                if (--bailCount == 0)
                {
                    break;
                }
            }

            LinkedListNode<IndexedVector<Vector2>> n0 = remainingPositions.First;
            LinkedListNode<IndexedVector<Vector2>> n1 = n0.Next;
            LinkedListNode<IndexedVector<Vector2>> n2 = n1.Next;
            //indices.AddTriangle(new TriangleIndicesUnsignedInt(n0.Value.Index, n1.Value.Index, n2.Value.Index));
            indices.Add((ushort)n0.Value.Index);
            indices.Add((ushort)n1.Value.Index);
            indices.Add((ushort)n2.Value.Index);
            return indices;
        }


        /// <summary>
        /// 计算凹凸性
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static bool IsTipConvex(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 u = p1 - p0;
            Vector3 v = p2 - p1;

            return Vector3.Dot(Vector3.Cross(u, v),p1) >= 0.0; 
        }

        /// <summary>
        /// 点是否在三角形中，vector2的计算,无论p0123是顺逆时针
        /// </summary>
        /// <returns></returns>
        private static bool TrangleContainPoint(Vector2 p0, Vector2 p1, Vector2 p2,Vector2 p)
        {
            Vector2 p0p =Vector2.Normalize(p - p0);
            Vector2 p1p =Vector2.Normalize(p - p1);
            Vector2 p2p =Vector2.Normalize(p - p2);
            double t1 = p0p.Cross(p1p);
            double t2 = p1p.Cross(p2p);
            double t3 = p2p.Cross(p0p);
            return t1 * t2 >= 0 && t1* t3 >= 0;
        }

        private static bool IsTipConvex(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Vector2 u = Vector2.Normalize(p1 - p0);
            Vector2 v =Vector2.Normalize(p2 - p1);
            ///大于0为逆时针，小于0为顺势整，如果与多边形相同则为凸多边形，这里默认多边形为顺时针
            return u.Cross(v) <= 0;
        }
    }
}