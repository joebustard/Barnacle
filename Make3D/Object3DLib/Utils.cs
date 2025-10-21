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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Object3DLib
{
    internal class Utils
    {
        private const double kEpsilon = 0.000000001F;

        public static bool RayTriangleIntersect(Vector3D orig, Vector3D dir,
                                                 Vector3D v0, Vector3D v1, Vector3D v2,
                                                 out double t)
        {
            t = -1;
            // compute the plane's normal
            Vector3D v0v1 = v1 - v0;
            Vector3D v0v2 = v2 - v0;
            // no need to normalize
            Vector3D N = Vector3D.CrossProduct(v0v1, v0v2); // N
            double area2 = N.Length;

            // Step 1: finding P

            // check if the ray and plane are parallel.
            double NdotRayDirection = Vector3D.DotProduct(N, dir);
            if (Math.Abs(NdotRayDirection) < kEpsilon) // almost 0
                return false; // they are parallel, so they don't intersect!

            // compute d parameter using equation 2
            double d = Vector3D.DotProduct(-N, v0);

            // compute t (equation 3)
            t = -(Vector3D.DotProduct(N, orig) + d) / NdotRayDirection;

            // check if the triangle is behind the ray
            if (t < 0) return false; // the triangle is behind

            // compute the,  intersection point using equation 1
            Vector3D P = orig + t * dir;

            // Step 2: inside-outside test
            Vector3D C; // vector perpendicular to triangle's plane

            // edge 0
            Vector3D edge0 = v1 - v0;
            Vector3D vp0 = P - v0;
            C = Vector3D.CrossProduct(edge0, vp0);
            if (Vector3D.DotProduct(N, C) < 0) return false; // P is on the right side,

            // edge 1
            Vector3D edge1 = v2 - v1;
            Vector3D vp1 = P - v1;
            C = Vector3D.CrossProduct(edge1, vp1);
            if (Vector3D.DotProduct(N, C) < 0) return false; // P is on the right side

            // edge 2
            Vector3D edge2 = v0 - v2;
            Vector3D vp2 = P - v2;
            C = Vector3D.CrossProduct(edge2, vp2);
            if (Vector3D.DotProduct(N, C) < 0) return false; // P is on the right side;

            // convert t to a distance
            t = (Math.Sqrt((t * dir.X) * (t * dir.X) + (t * dir.Y) * (t * dir.Y) + (t * dir.Z) * (t * dir.Z)));
            //System.Diagnostics.Debug.WriteLine($" P ={P.X},{P.Y},{P.Z}" );
            return true; // this ray hits the triangle
        }
    }
}