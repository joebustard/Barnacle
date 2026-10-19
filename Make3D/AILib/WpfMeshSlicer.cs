using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AiWpf3D
{
    public static class WpfMeshSlicer
    {
        private const double Epsilon = 1e-7;

        /// <summary>
        /// Evaluates mesh intersections, slices conflicting faces along their plane crossings,
        /// and replaces them with a cleanly split set of smaller triangles.
        /// </summary>
        public static MeshGeometry3D RemoveIntersectingFaces(MeshGeometry3D inputMesh)
        {
            if (inputMesh == null)
            {
                throw new ArgumentNullException(nameof(inputMesh));
            }

            List<SlicedTriangle> workingTriangles = new List<SlicedTriangle>();
            for (int i = 0; i < inputMesh.TriangleIndices.Count; i += 3)
            {
                workingTriangles.Add(new SlicedTriangle(
                    inputMesh.Positions[inputMesh.TriangleIndices[i]],
                    inputMesh.Positions[inputMesh.TriangleIndices[i + 1]],
                    inputMesh.Positions[inputMesh.TriangleIndices[i + 2]]
                ));
            }

            bool alterationsMade = true;
            int passes = 0;

            // Iteratively slice intersecting pairs until all geometric overlap loops clear out
            while (alterationsMade && passes < 3)
            {
                alterationsMade = false;
                passes++;

                SpatialNode root = SpatialNode.Build(workingTriangles);
                List<(SlicedTriangle T1, SlicedTriangle T2)> intersectionPairs = new List<(SlicedTriangle, SlicedTriangle)>();
                root.GetIntersectionPairs(root, intersectionPairs);

                HashSet<SlicedTriangle> toRemove = new HashSet<SlicedTriangle>();
                List<SlicedTriangle> toAdd = new List<SlicedTriangle>();

                foreach ((SlicedTriangle T1, SlicedTriangle T2) pair in intersectionPairs)
                {
                    if (toRemove.Contains(pair.T1) || toRemove.Contains(pair.T2))
                    {
                        continue;
                    }

                    if (TryIntersectAndSlice(pair.T1, pair.T2, out var fragments1, out var fragments2))
                    {
                        toRemove.Add(pair.T1);
                        toRemove.Add(pair.T2);
                        toAdd.AddRange(fragments1);
                        toAdd.AddRange(fragments2);
                        alterationsMade = true;
                    }
                }

                if (alterationsMade)
                {
                    workingTriangles.RemoveAll(t => toRemove.Contains(t));
                    workingTriangles.AddRange(toAdd);
                }
            }

            // Rebuild clean WPF MeshGeometry3D data
            MeshGeometry3D outputMesh = new MeshGeometry3D();
            Point3DCollection cleanPositions = new Point3DCollection();
            Int32Collection cleanIndices = new Int32Collection();
            Dictionary<string, int> vertexCache = new Dictionary<string, int>();

            foreach (SlicedTriangle tri in workingTriangles)
            {
                int i1 = GetOrAddVertex(tri.P1, cleanPositions, vertexCache);
                int i2 = GetOrAddVertex(tri.P2, cleanPositions, vertexCache);
                int i3 = GetOrAddVertex(tri.P3, cleanPositions, vertexCache);

                if (i1 == i2 || i2 == i3 || i3 == i1)
                {
                    continue; // Skip degenerates
                }

                cleanIndices.Add(i1);
                cleanIndices.Add(i2);
                cleanIndices.Add(i3);
            }

            outputMesh.Positions = cleanPositions;
            outputMesh.TriangleIndices = cleanIndices;
            return outputMesh;
        }

        private static int GetOrAddVertex(Point3D p, Point3DCollection list, Dictionary<string, int> cache)
        {
            string key = $"{Math.Round(p.X, 5)}_{Math.Round(p.Y, 5)}_{Math.Round(p.Z, 5)}";
            if (cache.TryGetValue(key, out int index))
            {
                return index;
            }

            index = list.Count;
            list.Add(p);
            cache[key] = index;
            return index;
        }

        private static List<SlicedTriangle> SliceTriangleByPlane(SlicedTriangle tri, Vector3D normal, double d)
        {
            List<SlicedTriangle> result = new List<SlicedTriangle>();
            Point3D[] pts = { tri.P1, tri.P2, tri.P3 };
            double[] dists = {
            Vector3D.DotProduct(normal, (Vector3D)tri.P1) + d,
            Vector3D.DotProduct(normal, (Vector3D)tri.P2) + d,
            Vector3D.DotProduct(normal, (Vector3D)tri.P3) + d
        };

            List<Point3D> sideA = new List<Point3D>();
            List<Point3D> sideB = new List<Point3D>();

            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;
                if (dists[i] >= 0) sideA.Add(pts[i]);
                else sideB.Add(pts[i]);

                // Edge crosses the intersection boundary plane
                if ((dists[i] > 0 && dists[next] < 0) || (dists[i] < 0 && dists[next] > 0))
                {
                    double t = dists[i] / (dists[i] - dists[next]);
                    Point3D intersectPt = new Point3D(
                        pts[i].X + t * (pts[next].X - pts[i].X),
                        pts[i].Y + t * (pts[next].Y - pts[i].Y),
                        pts[i].Z + t * (pts[next].Z - pts[i].Z)
                    );
                    sideA.Add(intersectPt);
                    sideB.Add(intersectPt);
                }
            }
            // Triangulate resulting sub-polygons on both sides of the slice plane
            TriangulatePolygon(sideA, result);
            TriangulatePolygon(sideB, result);
            return result.Count == 0 ? new List<SlicedTriangle> { tri } : result;
        }

        private static void TriangulatePolygon(List<Point3D> poly, List<SlicedTriangle> output)
        {
            if (poly.Count < 3)
            {
                return;
            }
            // Basic fan triangulation for convex fragments generated via a single straight plane slice
            for (int i = 1; i < poly.Count - 1; i++)
            {
                output.Add(new SlicedTriangle(poly[0], poly[i], poly[i + 1]));
            }
        }

        private static bool TryIntersectAndSlice(SlicedTriangle t1, SlicedTriangle t2, out List<SlicedTriangle> f1, out List<SlicedTriangle> f2)
        {
            f1 = null; f2 = null;

            // Don't slice neighbor triangles sharing an explicit vertex border
            if (t1.P1 == t2.P1 || t1.P1 == t2.P2 || t1.P1 == t2.P3 ||
                t1.P2 == t2.P1 || t1.P2 == t2.P2 || t1.P2 == t2.P3 ||
                t1.P3 == t2.P1 || t1.P3 == t2.P2 || t1.P3 == t2.P3)
            {
                return false;
            }

            // Plane 1 details
            Vector3D n1 = Vector3D.CrossProduct(t1.P2 - t1.P1, t1.P3 - t1.P1);
            if (n1.LengthSquared < Epsilon)
            {
                return false;
            }

            n1.Normalize();
            double d1 = -Vector3D.DotProduct(n1, (Vector3D)t1.P1);

            // Compute plane distances for T2 vertices
            double dist2_1 = Vector3D.DotProduct(n1, (Vector3D)t2.P1) + d1;
            double dist2_2 = Vector3D.DotProduct(n1, (Vector3D)t2.P2) + d1;
            double dist2_3 = Vector3D.DotProduct(n1, (Vector3D)t2.P3) + d1;

            if (Math.Abs(dist2_1) < Epsilon)
            {
                dist2_1 = 0;
            }

            if (Math.Abs(dist2_2) < Epsilon)
            {
                dist2_2 = 0;
            }

            if (Math.Abs(dist2_3) < Epsilon)
            {
                dist2_3 = 0;
            }

            if ((dist2_1 > 0 && dist2_2 > 0 && dist2_3 > 0) || (dist2_1 < 0 && dist2_2 < 0 && dist2_3 < 0))
            {
                return false;
            }

            // Plane 2 details
            Vector3D n2 = Vector3D.CrossProduct(t2.P2 - t2.P1, t2.P3 - t2.P1);
            if (n2.LengthSquared < Epsilon)
            {
                return false;
            }

            n2.Normalize();
            double d2 = -Vector3D.DotProduct(n2, (Vector3D)t2.P1);

            // Compute plane distances for T1 vertices
            double dist1_1 = Vector3D.DotProduct(n2, (Vector3D)t1.P1) + d2;
            double dist1_2 = Vector3D.DotProduct(n2, (Vector3D)t1.P2) + d2;
            double dist1_3 = Vector3D.DotProduct(n2, (Vector3D)t1.P3) + d2;

            if (Math.Abs(dist1_1) < Epsilon) dist1_1 = 0;
            if (Math.Abs(dist1_2) < Epsilon) dist1_2 = 0;
            if (Math.Abs(dist1_3) < Epsilon) dist1_3 = 0;

            if ((dist1_1 > 0 && dist1_2 > 0 && dist1_3 > 0) || (dist1_1 < 0 && dist1_2 < 0 && dist1_3 < 0)) return false;

            // Perform the cutting slices
            f1 = SliceTriangleByPlane(t1, n2, d2);
            f2 = SliceTriangleByPlane(t2, n1, d1);

            return f1.Count > 1 || f2.Count > 1;
        }

        public class SlicedTriangle
        {
            public Rect3D Bounds;
            public Point3D P1, P2, P3;

            public SlicedTriangle(Point3D p1, Point3D p2, Point3D p3)
            {
                P1 = p1; P2 = p2; P3 = p3;
                double minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
                double minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
                double minZ = Math.Min(p1.Z, Math.Min(p2.Z, p3.Z));
                double maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
                double maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
                double maxZ = Math.Max(p1.Z, Math.Max(p2.Z, p3.Z));
                Bounds = new Rect3D(minX, minY, minZ, Math.Max(Epsilon, maxX - minX), Math.Max(Epsilon, maxY - minY), Math.Max(Epsilon, maxZ - minZ));
            }
        }

        // AABB Tree for spatial acceleration O(N log N)
        private class SpatialNode
        {
            public Rect3D Bounds
            {
                get; set;
            }

            public SpatialNode Left
            {
                get; set;
            }

            public SpatialNode Right
            {
                get; set;
            }

            public List<SlicedTriangle> Triangles { get; set; } = new List<SlicedTriangle>();

            public static SpatialNode Build(List<SlicedTriangle> tris, int depth = 0)
            {
                if (tris.Count == 0)
                {
                    return null;
                }

                SpatialNode node = new SpatialNode();
                Rect3D box = tris[0].Bounds;
                for (int i = 1; i < tris.Count; i++)
                {
                    box.Union(tris[i].Bounds);
                }

                node.Bounds = box;

                if (tris.Count <= 8 || depth > 12)
                {
                    node.Triangles = tris;
                    return node;
                }

                int axis = depth % 3;
                tris.Sort((a, b) => axis == 0 ? a.Bounds.X.CompareTo(b.Bounds.X) :
                                   axis == 1 ? a.Bounds.Y.CompareTo(b.Bounds.Y) :
                                               a.Bounds.Z.CompareTo(b.Bounds.Z));

                int mid = tris.Count / 2;
                node.Left = Build(tris.GetRange(0, mid), depth + 1);
                node.Right = Build(tris.GetRange(mid, tris.Count - mid), depth + 1);
                return node;
            }

            public void GetIntersectionPairs(SpatialNode other, List<(SlicedTriangle, SlicedTriangle)> pairs)
            {
                if (!Bounds.IntersectsWith(other.Bounds))
                {
                    return;
                }

                if (Left == null && Right == null && other.Left == null && other.Right == null)
                {
                    foreach (var t1 in Triangles)
                    {
                        foreach (var t2 in other.Triangles)
                        {
                            if (t1 != t2 && t1.Bounds.IntersectsWith(t2.Bounds))
                                pairs.Add((t1, t2));
                        }
                    }
                    return;
                }

                if (Left != null)
                {
                    Left.GetIntersectionPairs(other, pairs);
                }

                if (Right != null)
                {
                    Right.GetIntersectionPairs(other, pairs);
                }
            }
        }
    }
}

// Slice all intersecting geometries and generate clean fragments
//MeshGeometry3D cleanlySlicedMesh = WpfMeshSlicer.RemoveIntersectingFaces(myComplexMesh);

// Hook back to your UI Viewport
//myGeometryModel3D.Geometry = cleanlySlicedMesh;