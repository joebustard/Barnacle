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
        private  double Dot(Vector3D a, Vector3D b)
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
                double t = -(Dot(planeNormal, new Vector3D(p0.X,p0.Y,p0.Z)) + D) / denominator;

                // Calculate the actual 3D intersection point coordinates using P(t) = P0 + t*V
                op.X = p0.X + t * lineVector.X;
                op.Y = p0.Y + t * lineVector.Y;
                op.Z = p0.Z + t * lineVector.Z;
                res = true;
            }
            return res;
        }
    }
}
