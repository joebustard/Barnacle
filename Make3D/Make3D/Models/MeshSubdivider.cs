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

using HalfEdgeLib;
using OctTreeLib;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class MeshSubdivider
    {
        private OctTree octTree;
        private Int32Collection origTri;
        private Point3DCollection origVer;

        public MeshSubdivider(Point3DCollection v, Int32Collection tri)
        {
            origVer = v;
            origTri = tri;
        }

        public int AddVerticeOctTree(Point3DCollection vertices, Point3D v)
        {
            int res = -1;
            if (octTree != null)
            {
                res = octTree.PointPresent(v);

                if (res == -1)
                {
                    res = vertices.Count;
                    octTree.AddPoint(res, v);
                }
            }
            return res;
        }

        public void Subdivide(Point3DCollection v, Int32Collection tri)
        {
            v.Clear();
            tri.Clear();
            double lx = 0;
            double ly = 0;
            double lz = 0;
            double rx = 0;
            double ry = 0;
            double rz = 0;
            CalculateExtents(origVer, out lx, out ly, out lz, out rx, out ry, out rz);
            octTree = new OctTree(v, new Point3D(lx, ly, lz), new Point3D(rx, ry, rz));

            for (int i = 0; i < origTri.Count; i += 3)
            {
                if (i + 2 < origTri.Count)
                {
                    int a = origTri[i];
                    int b = origTri[i + 1];
                    int c = origTri[i + 2];

                    Point3D pa = origVer[a];
                    Point3D pb = origVer[b];
                    Point3D pc = origVer[c];

                    Point3D pab = Midpoint(pa, pb);
                    Point3D pbc = Midpoint(pb, pc);
                    Point3D pca = Midpoint(pc, pa);

                    int ia = AddVerticeOctTree(v, pa);
                    int ib = AddVerticeOctTree(v, pb);
                    int ic = AddVerticeOctTree(v, pc);

                    int iab = AddVerticeOctTree(v, pab);
                    int ibc = AddVerticeOctTree(v, pbc);
                    int ica = AddVerticeOctTree(v, pca);

                    tri.Add(ia);
                    tri.Add(iab);
                    tri.Add(ica);

                    tri.Add(iab);
                    tri.Add(ib);
                    tri.Add(ibc);

                    tri.Add(ibc);
                    tri.Add(ic);
                    tri.Add(ica);

                    tri.Add(iab);
                    tri.Add(ibc);
                    tri.Add(ica);
                }
            }

            origTri.Clear();
            origVer.Clear();
        }

        private void CalculateExtents(Point3DCollection v, out double lx, out double ly, out double lz, out double tx, out double ty, out double tz)
        {
            lx = double.MaxValue;
            ly = double.MaxValue;
            lz = double.MaxValue;

            tx = double.MinValue;
            ty = double.MinValue;
            tz = double.MinValue;
            for (int i = 0; i < v.Count; i++)
            {
                lx = Math.Min(v[i].X, lx);
                ly = Math.Min(v[i].Y, ly);
                lz = Math.Min(v[i].Z, lz);
                tx = Math.Max(v[i].X, tx);
                ty = Math.Max(v[i].Y, ty);
                tz = Math.Max(v[i].Z, tz);
            }
        }

        private Point3D Midpoint(Point3D a, Point3D b)
        {
            return new Point3D( (a.X + b.X) / 2,
                                (a.Y + b.Y) / 2,
                                (a.Z + b.Z) / 2);
        }
    }
}