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
using MathsLib;
using OctTreeLib;
using Polygon3DTriangulationLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static Polygon3DTriangulationLib.Polygon3DTriangulator;

namespace MakerLib.PlaneCutter
{
    public class PlaneCutter
    {
        private Point3DCollection vertices;
        private Int32Collection faces;
        private PlaneEquation planeEquation;
        private Bounds3D bounds;

        private Int32Collection originalFaces;
        private Point3DCollection originalVertices;
        private double planeLevel;
        private Int32Collection workingFaces;
        private OctTree workingOctTree;
        private Point3DCollection workingVertices;

        public PlaneCutter(Point3DCollection vertices, Int32Collection faces, PlaneEquation planeEquation)
        {
            this.vertices = vertices;
            this.faces = faces;
            this.planeEquation = planeEquation;
            bounds = new Bounds3D();
            foreach (Point3D point3D in vertices)
            {
                bounds.Adjust(point3D);
            }
            workingVertices = new Point3DCollection();
            workingFaces = new Int32Collection();

            workingOctTree = CreateOctree(workingVertices, bounds.Lower, bounds.Upper);
            foreach (int i in faces)
            {
                int v = workingOctTree.AddPoint(vertices[i]);
                workingFaces.Add(v);
            }
            originalFaces = faces;
            originalVertices = vertices;
        }
        protected OctTree CreateOctree(Point3DCollection verts, Point3D minPoint, Point3D maxPoint)
        {
            return new OctTree(verts, minPoint, maxPoint, 200);
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
                if (planeEquation.WhichSideIsPointOn(workingVertices[a]) > 0)
                {
                    upCount++;
                    aUp = true;
                }
                if (planeEquation.WhichSideIsPointOn(workingVertices[b]) > 0)
                {
                    upCount++;
                    bUp = true;
                }
                if (planeEquation.WhichSideIsPointOn(workingVertices[c]) > 0)
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
                                ClipTriangle(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bUp)
                            {
                                ClipTriangle(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cUp)
                            {
                                ClipTriangle(c, ref a, ref b);
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
                    /*
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
                    */
                    List<Vector3> verticesToTriangulate = new List<Vector3>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = workingVertices[er.Start];
                        Vector3 pv = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
                        verticesToTriangulate.Add(pv);
                    }
                    Polygon3DTriangulator polygonTriangulator = new Polygon3DTriangulator();
                    List<Triangle> loopTriangles = polygonTriangulator.Triangulate3D(verticesToTriangulate);
                    if (loopTriangles != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"loopTriangles {loopTriangles.Count}");
                        int i = 0;
                        foreach (Triangle triangle in loopTriangles)
                        {
                            System.Diagnostics.Debug.WriteLine($"i {i} =  {triangle.V1}, {triangle.V2},{triangle.V3} ");
                            int v1 = triangle.V1;
                            Point3D p1 = new Point3D(verticesToTriangulate[v1].X, verticesToTriangulate[v1].Y, verticesToTriangulate[v1].Z);

                            int v2 = triangle.V2;
                            Point3D p2 = new Point3D(verticesToTriangulate[v2].X, verticesToTriangulate[v2].Y, verticesToTriangulate[v2].Z);

                            int v3 = triangle.V3;
                            Point3D p3 = new Point3D(verticesToTriangulate[v3].X, verticesToTriangulate[v3].Y, verticesToTriangulate[v3].Z);

                            int c0 = workingOctTree.AddPoint(p1.X, p1.Y, p1.Z);
                            int c1 = workingOctTree.AddPoint(p2.X, p2.Y, p2.Z);
                            int c2 = workingOctTree.AddPoint(p3.X, p3.Y, p3.Z);
                            newFaces.Add(c0);
                            newFaces.Add(c2);
                            newFaces.Add(c1);
                        }
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }

            ExtrackNewFaces(newFaces);
        }
        private void ClipTriangle(int a, ref int b, ref int c)
        {
           

            Point3D intersectionPoint = new Point3D(0, 0, 0);
            if (planeEquation.LineIntercepts(workingVertices[a], workingVertices[b], out intersectionPoint))
            {
                b = workingOctTree.AddPoint(intersectionPoint.X, intersectionPoint.Y, intersectionPoint.Z);

                if (planeEquation.LineIntercepts(workingVertices[a], workingVertices[c], out intersectionPoint))
                {
                    c = workingOctTree.AddPoint(intersectionPoint.X, intersectionPoint.Y, intersectionPoint.Z);
                }
            }

        }

        private int CrossingPointH(int a, int b)
        {
            int res = -1;
            Point3D intersectionPoint = new Point3D(0, 0, 0);
            if (planeEquation.LineIntercepts(workingVertices[a], workingVertices[b], out intersectionPoint))
            {

                res = workingOctTree.AddPoint(intersectionPoint.X, intersectionPoint.Y, intersectionPoint.Z);

            }
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
    }
}
