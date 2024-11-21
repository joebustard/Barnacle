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
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class ObjectSplitter
    {
        private Bounds3D bounds;

        private OctTree Result1OctTree;

        private OctTree Result2OctTree;

        public ObjectSplitter()
        {
            Orientation = SplitterOrientation.Vertical;
            OriginalVertices = null;
            OriginalFaces = null;

            Result1Vertices = null;
            Result1Faces = null;
            Result2Vertices = null;
            Result2Faces = null;

            Plane = 0;
            Result1OctTree = null;
            Result2OctTree = null;
        }

        public ObjectSplitter(Point3DCollection v, Int32Collection f, SplitterOrientation o)
        {
            OriginalVertices = v;
            OriginalFaces = f;
            Orientation = o;
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

            Plane = 0;
        }

        public enum SplitterOrientation
        {
            Horizontal,
            Vertical,
            Distal
        }

        public SplitterOrientation Orientation { get; set; }
        public Int32Collection OriginalFaces { get; set; }
        public Point3DCollection OriginalVertices { get; set; }
        public double Plane { get; set; }
        public Int32Collection Result1Faces { get; set; }
        public Point3DCollection Result1Vertices { get; set; }
        public Int32Collection Result2Faces { get; set; }
        public Point3DCollection Result2Vertices { get; set; }

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
                }
            }
            return false;
        }

        /// <summary>
        /// returns the vertices and edges of the first object soup
        /// as a new Object3D
        /// </summary>
        /// <returns></returns>
        internal Object3D GetObject1()
        {
            return GetObject(Result1Vertices, Result1Faces);
        }

        internal Object3D GetObject2()
        {
            return GetObject(Result2Vertices, Result2Faces);
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
                if (loop.Count > 3)
                {
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = Result1Vertices[er.Start];
                        switch (Orientation)
                        {
                            case SplitterOrientation.Horizontal:
                                {
                                    pf.Add(new System.Drawing.PointF((float)p.Z, (float)p.Y));
                                }
                                break;

                            case SplitterOrientation.Vertical:
                                {
                                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Z));
                                }
                                break;

                            case SplitterOrientation.Distal:
                                {
                                    pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                                }
                                break;
                        }
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = 0;
                        int c1 = 0;
                        int c2 = 0;
                        // close bottom of  top
                        switch (Orientation)
                        {
                            case SplitterOrientation.Horizontal:
                                {
                                    c0 = Result1OctTree.AddPoint(Plane, t.Points[0].Y, t.Points[0].X);
                                    c1 = Result1OctTree.AddPoint(Plane, t.Points[1].Y, t.Points[1].X);
                                    c2 = Result1OctTree.AddPoint(Plane, t.Points[2].Y, t.Points[2].X);
                                }
                                break;

                            case SplitterOrientation.Vertical:
                                {
                                    c0 = Result1OctTree.AddPoint(t.Points[0].X, Plane, t.Points[0].Y);
                                    c1 = Result1OctTree.AddPoint(t.Points[1].X, Plane, t.Points[1].Y);
                                    c2 = Result1OctTree.AddPoint(t.Points[2].X, Plane, t.Points[2].Y);
                                }
                                break;

                            case SplitterOrientation.Distal:
                                {
                                    c0 = Result1OctTree.AddPoint(t.Points[0].X, t.Points[0].Y, Plane);
                                    c2 = Result1OctTree.AddPoint(t.Points[1].X, t.Points[1].Y, Plane);
                                    c1 = Result1OctTree.AddPoint(t.Points[2].X, t.Points[2].Y, Plane);
                                }
                                break;
                        }

                        Result1Faces.Add(c0);
                        Result1Faces.Add(c1);
                        Result1Faces.Add(c2);

                        // close top of  bottom
                        switch (Orientation)
                        {
                            case SplitterOrientation.Horizontal:
                                {
                                    c0 = Result2OctTree.AddPoint(Plane, t.Points[0].Y, t.Points[0].X);
                                    c1 = Result2OctTree.AddPoint(Plane, t.Points[1].Y, t.Points[1].X);
                                    c2 = Result2OctTree.AddPoint(Plane, t.Points[2].Y, t.Points[2].X);
                                }
                                break;

                            case SplitterOrientation.Vertical:
                                {
                                    c0 = Result2OctTree.AddPoint(t.Points[0].X, Plane, t.Points[0].Y);
                                    c1 = Result2OctTree.AddPoint(t.Points[1].X, Plane, t.Points[1].Y);
                                    c2 = Result2OctTree.AddPoint(t.Points[2].X, Plane, t.Points[2].Y);
                                }
                                break;

                            case SplitterOrientation.Distal:
                                {
                                    c0 = Result2OctTree.AddPoint(t.Points[0].X, t.Points[0].Y, Plane);
                                    c2 = Result2OctTree.AddPoint(t.Points[1].X, t.Points[1].Y, Plane);
                                    c1 = Result2OctTree.AddPoint(t.Points[2].X, t.Points[2].Y, Plane);
                                }
                                break;
                        }

                        Result2Faces.Add(c0);
                        Result2Faces.Add(c2);
                        Result2Faces.Add(c1);
                    }
                }
                if (loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }
            }
        }

        /// <summary>
        /// Converts the given soup into an object3D
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        private Object3D GetObject(Point3DCollection verts, Int32Collection faces)
        {
            Object3D res = new Object3D();
            res.PrimType = "Mesh";
            Bounds3D bnds = new Bounds3D();
            foreach (Point3D p in verts)
            {
                res.AbsoluteObjectVertices.Add(new Point3D(p.X, p.Y, p.Z));
                bnds.Adjust(p);
            }
            for (int i = 0; i < faces.Count; i++)
            {
                res.TriangleIndices.Add(faces[i]);
            }
            Point3D mid = bnds.MidPoint();
            res.Position = new Point3D(mid.X, mid.Y, mid.Z);
            res.AbsoluteToRelative();
            return res;
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
            double t = 0;
            Point3D res = new Point3D(0, 0, 0);
            switch (Orientation)
            {
                case SplitterOrientation.Horizontal:
                    {
                        if (a.X != b.X)
                        {
                            t = (Plane - a.X) / (b.X - a.X);
                            res = new Point3D(Plane,
                                               a.Y + (b.Y - a.Y) * t,
                                               a.Z + (b.Z - a.Z) * t
                                               );
                        }
                    }
                    break;

                case SplitterOrientation.Vertical:
                    {
                        if (a.Y != b.Y)
                        {
                            t = (Plane - a.Y) / (b.Y - a.Y);
                            res = new Point3D(a.X + (b.X - a.X) * t,
                                              Plane,
                                              a.Z + (b.Z - a.Z) * t);
                        }
                    }
                    break;

                case SplitterOrientation.Distal:
                    {
                        if (a.Z != b.Z)
                        {
                            t = (Plane - a.Z) / (b.Z - a.Z);
                            res = new Point3D(a.X + (b.X - a.X) * t,
                                              a.Y + (b.Y - a.Y) * t,
                                              Plane);
                        }
                    }
                    break;
            }

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
            switch (Orientation)
            {
                case SplitterOrientation.Horizontal:
                    {
                        res = TriPointsX(a, b, c, ref aOn, ref bOn, ref cOn);
                    }
                    break;

                case SplitterOrientation.Vertical:
                    {
                        res = TriPointsY(a, b, c, ref aOn, ref bOn, ref cOn);
                    }
                    break;

                case SplitterOrientation.Distal:
                    {
                        res = TriPointsZ(a, b, c, ref aOn, ref bOn, ref cOn);
                    }
                    break;
            }
            return res;
        }

        /// <summary>
        /// How many points of a triangle are to the
        /// right of an X plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="aOn"></param>
        /// <param name="bOn"></param>
        /// <param name="cOn"></param>
        /// <returns></returns>
        private int TriPointsX(int a, int b, int c, ref bool aOn, ref bool bOn, ref bool cOn)
        {
            int count = 0;
            if (OriginalVertices[a].X > Plane)
            {
                aOn = true;
                count++;
            }
            else
            {
                aOn = false;
            }
            if (OriginalVertices[b].X > Plane)
            {
                bOn = true;
                count++;
            }
            else
            {
                bOn = false;
            }

            if (OriginalVertices[c].X > Plane)
            {
                cOn = true;
                count++;
            }
            else
            {
                cOn = false;
            }
            return count;
        }

        /// <summary>
        /// How many points of a triangle are above a Y plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="aOn"></param>
        /// <param name="bOn"></param>
        /// <param name="cOn"></param>
        /// <returns></returns>
        private int TriPointsY(int a, int b, int c, ref bool aOn, ref bool bOn, ref bool cOn)
        {
            int count = 0;
            if (OriginalVertices[a].Y > Plane)
            {
                aOn = true;
                count++;
            }
            else
            {
                aOn = false;
            }
            if (OriginalVertices[b].Y > Plane)
            {
                bOn = true;
                count++;
            }
            else
            {
                bOn = false;
            }

            if (OriginalVertices[c].Y > Plane)
            {
                cOn = true;
                count++;
            }
            else
            {
                cOn = false;
            }
            return count;
        }

        /// <summary>
        /// How many points of a triangle are in front of a Z plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="aOn"></param>
        /// <param name="bOn"></param>
        /// <param name="cOn"></param>
        /// <returns></returns>
        private int TriPointsZ(int a, int b, int c, ref bool aOn, ref bool bOn, ref bool cOn)
        {
            int count = 0;
            if (OriginalVertices[a].Z > Plane)
            {
                aOn = true;
                count++;
            }
            else
            {
                aOn = false;
            }
            if (OriginalVertices[b].Z > Plane)
            {
                bOn = true;
                count++;
            }
            else
            {
                bOn = false;
            }

            if (OriginalVertices[c].Z > Plane)
            {
                cOn = true;
                count++;
            }
            else
            {
                cOn = false;
            }
            return count;
        }
    }
}