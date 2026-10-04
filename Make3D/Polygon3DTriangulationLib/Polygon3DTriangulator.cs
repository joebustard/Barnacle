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

namespace Polygon3DTriangulationLib
{
     using System;
     using System.Collections.Generic;
     using System.Linq;
     using System.Threading;
     using System.Threading.Tasks;
    using System.Numerics;

    public class Polygon3DTriangulator
    {
        // Represents a index triplet for a triangle
        public struct Triangle
        {
            public int V1, V2, V3;
            public Triangle(int v1, int v2, int v3)
            {
                V1 = v1; V2 = v2; V3 = v3;
            }
        }

        /// <summary>
        /// Triangulates a simple (non-self-intersecting) 3D planar polygon.
        /// </summary>
        public  List<Triangle> Triangulate3D(List<Vector3> vertices)
        {
            if (vertices == null || vertices.Count < 3)
                return new List<Triangle>();

            // 1. Compute the surface normal using Newell's Method
            Vector3 normal = Vector3.Zero;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];
                normal.X += (current.Y - next.Y) * (current.Z + next.Z);
                normal.Y += (current.Z - next.Z) * (current.X + next.X);
                normal.Z += (current.X - next.X) * (current.Y + next.Y);
            }
            normal = Vector3.Normalize(normal);

            // 2. Determine the dominant axis to project onto (drop the largest normal component)
            float absX = Math.Abs(normal.X);
            float absY = Math.Abs(normal.Y);
            float absZ = Math.Abs(normal.Z);

            List<Vector2> vertices2D = new List<Vector2>(vertices.Count);

            // 3. Project 3D points to 2D
            for (int i = 0; i < vertices.Count; i++)
            {
                if (absZ >= absX && absZ >= absY) // Drop Z axis
                    vertices2D.Add(new Vector2(vertices[i].X, vertices[i].Y));
                else if (absY >= absX && absY >= absZ) // Drop Y axis
                    vertices2D.Add(new Vector2(vertices[i].X, vertices[i].Z));
                else // Drop X axis
                    vertices2D.Add(new Vector2(vertices[i].Y, vertices[i].Z));
            }

            // 4. Run Ear Clipping on the 2D projected polygon
            return Triangulate2DEarClipping(vertices2D);
        }

        private static List<Triangle> Triangulate2DEarClipping(List<Vector2> vertices)
        {
            List<Triangle> triangles = new List<Triangle>();
            List<int> indexList = new List<int>();
            for (int i = 0; i < vertices.Count; i++) indexList.Add(i);

            // Check overall winding order (true if clockwise)
            bool isClockwise = GetWindingOrder(vertices, indexList) > 0;

            int iterations = 0;
            int maxIterations = indexList.Count * indexList.Count;

            while (indexList.Count >= 3 && iterations < maxIterations)
            {
                iterations++;
                for (int i = 0; i < indexList.Count; i++)
                {
                    int prevIdx = indexList[(i - 1 + indexList.Count) % indexList.Count];
                    int currIdx = indexList[i];
                    int nextIdx = indexList[(i + 1) % indexList.Count];

                    if (IsEar(vertices, prevIdx, currIdx, nextIdx, indexList, isClockwise))
                    {
                        triangles.Add(new Triangle(prevIdx, currIdx, nextIdx));
                        indexList.RemoveAt(i);
                        break;
                    }
                }
            }

            return triangles;
        }

        private static float GetWindingOrder(List<Vector2> vertices, List<int> indices)
        {
            float area = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                Vector2 v1 = vertices[indices[i]];
                Vector2 v2 = vertices[indices[(i + 1) % indices.Count]];
                area += (v2.X - v1.X) * (v2.Y + v1.Y);
            }
            return area;
        }

        private static bool IsEar(List<Vector2> vertices, int p, int c, int n, List<int> indexList, bool isClockwise)
        {
            Vector2 va = vertices[p];
            Vector2 vb = vertices[c];
            Vector2 vc = vertices[n];

            // Cross product to check if the vertex forms a convex angle matching the winding order
            float cross = (vb.X - va.X) * (vc.Y - vb.Y) - (vb.Y - va.Y) * (vc.X - vb.X);
            if (isClockwise && cross > 0) return false;
            if (!isClockwise && cross < 0) return false;

            // Check if any other vertex lies inside this candidate triangle
            for (int i = 0; i < indexList.Count; i++)
            {
                int idx = indexList[i];
                if (idx == p || idx == c || idx == n) continue;

                if (PointInTriangle(vertices[idx], va, vb, vc))
                    return false; // Not an ear if a point is inside
            }

            return true;
        }

        private static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1 = Sign(pt, v1, v2);
            float d2 = Sign(pt, v2, v3);
            float d3 = Sign(pt, v3, v1);

            bool hasNeg = (d1 < 0) || d2 < 0 || d3 < 0;
            bool hasPos = (d1 > 0) || d2 > 0 || d3 > 0;

            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
    }


}
