﻿// **************************************************************************
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

using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    public partial class TankDialog2 : BaseModellerDialog
    {
        private System.Windows.Point[] m1LinkConnectorCoords =
    {
         new System.Windows.Point(0.6,-0.05),
         new System.Windows.Point(0.7,-0.9),
         new System.Windows.Point(1.15,-0.9),
        new System.Windows.Point(1.3,-0.05),
         new System.Windows.Point(1.3,0.05),
         new System.Windows.Point(1.2,0.9),
         new System.Windows.Point(1.1,0.9),
         new System.Windows.Point(1.0,3),
         new System.Windows.Point(0.9,3),
         new System.Windows.Point(0.8,0.9),
         new System.Windows.Point(0.7,0.9),
         new System.Windows.Point(0.6,0.05),
     };

        private System.Windows.Point[] m1MainPolyCoords =
    {
         new System.Windows.Point(0.0,-0.05),
         new System.Windows.Point(0.05,-0.25),
         new System.Windows.Point(0.1,-1.0),
         new System.Windows.Point(0.9,-1.0),
         new System.Windows.Point(1.0,-0.1),
         new System.Windows.Point(1.0,0.1),
         new System.Windows.Point(0.9,1.0),
         new System.Windows.Point(0.1,1.0),
         new System.Windows.Point(0.05,0.25),
         new System.Windows.Point(0.0,0.05),
        };

        private System.Windows.Point[] m1OuterLinkConnectorCoords =
              {
         new System.Windows.Point(0.8,-0.05),
         new System.Windows.Point(0.9,-0.7),
         new System.Windows.Point(1.0,-0.7),
        new System.Windows.Point(1.2,-0.05),
         new System.Windows.Point(1.2,0.05),
         new System.Windows.Point(1.0,0.7),

         new System.Windows.Point(0.9,0.7),
         new System.Windows.Point(0.8,0.05),
     };

        internal void GenerateLinkPart(System.Windows.Point p1, System.Windows.Point p2, Point3DCollection vertices, Int32Collection faces, bool firstCall, double width, double thickness, Link link)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            foreach (LinkPart lp in link.Parts)
            {
                List<PointF> rawprofile = lp.Profile;
                List<Triangle> rawTriangles = lp.Triangles;
                if (rawprofile != null)
                {
                    List<PointF> linkProfile = new List<PointF>();
                    List<Triangle> linkTriangles = new List<Triangle>();
                    // rawprofile should have the basic shape of the part
                    // horizontal, with coordinates in the range 0 to 1
                    if (Math.Abs(dx) < 0.000001 && dy > 0.0001)
                    {
                        VerticalDown(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                        VerticalDownTriangles(lp.X, lp.Y, p1, p2, rawTriangles, linkTriangles, thickness);
                    }
                    else if (Math.Abs(dx) < 0.000001 && dy < 0.0001)
                    {
                        VerticalUp(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                        VerticalUpTriangles(lp.X, lp.Y, p1, p2, rawTriangles, linkTriangles, thickness);
                    }
                    else if (dx > 0.0001 && Math.Abs(dy) < 0.0000011)
                    {
                        HorizontalLeft(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                        HorizontalLeftTriangles(lp.X, lp.Y, p1, p2, rawTriangles, linkTriangles, thickness);
                    }
                    else if (dx < 0.0001 && Math.Abs(dy) < 0.000001)
                    {
                        HorizontalRight(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                        HorizontalRightTriangles(lp.X, lp.Y, p1, p2, rawTriangles, linkTriangles, thickness);
                    }
                    else
                    {
                        NonOrthogonal(lp.X, lp.Y, p1, p2, rawprofile, linkProfile, thickness);
                        NonOrthogonalTriangles(lp.X, lp.Y, p1, p2, rawTriangles, linkTriangles, thickness);
                    }

                    double partBackZ = (lp.Z - lp.W / 2.0) * width;
                    double partFrontZ = (lp.Z + lp.W / 2.0) * width;
                    MakeFacesForLinkPart(linkProfile, linkTriangles, partBackZ, partFrontZ, vertices, faces);
                }
            }
        }

        private static void HorizontalLeft(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<PointF> rawProfile, List<PointF> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            foreach (PointF rawp in rawProfile)
            {
                double x = p1.X + ((rawp.X + xo) * dx);
                double y = p1.Y - ((rawp.Y + yo) * height);
                rotatedProfile.Add(new PointF((float)x, (float)y));
            }
        }

        private static void HorizontalLeftTriangles(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<Triangle> raw, List<Triangle> rotated, double height)
        {
            double dx = p2.X - p1.X;
            foreach (Triangle rawtri in raw)
            {
                Triangle rottri = new Triangle();
                int i = 0;
                foreach (PointF rawp in rawtri.Points)
                {
                    double x = p1.X + ((rawp.X + xo) * dx);
                    double y = p1.Y - ((rawp.Y + yo) * height);
                    rottri.Points[i] = new PointF((float)x, (float)y);
                    i++;
                }
                rotated.Add(rottri);
            }
        }

        private static void HorizontalRight(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<PointF> rawProfile, List<PointF> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            foreach (PointF rawp in rawProfile)
            {
                double x = p1.X + ((rawp.X + xo) * dx);
                double y = p1.Y + ((rawp.Y + yo) * height);
                rotatedProfile.Add(new System.Drawing.PointF((float)x, (float)y));
            }
        }

        private static void HorizontalRightTriangles(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<Triangle> raw, List<Triangle> rotated, double height)
        {
            double dx = p2.X - p1.X;
            foreach (Triangle rawtri in raw)
            {
                Triangle rottri = new Triangle();
                int i = 0;
                foreach (PointF rawp in rawtri.Points)
                {
                    double x = p1.X + ((rawp.X + xo) * dx);
                    double y = p1.Y + ((rawp.Y + yo) * height);
                    rottri.Points[i] = new PointF((float)x, (float)y);
                    i++;
                }
                rotated.Add(rottri);
            }
        }

        private static void NonOrthogonal(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<PointF> rawProfile, List<PointF> rotatedProfile, double height)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dist = TankTrackUtils.Distance(p1, p2);
            double sign = 1;
            // right down
            if (dx > 0 && dy < 0)
            {
                sign = -1;
            }
            else
            if (dx > 0 && dy > 0)
            {
                sign = 1;
            }
            else
            if (dx < 0 && dy < 0)
            {
                sign = -1;
            }
            else
            if (dx < 0 && dy > 0)
            {
                sign = 1;
            }

            foreach (PointF rawp in rawProfile)
            {
                PointF o1 = TankTrackUtils.PerpendicularF(p1, p2, rawp.X + xo, sign * (rawp.Y + yo) * height);
                rotatedProfile.Add(o1);
            }
        }

        private static void NonOrthogonalTriangles(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<Triangle> raw, List<Triangle> rotated, double height)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dist = TankTrackUtils.Distance(p1, p2);
            double sign = 1;
            // right down
            if (dx > 0)
            {
                if (dy < 0)
                {
                    sign = -1.0;
                }
                else
                {
                    sign = 1;
                }
            }
            else
            if (dx < 0)
            {
                if (dy < 0)
                {
                    sign = -1.0;
                }
                else
                {
                    sign = 1;
                }
            }

            foreach (Triangle rawtri in raw)
            {
                Triangle rottri = new Triangle();
                int i = 0;
                foreach (PointF rawp in rawtri.Points)
                {
                    rottri.Points[i] = TankTrackUtils.PerpendicularF(p1, p2, rawp.X + xo, sign * (rawp.Y + yo) * height); ;
                    i++;
                }
                rotated.Add(rottri);
            }
        }

        private static void VerticalDown(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<PointF> rawProfile, List<PointF> rotatedProfile, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (PointF rawp in rawProfile)
            {
                double x = p1.X + ((rawp.Y + yo) * height);
                double y = p1.Y + ((rawp.X + xo) * dy);
                rotatedProfile.Add(new PointF((float)x, (float)y));
            }
        }

        private static void VerticalDownTriangles(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<Triangle> raw, List<Triangle> rotated, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (Triangle rawtri in raw)
            {
                Triangle rottri = new Triangle();
                int i = 0;
                foreach (PointF rawp in rawtri.Points)
                {
                    double x = p1.X + ((rawp.Y + yo) * height);
                    double y = p1.Y + ((rawp.X + xo) * dy);
                    rottri.Points[i] = new PointF((float)x, (float)y);
                    i++;
                }
                rotated.Add(rottri);
            }
        }

        private static void VerticalUp(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<PointF> rawProfile, List<PointF> rotatedProfile, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (PointF rawp in rawProfile)
            {
                // yes this is meant to look odd
                double x = p1.X - ((rawp.Y + yo) * height);
                double y = p1.Y + ((rawp.X + xo) * dy);
                rotatedProfile.Add(new PointF((float)x, (float)y));
            }
        }

        private static void VerticalUpTriangles(double xo, double yo, System.Windows.Point p1, System.Windows.Point p2, List<Triangle> raw, List<Triangle> rotated, double height)
        {
            double dy = p2.Y - p1.Y;
            foreach (Triangle rawtri in raw)
            {
                Triangle rottri = new Triangle();
                int i = 0;
                foreach (PointF rawp in rawtri.Points)
                {
                    double x = p1.X - ((rawp.Y + yo) * height);
                    double y = p1.Y + ((rawp.X + xo) * dy);
                    rottri.Points[i] = new PointF((float)x, (float)y);
                    i++;
                }
                rotated.Add(rottri);
            }
        }

        private void CreateInnerFace(List<System.Windows.Point> ply, int i, Point3DCollection verts, Int32Collection facs)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(verts, ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(verts, ply[i].X, ply[i].Y, trackWidth);
            int c2 = AddVertice(verts, ply[v].X, ply[v].Y, trackWidth);
            int c3 = AddVertice(verts, ply[v].X, ply[v].Y, 0.0);
            facs.Add(c0);
            facs.Add(c1);
            facs.Add(c2);

            facs.Add(c0);
            facs.Add(c2);
            facs.Add(c3);
        }

        private void CreateOutsideFace(List<System.Windows.Point> ply, int i)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(ply[i].X, ply[i].Y, trackWidth);
            int c2 = AddVertice(ply[v].X, ply[v].Y, trackWidth);
            int c3 = AddVertice(ply[v].X, ply[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void CreateOutsideFace(List<System.Windows.Point> ply, int i, Point3DCollection verts, Int32Collection facs)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(verts, ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(verts, ply[i].X, ply[i].Y, trackWidth);
            int c2 = AddVertice(verts, ply[v].X, ply[v].Y, trackWidth);
            int c3 = AddVertice(verts, ply[v].X, ply[v].Y, 0.0);
            facs.Add(c0);
            facs.Add(c2);
            facs.Add(c1);

            facs.Add(c0);
            facs.Add(c3);
            facs.Add(c2);
        }

        private void GenerateCentreGuideTrack()
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    TankTrackUtils.CentreGuideLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, thickness, guideSize, spudSize);
                    firstCall = false;
                }
            }
        }

        private void GenerateFaces()
        {
            ClearShape();
            MakeFacesFrommOuterAndInner();
        }

        private void GenerateFaces(Point3DCollection verts, Int32Collection facs)
        {
            facs.Clear();
            verts.Clear();
            MakeFacesFrommOuterAndInner(verts, facs);
        }

        private void GenerateM1Main(Point3DCollection verts, Int32Collection facs)
        {
            if (outerPolygon != null)
            {
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    List<PointF> linkProfile = new List<PointF>();

                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1MainPolyCoords, thickness);
                    double partBackZ = 0;
                    double partFrontZ = trackWidth;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ, verts, facs);

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1LinkConnectorCoords, thickness + guideSize);
                    partBackZ = trackWidth;
                    partFrontZ = trackWidth * 1.1;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ, verts, facs);
                    GetLinkPartProfile(p1, p2, ref linkProfile, m1OuterLinkConnectorCoords, thickness + guideSize);
                    partBackZ = partFrontZ;
                    partFrontZ = trackWidth * 1.3;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ, verts, facs);

                    GetLinkPartProfile(p1, p2, ref linkProfile, m1LinkConnectorCoords, thickness + guideSize);
                    partBackZ = -0.1 * TrackWidth;
                    partFrontZ = 0;
                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ, verts, facs);
                    GetLinkPartProfile(p1, p2, ref linkProfile, m1OuterLinkConnectorCoords, thickness + guideSize);
                    partFrontZ = partBackZ;
                    partBackZ = partFrontZ - 0.2 * TrackWidth;

                    MakeFacesForLinkPart(linkProfile, partBackZ, partFrontZ, verts, facs);
                }
            }
        }

        private void GenerateM1Track()
        {
            ClearShape();
            GenerateM1Main(Vertices, Faces);
        }

        private void GenerateM1Track(Point3DCollection verts, Int32Collection facs)
        {
            facs.Clear();
            verts.Clear();
            GenerateM1Main(verts, facs);
        }

        private void GenerateSimpleTrack(int subType, Point3DCollection verts, Int32Collection facs)
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    switch (subType)
                    {
                        case 0:
                            {
                                SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, 1);
                            }
                            break;

                        case 1:
                            {
                                SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall, -1);
                            }
                            break;
                    }

                    firstCall = false;
                }
            }
        }

        private void GetLinkPartProfile(System.Windows.Point p1, System.Windows.Point p2, ref List<PointF> poly, System.Windows.Point[] shape, double size)
        {
            poly.Clear();

            for (int i = 0; i < shape.GetLength(0); i++)
            {
                System.Windows.Point po = shape[i];

                System.Windows.Point o1 = Perpendicular2(p1, p2, po.X, po.Y * size);
                if (PathEditor.LocalImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    o1.Y = -o1.Y;
                }
                else
                {
                    o1.X = ToMM(o1.X);
                    o1.Y = ToMM(-o1.Y);
                }

                poly.Add(new PointF((float)o1.X, (float)o1.Y));
            }
        }

        private void MakeFacesForLinkPart(List<System.Windows.Point> linkProfile, double partBackZ, double partFrontZ)
        {
            // make faces for this single link part
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in linkProfile)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            TriangulationPolygon ply = new TriangulationPolygon();
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, partBackZ);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, partBackZ);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, partBackZ);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, partFrontZ);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, partFrontZ);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, partFrontZ);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }

            for (int k = 0; k < linkProfile.Count; k++)
            {
                int l = k + 1;
                if (l >= linkProfile.Count)
                {
                    l = 0;
                }
                int c0 = AddVertice(linkProfile[k].X, linkProfile[k].Y, partBackZ);
                int c1 = AddVertice(linkProfile[l].X, linkProfile[l].Y, partBackZ);
                int c2 = AddVertice(linkProfile[l].X, linkProfile[l].Y, partFrontZ);
                int c3 = AddVertice(linkProfile[k].X, linkProfile[k].Y, partFrontZ);

                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);
            }
        }

        private void MakeFacesForLinkPart(List<PointF> linkProfile, double partBackZ, double partFrontZ, Point3DCollection verts, Int32Collection facs)
        {
            // make faces for this single link part
            /*
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in linkProfile)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            */
            TriangulationPolygon ply = new TriangulationPolygon();
            // ply.Points = pf.ToArray();
            ply.Points = linkProfile.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(verts, t.Points[0].X, t.Points[0].Y, partBackZ);
                int c1 = AddVertice(verts, t.Points[1].X, t.Points[1].Y, partBackZ);
                int c2 = AddVertice(verts, t.Points[2].X, t.Points[2].Y, partBackZ);
                facs.Add(c0);
                facs.Add(c2);
                facs.Add(c1);

                c0 = AddVertice(verts, t.Points[0].X, t.Points[0].Y, partFrontZ);
                c1 = AddVertice(verts, t.Points[1].X, t.Points[1].Y, partFrontZ);
                c2 = AddVertice(verts, t.Points[2].X, t.Points[2].Y, partFrontZ);
                facs.Add(c0);
                facs.Add(c1);
                facs.Add(c2);
            }

            for (int k = 0; k < linkProfile.Count; k++)
            {
                int l = k + 1;
                if (l >= linkProfile.Count)
                {
                    l = 0;
                }
                int c0 = AddVertice(verts, linkProfile[k].X, linkProfile[k].Y, partBackZ);
                int c1 = AddVertice(verts, linkProfile[l].X, linkProfile[l].Y, partBackZ);
                int c2 = AddVertice(verts, linkProfile[l].X, linkProfile[l].Y, partFrontZ);
                int c3 = AddVertice(verts, linkProfile[k].X, linkProfile[k].Y, partFrontZ);

                facs.Add(c0);
                facs.Add(c2);
                facs.Add(c1);

                facs.Add(c0);
                facs.Add(c3);
                facs.Add(c2);
            }
        }

        private void MakeFacesForLinkPart(List<PointF> linkProfile, List<Triangle> tris, double partBackZ, double partFrontZ, Point3DCollection verts, Int32Collection facs)
        {
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(verts, t.Points[0].X, t.Points[0].Y, partBackZ);
                int c1 = AddVertice(verts, t.Points[1].X, t.Points[1].Y, partBackZ);
                int c2 = AddVertice(verts, t.Points[2].X, t.Points[2].Y, partBackZ);
                facs.Add(c0);
                facs.Add(c1);
                facs.Add(c2);

                c0 = AddVertice(verts, t.Points[0].X, t.Points[0].Y, partFrontZ);
                c1 = AddVertice(verts, t.Points[1].X, t.Points[1].Y, partFrontZ);
                c2 = AddVertice(verts, t.Points[2].X, t.Points[2].Y, partFrontZ);
                facs.Add(c0);
                facs.Add(c2);
                facs.Add(c1);
            }

            for (int k = 0; k < linkProfile.Count; k++)
            {
                int l = k + 1;
                if (l >= linkProfile.Count)
                {
                    l = 0;
                }
                int c0 = AddVertice(verts, linkProfile[k].X, linkProfile[k].Y, partBackZ);
                int c1 = AddVertice(verts, linkProfile[l].X, linkProfile[l].Y, partBackZ);
                int c2 = AddVertice(verts, linkProfile[l].X, linkProfile[l].Y, partFrontZ);
                int c3 = AddVertice(verts, linkProfile[k].X, linkProfile[k].Y, partFrontZ);

                facs.Add(c0);
                facs.Add(c2);
                facs.Add(c1);

                facs.Add(c0);
                facs.Add(c3);
                facs.Add(c2);
            }
        }

        private void MakeFacesFrommOuterAndInner()
        {
            List<System.Windows.Point> otmp = new List<System.Windows.Point>();
            List<System.Windows.Point> itmp = new List<System.Windows.Point>();
            if (outerPolygon != null)
            {
                double top = 0;
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (outerPolygon[i].Y > top)
                    {
                        top = outerPolygon[i].Y;
                    }
                }
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (PathEditor.LocalImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        otmp.Insert(0, new System.Windows.Point(outerPolygon[i].X, top - outerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(outerPolygon[i].X);
                        double y = ToMM(top - outerPolygon[i].Y);
                        otmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                // generate side triangles so original points are already in list
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    CreateOutsideFace(otmp, i);
                }
                itmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    if (PathEditor.LocalImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        itmp.Insert(0, new System.Windows.Point(innerPolygon[i].X, top - innerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(innerPolygon[i].X);
                        double y = ToMM(top - innerPolygon[i].Y);
                        itmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    CreateInnerFace(itmp, i, Vertices, Faces);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, 0.0);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, 0.0);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, 0.0);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, trackWidth);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, trackWidth);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, trackWidth);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, trackWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }
                CentreVertices();
            }
        }

        private void MakeFacesFrommOuterAndInner(Point3DCollection verts, Int32Collection facs)
        {
            List<System.Windows.Point> otmp = new List<System.Windows.Point>();
            List<System.Windows.Point> itmp = new List<System.Windows.Point>();
            if (outerPolygon != null)
            {
                double top = 0;
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (outerPolygon[i].Y > top)
                    {
                        top = outerPolygon[i].Y;
                    }
                }
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (PathEditor.LocalImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        otmp.Insert(0, new System.Windows.Point(outerPolygon[i].X, top - outerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(outerPolygon[i].X);
                        double y = ToMM(top - outerPolygon[i].Y);
                        otmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                // generate side triangles so original points are already in list
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    CreateOutsideFace(otmp, i, verts, facs);
                }
                itmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    if (PathEditor.LocalImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        itmp.Insert(0, new System.Windows.Point(innerPolygon[i].X, top - innerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(innerPolygon[i].X);
                        double y = ToMM(top - innerPolygon[i].Y);
                        itmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    CreateInnerFace(itmp, i, verts, facs);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(verts, itmp[i].X, itmp[i].Y, 0.0);
                    int c1 = AddVertice(verts, otmp[i].X, otmp[i].Y, 0.0);
                    int c2 = AddVertice(verts, otmp[v].X, otmp[v].Y, 0.0);
                    int c3 = AddVertice(verts, itmp[v].X, itmp[v].Y, 0.0);
                    facs.Add(c0);
                    facs.Add(c2);
                    facs.Add(c1);

                    facs.Add(c0);
                    facs.Add(c3);
                    facs.Add(c2);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(verts, itmp[i].X, itmp[i].Y, trackWidth);
                    int c1 = AddVertice(verts, otmp[i].X, otmp[i].Y, trackWidth);
                    int c2 = AddVertice(verts, otmp[v].X, otmp[v].Y, trackWidth);
                    int c3 = AddVertice(verts, itmp[v].X, itmp[v].Y, trackWidth);
                    facs.Add(c0);
                    facs.Add(c1);
                    facs.Add(c2);

                    facs.Add(c0);
                    facs.Add(c2);
                    facs.Add(c3);
                }
                CentreVertices(verts, facs);
            }
        }

        private void SimpleLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall, int spudSign)
        {
            double spudDim = spudSign * spudSize;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spudDim, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p2.Y));
                }
                else
                {
                    // vertical upwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spudDim, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p2.Y));
                }
            }
            else
            if (dy == 0)
            {
                if (p2.X - p1.X > 0)
                {
                    // Horizontal Left to right
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spudDim));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - +thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness + spudDim));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y + thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y - thickness));
                }
            }
            else
            {
                double sign = -1;
                if (dx > 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx > 0 && dy > 0)
                {
                    sign = -1;
                }
                else
                if (dx < 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx < 0 && dy > 0)
                {
                    sign = -1;
                }
                System.Windows.Point o1 = TankTrackUtils.Perpendicular(p1, p2, 0.0, -sign * thickness);
                System.Windows.Point o2 = TankTrackUtils.Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o3 = TankTrackUtils.Perpendicular(p1, p2, 0.87, -sign * (thickness + spudDim));
                System.Windows.Point o4 = TankTrackUtils.Perpendicular(p1, p2, 1.0, -sign * thickness);
                if (firstCall)
                {
                    outter.Add(o1);
                }
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);

                System.Windows.Point i1 = TankTrackUtils.Perpendicular(p1, p2, 0.0, sign * thickness);
                System.Windows.Point i2 = TankTrackUtils.Perpendicular(p1, p2, 0.75, sign * thickness);
                System.Windows.Point i3 = TankTrackUtils.Perpendicular(p1, p2, 0.87, sign * thickness);
                System.Windows.Point i4 = TankTrackUtils.Perpendicular(p1, p2, 1.0, sign * thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
            }
        }
    }
}