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

using System.Numerics;

namespace MathLib
{
    public static class TriangleDistance
    {
        // Finds the closest point on triangle (a, b, c) to point p
        public static Vector3 ClosestPoint(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a, ac = c - a, ap = p - a;
            float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) return a;

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) return b;

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) return c;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f) return a + (d1 / (d1 - d3)) * ab;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f) return a + (d2 / (d2 - d6)) * ac;

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
                return b + ((d4 - d3) / ((d4 - d3) + (d5 - d6))) * (c - b);

            float denom = 1f / (va + vb + vc);
            return a + (vb * denom) * ab + (vc * denom) * ac;
        }

        // Calculates the final distance
        public static float GetDistance(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Distance(p, ClosestPoint(p, a, b, c));
        }

        public static float GetDistance(Vector3 p1, Vector3 p2)
        {
            return Vector3.Distance(p1, p2);
        }

        private static Vector3 Normal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 v = Vector3.Cross(v1 - v0, v2 - v0);
            return v;
        }
    }
}