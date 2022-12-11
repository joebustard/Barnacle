using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class BrickTowerMaker : MakerBase
    {
        private double brickHeight;
        private double brickLength;
        private double brickWidth;
        private double gapDepth;
        private double gapLength;
        private double towerHeight;
        private double towerRadius;

        public BrickTowerMaker(double brickLength, double brickHeight, double brickWidth, double gapLength, double gapDepth, double towerRadius, double towerHeight)
        {
            this.brickLength = brickLength;
            this.brickHeight = brickHeight;
            this.brickWidth = brickWidth;
            this.gapLength = gapLength;
            this.gapDepth = gapDepth;
            this.towerRadius = towerRadius;
            this.towerHeight = towerHeight;
        }

        private const int minBricknGap = 6;

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double halfBrickWidth = brickWidth / 2.0;
            double twoPi = 2.0 * Math.PI;
            double circumferance = (twoPi * (towerRadius + halfBrickWidth));
            double brickngapLength = brickLength + gapLength;
            int numberOfWholeBricknGaps = (int)(circumferance / brickngapLength);

            if (numberOfWholeBricknGaps < minBricknGap)
            {
                // Can't make this wall so have to increase radius
                circumferance = minBricknGap * brickngapLength;
                towerRadius = ((circumferance / twoPi) - halfBrickWidth);
            }

            // fudge the gap length value to avoid having a missing brick at the end.
            double leftOver = (circumferance - (numberOfWholeBricknGaps * brickngapLength));
            double gapAdjust = 0;
            if (leftOver > 0.0 && numberOfWholeBricknGaps > 0)
            {
                gapAdjust = leftOver / (double)numberOfWholeBricknGaps;
            }
            double brickSweep = (brickLength / circumferance) * twoPi;
            double gapSweep = ((gapLength + gapAdjust) / circumferance) * twoPi;

            double y = 0;
            double offsetSweep = 0;
            while (y < towerHeight)
            {
                MakeOuterBrickRing(numberOfWholeBricknGaps, halfBrickWidth, twoPi, brickSweep, gapSweep, y, offsetSweep);
                MakeInnerBrickRing(numberOfWholeBricknGaps, halfBrickWidth, twoPi, brickSweep, gapSweep, y, offsetSweep);
                y += brickHeight;

                y += gapLength;
                offsetSweep = (brickSweep / 2) - offsetSweep;
            }
            if ( y  > towerHeight)
            {
                towerHeight = y;
            }
            MakeInnerGapRing(numberOfWholeBricknGaps, brickSweep, gapSweep, 0, towerRadius - halfBrickWidth + gapDepth, towerHeight, offsetSweep);
            MakeOuterGapRing(numberOfWholeBricknGaps, brickSweep, gapSweep, 0, towerRadius + halfBrickWidth - gapDepth, towerHeight, offsetSweep);
            CloseEnd(towerHeight, towerRadius, halfBrickWidth, gapDepth, false);
            CloseEnd(0, towerRadius, halfBrickWidth, gapDepth, true);
        }

        private void CloseEnd(double y, double r, double halfBrickWidth, double gapDepth, bool bottom)
        {
            double innerR = r - halfBrickWidth + gapDepth;
            double outerR = r + halfBrickWidth - gapDepth;
            double dt = Math.PI / 72;
            double theta = 0;
            double theta2 = 0;
            while (theta2 < 2.0 * Math.PI)
            {
                theta2 = theta + dt;
                Point pt0 = CalcPoint(theta, innerR);
                Point pt1 = CalcPoint(theta, outerR);
                Point pt2 = CalcPoint(theta2, outerR);
                Point pt3 = CalcPoint(theta2, innerR);
                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt2.X, y, pt2.Y);
                Point3D p3 = new Point3D(pt3.X, y, pt3.Y);

                int v0 = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);

                if (bottom)
                {
                    AddFace(v0, v2, v1);
                    AddFace(v0, v3, v2);
                }
                else
                {
                    AddFace(v0, v1, v2);
                    AddFace(v0, v2, v3);
                }
                theta += dt;
            }
        }

        private void MakeOuterGapRing(int numberOfWholeBricknGaps, double brickSweep, double gapSweep, double y, double r, double h, double offsetSweep)
        {
            double dt = Math.PI / 72;
            double theta = 0;
            double theta2 = 0;
            while (theta2 < 2.0 * Math.PI)
            {
                theta2 = theta + dt;
                Point pt0 = CalcPoint(theta, r);
                Point pt1 = CalcPoint(theta2, r);
                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt1.X, y + h, pt1.Y);
                Point3D p3 = new Point3D(pt0.X, y + h, pt0.Y);

                int v0 = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);

                AddFace(v0, v1, v2);
                AddFace(v0, v2, v3);

                theta += dt;
            }
        }

        private void MakeInnerGapRing(int numberOfWholeBricknGaps, double brickSweep, double gapSweep, double y, double r, double h, double offsetSweep)
        {
            double dt = Math.PI / 72;
            double theta = 0;
            double theta2 = 0;
            while (theta2 < 2.0 * Math.PI)
            {
                theta2 = theta + dt;
                Point pt0 = CalcPoint(theta, r);
                Point pt1 = CalcPoint(theta2, r);
                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt1.X, y + h, pt1.Y);
                Point3D p3 = new Point3D(pt0.X, y + h, pt0.Y);

                int v0 = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);

                AddFace(v0, v2, v1);
                AddFace(v0, v3, v2);

                theta += dt;
            }
        }

        private void MakeOuterBrickRing(int numberOfWholeBricknGaps, double halfBrickWidth, double twoPi, double brickSweep, double gapSweep, double y, double offsetSweep)
        {
            double theta = offsetSweep;
            for (int i = 0; i < numberOfWholeBricknGaps; i++)

            {
                // start with brick outside
                double theta2 = theta + brickSweep;

                Point pt0 = CalcPoint(theta, towerRadius + halfBrickWidth);
                Point pt1 = CalcPoint(theta2, towerRadius + halfBrickWidth);
                //   Point pt8 = CalcPoint(theta, towerRadius + halfBrickWidth - gapDepth);
                Point pt8 = CalcPoint(theta, towerRadius);

                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt1.X, y + brickHeight, pt1.Y);
                Point3D p3 = new Point3D(pt0.X, y + brickHeight, pt0.Y);

                int v0 = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);

                AddFace(v0, v1, v2);
                AddFace(v0, v2, v3);

                // outter gap face
                theta += brickSweep;
                theta2 += gapSweep;

                pt0 = CalcPoint(theta, towerRadius);
                pt1 = CalcPoint(theta2, towerRadius);
                p0 = new Point3D(pt0.X, y, pt0.Y);
                p1 = new Point3D(pt1.X, y, pt1.Y);
                p2 = new Point3D(pt1.X, y + brickHeight, pt1.Y);
                p3 = new Point3D(pt0.X, y + brickHeight, pt0.Y);

                int v4 = AddVertice(p0);
                int v5 = AddVertice(p1);
                int v6 = AddVertice(p2);
                int v7 = AddVertice(p3);

                // link both at start
                Point3D p8 = new Point3D(pt8.X, y, pt8.Y);
                Point3D p9 = new Point3D(pt8.X, y + brickHeight, pt8.Y);
                int v8 = AddVertice(p8);
                int v9 = AddVertice(p9);

                AddFace(v0, v3, v9);
                AddFace(v0, v9, v8);

                // link both of them
                // at end
                AddFace(v1, v4, v7);
                AddFace(v1, v7, v2);

                // close top of brick
                AddFace(v3, v2, v7);
                AddFace(v3, v7, v9);

                // close bottom of brick
                AddFace(v0, v8, v4);
                AddFace(v0, v4, v1);
                theta += gapSweep;
            }
        }

        private void MakeInnerBrickRing(int numberOfWholeBricknGaps, double halfBrickWidth, double twoPi, double brickSweep, double gapSweep, double y, double offsetSweep)
        {
            double theta = offsetSweep;
            for (int i = 0; i < numberOfWholeBricknGaps; i++)
            //while (theta < twoPi)
            {
                double theta2 = theta + brickSweep;

                Point pt0 = CalcPoint(theta, towerRadius - halfBrickWidth);
                Point pt1 = CalcPoint(theta2, towerRadius - halfBrickWidth);
                // Point pt8 = CalcPoint(theta, towerRadius - halfBrickWidth + gapDepth);
                Point pt8 = CalcPoint(theta, towerRadius);
                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt1.X, y + brickHeight, pt1.Y);
                Point3D p3 = new Point3D(pt0.X, y + brickHeight, pt0.Y);

                int v0 = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);

                AddFace(v0, v2, v1);
                AddFace(v0, v3, v2);

                //gap face
                theta += brickSweep;
                theta2 += gapSweep;

                pt0 = CalcPoint(theta, towerRadius);
                pt1 = CalcPoint(theta2, towerRadius);
                p0 = new Point3D(pt0.X, y, pt0.Y);
                p1 = new Point3D(pt1.X, y, pt1.Y);
                p2 = new Point3D(pt1.X, y + brickHeight, pt1.Y);
                p3 = new Point3D(pt0.X, y + brickHeight, pt0.Y);

                int v4 = AddVertice(p0);
                int v5 = AddVertice(p1);
                int v6 = AddVertice(p2);
                int v7 = AddVertice(p3);

                // link both at start
                Point3D p8 = new Point3D(pt8.X, y, pt8.Y);
                Point3D p9 = new Point3D(pt8.X, y + brickHeight, pt8.Y);
                int v8 = AddVertice(p8);
                int v9 = AddVertice(p9);

                AddFace(v0, v9, v3);
                AddFace(v0, v8, v9);

                // link both of them
                // at end
                Faces.Add(v1);
                Faces.Add(v7);
                Faces.Add(v4);

                Faces.Add(v1);
                Faces.Add(v2);
                Faces.Add(v7);

                // close top of brick
                AddFace(v3, v7, v2);
                AddFace(v3, v9, v7);

                // close bottom of brick
                AddFace(v0, v4, v8);
                AddFace(v0, v1, v4);
                theta += gapSweep;
            }
        }

        private Point CalcPoint(double theta, double r)
        {
            Point p = new Point();
            p.X = r * Math.Sin(theta);
            p.Y = r * Math.Cos(theta);
            return p;
        }
    }
}