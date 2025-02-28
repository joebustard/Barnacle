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

using Barnacle.Object3DLib;
using OctTreeLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib.PlaneCutter
{
    public class PlaneCutter
    {
        private Bounds3D bounds;
        private PlaneOrientation orientation;
        private Int32Collection originalFaces;
        private Point3DCollection originalVertices;
        private double planeLevel;
        private Int32Collection workingFaces;
        private OctTree workingOctTree;
        private Point3DCollection workingVertices;

        public PlaneCutter(Point3DCollection point3Ds, Int32Collection f, double level)
        {
            bounds = new Bounds3D();
            foreach (Point3D point3D in point3Ds)
            {
                bounds.Adjust(point3D);
            }
            workingVertices = new Point3DCollection();
            workingFaces = new Int32Collection();

            planeLevel = level;

            workingOctTree = CreateOctree(workingVertices, bounds.Lower, bounds.Upper);
            foreach (int i in f)
            {
                int v = workingOctTree.AddPoint(point3Ds[i]);
                workingFaces.Add(v);
            }
            originalFaces = f;
            originalVertices = point3Ds;
            orientation = PlaneOrientation.Horizontal;
        }

        private enum PlaneOrientation
        {
            Horizontal,
            Vertical,
            Distal
        }

        public void Cut()
        {
            switch (orientation)
            {
                case PlaneOrientation.Horizontal:
                    {
                        CutH();
                    }
                    break;

                case PlaneOrientation.Vertical:
                    {
                        CutV();
                    }
                    break;
            }
        }

        public void CutH()
        {
            EdgeProcessor edgeProc = new EdgeProcessor();

            Int32Collection newFaces = new Int32Collection();
            for (int i = 0; i < workingFaces.Count; i += 3)
            {
                int a = workingFaces[i];
                int b = workingFaces[i + 1];
                int c = workingFaces[i + 2];

                int upCount = 0;
                bool aUp = false;
                bool bUp = false;
                bool cUp = false;
                if (workingVertices[a].Y > planeLevel)
                {
                    upCount++;
                    aUp = true;
                }
                if (workingVertices[b].Y > planeLevel)
                {
                    upCount++;
                    bUp = true;
                }
                if (workingVertices[c].Y > planeLevel)
                {
                    upCount++;
                    cUp = true;
                }

                switch (upCount)
                {
                    case 0:
                        {
                            //all three points of trinagle are on or below the cut plane
                        }
                        break;

                    case 1:
                        {
                            //one point of triangle is above the cut plane
                            // clip it against the plane
                            if (aUp)
                            {
                                MakeTri1H(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bUp)
                            {
                                MakeTri1H(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cUp)
                            {
                                MakeTri1H(c, ref a, ref b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                edgeProc.Add(a, b);
                            }
                        }
                        break;

                    case 2:
                        {
                            // two points are above the cut plane
                            if (aUp && bUp)
                            {
                                int dp = CrossingPointH(b, c);
                                int ep = CrossingPointH(a, c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(dp);

                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (bUp && cUp)
                            {
                                int dp = CrossingPointH(c, a);
                                int ep = CrossingPointH(a, b);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(dp);

                                newFaces.Add(b);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (cUp && aUp)
                            {
                                int dp = CrossingPointH(a, b);
                                int ep = CrossingPointH(b, c);
                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(c);

                                newFaces.Add(c);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            newFaces.Add(a);
                            newFaces.Add(b);
                            newFaces.Add(c);
                        }
                        break;
                }
            }
            bool moreLoops = true;
            while (moreLoops)
            {
                moreLoops = false;
                List<EdgeRecord> loop = edgeProc.MakeLoop();
                if (loop.Count > 3)
                {
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = workingVertices[er.Start];
                        pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Z));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = workingOctTree.AddPoint(t.Points[0].X, planeLevel, t.Points[0].Y);
                        int c1 = workingOctTree.AddPoint(t.Points[1].X, planeLevel, t.Points[1].Y);
                        int c2 = workingOctTree.AddPoint(t.Points[2].X, planeLevel, t.Points[2].Y);
                        newFaces.Add(c0);
                        newFaces.Add(c1);
                        newFaces.Add(c2);
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }

            ExtrackNewFaces(newFaces);
        }

        public void CutV()
        {
            EdgeProcessor edgeProc = new EdgeProcessor();

            Int32Collection newFaces = new Int32Collection();
            for (int i = 0; i < workingFaces.Count; i += 3)
            {
                int a = workingFaces[i];
                int b = workingFaces[i + 1];
                int c = workingFaces[i + 2];

                int rightCount = 0;
                bool aRight = false;
                bool bRight = false;
                bool cRight = false;
                if (workingVertices[a].X > planeLevel)
                {
                    rightCount++;
                    aRight = true;
                }
                if (workingVertices[b].X > planeLevel)
                {
                    rightCount++;
                    bRight = true;
                }
                if (workingVertices[c].X > planeLevel)
                {
                    rightCount++;
                    cRight = true;
                }

                switch (rightCount)
                {
                    case 0:
                        {
                            //all three points of trinagle are on or below the cut plane
                        }
                        break;

                    case 1:
                        {
                            //one point of triangle is above the cut plane
                            // clip it against the plane
                            if (aRight)
                            {
                                MakeTri1V(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bRight)
                            {
                                MakeTri1V(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cRight)
                            {
                                MakeTri1V(c, ref a, ref b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                edgeProc.Add(a, b);
                            }
                        }
                        break;

                    case 2:
                        {
                            // two points are above the cut plane
                            if (aRight && bRight)
                            {
                                int dp = CrossingPointV(b, c);
                                int ep = CrossingPointV(a, c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(dp);

                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (bRight && cRight)
                            {
                                int dp = CrossingPointV(c, a);
                                int ep = CrossingPointV(a, b);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(dp);

                                newFaces.Add(b);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (cRight && aRight)
                            {
                                int dp = CrossingPointV(a, b);
                                int ep = CrossingPointV(b, c);
                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(c);

                                newFaces.Add(c);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            newFaces.Add(a);
                            newFaces.Add(b);
                            newFaces.Add(c);
                        }
                        break;
                }
            }
            bool moreLoops = true;
            while (moreLoops)
            {
                moreLoops = false;
                List<EdgeRecord> loop = edgeProc.MakeLoop();
                if (loop.Count > 3)
                {
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = workingVertices[er.Start];
                        pf.Add(new System.Drawing.PointF((float)p.Z, (float)p.Y));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = workingOctTree.AddPoint(planeLevel, t.Points[0].Y, t.Points[0].X);
                        int c1 = workingOctTree.AddPoint(planeLevel, t.Points[1].Y, t.Points[1].X);
                        int c2 = workingOctTree.AddPoint(planeLevel, t.Points[2].Y, t.Points[2].X);
                        newFaces.Add(c0);
                        newFaces.Add(c1);
                        newFaces.Add(c2);
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }

            ExtrackNewFaces(newFaces);
        }

        public void SetDistal()
        {
            orientation = PlaneOrientation.Distal;
        }

        public void SetHorizontal()
        {
            orientation = PlaneOrientation.Horizontal;
        }

        public void SetVertical()
        {
            orientation = PlaneOrientation.Vertical;
        }

        protected OctTree CreateOctree(Point3DCollection verts, Point3D minPoint, Point3D maxPoint)
        {
            return new OctTree(verts, minPoint, maxPoint, 200);
        }

        private int CrossingPointH(int a, int b)
        {
            double t;
            double x0 = workingVertices[a].X;
            double y0 = workingVertices[a].Y;
            double z0 = workingVertices[a].Z;
            double x1 = workingVertices[b].X;
            double y1 = workingVertices[b].Y;
            double z1 = workingVertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);
            int res = workingOctTree.AddPoint(x, planeLevel, z);
            return res;
        }

        private int CrossingPointV(int a, int b)
        {
            double t;
            double x0 = workingVertices[a].X;
            double y0 = workingVertices[a].Y;
            double z0 = workingVertices[a].Z;
            double x1 = workingVertices[b].X;
            double y1 = workingVertices[b].Y;
            double z1 = workingVertices[b].Z;
            t = (planeLevel - x0) / (x1 - x0);

            double y = y0 + t * (y1 - y0);
            double z = z0 + t * (z1 - z0);
            int res = workingOctTree.AddPoint(planeLevel, y, z);
            return res;
        }

        private void ExtrackNewFaces(Int32Collection newFaces)
        {
            originalVertices.Clear();
            originalFaces.Clear();
            OctTree targetOctree = CreateOctree(originalVertices, bounds.Lower, bounds.Upper);
            foreach (int j in newFaces)
            {
                int v = targetOctree.AddPoint(workingVertices[j]);
                originalFaces.Add(v);
            }
        }

        private void MakeTri1H(int a, ref int b, ref int c)
        {
            double t;
            double x0 = workingVertices[a].X;
            double y0 = workingVertices[a].Y;
            double z0 = workingVertices[a].Z;
            double x1 = workingVertices[b].X;
            double y1 = workingVertices[b].Y;
            double z1 = workingVertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);

            b = workingOctTree.AddPoint(x, planeLevel, z);

            x1 = workingVertices[c].X;
            y1 = workingVertices[c].Y;
            z1 = workingVertices[c].Z;
            t = (planeLevel - y0) / (y1 - y0);

            x = x0 + t * (x1 - x0);
            z = z0 + t * (z1 - z0);

            c = workingOctTree.AddPoint(x, planeLevel, z);
        }

        private void MakeTri1V(int a, ref int b, ref int c)
        {
            double t;
            double x0 = workingVertices[a].X;
            double y0 = workingVertices[a].Y;
            double z0 = workingVertices[a].Z;
            double x1 = workingVertices[b].X;
            double y1 = workingVertices[b].Y;
            double z1 = workingVertices[b].Z;
            t = (planeLevel - x0) / (x1 - x0);

            double y = y0 + t * (y1 - y0);
            double z = z0 + t * (z1 - z0);

            b = workingOctTree.AddPoint(planeLevel, y, z);

            x1 = workingVertices[c].X;
            y1 = workingVertices[c].Y;
            z1 = workingVertices[c].Z;
            t = (planeLevel - x0) / (x1 - x0);

            y = y0 + t * (y1 - y0);
            z = z0 + t * (z1 - z0);

            c = workingOctTree.AddPoint(planeLevel, y, z);
        }
    }
}