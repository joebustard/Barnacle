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
        }

        public void Cut()
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
                                MakeTri1(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bUp)
                            {
                                MakeTri1(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cUp)
                            {
                                MakeTri1(c, ref a, ref b);
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
                                int dp = CrossingPoint(b, c);
                                int ep = CrossingPoint(a, c);
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
                                int dp = CrossingPoint(c, a);
                                int ep = CrossingPoint(a, b);
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
                                int dp = CrossingPoint(a, b);
                                int ep = CrossingPoint(b, c);
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

            /*
            Point3DCollection targetVertices = new Point3DCollection();
            Int32Collection targetFaces = new Int32Collection();
            OctTree targetOctree = CreateOctree(targetVertices, bounds.Lower, bounds.Upper);
            foreach (int j in newFaces)
            {
                int v = targetOctree.AddPoint(workingVertices[j]);
                targetFaces.Add(v);
            }

            originalVertices.Clear();
            foreach (Point3D p in targetVertices)
            {
                originalVertices.Add(p);
            }

            */
            /*
            originalFaces.Clear();
            foreach (int i in newFaces)
            {
                originalFaces.Add(i);
            }
            */
            /*
            originalVertices.Clear();
            foreach (Point3D p in workingVertices)
            {
                originalVertices.Add(p);
            }
            originalFaces.Clear();
            foreach (int i in newFaces)
            {
                originalFaces.Add(i);
            }

            */


            originalVertices.Clear();
            originalFaces.Clear();
            OctTree targetOctree = CreateOctree(originalVertices, bounds.Lower, bounds.Upper);
            foreach (int j in newFaces)
            {
                int v = targetOctree.AddPoint(workingVertices[j]);
                originalFaces.Add(v);
            }

        }

        protected OctTree CreateOctree(Point3DCollection verts, Point3D minPoint, Point3D maxPoint)
        {
            return new OctTree(verts, minPoint, maxPoint, 200);
        }

        protected virtual int CrossingPoint(int a, int b)
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

        private void MakeTri1(int a, ref int b, ref int c)
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
    }
}