using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AiWpf3D
{
    public static class WpfCsgEngine
    {
        private const double Epsilon = 1e-6;

        public enum CsgOperation
        {
            Union, Intersection, Difference
        }

        /// <summary>
        /// Executes a clean pure C# CSG boolean operation on two WPF meshes.
        /// Both meshes must be closed manifold solid shells for accurate results.
        /// </summary>
        public static MeshGeometry3D PerformCSG(MeshGeometry3D meshA, MeshGeometry3D meshB, CsgOperation operation)
        {
            List<CsgTriangle> trisA = UnpackMesh(meshA);
            List<CsgTriangle> trisB = UnpackMesh(meshB);

            // Step 1: Intersect and slice Mesh A against Mesh B and vice versa
            InterSliceCollections(trisA, trisB);

            // Step 2: Build spatial acceleration trees for final classification
            SpatialNode treeA = SpatialNode.Build(trisA);
            SpatialNode treeB = SpatialNode.Build(trisB);

            List<CsgTriangle> resultTris = new List<CsgTriangle>();

            // Step 3: Classify and retain fragments matching the operation rules
            Vector3D rayDir = new Vector3D(1, 0.312, 0.174); // Arbitrary unaligned vector to minimize edge alignments
            rayDir.Normalize();

            foreach (var tri in trisA)
            {
                bool isInsideB = IsPointInsideVolume(tri.GetCentroid(), rayDir, treeB);

                if (operation == CsgOperation.Union && !isInsideB) resultTris.Add(tri);
                else if (operation == CsgOperation.Intersection && isInsideB) resultTris.Add(tri);
                else if (operation == CsgOperation.Difference && !isInsideB) resultTris.Add(tri);
            }

            foreach (var tri in trisB)
            {
                bool isInsideA = IsPointInsideVolume(tri.GetCentroid(), rayDir, treeA);

                if (operation == CsgOperation.Union && !isInsideA) resultTris.Add(tri);
                else if (operation == CsgOperation.Intersection && isInsideA) resultTris.Add(tri);
                else if (operation == CsgOperation.Difference && isInsideA)
                {
                    tri.Invert(); // Inside boundaries are turned inside-out during subtraction
                    resultTris.Add(tri);
                }
            }

            return PackMesh(resultTris);
        }

        private static int GetOrAddVertex(Point3D p, Point3DCollection list, Dictionary<string, int> cache)
        {
            string key = $"{Math.Round(p.X, 4):F4}{Math.Round(p.Y, 4):F4}{Math.Round(p.Z, 4):F4}";
            if (cache.TryGetValue(key, out int index)) return index;
            index = list.Count;
            list.Add(p);
            cache[key] = index;
            return index;
        }

        private static void InterSliceCollections(List<CsgTriangle> listA, List<CsgTriangle> listB)
        {
            bool variationsDetected = true;
            while (variationsDetected)
            {
                variationsDetected = false;
                SpatialNode treeA = SpatialNode.Build(listA);
                SpatialNode treeB = SpatialNode.Build(listB);

                var intersectionPairs = new List<(CsgTriangle, CsgTriangle)>();
                treeA.GetIntersectionPairs(treeB, intersectionPairs);

                foreach (var pair in intersectionPairs)
                {
                    if (TryIntersectAndSlice(pair.Item1, pair.Item2, out var fragA, out var fragB))
                    {
                        listA.Remove(pair.Item1);
                        listA.AddRange(fragA);
                        listB.Remove(pair.Item2);
                        listB.AddRange(fragB);
                        variationsDetected = true;
                        break; // Restart loop to validate new fragment trees
                    }
                }
            }
        }

        // Jordan Boundary Theorem: ray cast count determines enclosure status (odd count = inside)
        private static bool IsPointInsideVolume(Point3D point, Vector3D direction, SpatialNode tree)
        {
            int crossCount = tree.CountRayIntersections(point, direction);
            return (crossCount % 2) != 0;
        }

        private static MeshGeometry3D PackMesh(List<CsgTriangle> triangles)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection positions = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Dictionary<string, int> vertexCache = new Dictionary<string, int>();
            foreach (var tri in triangles)
            {
                int i1 = GetOrAddVertex(tri.P1, positions, vertexCache);
                int i2 = GetOrAddVertex(tri.P2, positions, vertexCache);
                int i3 = GetOrAddVertex(tri.P3, positions, vertexCache);
                if (i1 == i2 || i2 == i3 || i3 == i1) continue;
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
            mesh.Positions = positions;
            mesh.TriangleIndices = indices;
            return mesh;
        }

        private static bool RayIntersectsBox(Point3D ro, Vector3D rd, Rect3D box)
        {
            double tmin = (box.X - ro.X) / rd.X;
            double tmax = (box.X + box.SizeX - ro.X) / rd.X;
            if (tmin > tmax) { double tmp = tmin; tmin = tmax; tmax = tmp; }
            double tymin = (box.Y - ro.Y) / rd.Y;
            double tymax = (box.Y + box.SizeY - ro.Y) / rd.Y;
            if (tymin > tymax) { double tmp = tymin; tymin = tymax; tymax = tmp; }
            if ((tmin > tymax) || (tymin > tmax)) return false;
            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;
            double tzmin = (box.Z - ro.Z) / rd.Z;
            double tzmax = (box.Z + box.SizeZ - ro.Z) / rd.Z;
            if (tzmin > tzmax) { double tmp = tzmin; tzmin = tzmax; tzmax = tmp; }
            if ((tmin > tzmax) || (tzmin > tmax)) return false;
            return true;
        }

        // Möller-Trumbore Ray-Triangle intersection
        private static bool RayIntersectsTriangle(Point3D ro, Vector3D rd, CsgTriangle tri)
        {
            Vector3D edge1 = tri.P2 - tri.P1;
            Vector3D edge2 = tri.P3 - tri.P1;
            Vector3D h = Vector3D.CrossProduct(rd, edge2);
            double a = Vector3D.DotProduct(edge1, h);
            if (a > -Epsilon && a < Epsilon)
            {
                return false;
            }

            double f = 1.0 / a;
            Vector3D s = ro - tri.P1;
            double u = f * Vector3D.DotProduct(s, h);
            if (u < 0.0 || u > 1.0)
            {
                return false;
            }

            Vector3D q = Vector3D.CrossProduct(s, edge1);
            double v = f * Vector3D.DotProduct(rd, q);
            if (v < 0.0 || u + v > 1.0)
            {
                return false;
            }

            double t = f * Vector3D.DotProduct(edge2, q);
            return t > Epsilon;
        }

        private static List<CsgTriangle> SliceTriangleByPlane(CsgTriangle tri, Vector3D normal, double d)
        {
            var result = new List<CsgTriangle>();
            Point3D[] pts = { tri.P1, tri.P2, tri.P3 };

            double[] dists = {
            Vector3D.DotProduct(normal, (Vector3D)tri.P1) + d,
            Vector3D.DotProduct(normal, (Vector3D)tri.P2) + d,
            Vector3D.DotProduct(normal, (Vector3D)tri.P3) + d
            };

            var sideA = new List<Point3D>();
            var sideB = new List<Point3D>();
            for (int i = 0; i < 3; i++)
            {
                int next = (i + 1) % 3;
                if (dists[i] >= 0) sideA.Add(pts[i]);
                else sideB.Add(pts[i]);
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
            // Triangulate parts
            for (int i = 1; i < sideA.Count - 1; i++) result.Add(new CsgTriangle(sideA[0], sideA[i], sideA[i + 1]));
            for (int i = 1; i < sideB.Count - 1; i++) result.Add(new CsgTriangle(sideB[0], sideB[i], sideB[i + 1]));
            return result.Count == 0 ? new List<CsgTriangle> { tri } : result;
        }

        private static bool TryIntersectAndSlice(CsgTriangle t1, CsgTriangle t2, out List<CsgTriangle> f1, out List<CsgTriangle> f2)
        {
            f1 = null;
            f2 = null;

            // Skip triangles that share identical vertex structures
            if (t1.P1 == t2.P1 || t1.P1 == t2.P2 || t1.P1 == t2.P3 ||
                t1.P2 == t2.P1 || t1.P2 == t2.P2 || t1.P2 == t2.P3) return false;

            double d1 = -Vector3D.DotProduct(t1.Normal, (Vector3D)t1.P1);
            double dist2_1 = Vector3D.DotProduct(t1.Normal, (Vector3D)t2.P1) + d1;
            double dist2_2 = Vector3D.DotProduct(t1.Normal, (Vector3D)t2.P2) + d1;
            double dist2_3 = Vector3D.DotProduct(t1.Normal, (Vector3D)t2.P3) + d1;

            if (Math.Abs(dist2_1) < Epsilon) dist2_1 = 0;
            if (Math.Abs(dist2_2) < Epsilon) dist2_2 = 0;
            if (Math.Abs(dist2_3) < Epsilon) dist2_3 = 0;

            if ((dist2_1 > 0 && dist2_2 > 0 && dist2_3 > 0) || (dist2_1 < 0 && dist2_2 < 0 && dist2_3 < 0)) return false;

            double d2 = -Vector3D.DotProduct(t2.Normal, (Vector3D)t2.P1);
            double dist1_1 = Vector3D.DotProduct(t2.Normal, (Vector3D)t1.P1) + d2;
            double dist1_2 = Vector3D.DotProduct(t2.Normal, (Vector3D)t1.P2) + d2;
            double dist1_3 = Vector3D.DotProduct(t2.Normal, (Vector3D)t1.P3) + d2;

            if (Math.Abs(dist1_1) < Epsilon)
            {
                dist1_1 = 0;
            }

            if (Math.Abs(dist1_2) < Epsilon)
            {
                dist1_2 = 0;
            }

            if (Math.Abs(dist1_3) < Epsilon)
            {
                dist1_3 = 0;
            }

            if ((dist1_1 > 0 && dist1_2 > 0 && dist1_3 > 0) || (dist1_1 < 0 && dist1_2 < 0 && dist1_3 < 0))
            {
                return false;
            }

            f1 = SliceTriangleByPlane(t1, t2.Normal, d2);
            f2 = SliceTriangleByPlane(t2, t1.Normal, d1);
            return f1.Count > 1 || f2.Count > 1;
        }

        private static List<CsgTriangle> UnpackMesh(MeshGeometry3D mesh)
        {
            var list = new List<CsgTriangle>();
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                list.Add(new CsgTriangle(
                    mesh.Positions[mesh.TriangleIndices[i]],
                    mesh.Positions[mesh.TriangleIndices[i + 1]],
                    mesh.Positions[mesh.TriangleIndices[i + 2]]
                ));
            }
            return list;
        }

        public class CsgTriangle
        {
            public Rect3D Bounds;
            public Vector3D Normal;
            public Point3D P1, P2, P3;

            public CsgTriangle(Point3D p1, Point3D p2, Point3D p3)
            {
                P1 = p1; P2 = p2; P3 = p3;

                // Compute Normal
                Vector3D v1 = p2 - p1;
                Vector3D v2 = p3 - p1;
                Vector3D n = Vector3D.CrossProduct(v1, v2);
                if (n.LengthSquared > Epsilon) n.Normalize();
                Normal = n;

                // Compute Bounding Box
                double minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
                double minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
                double minZ = Math.Min(p1.Z, Math.Min(p2.Z, p3.Z));
                double maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
                double maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
                double maxZ = Math.Max(p1.Z, Math.Max(p2.Z, p3.Z));
                Bounds = new Rect3D(minX, minY, minZ, Math.Max(Epsilon, maxX - minX), Math.Max(Epsilon, maxY - minY), Math.Max(Epsilon, maxZ - minZ));
            }

            public Point3D GetCentroid()
            {
                return new Point3D((P1.X + P2.X + P3.X) / 3.0, (P1.Y + P2.Y + P3.Y) / 3.0, (P1.Z + P2.Z + P3.Z) / 3.0);
            }

            public void Invert()
            {
                Point3D temp = P1;
                P1 = P3;
                P3 = temp;
                Normal = -Normal;
            }
        }

        // AABB Spatial Node for fast ray casting and intersection acceleration
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

            public List<CsgTriangle> Triangles { get; set; } = new List<CsgTriangle>();

            public static SpatialNode Build(List<CsgTriangle> tris, int depth = 0)
            {
                if (tris.Count == 0) return null;
                SpatialNode node = new SpatialNode();
                Rect3D box = tris[0].Bounds;
                for (int i = 1; i < tris.Count; i++) box.Union(tris[i].Bounds);
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

            public int CountRayIntersections(Point3D origin, Vector3D direction)
            {
                if (!RayIntersectsBox(origin, direction, Bounds)) return 0;

                int count = 0;
                if (Left == null && Right == null)
                {
                    foreach (var tri in Triangles)
                    {
                        if (RayIntersectsTriangle(origin, direction, tri)) count++;
                    }
                    return count;
                }

                if (Left != null) count += Left.CountRayIntersections(origin, direction);
                if (Right != null) count += Right.CountRayIntersections(origin, direction);
                return count;
            }

            public void GetIntersectionPairs(SpatialNode other, List<(CsgTriangle, CsgTriangle)> pairs)
            {
                if (!Bounds.IntersectsWith(other.Bounds)) return;

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

                if (Left != null) Left.GetIntersectionPairs(other, pairs);
                if (Right != null) Right.GetIntersectionPairs(other, pairs);
            }
        }
    }

    // Subtract Mesh B from Mesh A
    //MeshGeometry3D finalCsgMesh = WpfCsgEngine.PerformCSG(
    //    meshA,
    //    meshB,
    //    WpfCsgEngine.CsgOperation.Difference
    //);

    // Display the result
    //myGeometryModel3D.Geometry = finalCsgMesh;
}