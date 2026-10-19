using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace AiWpf3D
{
    public static class WpfMeshAdvancedHoleFiller
    {
        /// <summary>
        /// Finds and patches both complex convex and concave holes inside a WPF MeshGeometry3D.
        /// </summary>
        public static MeshGeometry3D FillAllMeshHoles(MeshGeometry3D inputMesh)
        {
            if (inputMesh == null)
            {
                throw new ArgumentNullException(nameof(inputMesh));
            }

            MeshGeometry3D patchedMesh = new MeshGeometry3D
            {
                Positions = inputMesh.Positions.Clone(),
                TriangleIndices = inputMesh.TriangleIndices.Clone()
            };

            if (inputMesh.Normals != null && inputMesh.Normals.Count == inputMesh.Positions.Count)
            {
                patchedMesh.Normals = inputMesh.Normals.Clone();
            }

            if (inputMesh.TextureCoordinates != null && inputMesh.TextureCoordinates.Count == inputMesh.Positions.Count)
            {
                patchedMesh.TextureCoordinates = inputMesh.TextureCoordinates.Clone();
            }

            // Step 1: Detect raw topology edge configurations
            Dictionary<Edge, List<Edge>> edgeOccurrences = new Dictionary<Edge, List<Edge>>();
            for (int i = 0; i < patchedMesh.TriangleIndices.Count; i += 3)
            {
                int v1 = patchedMesh.TriangleIndices[i];
                int v2 = patchedMesh.TriangleIndices[i + 1];
                int v3 = patchedMesh.TriangleIndices[i + 2];

                Edge[] triEdges = { new Edge(v1, v2), new Edge(v2, v3), new Edge(v3, v1) };
                foreach (var edge in triEdges)
                {
                    if (!edgeOccurrences.TryGetValue(edge, out var list))
                    {
                        list = new List<Edge>();
                        edgeOccurrences[edge] = list;
                    }
                    list.Add(edge);
                }
            }

            // Step 2: Extract individual boundary edges
            List<Edge> boundaryEdges = new List<Edge>();
            foreach (var pair in edgeOccurrences)
            {
                if (pair.Value.Count == 1)
                {
                    boundaryEdges.Add(pair.Value[0]);
                }
            }

            if (boundaryEdges.Count == 0)
            {
                return patchedMesh;
            }

            // Step 3: Link items sequentially into clean loops
            List<List<int>> holeLoops = FindHoleLoops(boundaryEdges);

            // Step 4: Triangulate loops using Ear Clipping projected onto a 2D Best-Fit plane
            foreach (List<int> loop in holeLoops)
            {
                if (loop.Count < 3) continue;

                // Calculate average face normal around the hole to define projection axes
                Vector3D avgNormal = CalculateLoopNormal(loop, patchedMesh.Positions);
                GetProjectionAxes(avgNormal, out Vector3D axisX, out Vector3D axisY);

                // Project 3D positions into a 2D polygon track
                List<System.Windows.Point> poly2D = new List<System.Windows.Point>();
                foreach (int index in loop)
                {
                    Point3D p3d = patchedMesh.Positions[index];
                    poly2D.Add(new System.Windows.Point(Vector3D.DotProduct((Vector3D)p3d, axisX), Vector3D.DotProduct((Vector3D)p3d, axisY)));
                }

                // Execute Ear Clipping
                List<int> localIndices = new List<int>();
                for (int i = 0; i < loop.Count; i++)
                {
                    localIndices.Add(i);
                }

                // Ensure our 2D winding orientation calculation loops correctly
                bool isCcw = IsPolygonClockwise(poly2D);

                while (localIndices.Count >= 3)
                {
                    bool earFound = false;

                    for (int i = 0; i < localIndices.Count; i++)
                    {
                        int prev = localIndices[(i - 1 + localIndices.Count) % localIndices.Count];
                        int curr = localIndices[i];
                        int next = localIndices[(i + 1) % localIndices.Count];

                        if (IsEar(prev, curr, next, localIndices, poly2D, isCcw))
                        {
                            // Add the newly isolated triangle to the global index buffer
                            patchedMesh.TriangleIndices.Add(loop[prev]);
                            patchedMesh.TriangleIndices.Add(loop[curr]);
                            patchedMesh.TriangleIndices.Add(loop[next]);

                            localIndices.RemoveAt(i);
                            earFound = true;
                            break;
                        }
                    }

                    if (!earFound)
                    {
                        // Fallback configuration if polygon loops are too twisted or self-intersecting
                        int anchor = localIndices[0];
                        for (int i = 1; i < localIndices.Count - 1; i++)
                        {
                            patchedMesh.TriangleIndices.Add(loop[anchor]);
                            patchedMesh.TriangleIndices.Add(loop[localIndices[i]]);
                            patchedMesh.TriangleIndices.Add(loop[localIndices[i + 1]]);
                        }
                        break;
                    }
                }
            }

            return patchedMesh;
        }

        private static Vector3D CalculateLoopNormal(List<int> loop, Point3DCollection positions)
        {
            Vector3D normal = new Vector3D(0, 0, 0);
            for (int i = 0; i < loop.Count; i++)
            {
                Point3D p1 = positions[loop[i]];
                Point3D p2 = positions[loop[(i + 1) % loop.Count]];
                normal.X += (p1.Y - p2.Y) * (p1.Z + p2.Z);
                normal.Y += (p1.Z - p2.Z) * (p1.X + p2.X);
                normal.Z += (p1.X - p2.X) * (p1.Y + p2.Y);
            }
            if (normal.LengthSquared > 1e-6)
            {
                normal.Normalize();
            }

            return normal;
        }

        private static List<List<int>> FindHoleLoops(List<Edge> looseEdges)
        {
            List<List<int>> loops = new List<List<int>>();
            HashSet<Edge> visitedEdges = new HashSet<Edge>();
            Dictionary<int, Edge> nextEdgeLookup = new Dictionary<int, Edge>();

            foreach (var edge in looseEdges)
            {
                nextEdgeLookup[edge.V1] = edge;
            }

            foreach (var startEdge in looseEdges)
            {
                if (visitedEdges.Contains(startEdge)) continue;

                List<int> currentLoop = new List<int>();
                Edge currentEdge = startEdge;

                while (!visitedEdges.Contains(currentEdge))
                {
                    visitedEdges.Add(currentEdge);
                    currentLoop.Add(currentEdge.V1);

                    if (nextEdgeLookup.TryGetValue(currentEdge.V2, out Edge nextEdge))
                    {
                        currentEdge = nextEdge;
                        if (currentEdge.V1 == startEdge.V1)
                        {
                            break;
                        }
                    }
                    else break;
                }

                if (currentLoop.Count >= 3)
                {
                    loops.Add(currentLoop);
                }
            }
            return loops;
        }

        private static void GetProjectionAxes(Vector3D normal, out Vector3D axisX, out Vector3D axisY)
        {
            axisX = Math.Abs(normal.X) < 0.9 ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);
            axisY = Vector3D.CrossProduct(normal, axisX);
            axisY.Normalize();
            axisX = Vector3D.CrossProduct(axisY, normal);
            axisX.Normalize();
        }

        private static bool IsEar(int prev, int curr, int next, List<int> indices, List<System.Windows.Point> poly, bool isCcw)
        {
            System.Windows.Point a = poly[prev];
            System.Windows.Point b = poly[curr];
            System.Windows.Point c = poly[next];

            // Check if the vertex forms a reflex angle (pointing inwards for a concave turn)
            double crossProduct = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            if (isCcw && crossProduct <= 0)
            {
                return false;
            }

            if (!isCcw && crossProduct >= 0)
            {
                return false;
            }

            // Ensure no other remaining polygon points lie inside this candidate triangle patch
            foreach (int index in indices)
            {
                if (index == prev || index == curr || index == next)
                {
                    continue;
                }

                if (PointInTriangle2D(poly[index], a, b, c))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPolygonClockwise(List<System.Windows.Point> poly)
        {
            double area = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                var p1 = poly[i];
                var p2 = poly[(i + 1) % poly.Count];
                area += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            return area > 0;
        }

        private static bool PointInTriangle2D(System.Windows.Point p, System.Windows.Point a, System.Windows.Point b, System.Windows.Point c)
        {
            double d1 = Sign(p, a, b);
            double d2 = Sign(p, b, c);
            double d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private static double Sign(System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private struct Edge : IEquatable<Edge>
        {
            public readonly int V1;
            public readonly int V2;

            public Edge(int v1, int v2)
            {
                V1 = v1; V2 = v2;
            }

            public bool Equals(Edge other) => (V1 == other.V1 && V2 == other.V2) || (V1 == other.V2 && V2 == other.V1);

            public override bool Equals(object obj) => obj is Edge other && Equals(other);

            public override int GetHashCode() => Math.Min(V1, V2).GetHashCode() ^ Math.Max(V1, V2).GetHashCode();
        }
    }
}