using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

public class VoronoiMeshConverter
{
    /// <summary>
    /// Converts an existing WPF 3D mesh into a single combined Voronoi fractured mesh.
    /// </summary>
    /// <param name="inputMesh">The original mesh to be transformed into Voronoi cells.</param>
    /// <param name="explodeFactor">Optional factor to push cells outward from the center (e.g., 0.05 for a fractured look).</param>
    /// <returns>A new MeshGeometry3D containing the combined Voronoi pieces.</returns>
    public static MeshGeometry3D ConvertToVoronoiMesh(MeshGeometry3D inputMesh, double explodeFactor = 0.0)
    {
        if (inputMesh == null || inputMesh.Positions.Count < 4)
            throw new ArgumentException("Input mesh must have at least 4 unique vertices to form a 3D volume.");

        // 1. Gather distinct vertices from the original mesh to use as Voronoi seed sites
        List<Point3D> sites = ExtractUniqueSites(inputMesh);

        // 2. Find the bounding structure of the original mesh to use as the clipping container
        List<PolyFace> boundingVolume = ExtractFacesFromMesh(inputMesh);
        Point3D meshCenter = CalculateMeshCenter(inputMesh);

        // Prepare standard structures for merging the generated cells
        MeshGeometry3D combinedVoronoiMesh = new MeshGeometry3D();
        int globalVertexIndex = 0;

        // 3. Generate and clip a Voronoi cell for every unique vertex site
        foreach (var currentSite in sites)
        {
            List<PolyFace> cellVolume = CloneVolume(boundingVolume);

            foreach (var otherSite in sites)
            {
                if (currentSite.Equals(otherSite)) continue;

                // Define the perpendicular bisector cutting plane between the two vertex sites
                Vector3D normal = otherSite - currentSite;
                normal.Normalize();

                Point3D planePoint = new Point3D(
                    (currentSite.X + otherSite.X) * 0.5,
                    (currentSite.Y + otherSite.Y) * 0.5,
                    (currentSite.Z + otherSite.Z) * 0.5
                );

                // Clip the current cell's geometry
                cellVolume = ClipPolyhedronWithPlane(cellVolume, planePoint, normal);
                if (cellVolume.Count == 0) break;
            }

            // 4. If the cell survived clipping, stitch it into the final unified mesh
            if (cellVolume.Count > 0)
            {
                // Optional: Calculate explode offset vector to create distinct gaps between cells
                Vector3D explodeOffset = new Vector3D(0, 0, 0);
                if (explodeFactor > 0.0)
                {
                    explodeOffset = currentSite - meshCenter;
                    if (explodeOffset.LengthSquared > 0)
                    {
                        explodeOffset.Normalize();
                    }

                    explodeOffset *= explodeFactor;
                }

                // Triangulate the multi-sided polygons directly into the final combined mesh
                foreach (var face in cellVolume)
                {
                    for (int i = 1; i < face.Vertices.Count - 1; i++)
                    {
                        combinedVoronoiMesh.Positions.Add(face.Vertices[0] + explodeOffset);
                        combinedVoronoiMesh.Positions.Add(face.Vertices[i] + explodeOffset);
                        combinedVoronoiMesh.Positions.Add(face.Vertices[i + 1] + explodeOffset);

                        combinedVoronoiMesh.TriangleIndices.Add(globalVertexIndex);
                        combinedVoronoiMesh.TriangleIndices.Add(globalVertexIndex + 1);
                        combinedVoronoiMesh.TriangleIndices.Add(globalVertexIndex + 2);
                        globalVertexIndex += 3;
                    }
                }
            }
        }

        combinedVoronoiMesh.Freeze(); // Optimize for WPF rendering architecture
        return combinedVoronoiMesh;
    }

    #region Geometry and Clipping Engine

    private static Point3D CalculateMeshCenter(MeshGeometry3D mesh)
    {
        double x = 0, y = 0, z = 0;
        foreach (var p in mesh.Positions)
        {
            x += p.X;
            y += p.Y;
            z += p.Z;
        }
        return new Point3D(x / mesh.Positions.Count, y / mesh.Positions.Count, z / mesh.Positions.Count);
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

                if (currentDist <= 0)
                {
                    newVertices.Add(current);
                }

                if ((currentDist < 0 && nextDist > 0) || (currentDist > 0 && nextDist < 0))
                {
                    double t = currentDist / (currentDist - nextDist);
                    Point3D intersect = new Point3D(
                        current.X + t * (next.X - current.X),
                        current.Y + t * (next.Y - current.Y),
                        current.Z + t * (next.Z - current.Z)
                    );
                    newVertices.Add(intersect);

                    if (!cutPolygonVertices.Contains(intersect)) cutPolygonVertices.Add(intersect);
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
        // Use a small epsilon to avoid duplicate site registration from overlapping vertices
        double epsilon = 0.00001;

        foreach (var p in mesh.Positions)
        {
            bool found = false;
            for (int i = 0; i < uniquePoints.Count; i++)
            {
                if (Math.Abs(uniquePoints[i].X - p.X) < epsilon &&
                    Math.Abs(uniquePoints[i].Y - p.Y) < epsilon &&
                    Math.Abs(uniquePoints[i].Z - p.Z) < epsilon)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                uniquePoints.Add(p);
            }
        }
        return uniquePoints;
    }

    private static double SignedDistanceToPlane(Point3D point, Point3D planePoint, Vector3D normal)
    {
        return (point.X - planePoint.X) * normal.X +
               (point.Y - planePoint.Y) * normal.Y +
               (point.Z - planePoint.Z) * normal.Z;
    }

    private static void SortVerticesPolygonOrder(List<Point3D> vertices, Vector3D normal)
    {
        Point3D centroid = new Point3D(0, 0, 0);
        foreach (var v in vertices)
        {
            centroid.X += v.X;

            centroid.Y += v.Y;
            centroid.Z += v.Z;
        }
        centroid.X /= vertices.Count;
        centroid.Y /= vertices.Count;
        centroid.Z /= vertices.Count;

        Vector3D u = Vector3D.CrossProduct(normal, new Vector3D(1, 0, 0));
        if (u.LengthSquared < 0.001)
        {
            u = Vector3D.CrossProduct(normal, new Vector3D(0, 1, 0));
        }

        u.Normalize();
        Vector3D vBasis = Vector3D.CrossProduct(normal, u);

        vertices.Sort((p1, p2) => {
            Vector3D v1 = p1 - centroid;
            Vector3D v2 = p2 - centroid;
            double angle1 = Math.Atan2(Vector3D.DotProduct(v1, vBasis), Vector3D.DotProduct(v1, u));
            double angle2 = Math.Atan2(Vector3D.DotProduct(v2, vBasis), Vector3D.DotProduct(v2, u));
            return angle1.CompareTo(angle2);
        });
    }

    private class PolyFace
    {
        public List<Point3D> Vertices { get; set; } = new List<Point3D>();
    }

    #endregion Geometry and Clipping Engine
}