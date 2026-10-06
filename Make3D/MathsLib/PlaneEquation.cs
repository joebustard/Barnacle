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
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MathsLib
{
    public class PlaneEquation
    {
        public double A;
        public double B;
        public double C;
        public double D;

        public Point3D Origin;

        public PlaneEquation(double a, double b, double c, double x, double y, double z)
        {
            A = a;
            B = b;
            C = c;
            Origin = new Point3D(x, y, z);
            D = -(A * Origin.X + B * Origin.Y + C * Origin.Z);
        }

        public PlaneEquation()
        {
            A = 0.0;
            B = 0.0;
            C = 0.0;
            D = 0.0;
            Origin = new Point3D(0, 0, 0);
        }

        public PlaneEquation(double a, double b, double c, Point3D o)
        {
            A = a;
            B = b;
            C = c;
            Origin = new Point3D(o.X, o.Y, o.Z);
            D = -(A * Origin.X + B * Origin.Y + C * Origin.Z);
        }

        private const double epsilon = 1.0e-9;

        public double WhichSideIsPointOn(double x, double y, double z)
        {
            double res = 0;
            double v = (A * x) + (B * y) + (C * z) + D;
            if (v < -epsilon)
            {
                res = -1;
            }
            if (v > epsilon)
            {
                res = 1;
            }
            return res;
        }

        public double WhichSideIsPointOn(Point3D p)
        {
            return WhichSideIsPointOn(p.X, p.Y, p.Z);
        }

        private double Dot(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public bool LineIntercepts(Point3D p0, Point3D p1, out Point3D op)
        {
            bool res = false;
            op = new Point3D();
            op.X = 0;
            op.Y = 0;
            op.Z = 0;
            Vector3D lineVector = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D planeNormal = new Vector3D(A, B, C);

            // Calculate the dot product of the plane normal and line direction
            double denominator = Dot(planeNormal, lineVector);

            // If denominator is 0 (or very close to it), the line is parallel to the plane
            if (Math.Abs(denominator) > 1.0e-9)
            {
                // Solve for t: (A*x0 + B*y0 + C*z0 + D) / (A*vx + B*vy + C*vz)
                // Using a negative sign because the standard plane equation is Ax + By + Cz + D = 0
                double t = -(Dot(planeNormal, new Vector3D(p0.X, p0.Y, p0.Z)) + D) / denominator;

                // Calculate the actual 3D intersection point coordinates using P(t) = P0 + t*V
                op.X = p0.X + t * lineVector.X;
                op.Y = p0.Y + t * lineVector.Y;
                op.Z = p0.Z + t * lineVector.Z;
                res = true;
            }
            return res;
        }

        /// <summary>
        /// Calculates a list of 3D points on a plane at a specific distance from the center.
        /// </summary>
        /// <param name="planeCenter">The center point of the plane.</param>
        /// <param name="planeNormal">The normal vector defining the plane's orientation.</param>
        /// <param name="distance">The exact radius/distance from the center.</param>
        /// <param name="pointCount">How many points you want to generate along the perimeter.</param>
        /// <returns>A list of Vector3 points lying on the plane.</returns>
        public Point3DCollection GetPointsOnPlane(double distance, int pointCount)
        {
            Vector3D planeCenter = new Vector3D(Origin.X, Origin.Y, Origin.Z);
            Vector3D planeNormal = new Vector3D(A, B, C);
            Point3DCollection points = new Point3DCollection();

            if (pointCount <= 0 || distance < 0)
                return points;

            // 1. Ensure the normal vector is normalized (length of 1)
            planeNormal.Normalize();

            // 2. Find a vector that is NOT parallel to the normal vector
            // We check against the absolute values to avoid division by zero or precision issues
            Vector3D helperVector = (Math.Abs(planeNormal.X) < 0.9f) ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);

            // 3. Create two orthogonal (perpendicular) axes that lie perfectly on the plane
            Vector3D localX = Vector3D.CrossProduct(planeNormal, helperVector);
            localX.Normalize();
            Vector3D localY = Vector3D.CrossProduct(planeNormal, localX);
            localY.Normalize();

            // 4. Calculate the points using trigonometry
            double angleStep = (2 * Math.PI) / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                double angle = i * angleStep;

                // Compute the local offsets
                double cos = Math.Cos(angle) * distance;
                double sin = Math.Sin(angle) * distance;

                // Combine the local offsets with the plane center to get the final 3D coordinates
                Vector3D pointOnPlane = planeCenter + (localX * cos) + (localY * sin);
                points.Add(new Point3D(pointOnPlane.X, pointOnPlane.Y, pointOnPlane.Z));
            }

            return points;
        }
    }
}