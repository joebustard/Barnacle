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
using MakerLib.PlaneCutter;
using MathsLib;
using OctTreeLib;
using Polygon3DTriangulationLib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static Polygon3DTriangulationLib.Polygon3DTriangulator;

namespace MakerLib.PlaneSplitter
{
    public class PlaneSplitter
    {
        private Bounds3D bounds;

        private OctTree Result1OctTree;

        private OctTree Result2OctTree;

        public PlaneSplitter()
        {
            planeEquation = new PlaneEquation();
            OriginalVertices = null;
            OriginalFaces = null;

            Result1Vertices = null;
            Result1Faces = null;
            Result2Vertices = null;
            Result2Faces = null;

            Result1OctTree = null;
            Result2OctTree = null;
        }

        public PlaneSplitter(Point3DCollection v, Int32Collection f, PlaneEquation planeEquation)
        {
            this.planeEquation = planeEquation;
            OriginalVertices = v;
            OriginalFaces = f;
            bounds = new Bounds3D();
            bounds.Zero();
            if (v != null)
            {
                foreach (Point3D p in v)
                {
                    bounds.Adjust(p);
                }
                bounds.Expand(new Point3D(1, 1, 1));
            }
            Result1Vertices = null;
            Result1Faces = null;
            Result2Vertices = null;
            Result2Faces = null;
        }

        public Int32Collection OriginalFaces
        {
            get; set;
        }

        public Point3DCollection OriginalVertices
        {
            get; set;
        }

        public Int32Collection Result1Faces
        {
            get; set;
        }

        public Point3DCollection Result1Vertices
        {
            get; set;
        }

        public Int32Collection Result2Faces
        {
            get; set;
        }

        public Point3DCollection Result2Vertices
        {
            get; set;
        }

        private PlaneEquation planeEquation;

        public bool Split()
        {
            bool result = false;

            if (OriginalFaces != null && OriginalFaces.Count > 0)
            {
                if (OriginalVertices != null && OriginalVertices.Count > 0)
                {
                    EdgeProcessor edgeProc = new EdgeProcessor();

                    Result1Vertices = new Point3DCollection();
                    Result1Faces = new Int32Collection();
                    Result1OctTree = new OctTree(Result1Vertices, bounds.Lower, bounds.Upper);

                    Result2Vertices = new Point3DCollection();
                    Result2Faces = new Int32Collection();
                    Result2OctTree = new OctTree(Result2Vertices, bounds.Lower, bounds.Upper);

                    SeperateSurfaces(edgeProc);

                    CloseEnds(edgeProc);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// returns the vertices and edges of the seconds object soup
        /// as a new Object3D
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Adds the specified triangle to the given Octree
        /// Just a util function to reduce repetition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="octTree"></param>
        /// <param name="faces"></param>
        private void AddTriangle(Point3D a, Point3D b, Point3D c, OctTree octTree, Int32Collection faces)
        {
            int v0 = octTree.AddPoint(a);
            int v1 = octTree.AddPoint(b);
            int v2 = octTree.AddPoint(c);
            faces.Add(v0);
            faces.Add(v1);
            faces.Add(v2);
        }

        /// <summary>
        /// Close up the ends of the two new objects that
        /// are currently open
        /// </summary>
        /// <param name="edgeProc"></param>
        private void CloseEnds(EdgeProcessor edgeProc)
        {
            // stitch and triangulate edge for Result1
            bool moreLoops = true;

            while (moreLoops)
            {
                moreLoops = false;
                List<EdgeRecord> loop = edgeProc.MakeLoop();

                if (loop.Count >= 2)
                {
                    List<Vector3> verticesToTriangulate = new List<Vector3>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = Result1Vertices[er.Start];
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
                            // System.Diagnostics.Debug.WriteLine($"i {i} =  {triangle.V1}, {triangle.V2},{triangle.V3} ");
                            int v1 = triangle.V1;
                            Point3D p1 = new Point3D(verticesToTriangulate[v1].X, verticesToTriangulate[v1].Y, verticesToTriangulate[v1].Z);

                            int v2 = triangle.V2;
                            Point3D p2 = new Point3D(verticesToTriangulate[v2].X, verticesToTriangulate[v2].Y, verticesToTriangulate[v2].Z);

                            int v3 = triangle.V3;
                            Point3D p3 = new Point3D(verticesToTriangulate[v3].X, verticesToTriangulate[v3].Y, verticesToTriangulate[v3].Z);
                            AddTriangle(p1, p3, p2, Result1OctTree, Result1Faces);
                            AddTriangle(p1, p2, p3, Result2OctTree, Result2Faces);
                        }
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }
        }

        /// <summary>
        /// Reallocate triangles of original object to either
        /// of the two new ones. Split triangles which cross
        /// the border
        /// </summary>
        /// <param name="edgeProc"></param>
        private void SeperateSurfaces(EdgeProcessor edgeProc)
        {
            for (int i = 0; i < OriginalFaces.Count; i += 3)
            {
                int a = OriginalFaces[i];
                int b = OriginalFaces[i + 1];
                int c = OriginalFaces[i + 2];
                Point3D aP = OriginalVertices[a];
                Point3D bP = OriginalVertices[b];
                Point3D cP = OriginalVertices[c];

                bool aUp = false;
                bool bUp = false;
                bool cUp = false;
                int count = TriPointsOnSide(a, b, c, ref aUp, ref bUp, ref cUp);
                switch (count)
                {
                    case 0:
                        {
                            // all three points of triangle are on or below the cut plane
                            AddTriangle(aP, bP, cP, Result2OctTree, Result2Faces);
                        }
                        break;

                    case 1:
                        {
                            //one point of triangle is above the cut plane
                            // clip it against the plane
                            if (aUp)
                            {
                                Point3D sP = SplitEdge(aP, bP);
                                Point3D eP = SplitEdge(aP, cP);
                                // add triangle aP, sP, eP to Result 1
                                AddTriangle(aP, sP, eP, Result1OctTree, Result1Faces);

                                // add triangle  bP, sP, eP to result 2
                                AddTriangle(bP, eP, sP, Result2OctTree, Result2Faces);

                                // add triangle bp , eP cP to result 2
                                AddTriangle(bP, cP, eP, Result2OctTree, Result2Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);
                            }
                            else if (bUp)
                            {
                                Point3D sP = SplitEdge(bP, cP);
                                Point3D eP = SplitEdge(bP, aP);
                                // add triangle bP, sP, eP to Result 1
                                AddTriangle(bP, sP, eP, Result1OctTree, Result1Faces);

                                // add triangle  aP, sP, eP to result 2
                                AddTriangle(aP, eP, sP, Result2OctTree, Result2Faces);

                                // add triangle ap , eP cP to result 2
                                AddTriangle(aP, sP, cP, Result2OctTree, Result2Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);
                            }
                            else if (cUp)
                            {
                                Point3D sP = SplitEdge(cP, aP);
                                Point3D eP = SplitEdge(cP, bP);

                                // add triangle cP, sP, eP to Result 1
                                AddTriangle(cP, sP, eP, Result1OctTree, Result1Faces);

                                // add triangle  bP, sP, eP to result 2
                                AddTriangle(bP, eP, sP, Result2OctTree, Result2Faces);

                                // add triangle bp , eP aP to result 2
                                AddTriangle(bP, sP, aP, Result2OctTree, Result2Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);
                            }
                        }
                        break;

                    case 2:
                        {
                            // two points are above the cut plane
                            if (aUp && bUp)
                            {
                                Point3D sP = SplitEdge(bP, cP);
                                Point3D eP = SplitEdge(aP, cP);

                                AddTriangle(aP, bP, sP, Result1OctTree, Result1Faces);
                                AddTriangle(aP, sP, eP, Result1OctTree, Result1Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);

                                AddTriangle(cP, eP, sP, Result2OctTree, Result2Faces);
                            }
                            else if (bUp && cUp)
                            {
                                Point3D sP = SplitEdge(cP, aP);
                                Point3D eP = SplitEdge(aP, bP);

                                AddTriangle(bP, cP, sP, Result1OctTree, Result1Faces);
                                AddTriangle(bP, sP, eP, Result1OctTree, Result1Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);

                                AddTriangle(aP, eP, sP, Result2OctTree, Result2Faces);
                            }
                            else if (cUp && aUp)
                            {
                                Point3D sP = SplitEdge(aP, bP);
                                Point3D eP = SplitEdge(bP, cP);

                                AddTriangle(aP, sP, cP, Result1OctTree, Result1Faces);
                                AddTriangle(cP, sP, eP, Result1OctTree, Result1Faces);

                                int s = Result1OctTree.PointPresent(sP);
                                int e = Result1OctTree.PointPresent(eP);
                                edgeProc.Add(s, e);

                                AddTriangle(bP, eP, sP, Result2OctTree, Result2Faces);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            AddTriangle(aP, bP, cP, Result1OctTree, Result1Faces);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Splits the segment between two Point3Ds at the split plane
        /// given the current Orientation setting
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Point3D SplitEdge(Point3D a, Point3D b)
        {
            Point3D res = new Point3D(0, 0, 0);
            planeEquation.LineIntercepts(a, b, out res);
            return res;
        }

        /// <summary>
        /// Counts how many points of a triangle are on one side
        /// of the given plane.
        /// Also returns which of the points are affected
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="aOn"></param>
        /// <param name="bOn"></param>
        /// <param name="cOn"></param>
        /// <returns></returns>
        private int TriPointsOnSide(int a, int b, int c, ref bool aOn, ref bool bOn, ref bool cOn)
        {
            int res = 0;
            aOn = false;
            bOn = false;
            cOn = false;
            if (planeEquation.WhichSideIsPointOn(OriginalVertices[a].X, OriginalVertices[a].Y, OriginalVertices[a].Z) > 0)
            {
                aOn = true;
                res++;
            }

            if (planeEquation.WhichSideIsPointOn(OriginalVertices[b].X, OriginalVertices[b].Y, OriginalVertices[b].Z) > 0)
            {
                bOn = true;
                res++;
            }

            if (planeEquation.WhichSideIsPointOn(OriginalVertices[c].X, OriginalVertices[c].Y, OriginalVertices[c].Z) > 0)
            {
                cOn = true;
                res++;
            }
            return res;
        }

        public void Cut()
        {
            Split();
            Result2Faces.Clear();
            Result2Vertices.Clear();
        }
    }
}