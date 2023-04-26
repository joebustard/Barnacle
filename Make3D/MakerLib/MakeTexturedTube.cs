using MakerLib.TextureUtils;
using OctTreeLib;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TexturedTubeMaker : MakerBase
    {
        private const double twoPI = Math.PI * 2.0;
        private double hTextureResolution;
        private double innerRadius;
        private double radius;

        // side mask 1 means out side textured, 2 means indside, 3 means both
        private int sideMask;

        private double sweep;
        private string texture;
        private double textureDepth;
        private TextureManager textureManager;
        private double textureResolution;
        private double thickness;
        private double tubeHeight;
        private double vTextureResolution;

        public TexturedTubeMaker(double tubeHeight, double radius, double thickness, double sweep, string texture, double textureDepth, double textureResolution, int sideMask = 1)
        {
            this.tubeHeight = tubeHeight;
            this.radius = radius;
            this.thickness = thickness;
            this.innerRadius = radius - thickness;
            this.sweep = sweep;
            this.texture = texture;
            this.textureDepth = textureDepth;
            this.textureResolution = textureResolution;
            this.sideMask = sideMask;
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureImage(texture);
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            double inswe = 0.1;
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            double xExtent = 1.5 * (textureDepth + radius);
            double yExtent = 1.5 * tubeHeight;
            OctTree octTree = CreateOctree(new Point3D(-xExtent, -yExtent, -xExtent),
                          new Point3D(+xExtent, +yExtent, +xExtent));
            Faces = faces;
            double maxSweep = DegToRad(sweep);

            if ((sideMask & 1) != 0)
            {
                GenerateOutsideTexture(maxSweep, out inswe);
            }
            else
            {
                BlankOutside(maxSweep);
            }

            if ((sideMask & 2) != 0)
            {
                GenerateInsideTexture(maxSweep, out inswe);
            }
            else
            {
                BlankInside(maxSweep);
            }

            // generate top and bottom
            if (Faces.Count > 0)
            {
                Bottom(inswe, radius, innerRadius, maxSweep);
                End(inswe, radius, innerRadius, maxSweep, tubeHeight, false);
            }

            // if its not a complete ring we need to close it
            if (sweep < 360.0)
            {
                // need to close off swept ends
                Point p = CalcPoint(0, radius);
                int v0 = AddVerticeOctTree(p.X, 0, p.Y);
                int v1 = AddVerticeOctTree(p.X, tubeHeight, p.Y);
                Point p1 = CalcPoint(0, innerRadius);
                int v2 = AddVerticeOctTree(p1.X, 0, p1.Y);
                int v3 = AddVerticeOctTree(p1.X, tubeHeight, p1.Y);
                AddFace(v0, v2, v1);
                AddFace(v2, v3, v1);

                p = CalcPoint(maxSweep, radius);
                p1 = CalcPoint(maxSweep, innerRadius);
                v0 = AddVerticeOctTree(p.X, 0, p.Y);
                v1 = AddVerticeOctTree(p.X, tubeHeight, p.Y);
                v2 = AddVerticeOctTree(p1.X, 0, p1.Y);
                v3 = AddVerticeOctTree(p1.X, tubeHeight, p1.Y);
                AddFace(v0, v1, v2);
                AddFace(v2, v1, v3);
            }
        }

        private double BlankInside(double maxSweep)
        {
            double dt = 0.1;
            // do the inside of the tube
            double theta = 0;
            while (theta < maxSweep)
            {
                Point p = CalcPoint(theta, innerRadius);
                Point p1 = CalcPoint(theta + dt, innerRadius);
                int v0 = AddVerticeOctTree(p.X, 0, p.Y);
                int v1 = AddVerticeOctTree(p.X, tubeHeight, p.Y);
                int v2 = AddVerticeOctTree(p1.X, tubeHeight, p1.Y);

                int v3 = AddVerticeOctTree(p1.X, 0, p1.Y);
                AddFace(v0, v2, v1);
                AddFace(v2, v0, v3);
                theta += dt;
            }

            return theta;
        }

        private void BlankOutside(double maxSweep)
        {
            double inswe = 0.1;
            // do the inside of the tube
            double theta = 0;
            while (theta < maxSweep)
            {
                Point p = CalcPoint(theta, radius);
                Point p1 = CalcPoint(theta + inswe, radius);
                int v0 = AddVerticeOctTree(p.X, 0, p.Y);
                int v1 = AddVerticeOctTree(p.X, tubeHeight, p.Y);
                int v2 = AddVerticeOctTree(p1.X, tubeHeight, p1.Y);

                int v3 = AddVerticeOctTree(p1.X, 0, p1.Y);
                AddFace(v0, v1, v2);
                AddFace(v2, v3, v0);
                theta += inswe;
            }
        }

        private void Bottom(double dtheta, double radius, double innerRadius, double sweep)
        {
            End(dtheta, radius, innerRadius, sweep, 0);
        }

        private void End(double inswe, double radius, double innerRadius, double sweep, double y, bool normal = true)
        {
            double theta = 0;

            while (theta < sweep)
            {
                Point p = CalcPoint(theta, radius);
                Point p1 = CalcPoint(theta + inswe, radius);
                Point p2 = CalcPoint(theta, innerRadius);
                Point p3 = CalcPoint(theta + inswe, innerRadius);
                int v0 = AddVerticeOctTree(p2.X, y, p2.Y);
                int v1 = AddVerticeOctTree(p.X, y, p.Y);
                int v2 = AddVerticeOctTree(p1.X, y, p1.Y);
                int v3 = AddVerticeOctTree(p3.X, y, p3.Y);
                if (normal)
                {
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }
                else
                {
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);
                }
                theta += inswe;
            }
        }

        private void GenerateInsideTexture(double maxSweep, out double inswe)
        {
            inswe = 0;
            if (innerRadius >= 0.1)
            {
                if (textureResolution <= 0)
                {
                    textureResolution = 0.1;
                }
                vTextureResolution = textureResolution;
                hTextureResolution = textureResolution;

                if (textureManager.Mode == TextureManager.MapMode.FittedTile)
                {
                    // estimate vertical size in steps
                    double ySteps = tubeHeight / vTextureResolution;
                    double vRepeats = (ySteps / textureManager.PatternHeight);
                    vRepeats = Math.Ceiling(vRepeats);
                    vTextureResolution = tubeHeight / (vRepeats * textureManager.PatternHeight);

                    double circumference = innerRadius * Math.PI * 2.0;
                    circumference = (DegToRad(sweep) / (Math.PI * 2.0)) * circumference;
                    double xSteps = circumference / hTextureResolution;
                    double hRepeats = (xSteps / textureManager.PatternWidth);
                    hRepeats = Math.Ceiling(hRepeats);
                    hTextureResolution = circumference / (hRepeats * textureManager.PatternWidth);
                }
                if (textureManager.Mode == TextureManager.MapMode.FittedSingle)
                {
                    // estimate vertical size in steps
                    vTextureResolution = tubeHeight / textureManager.PatternHeight;

                    double circumference = radius * Math.PI * 2.0;
                    circumference = (DegToRad(sweep) / (Math.PI * 2.0)) * circumference;
                    hTextureResolution = circumference / (textureManager.PatternWidth + 2);
                }
                // Whats the inner sweep angle in degrees
                inswe = (hTextureResolution * 360.0) / (twoPI * radius);
                System.Diagnostics.Debug.WriteLine($"Texture res h {hTextureResolution} v {vTextureResolution}");
                // outer sweep is going to be close but just takes into account the depth
                double outswe = (hTextureResolution * 360.0) / (twoPI * (radius + textureDepth));
                inswe = DegToRad(inswe);
                outswe = DegToRad(outswe);

                // so a "FacePIXEL" corresponds to a sweep of inswe
                // In practice how many X-pixels does that mean we have

                int maxFacePixel = (int)((Math.PI * 2.0) / inswe);
                double facePixelHeight = tubeHeight / vTextureResolution;
                double x = 0;
                double y = 0;
                int tx = 0;
                int ty = 0;
                TextureCell cell;

                double deltaY = vTextureResolution;

                double theta = 0;

                while (y < tubeHeight)
                {
                    if (tubeHeight - y < deltaY)
                    {
                        deltaY = tubeHeight - y;
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
                                Point p = CalcPoint(theta, innerRadius);
                                Point p2 = CalcPoint(theta + inswe, innerRadius);
                                MakeRevVSquareFace(p.X, y, p.Y, p2.X, y + deltaY, p2.Y);
                            }
                            else
                            {
                                double zoff = ((double)cell.Width * textureDepth) / 255.0;
                                Point p = CalcPoint(theta, innerRadius - zoff);
                                Point p2 = CalcPoint(theta + inswe, innerRadius - zoff);
                                MakeRevVSquareFace(p.X, y, p.Y, p2.X, y + deltaY, p2.Y);

                                if (cell.WestWall > 0)
                                {
                                    double westSideDepth = ((double)(cell.Width - cell.WestWall) * textureDepth) / 255.0;
                                    Point p3 = CalcPoint(theta, innerRadius - westSideDepth);
                                    MakeRevVSquareFace(p3.X, y, p3.Y, p.X, y + deltaY, p.Y);
                                }

                                // must always close left edge of pattern
                                if (tx == 0)
                                {
                                    Point p3 = CalcPoint(theta, innerRadius);
                                    MakeRevVSquareFace(p3.X, y, p3.Y, p.X, y + deltaY, p.Y);
                                }

                                if (cell.EastWall > 0)
                                {
                                    double sideDepth = ((double)(cell.Width - cell.EastWall) * textureDepth) / 255.0;
                                    Point p3 = CalcPoint(theta + inswe, innerRadius - sideDepth);
                                    MakeRevVSquareFace(p2.X, y, p2.Y, p3.X, y + deltaY, p3.Y);
                                }

                                if (theta + inswe >= maxSweep)
                                {
                                    Point p3 = CalcPoint(theta + inswe, innerRadius);
                                    MakeRevVSquareFace(p2.X, y, p2.Y, p3.X, y + deltaY, p3.Y);
                                }
                                if (cell.NorthWall > 0)
                                {
                                    double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                    Point p4 = CalcPoint(theta, innerRadius - zoff);
                                    Point p5 = CalcPoint(theta, innerRadius - sideDepth);
                                    Point p6 = CalcPoint(theta + inswe, innerRadius - sideDepth);
                                    Point p7 = CalcPoint(theta + inswe, innerRadius - zoff);

                                    int v0 = AddVerticeOctTree(p4.X, y, p4.Y);
                                    int v1 = AddVerticeOctTree(p5.X, y, p5.Y);
                                    int v2 = AddVerticeOctTree(p6.X, y, p6.Y);
                                    int v3 = AddVerticeOctTree(p7.X, y, p7.Y);

                                    AddFace(v0, v1, v2);
                                    AddFace(v0, v2, v3);
                                }
                                else
                                {
                                    if (y + deltaY >= tubeHeight)
                                    {
                                        double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                        Point p4 = CalcPoint(theta, innerRadius - zoff);
                                        Point p5 = CalcPoint(theta, innerRadius);
                                        Point p6 = CalcPoint(theta + inswe, innerRadius);
                                        Point p7 = CalcPoint(theta + inswe, innerRadius - zoff);

                                        int v0 = AddVerticeOctTree(p4.X, y, p4.Y);
                                        int v1 = AddVerticeOctTree(p5.X, y, p5.Y);
                                        int v2 = AddVerticeOctTree(p6.X, y, p6.Y);
                                        int v3 = AddVerticeOctTree(p7.X, y, p7.Y);

                                        AddFace(v0, v1, v2);
                                        AddFace(v0, v2, v3);
                                    }
                                }

                                if (cell.SouthWall > 0)
                                {
                                    double sideDepth = ((double)(cell.Width - cell.SouthWall) * textureDepth) / 255.0;

                                    Point p4 = CalcPoint(theta, innerRadius - zoff);
                                    Point p5 = CalcPoint(theta, innerRadius - sideDepth);
                                    Point p6 = CalcPoint(theta + inswe, innerRadius - sideDepth);
                                    Point p7 = CalcPoint(theta + inswe, innerRadius - zoff);

                                    int v0 = AddVerticeOctTree(p4.X, y + deltaY, p4.Y);
                                    int v1 = AddVerticeOctTree(p5.X, y + deltaY, p5.Y);
                                    int v2 = AddVerticeOctTree(p6.X, y + deltaY, p6.Y);
                                    int v3 = AddVerticeOctTree(p7.X, y + deltaY, p7.Y);

                                    AddFace(v0, v1, v2);
                                    AddFace(v0, v3, v2);
                                }
                                else
                                {
                                    if (y + deltaY >= tubeHeight)
                                    {
                                        double sideDepth = ((double)(cell.Width - cell.NorthWall) * textureDepth) / 255.0;

                                        Point p4 = CalcPoint(theta, innerRadius - zoff);
                                        Point p5 = CalcPoint(theta, innerRadius);
                                        Point p6 = CalcPoint(theta + inswe, innerRadius);
                                        Point p7 = CalcPoint(theta + inswe, innerRadius - zoff);

                                        int v0 = AddVerticeOctTree(p4.X, y + deltaY, p4.Y);
                                        int v1 = AddVerticeOctTree(p5.X, y + deltaY, p5.Y);
                                        int v2 = AddVerticeOctTree(p6.X, y + deltaY, p6.Y);
                                        int v3 = AddVerticeOctTree(p7.X, y + deltaY, p7.Y);

                                        AddFace(v0, v2, v1);
                                        AddFace(v0, v3, v2);
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
            }
        }

        private void GenerateOutsideTexture(double maxSweep, out double inswe)
        {
            inswe = 0;
            if (innerRadius >= 0.1)
            {
                if (textureResolution <= 0)
                {
                    textureResolution = 0.1;
                }
                vTextureResolution = textureResolution;
                hTextureResolution = textureResolution;

                if (textureManager.Mode == TextureManager.MapMode.FittedTile)
                {
                    // estimate vertical size in steps
                    double ySteps = tubeHeight / vTextureResolution;
                    double vRepeats = (ySteps / textureManager.PatternHeight);
                    vRepeats = Math.Ceiling(vRepeats);
                    vTextureResolution = tubeHeight / (vRepeats * textureManager.PatternHeight);

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

                    vTextureResolution = tubeHeight / textureManager.PatternHeight;

                    double circumference = radius * twoPI;
                    circumference = (DegToRad(sweep) / (Math.PI * 2.0)) * circumference;
                    hTextureResolution = circumference / (textureManager.PatternWidth + 2);

                    // should check if the original rsolution is smaller, if so add an offset to shift the pattern up or round
                }
                System.Diagnostics.Debug.WriteLine($"Texture res h {hTextureResolution} v {vTextureResolution}");
                // Whats the inner sweep angle in degrees
                inswe = (hTextureResolution * 360.0) / (twoPI * radius);

                // outer sweep is going to be close but just takes into account the depth
                double outswe = (hTextureResolution * 360.0) / (twoPI * (radius + textureDepth));
                inswe = DegToRad(inswe);
                outswe = DegToRad(outswe);

                // so a "FacePIXEL" corresponds to a sweep of inswe
                // In practice how many X-pixels does that mean we have

                int maxFacePixel = (int)((Math.PI * 2.0) / inswe);
                double facePixelHeight = tubeHeight / vTextureResolution;
                double x = 0;
                double y = 0;
                int tx = 0;
                int ty = 0;
                TextureCell cell;

                double deltaY = vTextureResolution;

                double theta = 0;

                while (y < tubeHeight)
                {
                    if (tubeHeight - y < deltaY)
                    {
                        deltaY = tubeHeight - y;
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
                                    if (y + deltaY >= tubeHeight)
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
                                    if (y + deltaY >= tubeHeight)
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
            }
        }
    }
}