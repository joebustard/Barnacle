/**************************************************************************
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

using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class SidePlateModel : PlateModel
    {
        private void TriangulateSide(List<PointF> points, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(new Point3D(t.Points[0].X, t.Points[0].Y, 0.0));
                int c1 = AddVertice(new Point3D(t.Points[1].X, t.Points[1].Y, 0.0));
                int c2 = AddVertice(new Point3D(t.Points[2].X, t.Points[2].Y, 0.0));
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        public float MiddleOffset { get; set; } = 0;
        public float LeftOffset { get; private set; }

        public void Log(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        internal override void SetPoints(List<PointF> dp)
        {
            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;
            foreach (PointF p in dp)
            {
                left = Math.Min(left, p.X);
                right = Math.Max(right, p.X);
                bottom = Math.Min(bottom, p.Y);
                top = Math.Max(top, p.Y);
            }
            float dx = (right - left) / 2;
            float dy = (top - bottom);
            MiddleOffset = dy / 2;
            //LeftOffset = - left - dx;
            LeftOffset = float.MaxValue;
            points = new List<PointF>();
            Log("SidePlateModel : ");
            foreach (PointF p in dp)
            {
                float xn = p.X - left - dx;
                Log($"{xn},{(-(p.Y - bottom) + dy)}");
                LeftOffset = Math.Min(LeftOffset, xn);
                points.Add(new PointF(xn, -(p.Y - bottom) + dy));
            }
            ClearShape();

            TriangulateSide(points, false);
        }
    }
}