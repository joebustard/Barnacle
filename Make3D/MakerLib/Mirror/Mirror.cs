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
using FixLib;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace MakerLib.Mirror
{
    public class Mirror
    {
        public static void Reflect(Object3D ob, string direction)
        {
            Bounds3D bnds = new Bounds3D();

            foreach (P3D p in ob.RelativeObjectVertices)
            {
                bnds.Adjust(new Point3D((double)p.X, (double)p.Y, (double)p.Z));
            }
            int numPoints = ob.RelativeObjectVertices.Count;
            int numFaces = ob.TriangleIndices.Count;
            bool addFaces = false;
            switch (direction.ToLower())
            {
                case "left":
                    {
                        double ox = 2 * bnds.Lower.X - 0.1;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(ox - op.X, op.Y, op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;

                case "right":
                    {
                        double ox = bnds.Upper.X + bnds.Width + bnds.Lower.X;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(ox - op.X, op.Y, op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;

                case "front":
                    {
                        double oz = 2 * bnds.Upper.Z;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(op.X, op.Y, oz - op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;

                case "back":
                    {
                        double oz = 2 * bnds.Lower.Z;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(op.X, op.Y, oz - op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;

                case "up":
                    {
                        double oy = 2 * bnds.Upper.Y;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(op.X, oy - op.Y, op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;

                case "down":
                    {
                        double oy = 2 * bnds.Lower.Y;
                        for (int i = 0; i < numPoints; i++)
                        {
                            P3D op = ob.RelativeObjectVertices[i];
                            P3D np = new P3D(op.X, oy - op.Y, op.Z);
                            ob.RelativeObjectVertices.Add(np);
                        }
                        addFaces = true;
                    }
                    break;
            }
            if (addFaces)
            {
                for (int f = 0; f < numFaces; f += 3)
                {
                    int v0 = ob.TriangleIndices[f] + numPoints;
                    int v1 = ob.TriangleIndices[f + 1] + numPoints;
                    int v2 = ob.TriangleIndices[f + 2] + numPoints;
                    ob.TriangleIndices.Add(v0);
                    ob.TriangleIndices.Add(v2);
                    ob.TriangleIndices.Add(v1);
                }
            }
            RemoveDuplicateVertices(ob);
            ob.Remesh();
            RemoveDuplicateFaces(ob);
            ob.Remesh();
            ob.CalcScale(false);
        }

        private static void RemoveDuplicateFaces(Object3D ob)
        {
            DuplicateTriangles.RemoveBothDuplicateTriangles(ob.RelativeObjectVertices.Count, ob.TriangleIndices);
        }

        private static void RemoveDuplicateVertices(Object3D ob)
        {
            Fixer checker = new Fixer();
            Point3DCollection points = new Point3DCollection();
            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, points);

            checker.RemoveDuplicateVertices(points, ob.TriangleIndices);
            PointUtils.PointCollectionToP3D(checker.Vertices, ob.RelativeObjectVertices);
            ob.TriangleIndices = checker.Faces;
            ob.Remesh();
        }
    }
}