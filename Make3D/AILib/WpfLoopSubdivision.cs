using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// Smooth the mesh by executing 2 iterations of Loop Subdivision
//MeshGeometry3D subdividedMesh = myRawMesh;
//for (int iter = 0; iter < 2; iter++)
//{
//subdividedMesh = WpfLoopSubdivision.ApplyLoopSubdivision(subdividedMesh);
//}
// Push the silky-smooth geometry to your Viewport 3D
//myGeometryModel3D.Geometry = subdividedMesh;
namespace AiWpf3D
{
    public static class WpfLoopSubdivision
    {
        /// <summary>
        /// Applies one iteration of Loop Subdivision and smoothing to a WPF MeshGeometry3D.
        /// The input mesh should ideally be a closed manifold shell for optimal smoothing weights.
        /// </summary>
        public static MeshGeometry3D ApplyLoopSubdivision(MeshGeometry3D inputMesh)
        {
            if (inputMesh == null) throw new ArgumentNullException(nameof(inputMesh));

            // 1. Gather topological maps
            int originalVertexCount = inputMesh.Positions.Count;

            // Track the immediate vertex neighbors of every vertex (used to smooth original vertices)
            List<HashSet<int>> vertexNeighbors = new List<HashSet<int>>(originalVertexCount);
            for (int i = 0; i < originalVertexCount; i++) vertexNeighbors.Add(new HashSet<int>());

            // Map an edge to the two "opposite" vertices in its sharing triangles (used to smooth edge vertices)
            Dictionary<StructuralEdge, List<int>> edgeOppositeVertices = new Dictionary<StructuralEdge, List<int>>();
            // Map an edge to its newly generated midpoint vertex index
            Dictionary<StructuralEdge, int> edgeToNewVertexMap = new Dictionary<StructuralEdge, int>();

            // Populated structures from input triangles
            for (int i = 0; i < inputMesh.TriangleIndices.Count; i += 3)
            {
                int v0 = inputMesh.TriangleIndices[i];
                int v1 = inputMesh.TriangleIndices[i + 1];
                int v2 = inputMesh.TriangleIndices[i + 2];

                // Build vertex adjacency lists
                vertexNeighbors[v0].Add(v1); vertexNeighbors[v0].Add(v2);
                vertexNeighbors[v1].Add(v0); vertexNeighbors[v1].Add(v2);
                vertexNeighbors[v2].Add(v0); vertexNeighbors[v2].Add(v1);

                // Register edge definitions
                RegisterEdgeData(new StructuralEdge(v0, v1), v2, edgeOppositeVertices);
                RegisterEdgeData(new StructuralEdge(v1, v2), v0, edgeOppositeVertices);
                RegisterEdgeData(new StructuralEdge(v2, v0), v1, edgeOppositeVertices);
            }

            // Prepare collections for our new mesh geometry
            Point3DCollection newPositions = new Point3DCollection();
            // Pre-fill the array to hold original vertex positions while keeping spatial index consistency
            for (int i = 0; i < originalVertexCount; i++) newPositions.Add(new Point3D());

            // --- PHASE 1: Generate and Smooth Edge Vertices (Midpoints) ---
            foreach (var pair in edgeOppositeVertices)
            {
                StructuralEdge edge = pair.Key;
                List<int> opposites = pair.Value;

                Point3D vA = inputMesh.Positions[edge.V1];
                Point3D vB = inputMesh.Positions[edge.V2];
                Point3D smoothedEdgePt;

                if (opposites.Count == 2)
                {
                    // Interior edge: Weighted mix using standard Loop mask (3/8 * (A + B) + 1/8 * (C + D))
                    Point3D vC = inputMesh.Positions[opposites[0]];
                    Point3D vD = inputMesh.Positions[opposites[1]];

                    smoothedEdgePt = new Point3D(
                        3.0 / 8.0 * (vA.X + vB.X) + 1.0 / 8.0 * (vC.X + vD.X),
                        3.0 / 8.0 * (vA.Y + vB.Y) + 1.0 / 8.0 * (vC.Y + vD.Y),
                        3.0 / 8.0 * (vA.Z + vB.Z) + 1.0 / 8.0 * (vC.Z + vD.Z)
                    );
                }
                else
                {
                    // Boundary edge fallback rule: Simply place the new vertex at the midpoint (1/2 * (A + B))
                    smoothedEdgePt = new Point3D(
                        0.5 * (vA.X + vB.X),
                        0.5 * (vA.Y + vB.Y),
                        0.5 * (vA.Z + vB.Z)
                    );
                }

                int newIndex = newPositions.Count;
                edgeToNewVertexMap[edge] = newIndex;
                newPositions.Add(smoothedEdgePt);
            }

            // --- PHASE 2: Smooth Original Vertices ---
            for (int i = 0; i < originalVertexCount; i++)
            {
                int valence = vertexNeighbors[i].Count;
                Point3D originalPos = inputMesh.Positions[i];

                if (valence < 3)
                {
                    // Fallback for boundaries/degenerate topologies
                    newPositions[i] = originalPos;
                    continue;
                }

                // Calculate Warren's smoothing beta weight parameter
                double beta = (1.0 / valence) * (5.0 / 8.0 - Math.Pow(3.0 / 8.0 + Math.Cos(2.0 * Math.PI / valence) / 4.0, 2));

                double sumX = 0, sumY = 0, sumZ = 0;
                foreach (int neighborIndex in vertexNeighbors[i])
                {
                    Point3D nPos = inputMesh.Positions[neighborIndex];
                    sumX += nPos.X;
                    sumY += nPos.Y;
                    sumZ += nPos.Z;
                }

                // Compute standard Loop weighted position: (1 - n*beta) * Original + beta * Sum(Neighbors)
                double selfWeight = 1.0 - (valence * beta);
                newPositions[i] = new Point3D(
                    selfWeight * originalPos.X + beta * sumX,
                    selfWeight * originalPos.Y + beta * sumY,
                    selfWeight * originalPos.Z + beta * sumZ
                );
            }

            // --- PHASE 3: Reconnect Topology (1 Triangle -> 4 Triangles) ---
            Int32Collection newIndices = new Int32Collection();

            for (int i = 0; i < inputMesh.TriangleIndices.Count; i += 3)
            {
                int v0 = inputMesh.TriangleIndices[i];
                int v1 = inputMesh.TriangleIndices[i + 1];
                int v2 = inputMesh.TriangleIndices[i + 2];

                // Fetch the generated midpoint indices
                int m01 = edgeToNewVertexMap[new StructuralEdge(v0, v1)];
                int m12 = edgeToNewVertexMap[new StructuralEdge(v1, v2)];
                int m20 = edgeToNewVertexMap[new StructuralEdge(v2, v0)];

                // Sub-Triangle 1 (Corner 0)
                newIndices.Add(v0); newIndices.Add(m01); newIndices.Add(m20);
                // Sub-Triangle 2 (Corner 1)
                newIndices.Add(m01); newIndices.Add(v1); newIndices.Add(m12);
                // Sub-Triangle 3 (Corner 2)
                newIndices.Add(m20); newIndices.Add(m12); newIndices.Add(v2);
                // Sub-Triangle 4 (Center Piece)
                newIndices.Add(m01); newIndices.Add(m12); newIndices.Add(m20);
            }

            // Construct and return the subdivided WPF mesh layout
            MeshGeometry3D outputMesh = new MeshGeometry3D
            {
                Positions = newPositions,
                TriangleIndices = newIndices
            };

            return outputMesh;
        }

        private static void RegisterEdgeData(StructuralEdge edge, int oppositeVertex, Dictionary<StructuralEdge, List<int>> dict)
        {
            if (!dict.TryGetValue(edge, out var list))
            {
                list = new List<int>();
                dict[edge] = list;
            }
            list.Add(oppositeVertex);
        }

        // Represents an undirected edge between two vertices
        private struct StructuralEdge : IEquatable<StructuralEdge>
        {
            public readonly int V1;
            public readonly int V2;

            public StructuralEdge(int v1, int v2)
            {
                V1 = Math.Min(v1, v2);
                V2 = Math.Max(v1, v2);
            }

            public bool Equals(StructuralEdge other) => V1 == other.V1 && V2 == other.V2;

            public override bool Equals(object obj) => obj is StructuralEdge other && Equals(other);

            public override int GetHashCode() => V1.GetHashCode() ^ V2.GetHashCode();
        }
    }
}