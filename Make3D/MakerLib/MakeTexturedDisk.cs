﻿using Barnacle.Object3DLib;
using MakerLib.TextureUtils;
using OctTreeLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TexturedDiskMaker : MakerBase
    {
        private double diskHeight;
        private double radius;

        private double sweep;
        private string texture;
        private double textureDepth;
        private double textureResolution;
        private double vTextureResolution;
        private double hTextureResolution;
        private TextureManager textureManager;

        public TexturedDiskMaker(double diskHeight, double radius, double sweep, string texture, double textureDepth, double textureResolution)
        {
            this.diskHeight = diskHeight;
            this.radius = radius - textureDepth;

            this.sweep = sweep;
            this.texture = texture;
            this.textureDepth = textureDepth;
            this.textureResolution = textureResolution;
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureImage(texture);
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            double twoPI = Math.PI * 2.0;
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            double maxSweep = DegToRad(sweep);
            double xExtent = 1.5 * (textureDepth + radius);
            double yExtent = 1.5 * diskHeight;
            OctTree octTree = CreateOctree(new Point3D(-xExtent, -yExtent, -xExtent),
                          new Point3D(+xExtent, +yExtent, +xExtent));
            Faces = faces;
            if (textureResolution <= 0)
            {
                textureResolution = 0.1;
            }
            vTextureResolution = textureResolution;
            hTextureResolution = textureResolution;

            if (textureManager.Mode == TextureManager.MapMode.FittedTile)
            {
                // estimate vertical size in steps
                double ySteps = diskHeight / vTextureResolution;
                double vRepeats = (ySteps / textureManager.PatternHeight);
                vRepeats = Math.Ceiling(vRepeats);
                vTextureResolution = diskHeight / (vRepeats * textureManager.PatternHeight);

                double circumference = radius * Math.PI * 2.0;
                circumference = (DegToRad(sweep) / (Math.PI * 2.0)) * circumference;
                double xSteps = circumference / hTextureResolution;
                double hRepeats = (xSteps / textureManager.PatternWidth);
                hRepeats = Math.Ceiling(hRepeats);
                hTextureResolution = circumference / (hRepeats * textureManager.PatternWidth);
            }
            if (textureManager.Mode == TextureManager.MapMode.FittedSingle)
            {
                // estimate vertical size in steps

                vTextureResolution = diskHeight / textureManager.PatternHeight;

                double circumference = radius * Math.PI * 2.0;
                circumference = (DegToRad(sweep) / (Math.PI * 2.0)) * circumference;
                hTextureResolution = circumference / (textureManager.PatternWidth + 2);

                // should check if the original rsolution is smaller, if so add an offset to shift the pattern up or round
            }
            // Whats the inner sweep angle in degrees
            double inswe = (hTextureResolution * 360.0) / (twoPI * radius);

            // outer sweep is going to be close but just takes into account the depth
            double outswe = (hTextureResolution * 360.0) / (twoPI * (radius + textureDepth));
            inswe = DegToRad(inswe);
            outswe = DegToRad(outswe);

            // so a "FacePIXEL" corresponds to a sweep of inswe
            // In practice how many X-pixels does that mean we have

            int maxFacePixel = (int)((Math.PI * 2.0) / inswe);
            double facePixelHeight = diskHeight / vTextureResolution;

            double y = 0;
            int tx = 0;
            int ty = 0;
            TextureCell cell;

            double deltaY = vTextureResolution;

            double theta = 0;

            while (y < diskHeight)
            {
                if (diskHeight - y < deltaY)
                {
                    deltaY = diskHeight - y;
                }
                tx = 0;
                theta = 0;
                while (theta < maxSweep)
                {
                    cell = textureManager.GetCell(tx, ty);
                    if (cell != null)
                    {
                        if (cell.Width == 0)
                        {
                            Point p = CalcPoint(theta, radius);
                            Point p2 = CalcPoint(theta + inswe, radius);
                            MakeVSquareFace(p.X, y, p.Y, p2.X, y + deltaY, p2.Y);
                        }
                        else
                        {
                            double zoff = ((double)cell.Width * textureDepth) / 255.0;
                            Point p = CalcPoint(theta, radius + zoff);
                            Point p2 = CalcPoint(theta + inswe, radius + zoff);
                            MakeVSquareFace(p.X, y, p.Y, p2.X, y + deltaY, p2.Y);

                            if (cell.WestWall > 0)
                            {
                                double westSideDepth = ((double)(cell.Width - cell.WestWall) * textureDepth) / 255.0;
                                Point p3 = CalcPoint(theta, radius + westSideDepth);
                                MakeVSquareFace(p3.X, y, p3.Y, p.X, y + deltaY, p.Y);
                            }

                            // must always close left edge of pattern
                            if (tx == 0)
                            {
                                Point p3 = CalcPoint(theta, radius);
                                MakeVSquareFace(p3.X, y, p3.Y, p.X, y + deltaY, p.Y);
                            }

                            if (cell.EastWall > 0)
                            {
                                double sideDepth = ((double)(cell.Width - cell.EastWall) * textureDepth) / 255.0;
                                Point p3 = CalcPoint(theta + inswe, radius + sideDepth);
                                MakeVSquareFace(p2.X, y, p2.Y, p3.X, y + deltaY, p3.Y);
                            }

                            if (theta + inswe >= maxSweep)
                            {
                                Point p3 = CalcPoint(theta + inswe, radius);
                                MakeVSquareFace(p2.X, y, p2.Y, p3.X, y + deltaY, p3.Y);
                            }
                            if (cell.NorthWall > 0)
                            {
                                double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                Point p4 = CalcPoint(theta, radius + zoff);
                                Point p5 = CalcPoint(theta, radius + sideDepth);
                                Point p6 = CalcPoint(theta + inswe, radius + sideDepth);
                                Point p7 = CalcPoint(theta + inswe, radius + zoff);

                                int v0 = AddVerticeOctTree(p4.X, y, p4.Y);
                                int v1 = AddVerticeOctTree(p5.X, y, p5.Y);
                                int v2 = AddVerticeOctTree(p6.X, y, p6.Y);
                                int v3 = AddVerticeOctTree(p7.X, y, p7.Y);

                                AddFace(v0, v2, v1);
                                AddFace(v0, v3, v2);
                            }
                            else
                            {
                                if (y + deltaY >= diskHeight)
                                {
                                    double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                    Point p4 = CalcPoint(theta, radius + zoff);
                                    Point p5 = CalcPoint(theta, radius);
                                    Point p6 = CalcPoint(theta + inswe, radius);
                                    Point p7 = CalcPoint(theta + inswe, radius + zoff);

                                    int v0 = AddVerticeOctTree(p4.X, y, p4.Y);
                                    int v1 = AddVerticeOctTree(p5.X, y, p5.Y);
                                    int v2 = AddVerticeOctTree(p6.X, y, p6.Y);
                                    int v3 = AddVerticeOctTree(p7.X, y, p7.Y);

                                    AddFace(v0, v2, v1);
                                    AddFace(v0, v3, v2);
                                }
                            }

                            if (cell.SouthWall > 0)
                            {
                                double sideDepth = ((double)(cell.Width - cell.SouthWall) * textureDepth) / 255.0;

                                Point p4 = CalcPoint(theta, radius + zoff);
                                Point p5 = CalcPoint(theta, radius + sideDepth);
                                Point p6 = CalcPoint(theta + inswe, radius + sideDepth);
                                Point p7 = CalcPoint(theta + inswe, radius + zoff);

                                int v0 = AddVerticeOctTree(p4.X, y + deltaY, p4.Y);
                                int v1 = AddVerticeOctTree(p5.X, y + deltaY, p5.Y);
                                int v2 = AddVerticeOctTree(p6.X, y + deltaY, p6.Y);
                                int v3 = AddVerticeOctTree(p7.X, y + deltaY, p7.Y);

                                AddFace(v0, v1, v2);
                                AddFace(v0, v2, v3);
                            }
                            else
                            {
                                if (y + deltaY >= diskHeight)
                                {
                                    double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                    Point p4 = CalcPoint(theta, radius + zoff);
                                    Point p5 = CalcPoint(theta, radius);
                                    Point p6 = CalcPoint(theta + inswe, radius);
                                    Point p7 = CalcPoint(theta + inswe, radius + zoff);

                                    int v0 = AddVerticeOctTree(p4.X, y + deltaY, p4.Y);
                                    int v1 = AddVerticeOctTree(p5.X, y + deltaY, p5.Y);
                                    int v2 = AddVerticeOctTree(p6.X, y + deltaY, p6.Y);
                                    int v3 = AddVerticeOctTree(p7.X, y + deltaY, p7.Y);

                                    AddFace(v0, v1, v2);
                                    AddFace(v0, v2, v3);
                                }
                            }
                        }
                    }

                    tx++;
                    theta += inswe;
                }
                y += deltaY;
                ty++;
            }
            if (Faces.Count > 0)
            {
                Bottom(inswe, radius, maxSweep);
                End(inswe, radius, maxSweep, y, false);
            }
            if (sweep < 360.0)
            {
                // need to close off swept ends
                Point p = CalcPoint(0, radius);
                int v0 = AddVerticeOctTree(p.X, 0, p.Y);
                int v1 = AddVerticeOctTree(p.X, diskHeight, p.Y);
                int v2 = AddVerticeOctTree(0, 0, 0);
                int v3 = AddVerticeOctTree(0, diskHeight, 0);
                AddFace(v0, v2, v1);
                AddFace(v2, v3, v1);

                p = CalcPoint(theta, radius);
                v0 = AddVerticeOctTree(p.X, 0, p.Y);
                v1 = AddVerticeOctTree(p.X, diskHeight, p.Y);
                AddFace(v0, v1, v2);
                AddFace(v2, v1, v3);
            }
        }

        private void Bottom(double dtheta, double radius, double sweep)
        {
            End(dtheta, radius, sweep, 0);
        }

        private void End(double inswe, double radius, double sweep, double y, bool normal = true)
        {
            double theta = 0;
            int v0 = AddVerticeOctTree(0, y, 0);
            while (theta < sweep)
            {
                Point p = CalcPoint(theta, radius);
                Point p2 = CalcPoint(theta + inswe, radius);

                int v1 = AddVerticeOctTree(p.X, y, p.Y);
                int v2 = AddVerticeOctTree(p2.X, y, p2.Y);
                if (normal)
                {
                    AddFace(v0, v1, v2);
                }
                else
                {
                    AddFace(v0, v2, v1);
                }
                theta += inswe;
            }
        }
    }
}