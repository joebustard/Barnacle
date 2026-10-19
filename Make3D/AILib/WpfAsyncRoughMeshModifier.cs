// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AILib
{
    public enum NoiseType
    {
        Random,
        Perlin
    }

    public static class AsyncRoughMeshModifier
    {
        /// <summary>
        /// Asynchronously subdivides a mesh, ensures baseline normal math is computed,
        /// and applies vertex roughness with cancellation support.
        /// </summary>
        public static async Task<MeshGeometry3D> MakeMeshRoughAsync(
            MeshGeometry3D inputMesh,
            int subdivisionDepth,
            NoiseType noiseType,
            double intensity,
            double frequency,
            CancellationToken token)
        {
            if (inputMesh == null)
            {
                return null;
            }

            // 1. EXTRACT DATA: Safely copy properties on the UI thread before backgrounding.
            var dataBuffer = new MeshDataBuffer();
            foreach (var p in inputMesh.Positions)
            {
                dataBuffer.Positions.Add(p);
            }

            foreach (var i in inputMesh.TriangleIndices)
            {
                dataBuffer.TriangleIndices.Add(i);
            }

            token.ThrowIfCancellationRequested();

            // 2. OFF-THREAD PROCESSING: Hand off heavy computation to a background worker thread.
            MeshDataBuffer processedBuffer = await Task.Run(() => {
                // Subdivide geometry on worker thread with explicit cancellation monitoring
                MeshDataBuffer workingData = SubdivideMeshPure(dataBuffer, subdivisionDepth, token);

                // FIX: Recalculate baseline normal directions first, in case the original mesh didn't have any.
                // We must know what path is "outward" from the surface before displacing vertices.
                CalculateNormalsPure(workingData);
                token.ThrowIfCancellationRequested();

                Random rand = new Random();
                PerlinNoise perlin = new PerlinNoise(rand.Next());

                List<Point3D> displacedPositions = new List<Point3D>(workingData.Positions.Count);

                // Apply vertex displacement along newly generated baseline normals
                for (int i = 0; i < workingData.Positions.Count; i++)
                {
                    // Check cancellation periodically inside the loop
                    if (i % 500 == 0)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    Point3D vertex = workingData.Positions[i];
                    Vector3D normal = workingData.Normals[i];
                    if (normal.Length > 0)
                    {
                        normal.Normalize();
                    }

                    double offset = 0;

                    if (noiseType == NoiseType.Random)
                    {
                        offset = (rand.NextDouble() * 2.0 - 1.0) * intensity;
                    }
                    else if (noiseType == NoiseType.Perlin)
                    {
                        double noiseVal = perlin.Noise(vertex.X * frequency, vertex.Y * frequency, vertex.Z * frequency);
                        offset = noiseVal * intensity;
                    }

                    double newX = vertex.X + (normal.X * offset);
                    double newY = vertex.Y + (normal.Y * offset);
                    double newZ = vertex.Z + (normal.Z * offset);

                    displacedPositions.Add(new Point3D(newX, newY, newZ));
                }

                workingData.Positions = displacedPositions;

                token.ThrowIfCancellationRequested();

                // Final calculation pass so lighting reflects off the rough features correctly
                CalculateNormalsPure(workingData);

                return workingData;
            }, token);

            // 3. UI MARSHALING: Return to the main thread and re-package data back into WPF objects.
            token.ThrowIfCancellationRequested();

            MeshGeometry3D outputMesh = new MeshGeometry3D();
            outputMesh.Positions = new Point3DCollection(processedBuffer.Positions);
            outputMesh.TriangleIndices = new Int32Collection(processedBuffer.TriangleIndices);
            outputMesh.Normals = new Vector3DCollection(processedBuffer.Normals);

            return outputMesh;
        }

        #region Thread-Safe Pure Math Operations

        private const double lim = 0.000001;

        private static int AddVertexPure(MeshDataBuffer source, MeshDataBuffer target, int index)
        {
            int res = Find(target.Positions, source.Positions[index]);
            if (res == -1)
            {
                target.Positions.Add(source.Positions[index]);
                res = target.Positions.Count - 1;
            }
            return res;
        }

        /// <summary>
        /// Recomputes weighted vertex normals dynamically from basic triangle topology.
        /// </summary>
        private static void CalculateNormalsPure(MeshDataBuffer mesh)
        {
            Vector3D[] normalsArray = new Vector3D[mesh.Positions.Count];

            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                int i0 = mesh.TriangleIndices[i];
                int i1 = mesh.TriangleIndices[i + 1];
                int i2 = mesh.TriangleIndices[i + 2];

                Vector3D v1 = mesh.Positions[i1] - mesh.Positions[i0];
                Vector3D v2 = mesh.Positions[i2] - mesh.Positions[i0];
                Vector3D normal = Vector3D.CrossProduct(v1, v2);

                // Accumulate shared face normals on respective shared points
                normalsArray[i0] += normal;
                normalsArray[i1] += normal;
                normalsArray[i2] += normal;
            }

            for (int i = 0; i < normalsArray.Length; i++)
            {
                if (normalsArray[i].Length > 0)
                {
                    normalsArray[i].Normalize();
                }
            }

            mesh.Normals = new List<Vector3D>(normalsArray);
        }

        private static int Find(List<Point3D> target, Point3D p)
        {
            int res = -1;
            for (int i = 0; i < target.Count; i++)
            {
                if ((Math.Abs(target[i].X - p.X) < lim) &&
                     (Math.Abs(target[i].Y - p.Y) < lim) &&
                     (Math.Abs(target[i].Z - p.Z) < lim))
                {
                    res = i; break;
                }
            }
            return res;
        }

        private static int GetMidpointIndexPure(MeshDataBuffer source, MeshDataBuffer target, int index1, int index2, Dictionary<long, int> cache)
        {
            long first = Math.Min(index1, index2);
            long second = Math.Max(index1, index2);
            long key = (first << 32) + second;

            if (cache.TryGetValue(key, out int cachedIndex))
            {
                return cachedIndex;
            }

            Point3D p1 = source.Positions[index1];
            Point3D p2 = source.Positions[index2];
            Point3D midpoint = new Point3D((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0, (p1.Z + p2.Z) / 2.0);

            target.Positions.Add(midpoint);
            int newIndex = target.Positions.Count - 1;
            cache[key] = newIndex;
            return newIndex;
        }

        private static MeshDataBuffer SubdivideMeshPure(MeshDataBuffer input, int depth, CancellationToken token)
        {
            MeshDataBuffer current = input;

            for (int d = 0; d < depth; d++)
            {
                token.ThrowIfCancellationRequested();

                MeshDataBuffer next = new MeshDataBuffer();
                Dictionary<long, int> midpointCache = new Dictionary<long, int>();

                for (int i = 0; i < current.TriangleIndices.Count; i += 3)
                {
                    if (i % 300 == 0)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    int i0 = current.TriangleIndices[i];
                    int i1 = current.TriangleIndices[i + 1];
                    int i2 = current.TriangleIndices[i + 2];

                    int m01 = GetMidpointIndexPure(current, next, i0, i1, midpointCache);
                    int m12 = GetMidpointIndexPure(current, next, i1, i2, midpointCache);
                    int m20 = GetMidpointIndexPure(current, next, i2, i0, midpointCache);

                    int ni0 = AddVertexPure(current, next, i0);
                    int ni1 = AddVertexPure(current, next, i1);
                    int ni2 = AddVertexPure(current, next, i2);

                    next.TriangleIndices.AddRange(new[] { ni0, m01, m20 });
                    next.TriangleIndices.AddRange(new[] { m01, ni1, m12 });
                    next.TriangleIndices.AddRange(new[] { m20, m12, ni2 });
                    next.TriangleIndices.AddRange(new[] { m01, m12, m20 });
                }
                current = next;
            }

            return current;
        }

        #endregion Thread-Safe Pure Math Operations
    }

    /// <summary>
    /// Thread-safe data structure to pass geometric data safely across background threads.
    /// This bypasses WPF's single-threaded apartment (STA) limitations.
    /// </summary>
    public class MeshDataBuffer
    {
        public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
        public List<Point3D> Positions { get; set; } = new List<Point3D>();
        public List<int> TriangleIndices { get; set; } = new List<int>();
    }

    /// <summary>
    /// Thread-safe standard 3D Ken Perlin Noise implementation.
    /// </summary>
    public class PerlinNoise
    {
        private readonly int[] p = new int[512];

        public PerlinNoise(int seed)
        {
            Random rand = new Random(seed);
            List<int> permutation = new List<int>();
            for (int i = 0; i < 256; i++)
            {
                permutation.Add(i);
            }

            for (int i = 255; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int temp = permutation[i];
                permutation[i] = permutation[j];
                permutation[j] = temp;
            }

            for (int i = 0; i < 256; i++)
            {
                p[i] = permutation[i];
                p[256 + i] = permutation[i];
            }
        }

        public double Noise(double x, double y, double z)
        {
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;
            int Z = (int)Math.Floor(z) & 255;

            x -= Math.Floor(x);
            y -= Math.Floor(y);
            z -= Math.Floor(z);
            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);

            int A = p[X] + Y;
            int AA = p[A] + Z; int AB = p[A + 1] + Z;

            int B = p[X + 1] + Y;
            int BA = p[B] + Z;
            int BB = p[B + 1] + Z;
            return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA], x, y, z), Grad(p[BA], x - 1, y, z)),
            Lerp(u, Grad(p[AB], x, y - 1, z), Grad(p[BB], x - 1, y - 1, z))),
            Lerp(v, Lerp(u, Grad(p[AA + 1], x, y, z - 1), Grad(p[BA + 1], x - 1, y, z - 1)),
            Lerp(u, Grad(p[AB + 1], x, y - 1, z - 1), Grad(p[BB + 1], x - 1, y - 1, z - 1))));
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        private static double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static double Lerp(double t, double a, double b) => a + t * (b - a);
    }
}