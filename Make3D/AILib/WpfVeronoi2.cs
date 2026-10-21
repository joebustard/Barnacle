using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace AiLib
{
    public class VoronoiNetworkGenerator
    {
        /// <summary>
        /// Generates a unified 3D network/wireframe tubular mesh from the shared edges of Voronoi cells.
        /// </summary>
        /// <param name="inputMesh">The container mesh defining the boundaries.</param>
        /// <param name="tubeRadius">The thickness of the network strands.</param>
        /// <param name="tubeSides">The cylindrical resolution of each strand (e.g., 4 for a square tube, 6-8 for round tubes).</param>
        /// <returns>A unified MeshGeometry3D representing the Voronoi network.</returns>
        public static MeshGeometry3D CreateVoronoiNetwork(MeshGeometry3D inputMesh, double tubeRadius = 0.05, int tubeSides = 6)
        {
            // 1. Gather distinct vertex sites from the input mesh
            List<Point3D> sites = ExtractUniqueSites(inputMesh);
            List<PolyFace> boundingVolume = ExtractFacesFromMesh(inputMesh);

            // Track unique edges to prevent creating duplicate tubes on shared cell boundaries
            var uniqueEdges = new HashSet<IndexedEdge>();
            var networkPoints = new List<Point3D>();

            // 2. Compute individual Voronoi cell polyhedrons
            foreach (var currentSite in sites)
            {
                List<PolyFace> cellVolume = CloneVolume(boundingVolume);

                foreach (var otherSite in sites)
                {
                    if (currentSite.Equals(otherSite)) continue;

                    Vector3D normal = otherSite - currentSite;
                    normal.Normalize();

                    Point3D planePoint = new Point3D(
                        (currentSite.X + otherSite.X) * 0.5,
                        (currentSite.Y + otherSite.Y) * 0.5,
                        (currentSite.Z + otherSite.Z) * 0.5
                    );

                    cellVolume = ClipPolyhedronWithPlane(cellVolume, planePoint, normal);
                    if (cellVolume.Count == 0) break;
                }

                // 3. Extract edges from the surviving cell polyhedron
                foreach (var face in cellVolume)
                {
                    for (int i = 0; i < face.Vertices.Count; i++)
                    {
                        Point3D p1 = face.Vertices[i];
                        Point3D p2 = face.Vertices[(i + 1) % face.Vertices.Count];

                        // Register points globally to simplify indexing
                        int idx1 = RegisterPointInList(networkPoints, p1, 0.001);
                        int idx2 = RegisterPointInList(networkPoints, p2, 0.001);

                        if (idx1 != idx2)
                        {
                            var edge = new IndexedEdge(idx1, idx2);
                            uniqueEdges.Add(edge);
                        }
                    }
                }
            }

            // 4. Generate 3D cylinders around every unique network wire edge
            MeshGeometry3D finalNetworkMesh = new MeshGeometry3D();
            int globalVertexOffset = 0;

            foreach (var edge in uniqueEdges)
            {
                Point3D start = networkPoints[edge.V1];
                Point3D end = networkPoints[edge.V2];

                AddTubeSegment(finalNetworkMesh, start, end, tubeRadius, tubeSides, ref globalVertexOffset);
            }

            finalNetworkMesh.Freeze();
            return finalNetworkMesh;
        }

        #region Edge Representation Structure

        private struct IndexedEdge : IEquatable<IndexedEdge>
        {
            public IndexedEdge(int v1, int v2)
            {
                V1 = Math.Min(v1, v2);
                V2 = Math.Max(v1, v2);
            }

            public int V1
            {
                get;
            }

            public int V2
            {
                get;
            }

            public bool Equals(IndexedEdge other) => V1 == other.V1 && V2 == other.V2;

            public override bool Equals(object obj) => obj is IndexedEdge other && Equals(other);

            public override int GetHashCode() => (V1, V2).GetHashCode();
        }

        #endregion Edge Representation Structure

        #region Inner Polyhedral Representation & Core Geometry Engine

        private static void AddTubeSegment(MeshGeometry3D mesh, Point3D start, Point3D end, double radius, int sides, ref int vertexOffset)
        {
            Vector3D axis = end - start;
            double length = axis.Length;
            if (length < 0.001) return;
            axis.Normalize();

            // Calculate perpendicular coordinate vectors for generating ring coordinates
            Vector3D u = Vector3D.CrossProduct(axis, new Vector3D(1, 0, 0));
            if (u.LengthSquared < 0.001)
            {
                u = Vector3D.CrossProduct(axis, new Vector3D(0, 1, 0));
            }

            u.Normalize();
            Vector3D v = Vector3D.CrossProduct(axis, u);

            // Create cross-section rings at the start and end of the segment
            for (int i = 0; i <= sides; i++)
            {
                double angle = (2.0 * Math.PI * i) / sides;
                double cos = Math.Cos(angle) * radius;
                double sin = Math.Sin(angle) * radius;

                Vector3D offset = (u * cos) + (v * sin);

                mesh.Positions.Add(start + offset);
                mesh.Positions.Add(end + offset);
            }

            // Generate triangles wrapping the tube surface
            for (int i = 0; i < sides; i++)
            {
                int baseIdx = vertexOffset + (i * 2);

                mesh.TriangleIndices.Add(baseIdx);
                mesh.TriangleIndices.Add(baseIdx + 3);
                mesh.TriangleIndices.Add(baseIdx + 1);

                mesh.TriangleIndices.Add(baseIdx);
                mesh.TriangleIndices.Add(baseIdx + 2);
                mesh.TriangleIndices.Add(baseIdx + 3);
            }

            vertexOffset += (sides + 1) * 2;
        }

        private static List<PolyFace> ClipPolyhedronWithPlane(List<PolyFace> faces, Point3D planePoint, Vector3D normal)
        {
            List<PolyFace> clippedFaces = new List<PolyFace>();
            List<Point3D> cutPolygonVertices = new List<Point3D>();

            foreach (var face in faces)
            {
                List<Point3D> newVertices = new List<Point3D>();

                for (int i = 0; i < face.Vertices.Count; i++)
                {
                    Point3D current = face.Vertices[i];
                    Point3D next = face.Vertices[(i + 1) % face.Vertices.Count];

                    double currentDist = SignedDistanceToPlane(current, planePoint, normal);
                    double nextDist = SignedDistanceToPlane(next, planePoint, normal);

                    if (currentDist <= 0) newVertices.Add(current);

                    if ((currentDist < 0 && nextDist > 0) || (currentDist > 0 && nextDist < 0))
                    {
                        double t = currentDist / (currentDist - nextDist);
                        Point3D intersect = new Point3D(
                            current.X + t * (next.X - current.X),
                            current.Y + t * (next.Y - current.Y),
                            current.Z + t * (next.Z - current.Z)
                        );
                        newVertices.Add(intersect);

                        if (!cutPolygonVertices.Contains(intersect))
                        {
                            cutPolygonVertices.Add(intersect);
                        }
                    }
                }

                if (newVertices.Count >= 3)
                {
                    clippedFaces.Add(new PolyFace { Vertices = newVertices });
                }
            }

            if (cutPolygonVertices.Count >= 3)
            {
                SortVerticesPolygonOrder(cutPolygonVertices, normal);
                clippedFaces.Add(new PolyFace { Vertices = cutPolygonVertices });
            }

            return clippedFaces;
        }

        private static List<PolyFace> CloneVolume(List<PolyFace> original)
        {
            var clone = new List<PolyFace>();
            foreach (var face in original)
            {
                clone.Add(new PolyFace { Vertices = new List<Point3D>(face.Vertices) });
            }
            return clone;
        }

        private static List<PolyFace> ExtractFacesFromMesh(MeshGeometry3D mesh)
        {
            List<PolyFace> faces = new List<PolyFace>();
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                var face = new PolyFace();
                face.Vertices.Add(mesh.Positions[mesh.TriangleIndices[i]]);
                face.Vertices.Add(mesh.Positions[mesh.TriangleIndices[i + 1]]);

                face.Vertices.Add(mesh.Positions[mesh.TriangleIndices[i + 2]]);
                faces.Add(face);
            }
            return faces;
        }

        private static List<Point3D> ExtractUniqueSites(MeshGeometry3D mesh)
        {
            var uniquePoints = new List<Point3D>();
            double epsilon = 0.001;
            foreach (var p in mesh.Positions)
            {
                if (!uniquePoints.Exists(sp => Math.Abs(sp.X - p.X) < epsilon && Math.Abs(sp.Y - p.Y) < epsilon && Math.Abs(sp.Z - p.Z) < epsilon))
                {
                    uniquePoints.Add(p);
                }
            }
            return uniquePoints;
        }

        private static int RegisterPointInList(List<Point3D> list, Point3D point, double epsilon)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (Math.Abs(list[i].X - point.X) < epsilon &&
                    Math.Abs(list[i].Y - point.Y) < epsilon &&
                    Math.Abs(list[i].Z - point.Z) < epsilon)
                {
                    return i;
                }
            }
            list.Add(point);
            return list.Count - 1;
        }

        private static double SignedDistanceToPlane(Point3D point, Point3D planePoint, Vector3D normal)
        {
            return (point.X - planePoint.X) * normal.X + (point.Y - planePoint.Y) * normal.Y + (point.Z - planePoint.Z) * normal.Z;
        }

        private static void SortVerticesPolygonOrder(List<Point3D> vertices, Vector3D normal)
        {
            Point3D centroid = new Point3D(0, 0, 0);
            foreach (var v in vertices) { centroid.X += v.X; centroid.Y += v.Y; centroid.Z += v.Z; }
            centroid.X /= vertices.Count; centroid.Y /= vertices.Count; centroid.Z /= vertices.Count;

            Vector3D u = Vector3D.CrossProduct(normal, new Vector3D(1, 0, 0));
            if (u.LengthSquared < 0.001) u = Vector3D.CrossProduct(normal, new Vector3D(0, 1, 0));
            u.Normalize();
            Vector3D vBasis = Vector3D.CrossProduct(normal, u);

            vertices.Sort((p1, p2) => {
                Vector3D v1 = p1 - centroid; Vector3D v2 = p2 - centroid;
                double angle1 = Math.Atan2(Vector3D.DotProduct(v1, vBasis), Vector3D.DotProduct(v1, u));
                double angle2 = Math.Atan2(Vector3D.DotProduct(v2, vBasis), Vector3D.DotProduct(v2, u));
                return angle1.CompareTo(angle2);
            });
        }

        private class PolyFace
        {
            public List<Point3D> Vertices { get; set; } = new List<Point3D>();
        }

        #endregion Inner Polyhedral Representation & Core Geometry Engine
    }
}