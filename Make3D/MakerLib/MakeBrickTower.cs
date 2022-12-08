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

            double brickSweep = (brickLength / circumferance) * twoPi;
            double gapSweep = (gapLength / circumferance) * twoPi;
            double y = 0;
            double theta = 0;
            while (theta < twoPi)
            {
                // start with brick outside
                double theta2 = theta + brickSweep;

                Point pt0 = CalcPoint(theta, towerRadius + halfBrickWidth);
                Point pt1 = CalcPoint(theta2, towerRadius + halfBrickWidth);
                Point3D p0 = new Point3D(pt0.X, y, pt0.Y);
                Point3D p1 = new Point3D(pt1.X, y, pt1.Y);
                Point3D p2 = new Point3D(pt1.X, y + brickHeight, pt1.Y);
                Point3D p3 = new Point3D(pt0.X, y + brickHeight, pt0.Y);

                int vo = AddVertice(p0);
                int v1 = AddVertice(p1);
                int v2 = AddVertice(p2);
                int v3 = AddVertice(p3);
                Faces.Add(vo);
                Faces.Add(v1);
                Faces.Add(v2);

                Faces.Add(vo);
                Faces.Add(v2);
                Faces.Add(v3);

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
                Faces.Add(v4);
                Faces.Add(v5);
                Faces.Add(v6);

                Faces.Add(v4);
                Faces.Add(v6);
                Faces.Add(v7);

                // link both of them
                Faces.Add(v1);
                Faces.Add(v4);
                Faces.Add(v7);

                Faces.Add(v1);
                Faces.Add(v7);
                Faces.Add(v2);

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