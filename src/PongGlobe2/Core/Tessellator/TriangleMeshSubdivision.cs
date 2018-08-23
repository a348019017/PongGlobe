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
using System.Linq;
using PongGlobe.Core.Extension;
namespace PongGlobe.Core
{
   /// <summary>
   /// 三角网细分算法，结合当前视野的平均分辨率和高程的数据作出合适的细分
   /// </summary>
    public static class TriangleMeshSubdivision
    {
        /// <summary>
        /// 返回一个mesh对象
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="indices"></param>
        /// <param name="granularity"></param>
        /// <returns></returns>
        public static Mesh<Vector3> Compute(IEnumerable<Vector3> positions, ushort[] indices, double granularity)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (indices == null)
            {
                throw new ArgumentNullException("positions");
            }

            if (indices.Count() < 3)
            {
                throw new ArgumentOutOfRangeException("indices", "At least three indices are required.");
            }

            if (indices.Count() % 3 != 0)
            {
                throw new ArgumentException("indices", "The number of indices must be divisable by three.");
            }

            if (granularity <= 0.0)
            {
                throw new ArgumentOutOfRangeException("granularity", "Granularity must be greater than zero.");
            }

            //
            // Use two queues:  one for triangles that need (or might need) to be 
            // subdivided and other for triangles that are fully subdivided.
            //
            Queue<TriangleIndicesUnsignedShort> triangles = new Queue<TriangleIndicesUnsignedShort>(indices.Count() / 3);
            Queue<TriangleIndicesUnsignedShort> done = new Queue<TriangleIndicesUnsignedShort>(indices.Count() / 3);

           
            for (int i = 0; i < indices.Count(); i += 3)
            {
                triangles.Enqueue(new TriangleIndicesUnsignedShort( indices[i], indices[i + 1], indices[i + 2] ));
            }

            //
            // New positions due to edge splits are appended to the positions list.
            //
            IList<Vector3> subdividedPositions = CollectionAlgorithms.CopyEnumerableToList(positions);

            //
            // Used to make sure shared edges are not split more than once.
            //
            Dictionary<Edge, int> edges = new Dictionary<Edge, int>();

            //
            // Subdivide triangles until we run out
            //
            while (triangles.Count != 0)
            {
                TriangleIndicesUnsignedShort triangle = triangles.Dequeue();

                Vector3 v0 = subdividedPositions[triangle.UI0];
                Vector3 v1 = subdividedPositions[triangle.UI1];
                Vector3 v2 = subdividedPositions[triangle.UI2];

                double g0 = v0.AngleBetween(v1);
                double g1 = v1.AngleBetween(v2);
                double g2 = v2.AngleBetween(v0);
             
                double max = Math.Max(g0, Math.Max(g1, g2));

                if (max > granularity)
                {
                    if (g0 == max)
                    {
                        Edge edge = new Edge(Math.Min(triangle.UI0, triangle.UI1), Math.Max(triangle.UI0, triangle.UI1));
                        int i;
                        if (!edges.TryGetValue(edge, out i))
                        {
                            subdividedPositions.Add((v0 + v1) * 0.5f);
                            i = subdividedPositions.Count - 1;
                            edges.Add(edge, i);
                        }
                        triangles.Enqueue(new TriangleIndicesUnsignedShort(triangle.UI0, (ushort)i, triangle.UI2));
                        triangles.Enqueue(new TriangleIndicesUnsignedShort((ushort)i, triangle.UI1, triangle.UI2));
                    }
                    else if (g1 == max)
                    {
                        Edge edge = new Edge(Math.Min(triangle.I1, triangle.I2), Math.Max(triangle.I1, triangle.I2));
                        int i;
                        if (!edges.TryGetValue(edge, out i))
                        {
                            subdividedPositions.Add((v1 + v2) * 0.5f);
                            i = subdividedPositions.Count - 1;
                            edges.Add(edge, i);
                        }

                        triangles.Enqueue(new TriangleIndicesUnsignedShort(triangle.UI1, (ushort)i, triangle.UI0));
                        triangles.Enqueue(new TriangleIndicesUnsignedShort((ushort)i, triangle.UI2, triangle.UI0));
                    }
                    else if (g2 == max)
                    {
                        Edge edge = new Edge(Math.Min(triangle.I2, triangle.I0), Math.Max(triangle.I2, triangle.I0));
                        int i;
                        if (!edges.TryGetValue(edge, out i))
                        {
                            subdividedPositions.Add((v2 + v0) * 0.5f);
                            i = subdividedPositions.Count - 1;
                            edges.Add(edge, i);
                        }

                        triangles.Enqueue(new TriangleIndicesUnsignedShort(triangle.UI2, (ushort)i, triangle.UI1));
                        triangles.Enqueue(new TriangleIndicesUnsignedShort((ushort)i, triangle.UI0, triangle.UI1));
                    }
                }
                else
                {
                    done.Enqueue(triangle);
                }
            }

            //
            // New indices
            //
            List<ushort> subdividedIndices = new List<ushort>(done.Count * 3);
            foreach (TriangleIndicesUnsignedShort t in done)
            {
                subdividedIndices.Add(t.UI0);
                subdividedIndices.Add(t.UI1);
                subdividedIndices.Add(t.UI2);

            }
            var mesh= new Mesh<Vector3>();
            mesh.Positions = subdividedPositions.ToArray();
            mesh.Indices = subdividedIndices.ToArray();
            return mesh;
        }
    }
}