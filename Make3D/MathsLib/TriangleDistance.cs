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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace MathsLib
{
    public static class TriangleDistance
    {
        // Finds the closest point on triangle (a, b, c) to point p
        public static Vector3D ClosestPoint(Vector3D p, Vector3D a, Vector3D b, Vector3D c)
        {
            Vector3D ab = b - a, ac = c - a, ap = p - a;
            double d1 = Dot(ab, ap), d2 = Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f) return a;

            Vector3D bp = p - b;
            double d3 = Dot(ab, bp), d4 = Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3) return b;

            Vector3D cp = p - c;
            double d5 = Dot(ab, cp), d6 = Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6) return c;

            double vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f) return a + (d1 / (d1 - d3)) * ab;

            double vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f) return a + (d2 / (d2 - d6)) * ac;

            double va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
                return b + ((d4 - d3) / ((d4 - d3) + (d5 - d6))) * (c - b);

            double denom = 1f / (va + vb + vc);
            return a + (vb * denom) * ab + (vc * denom) * ac;
        }

        // Calculates the final distance
        public static double GetDistance(Vector3D p, Vector3D a, Vector3D b, Vector3D c)
        {
            return Distance(p, ClosestPoint(p, a, b, c));
        }

        public static double GetDistance(Vector3D p1, Vector3D p2)
        {
            return Distance(p1, p2);
        }

        private static Vector3D Cross(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        private static double Distance(Vector3D a, Vector3D b)
        {
            double xd = b.X - a.X;
            double yd = b.Y - a.Y;
            double zd = b.Z - a.Z;
            return Math.Sqrt(xd * xd + yd * yd + zd * zd);
        }

        private static double Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        private static Vector3D Normal(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            Vector3D v = Cross(v1 - v0, v2 - v0);
            return v;
        }
    }
}