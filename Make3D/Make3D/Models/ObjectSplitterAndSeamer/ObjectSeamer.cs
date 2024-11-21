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
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class ObjectSeamer
    {
        private Bounds3D bounds;
        private OctTree ResultOctTree;

        public ObjectSeamer()
        {
            Orientation = SeamerOrientation.Vertical;
            OriginalVertices = null;
            OriginalFaces = null;
            ResultVertices = null;
            ResultFaces = null;
            Plane = 0;
            ResultOctTree = null;
        }

        public ObjectSeamer(Point3DCollection v, Int32Collection f, SeamerOrientation o)
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
            ResultVertices = null;
            ResultFaces = null;
            Plane = 0;
        }

        public enum SeamerOrientation
        {
            Horizontal,
            Vertical,
            Distal
        }

        public SeamerOrientation Orientation { get; set; }
        public Int32Collection OriginalFaces { get; set; }
        public Point3DCollection OriginalVertices { get; set; }
        public double Plane { get; set; }
        public Int32Collection ResultFaces { get; set; }
        public Point3DCollection ResultVertices { get; set; }

        public void Seam()
        {
            if (OriginalFaces != null && OriginalFaces.Count > 0)
            {
                if (OriginalVertices != null && OriginalVertices.Count > 0)
                {
                    ResultVertices = new Point3DCollection();
                    ResultFaces = new Int32Collection();
                    ResultOctTree = new OctTree(ResultVertices, bounds.Lower, bounds.Upper);

                    SeperateSurfaces();
                }
            }
            OriginalFaces.Clear();
            OriginalVertices.Clear();
            for (int i = 0; i < ResultFaces.Count; i++)
            {
                OriginalFaces.Add(ResultFaces[i]);
            }

            for (int i = 0; i < ResultVertices.Count; i++)
            {
                OriginalVertices.Add(new Point3D(ResultVertices[i].X,
                                              ResultVertices[i].Y,
                                              ResultVertices[i].Z));
            }
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
        /// Reallocate triangles of original object
        /// to the  new one. Split triangles which cross
        /// the border
        /// </summary>
        private void SeperateSurfaces()
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
                            AddTriangle(aP, bP, cP, ResultOctTree, ResultFaces);
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
                                // add triangle aP, sP, eP
                                AddTriangle(aP, sP, eP, ResultOctTree, ResultFaces);

                                // add triangle  bP, sP, eP
                                AddTriangle(bP, eP, sP, ResultOctTree, ResultFaces);

                                // add triangle bp , eP cP
                                AddTriangle(bP, cP, eP, ResultOctTree, ResultFaces);
                            }
                            else if (bUp)
                            {
                                Point3D sP = SplitEdge(bP, cP);
                                Point3D eP = SplitEdge(bP, aP);
                                // add triangle bP, sP, eP
                                AddTriangle(bP, sP, eP, ResultOctTree, ResultFaces);

                                // add triangle  aP, sP, eP
                                AddTriangle(aP, eP, sP, ResultOctTree, ResultFaces);

                                // add triangle ap , eP cP
                                AddTriangle(aP, sP, cP, ResultOctTree, ResultFaces);
                            }
                            else if (cUp)
                            {
                                Point3D sP = SplitEdge(cP, aP);
                                Point3D eP = SplitEdge(cP, bP);

                                // add triangle cP, sP, eP
                                AddTriangle(cP, sP, eP, ResultOctTree, ResultFaces);

                                // add triangle  bP, sP, eP
                                AddTriangle(bP, eP, sP, ResultOctTree, ResultFaces);

                                // add triangle bp , eP aP
                                AddTriangle(bP, sP, aP, ResultOctTree, ResultFaces);
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

                                AddTriangle(aP, bP, sP, ResultOctTree, ResultFaces);
                                AddTriangle(aP, sP, eP, ResultOctTree, ResultFaces);

                                AddTriangle(cP, eP, sP, ResultOctTree, ResultFaces);
                            }
                            else if (bUp && cUp)
                            {
                                Point3D sP = SplitEdge(cP, aP);
                                Point3D eP = SplitEdge(aP, bP);

                                AddTriangle(bP, cP, sP, ResultOctTree, ResultFaces);
                                AddTriangle(bP, sP, eP, ResultOctTree, ResultFaces);

                                AddTriangle(aP, eP, sP, ResultOctTree, ResultFaces);
                            }
                            else if (cUp && aUp)
                            {
                                Point3D sP = SplitEdge(aP, bP);
                                Point3D eP = SplitEdge(bP, cP);

                                AddTriangle(aP, sP, cP, ResultOctTree, ResultFaces);
                                AddTriangle(cP, sP, eP, ResultOctTree, ResultFaces);
                                AddTriangle(bP, eP, sP, ResultOctTree, ResultFaces);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            AddTriangle(aP, bP, cP, ResultOctTree, ResultFaces);
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
                case SeamerOrientation.Horizontal:
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

                case SeamerOrientation.Vertical:
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

                case SeamerOrientation.Distal:
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
                case SeamerOrientation.Horizontal:
                    {
                        res = TriPointsX(a, b, c, ref aOn, ref bOn, ref cOn);
                    }
                    break;

                case SeamerOrientation.Vertical:
                    {
                        res = TriPointsY(a, b, c, ref aOn, ref bOn, ref cOn);
                    }
                    break;

                case SeamerOrientation.Distal:
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