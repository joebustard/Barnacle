using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Media3D;

namespace AiWpf3D
{
    // Define grid densities and destination file
    // int resX = 32;
    //int resY = 32;
    //int resZ = 32;
    //string outputPath = @"C:\Output\mesh_field.txt"
    // Convert your WPF geometry to SDF data rows
    //WpfMeshToSdf.GenerateAndSaveSdf(myMeshGeometry, resX, resY, resZ, outputPath);

    public static class WpfMeshToSdf
    {
        private const double Epsilon = 1e-7;

        /// <summary>
        /// Generates an SDF grid from a WPF MeshGeometry3D and exports it directly to a text file.
        /// Padding defines how far outside the mesh bounding box the grid extends.
        /// </summary>
        public static void GenerateAndSaveSdf(MeshGeometry3D mesh, int resX, int resY, int resZ, string filePath, double padding = 0.1)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            // 1. Unpack triangles and build spatial hierarchy tree
            List<SdfTriangle> triangles = new List<SdfTriangle>();
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                triangles.Add(new SdfTriangle(
                    mesh.Positions[mesh.TriangleIndices[i]],
                    mesh.Positions[mesh.TriangleIndices[i + 1]],
                    mesh.Positions[mesh.TriangleIndices[i + 2]]
                ));
            }
            SdfSpatialNode root = SdfSpatialNode.Build(triangles);

            // 2. Compute grid boundaries based on mesh dimensions
            Rect3D meshBounds = mesh.Bounds;
            double minX = meshBounds.X - padding;
            double minY = meshBounds.Y - padding;
            double minZ = meshBounds.Z - padding;
            double maxX = meshBounds.X + meshBounds.SizeX + padding;
            double maxY = meshBounds.Y + meshBounds.SizeY + padding;
            double maxZ = meshBounds.Z + meshBounds.SizeZ + padding;

            double stepX = (resX > 1) ? (maxX - minX) / (resX - 1) : 0;
            double stepY = (resY > 1) ? (maxY - minY) / (resY - 1) : 0;
            double stepZ = (resZ > 1) ? (maxZ - minZ) / (resZ - 1) : 0;

            Vector3D rayDir = new Vector3D(1.0, 0.312, 0.174); // Skewed ray direction to avoid clean grid alignments
            rayDir.Normalize();

            // 3. Write structured SDF records straight to text file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Header information: Grid dimensions, bounding minima, and cell size configurations
                writer.WriteLine($"# SDF Grid Resolution: {resX} {resY} {resZ}");
                writer.WriteLine($"# Min bounds: {minX:F6} {minY:F6} {minZ:F6}");
                writer.WriteLine($"# Cell steps: {stepX:F6} {stepY:F6} {stepZ:F6}");
                writer.WriteLine("X,Y,Z,Distance");

                for (int x = 0; x < resX; x++)
                {
                    double px = minX + x * stepX;
                    for (int y = 0; y < resY; y++)
                    {
                        double py = minY + y * stepY;
                        for (int z = 0; z < resZ; z++)
                        {
                            double pz = minZ + z * stepZ;
                            Point3D queryPt = new Point3D(px, py, pz);

                            // Find shortest unsigned distance squared
                            double minSqDist = double.MaxValue;
                            root.FindClosestDistance(queryPt, ref minSqDist);
                            double distance = Math.Sqrt(minSqDist);

                            // Determine sign via Jordan boundary ray intersection loops (odd = inside)
                            int crossings = root.CountRayIntersections(queryPt, rayDir);
                            if (crossings % 2 != 0)
                            {
                                distance = -distance; // Inside mesh volume
                            }

                            // Write data row
                            writer.WriteLine($"{px:F5},{py:F5},{pz:F5},{distance:F5}");
                        }
                    }
                }
            }
        }

        public class SdfTriangle
        {
            public Rect3D Bounds;
            public Point3D P1, P2, P3;

            public SdfTriangle(Point3D p1, Point3D p2, Point3D p3)
            {
                P1 = p1; P2 = p2; P3 = p3;
                double minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
                double minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
                double minZ = Math.Min(p1.Z, Math.Min(p2.Z, p3.Z));
                double maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
                double maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
                double maxZ = Math.Max(p1.Z, Math.Max(p2.Z, p3.Z));
                Bounds = new Rect3D(minX, minY, minZ, maxX - minX, maxY - minY, maxZ - minZ);
            }
        }

        // AABB Spatial Node to avoid testing every grid point against every single triangle
        private class SdfSpatialNode
        {
            public Rect3D Bounds
            {
                get; set;
            }

            public SdfSpatialNode Left
            {
                get; set;
            }

            public SdfSpatialNode Right
            {
                get; set;
            }

            public List<SdfTriangle> Triangles { get; set; } = new List<SdfTriangle>();

            public static SdfSpatialNode Build(List<SdfTriangle> tris, int depth = 0)
            {
                if (tris.Count == 0)
                {
                    return null;
                }

                var node = new SdfSpatialNode();
                Rect3D box = tris[0].Bounds;
                for (int i = 1; i < tris.Count; i++) box.Union(tris[i].Bounds);
                node.Bounds = box;

                if (tris.Count <= 16 || depth > 12)
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
                if (!RayIntersectsBox(origin, direction, Bounds))
                {
                    return 0;
                }

                int count = 0;
                if (Left == null && Right == null)
                {
                    foreach (var tri in Triangles)
                    {
                        if (RayIntersectsTriangle(origin, direction, tri))
                        {
                            count++;
                        }
                    }
                    return count;
                }

                if (Left != null)
                {
                    count += Left.CountRayIntersections(origin, direction);
                }

                if (Right != null)
                {
                    count += Right.CountRayIntersections(origin, direction);
                }

                return count;
            }

            public void FindClosestDistance(Point3D query, ref double minSqDist)
            {
                double sqDistToBox = SqDistPointToAABB(query, Bounds);
                if (sqDistToBox >= minSqDist)
                {
                    return;
                }

                if (Left == null && Right == null)
                {
                    foreach (var tri in Triangles)
                    {
                        double dSq = SqDistPointToTriangle(query, tri);
                        if (dSq < minSqDist) minSqDist = dSq;
                    }
                    return;
                }

                // Prioritize searching the closer child bounding box first to prune branches early
                double dLeft = Left != null ? SqDistPointToAABB(query, Left.Bounds) : double.MaxValue;
                double dRight = Right != null ? SqDistPointToAABB(query, Right.Bounds) : double.MaxValue;

                if (dLeft < dRight)
                {
                    Left?.FindClosestDistance(query, ref minSqDist);
                    Right?.FindClosestDistance(query, ref minSqDist);
                }
                else
                {
                    Right?.FindClosestDistance(query, ref minSqDist);
                    Left?.FindClosestDistance(query, ref minSqDist);
                }
            }
        }

        #region Mathematical Distance Calculations

        private static bool RayIntersectsBox(Point3D ro, Vector3D rd, Rect3D box)
        {
            double tmin = (box.X - ro.X) / rd.X;
            double tmax = (box.X + box.SizeX - ro.X) / rd.X;
            if (tmin > tmax) { double tmp = tmin; tmin = tmax; tmax = tmp; }
            double tymin = (box.Y - ro.Y) / rd.Y;
            double tymax = (box.Y + box.SizeY - ro.Y) / rd.Y;
            if (tymin > tymax) { double tmp = tymin; tymin = tymax; tymax = tmp; }
            if ((tmin > tymax) || (tymin > tmax))
            {
                return false;
            }

            if (tymin > tmin) tmin = tymin; if (tymax < tmax) tmax = tymax;
            double tzmin = (box.Z - ro.Z) / rd.Z;
            double tzmax = (box.Z + box.SizeZ - ro.Z) / rd.Z;
            if (tzmin > tzmax) { double tmp = tzmin; tzmin = tzmax; tzmax = tmp; }
            if ((tmin > tzmax) || (tzmin > tmax)) return false;
            return true;
        }

        private static bool RayIntersectsTriangle(Point3D ro, Vector3D rd, SdfTriangle tri)
        {
            Vector3D edge1 = tri.P2 - tri.P1; Vector3D edge2 = tri.P3 - tri.P1;
            Vector3D h = Vector3D.CrossProduct(rd, edge2); double a = Vector3D.DotProduct(edge1, h);
            if (a > -Epsilon && a < Epsilon)
            {
                return false;
            }

            double f = 1.0 / a; Vector3D s = ro - tri.P1;
            double u = f * Vector3D.DotProduct(s, h);
            if (u < 0.0 || u > 1.0) return false;
            Vector3D q = Vector3D.CrossProduct(s, edge1);
            double v = f * Vector3D.DotProduct(rd, q);
            if (v < 0.0 || u + v > 1.0) return false;
            double t = f * Vector3D.DotProduct(edge2, q);
            return t > Epsilon;
        }

        private static double SqDistPointToAABB(Point3D p, Rect3D b)
        {
            double sqDist = 0.0;
            double minX = b.X, maxX = b.X + b.SizeX;
            double minY = b.Y, maxY = b.Y + b.SizeY;
            double minZ = b.Z, maxZ = b.Z + b.SizeZ;

            if (p.X < minX) sqDist += (minX - p.X) * (minX - p.X);
            if (p.X > maxX) sqDist += (p.X - maxX) * (p.X - maxX);
            if (p.Y < minY) sqDist += (minY - p.Y) * (minY - p.Y);
            if (p.Y > maxY) sqDist += (p.Y - maxY) * (p.Y - maxY);
            if (p.Z < minZ) sqDist += (minZ - p.Z) * (minZ - p.Z);
            if (p.Z > maxZ) sqDist += (p.Z - maxZ) * (p.Z - maxZ);

            return sqDist;
        }

        private static double SqDistPointToTriangle(Point3D p, SdfTriangle tri)
        {
            Vector3D diff = tri.P1 - p;
            Vector3D edge0 = tri.P2 - tri.P1;
            Vector3D edge1 = tri.P3 - tri.P1;
            double a00 = edge0.LengthSquared;
            double a01 = Vector3D.DotProduct(edge0, edge1);
            double a11 = edge1.LengthSquared;
            double b0 = Vector3D.DotProduct(diff, edge0);
            double b1 = Vector3D.DotProduct(diff, edge1);
            double c = diff.LengthSquared;
            double det = Math.Abs(a00 * a11 - a01 * a01);
            double s = a01 * b1 - a11 * b0;
            double t = a01 * b0 - a00 * b1;

            if (s + t <= det)
            {
                if (s < 0)
                {
                    if (t < 0)
                    {
                        if (b0 < 0) { t = 0; s = -b0 >= a00 ? 1 : -b0 / a00; }
                        else { s = 0; t = b1 >= 0 ? 0 : (-b1 >= a11 ? 1 : -b1 / a11); }
                    }
                    else
                    {
                        s = 0; t = b1 >= 0 ? 0 : (-b1 >= a11 ? 1 : -b1 / a11);
                    }
                }
                else if (t < 0)
                {
                    t = 0; s = b0 >= 0 ? 0 : (-b0 >= a00 ? 1 : -b0 / a00);
                }
                else
                {
                    double invDet = 1.0 / det; s *= invDet; t *= invDet;
                }
            }
            else
            {
                if (s < 0)
                {
                    double tmp0 = a01 + b0; double tmp1 = a11 + b1;
                    if (tmp1 > tmp0) { double numer = tmp1 - tmp0; double denom = a00 - 2.0 * a01 + a11; s = numer >= denom ? 1 : numer / denom; t = 1 - s; }
                    else { s = 0; t = tmp1 >= 0 ? 0 : (-b1 >= a11 ? 1 : -b1 / a11); }
                }
                else if (t < 0)
                {
                    double tmp0 = a01 + b1; double tmp1 = a00 + b0;
                    if (tmp1 > tmp0) { double numer = tmp1 - tmp0; double denom = a00 - 2.0 * a01 + a11; t = numer >= denom ? 1 : numer / denom; s = 1 - t; }
                    else { t = 0; s = tmp1 >= 0 ? 0 : (-b0 >= a00 ? 1 : -b0 / a00); }
                }
                else
                {
                    double numer = (a11 + b1) - (a01 + b0); double denom = a00 - 2.0 * a01 + a11; s = numer <= 0 ? 0 : (numer >= denom ? 1 : numer / denom); t = 1 - s;
                }
            }
            return Math.Max(0, a00 * s * s + 2.0 * a01 * s * t + a11 * t * t + 2.0 * b0 * s + 2.0 * b1 * t + c);
        }

        #endregion Mathematical Distance Calculations
    }
}