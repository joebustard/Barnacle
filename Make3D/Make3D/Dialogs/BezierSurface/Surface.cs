﻿/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using Barnacle.Object3DLib;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HalfEdgeLib;

namespace Barnacle.Dialogs.BezierSurface
{
    internal class Surface : BaseModellerDialog
    {
        private double thickness = 1.0;

        public Surface()
        {
        }

        public ControlPointManager controlPointManager { get; set; }

        public double Thickness
        {
            get
            {
                return thickness;
            }

            set
            {
                if (value > 0)
                {
                    thickness = value;
                }
            }
        }

        public void GenerateSurface(Point3DCollection vertices, Int32Collection tris, double numDivs = 4)
        {
            DateTime startTime = DateTime.Now;
            double delta = 1.0 / numDivs;
            vertices.Clear();
            tris.Clear();
            Vector3D off = new Vector3D(0, Thickness, 0);
            if (controlPointManager != null)
            {
                TopOnly(vertices, tris, delta, off);
                SurfaceToSolid(vertices, tris, Thickness);
            }
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
        }

        public Point3D GetBezier3D(Point3D p1, Point3D p2, Point3D p3, Point3D p4, double t)
        {
            if (t < 0) t = 0;
            if (t > 1.0) t = 1.0;
            double k1 = (1 - t) * (1 - t) * (1 - t);
            double k2 = 3 * (1 - t) * (1 - t) * t;
            double k3 = 3 * (1 - t) * t * t;
            double k4 = t * t * t;
            double x = (p1.X * k1) + (p2.X * k2) + (p3.X * k3) + (p4.X * k4);
            double y = (p1.Y * k1) + (p2.Y * k2) + (p3.Y * k3) + (p4.Y * k4);
            double z = (p1.Z * k1) + (p2.Z * k2) + (p3.Z * k3) + (p4.Z * k4);
            return new Point3D(x, y, z);
        }

        private int AddVertice(Point3D v, Point3DCollection vertices)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (PointUtils.equals(vertices[i], v.X, v.Y, v.Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(v.X, v.Y, v.Z));
                res = vertices.Count - 1;
            }
            return res;
        }

        private Point3D GetSurfacePoint(int rs, int rc, double u, double v)
        {
            Point3D[] upoints = new Point3D[4];
            ControlPoint[,] pnts = controlPointManager.AllcontrolPoints;
            for (int r = 0; r < 4; r++)
            {
                upoints[r] = GetBezier3D(
                    pnts[rs + r, rc].Position,
                    pnts[rs + r, rc + 1].Position,
                    pnts[rs + r, rc + 2].Position,
                    pnts[rs + r, rc + 3].Position,
                    u);
            }

            Point3D vpoint = GetBezier3D(upoints[0], upoints[1], upoints[2], upoints[3], v);
            return vpoint;
        }

        private void TopOnly(Point3DCollection vertices, Int32Collection tris, double delta, Vector3D off)
        {
            int patchStartRow = 0;
            int patchStartColumn = 0;
            for (patchStartRow = 0; patchStartRow < controlPointManager.PatchRows - 3; patchStartRow += 3)
            {
                for (patchStartColumn = 0; patchStartColumn < controlPointManager.PatchColumns - 3; patchStartColumn += 3)
                {
                    double u;
                    double v;
                    for (u = 0; u < 1; u += delta)
                    {
                        double u1 = u + delta;
                        double um = u + (delta / 2);
                        for (v = 0; v < 1; v += delta)
                        {
                            double v1 = v + delta;
                            double vm = v + (delta / 2.0);

                            // top
                            Point3D p1 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v);
                            Point3D p2 = GetSurfacePoint(patchStartRow, patchStartColumn, u1, v);
                            Point3D p3 = GetSurfacePoint(patchStartRow, patchStartColumn, u1, v1);
                            Point3D p4 = GetSurfacePoint(patchStartRow, patchStartColumn, u, v1);
                            Point3D p5 = GetSurfacePoint(patchStartRow, patchStartColumn, um, vm);

                            int ve1 = AddVertice(p1, vertices);
                            int ve2 = AddVertice(p2, vertices);
                            int ve3 = AddVertice(p3, vertices);
                            int ve4 = AddVertice(p4, vertices);
                            int ve5 = AddVertice(p5, vertices);

                            tris.Add(ve1);
                            tris.Add(ve5);
                            tris.Add(ve2);

                            tris.Add(ve2);
                            tris.Add(ve5);
                            tris.Add(ve3);

                            tris.Add(ve3);
                            tris.Add(ve5);
                            tris.Add(ve4);

                            tris.Add(ve4);
                            tris.Add(ve5);
                            tris.Add(ve1);
                        }
                    }
                }
            }
        }
    }
}