using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace AiWpf3D
{
    public static class WpfBallPivotingMst
    {
        private const double Epsilon = 1e-6;

        /// <summary>
        /// Reconstructs the 3D mesh surface using Ball Pivoting, calculating unoriented normals
        /// via Local PCA and propagating direction globally using a Minimum Spanning Tree (MST).
        /// </summary>
        public static MeshGeometry3D ReconstructWithMstNormals(MeshGeometry3D inputMesh, double ballRadius, int knnNeighbors = 14)
        {
            if (inputMesh == null) throw new ArgumentNullException(nameof(inputMesh));
            Point3DCollection positions = inputMesh.Positions;

            // Step 1: Compute and orient point normals via MST
            Vector3DCollection orientedNormals = ComputeMstOrientedNormals(positions, ballRadius, knnNeighbors);

            // Step 2: Initialize BPA Environment
            MeshGeometry3D mesh = new MeshGeometry3D { Positions = positions.Clone(), Normals = orientedNormals };
            var indices = new System.Windows.Media.Int32Collection();
            double cellSize = ballRadius * 2.0;
            var grid = new SpatialGrid(positions, cellSize);

            var vertexUsedCount = new int[positions.Count];
            var edgeTracker = new HashSet<string>();
            var front = new Queue<FrontEdge>();

            // --- BPA STAGE 1: Seed Triangles ---
            for (int i = 0; i < positions.Count; i++)
            {
                if (vertexUsedCount[i] > 0) continue;
                var candidates = grid.GetNeighbors(positions[i], ballRadius * 2.0);
                bool seedFound = false;

                for (int jIndex = 0; jIndex < candidates.Count; jIndex++)
                {
                    int j = candidates[jIndex];
                    if (i == j || vertexUsedCount[j] > 0) continue;
                    for (int kIndex = 0; kIndex < candidates.Count; kIndex++)

                    {
                        int k = candidates[kIndex];
                        if (i == k || j == k || vertexUsedCount[k] > 0) continue;

                        if (TryFindBallCenter(positions[i], positions[j], positions[k], ballRadius, out Point3D center))
                        {
                            Vector3D triNormal = Vector3D.CrossProduct(positions[j] - positions[i], positions[k] - positions[i]);
                            if (Vector3D.DotProduct(triNormal, orientedNormals[i]) < 0)
                            {
                                int tmp = j; j = k; k = tmp;
                                triNormal = -triNormal;
                            }

                            if (IsBallEmpty(center, ballRadius, positions, grid, i, j, k))
                            {
                                indices.Add(i); indices.Add(j); indices.Add(k);
                                vertexUsedCount[i]++; vertexUsedCount[j]++; vertexUsedCount[k]++;
                                PushEdge(new FrontEdge(i, j, k, center), front, edgeTracker);
                                PushEdge(new FrontEdge(j, k, i, center), front, edgeTracker);
                                PushEdge(new FrontEdge(k, i, j, center), front, edgeTracker);
                                seedFound = true; break;
                            }
                        }
                    }
                    if (seedFound) break;
                }
            }

            // --- BPA STAGE 2: Front Expansion ---
            while (front.Count > 0)
            {
                var edge = front.Dequeue();
                string edgeKey = GetEdgeKey(edge.V1, edge.V2);
                if (!edgeTracker.Contains(edgeKey)) continue;

                Point3D p1 = positions[edge.V1]; Point3D p2 = positions[edge.V2];
                Point3D midpoint = new Point3D((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5, (p1.Z + p2.Z) * 0.5);

                Vector3D edgeDir = p2 - p1; edgeDir.Normalize();
                Vector3D outDir = edge.BallCenter - midpoint; outDir.Normalize();

                double bestAngle = double.MaxValue;
                int bestCandidateIdx = -1;
                Point3D bestBallCenter = new Point3D();

                var neighbors = grid.GetNeighbors(midpoint, ballRadius * 2.0);
                foreach (int candidate in neighbors)
                {
                    if (candidate == edge.V1 || candidate == edge.V2 || candidate == edge.OppositeV) continue;

                    if (TryFindBallCenter(p1, p2, positions[candidate], ballRadius, out Point3D cNew))
                    {
                        Vector3D newNormal = Vector3D.CrossProduct(p2 - p1, positions[candidate] - p1);
                        if (Vector3D.DotProduct(newNormal, orientedNormals[edge.V1]) < 0) continue;

                        Vector3D newOutDir = cNew - midpoint; newOutDir.Normalize();
                        double angle = Math.Acos(Math.Max(-1.0, Math.Min(1.0, Vector3D.DotProduct(outDir, newOutDir))));
                        Vector3D cross = Vector3D.CrossProduct(outDir, newOutDir);
                        if (Vector3D.DotProduct(cross, edgeDir) < 0) angle = (Math.PI * 2.0) - angle;

                        if (angle < bestAngle && IsBallEmpty(cNew, ballRadius, positions, grid, edge.V1, edge.V2, candidate))
                        {
                            bestAngle = angle; bestCandidateIdx = candidate; bestBallCenter = cNew;
                        }
                    }
                }

                if (bestCandidateIdx != -1 && vertexUsedCount[bestCandidateIdx] < 4)
                {
                    edgeTracker.Remove(edgeKey);
                    indices.Add(edge.V2); indices.Add(edge.V1); indices.Add(bestCandidateIdx);
                    vertexUsedCount[edge.V1]++; vertexUsedCount[edge.V2]++; vertexUsedCount[bestCandidateIdx]++;
                    PushEdge(new FrontEdge(edge.V2, bestCandidateIdx, edge.V1, bestBallCenter), front, edgeTracker);
                    PushEdge(new FrontEdge(bestCandidateIdx, edge.V1, edge.V2, bestBallCenter), front, edgeTracker);
                }
            }

            mesh.TriangleIndices = indices;
            return mesh;
        }

        private static Vector3D CalculateSmallestEigenvector(double m00, double m01, double m02, double m11, double m12, double m22)
        {
            double v0 = 1.0, v1 = 1.0, v2 = 1.0;
            for (int iter = 0; iter < 10; iter++)
            {
                double tx = m00 * v0 + m01 * v1 + m02 * v2;
                double ty = m01 * v0 + m11 * v1 + m12 * v2;
                double tz = m02 * v0 + m12 * v1 + m22 * v2;
                double norm = Math.Sqrt(tx * tx + ty * ty + tz * tz);
                if (norm < Epsilon) break;
                v0 = tx / norm; v1 = ty / norm; v2 = tz / norm;
            }
            Vector3D dominantAxis = new Vector3D(v0, v1, v2);
            Vector3D orthoGuess = Math.Abs(dominantAxis.X) < 0.8 ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);
            Vector3D normal = Vector3D.CrossProduct(dominantAxis, orthoGuess);
            normal.Normalize();
            return normal;
        }

        private static Vector3DCollection ComputeMstOrientedNormals(Point3DCollection positions, double radius, int k)
        {
            int n = positions.Count;
            var rawNormals = new Vector3D[n];
            var grid = new SpatialGrid(positions, radius * 2.0);
            var adjacencyList = new List<int>[n];

            // 1. Compute local unoriented PCA normals and store structural neighbors
            for (int i = 0; i < n; i++)
            {
                var neighbors = grid.GetKNearest(positions[i], k);
                adjacencyList[i] = neighbors;

                if (neighbors.Count < 3) { rawNormals[i] = new Vector3D(0, 1, 0); continue; }

                double cx = 0, cy = 0, cz = 0;
                foreach (int idx in neighbors) { cx += positions[idx].X; cy += positions[idx].Y; cz += positions[idx].Z; }
                cx /= neighbors.Count; cy /= neighbors.Count; cz /= neighbors.Count;

                double m00 = 0, m01 = 0, m02 = 0, m11 = 0, m12 = 0, m22 = 0;
                foreach (int idx in neighbors)
                {
                    double dx = positions[idx].X - cx; double dy = positions[idx].Y - cy; double dz = positions[idx].Z - cz;
                    m00 += dx * dx; m01 += dx * dy; m02 += dx * dz; m11 += dy * dy; m12 += dy * dz; m22 += dz * dz;
                }
                rawNormals[i] = CalculateSmallestEigenvector(m00, m01, m02, m11, m12, m22);
            }
            // 2. Build Riemannian Graph edges weighted by normal variation: 1.0 - |NormalA . NormalB|
            var allEdges = new List<GraphEdge>();
            for (int u = 0; u < n; u++)
            {
                foreach (int v in adjacencyList[u])
                {
                    if (u >= v) continue;
                    double dot = Math.Abs(Vector3D.DotProduct(rawNormals[u], rawNormals[v]));
                    double weight = 1.0 - dot; // Higher normal alignment = lower edge cost weight
                    allEdges.Add(new GraphEdge(u, v, weight));
                }
            }
            allEdges.Sort((a, b) => a.Weight.CompareTo(b.Weight));
            // 3. Kruskal's Algorithm to generate the Minimum Spanning Tree
            var mstAdjacency = new List<int>[n];
            for (int i = 0; i < n; i++) mstAdjacency[i] = new List<int>();
            int[] parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
            int Find(int i) { while (parent[i] != i) i = parent[i]; return i; }
            void Union(int i, int j) { parent[Find(i)] = Find(j); }
            foreach (var edge in allEdges)
            {
                int rootU = Find(edge.U); int rootV = Find(edge.V);
                if (rootU != rootV)
                {
                    Union(rootU, rootV);
                    mstAdjacency[edge.U].Add(edge.V);
                    mstAdjacency[edge.V].Add(edge.U);
                }
            }
            // 4. Find the absolute highest node along the Z-axis to seed as "definitely pointing out"
            int seedNode = 0;
            double maxZ = double.MinValue;
            for (int i = 0; i < n; i++)
            {
                if (positions[i].Z > maxZ) { maxZ = positions[i].Z; seedNode = i; }
            }
            if (rawNormals[seedNode].Z < 0) rawNormals[seedNode] = -rawNormals[seedNode];
            // 5. Run Breadth-First Search (BFS) over MST to propagate direction consistently
            var orientedNormals = new Vector3DCollection(new Vector3D[n]);
            var visited = new bool[n];
            var queue = new Queue<int>();
            queue.Enqueue(seedNode);
            visited[seedNode] = true;
            orientedNormals[seedNode] = rawNormals[seedNode];
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                Vector3D currentNormal = orientedNormals[current];
                foreach (int neighbor in mstAdjacency[current])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        Vector3D neighborNormal = rawNormals[neighbor];
                        // If normal alignment points opposite, flip it to preserve orientation flow
                        if (Vector3D.DotProduct(currentNormal, neighborNormal) < 0)
                        {
                            neighborNormal = -neighborNormal;
                        }
                        orientedNormals[neighbor] = neighborNormal;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            // Handle any disconnected components in the neighborhood graph with a baseline default fallback orientation
            for (int i = 0; i < n; i++) if (!visited[i]) orientedNormals[i] = new Vector3D(0, 0, 1);
            return orientedNormals;
        }

        private static string GetEdgeKey(int v1, int v2) => $"{v1}_{v2}";

        private static bool IsBallEmpty(Point3D center, double radius, Point3DCollection points, SpatialGrid grid, int i, int j, int k)
        {
            var insideCandidates = grid.GetNeighbors(center, radius - Epsilon);
            foreach (int idx in insideCandidates) if (idx != i && idx != j && idx != k) return false;
            return true;
        }

        private static void PushEdge(FrontEdge edge, Queue<FrontEdge> queue, HashSet<string> tracker)
        {
            string key = GetEdgeKey(edge.V1, edge.V2);
            string revKey = GetEdgeKey(edge.V2, edge.V1);
            if (tracker.Contains(revKey)) tracker.Remove(revKey);
            else { tracker.Add(key); queue.Enqueue(edge); }
        }

        private static bool TryFindBallCenter(Point3D p1, Point3D p2, Point3D p3, double r, out Point3D center)
        {
            center = new Point3D();
            Vector3D v12 = p2 - p1; Vector3D v13 = p3 - p1;
            Vector3D normal = Vector3D.CrossProduct(v12, v13);
            if (normal.LengthSquared < Epsilon) return false;
            normal.Normalize();
            double e12Sq = v12.LengthSquared; double e13Sq = v13.LengthSquared;
            Vector3D cross123 = Vector3D.CrossProduct(v12, v13);
            Vector3D num = (Vector3D.CrossProduct(v12, cross123) * e13Sq + Vector3D.CrossProduct(cross123, v13) * e12Sq) / (2.0 * cross123.LengthSquared);
            Point3D circumcenter = p1 + num;
            double distSq = (circumcenter - p1).LengthSquared; double rSq = r * r;
            if (distSq > rSq) return false;
            double height = Math.Sqrt(rSq - distSq);
            center = circumcenter + (normal * height);
            return true;
        }

        private class FrontEdge
        {
            public FrontEdge(int v1, int v2, int oppositeV, Point3D ballCenter)
            {
                V1 = v1; V2 = v2; OppositeV = oppositeV; BallCenter = ballCenter;
            }

            public Point3D BallCenter
            {
                get;
            }

            public int OppositeV
            {
                get;
            }

            public int V1
            {
                get;
            }

            public int V2
            {
                get;
            }
        }

        private class GraphEdge
        {
            public GraphEdge(int u, int v, double weight)
            {
                U = u; V = v; Weight = weight;
            }

            public int U
            {
                get;
            }

            public int V
            {
                get;
            }

            public double Weight
            {
                get;
            }
        }

        // A lightweight 3D Voxel Grid to optimize spatial lookups for both KNN and BPA
        private class SpatialGrid
        {
            private readonly Dictionary<string, List<int>> _cells = new Dictionary<string, List<int>>();
            private readonly double _cellSize;
            private readonly Point3DCollection _points;

            public SpatialGrid(Point3DCollection points, double cellSize)
            {
                _points = points; _cellSize = cellSize;
                for (int i = 0; i < points.Count; i++)
                {
                    string key = GetKey(points[i]);
                    if (!_cells.TryGetValue(key, out var list)) { list = new List<int>(); _cells[key] = list; }
                    list.Add(i);
                }
            }

            public List<int> GetKNearest(Point3D center, int k)
            {
                var candidates = new List<(int index, double distSq)>();
                double searchRadius = _cellSize;
                while (candidates.Count < k && searchRadius < _cellSize * 10)
                {
                    candidates.Clear();
                    var indices = GetNeighbors(center, searchRadius);
                    foreach (int idx in indices) candidates.Add((idx, (_points[idx] - center).LengthSquared));
                    searchRadius *= 1.5;
                }
                candidates.Sort((a, b) => a.distSq.CompareTo(b.distSq));
                var result = new List<int>();
                for (int i = 0; i < Math.Min(k, candidates.Count); i++) result.Add(candidates[i].index);
                return result;
            }

            public List<int> GetNeighbors(Point3D center, double radius)
            {
                var neighbors = new List<int>();
                int minX = (int)Math.Floor((center.X - radius) / _cellSize);
                int maxX = (int)Math.Floor((center.X + radius) / _cellSize);
                int minY = (int)Math.Floor((center.Y - radius) / _cellSize);
                int maxY = (int)Math.Floor((center.Y + radius) / _cellSize);
                int minZ = (int)Math.Floor((center.Z - radius) / _cellSize);
                int maxZ = (int)Math.Floor((center.Z + radius) / _cellSize);

                for (int x = minX; x <= maxX; x++)
                    for (int y = minY; y <= maxY; y++)
                        for (int z = minZ; z <= maxZ; z++)
                            if (_cells.TryGetValue($"{x}_{y}_{z}", out var list))
                                foreach (int idx in list)
                                    if ((_points[idx] - center).LengthSquared <= radius * radius) neighbors.Add(idx);
                return neighbors;
            }

            private string GetKey(Point3D p)
            {
                int ix = (int)Math.Floor(p.X / _cellSize);
                int iy = (int)Math.Floor(p.Y / _cellSize);
                int iz = (int)Math.Floor(p.Z / _cellSize);
                return $"{ix}_{iy}_{iz}";
            }
        }
    }
}