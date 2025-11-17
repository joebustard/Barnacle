using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class GearMaker : MakerBase
    {
        private double baseRadius;
        private double baseThickness;
        private bool bevelled;
        private double boreHoleRadius;

        private bool fillBase;
        private double gearHeight;
        private int numberOfTeeth;

        // need to make the base a smidge bigger than the toot radius to we can create triangles

        private double toothLength;

        public GearMaker()
        {
            paramLimits = new ParamLimits();
            SetLimits();
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            double TwoPi = 2.0 * Math.PI;
            double baserad = baseRadius + toothLength;
            double toprad = (baseRadius - gearHeight) + toothLength;

            double basediameter = TwoPi * baserad;
            double topdiameter = TwoPi * toprad;
            // sweep of a single tooth
            double sweepangle = TwoPi / (2 * numberOfTeeth);
            double basesweeplen = basediameter / (2 * numberOfTeeth);
            double topsweeplen = topdiameter / (2 * numberOfTeeth);
            double radoff = 0.1 * toothLength;
            double topy = gearHeight;
            double by = -baseThickness;
            double dt = sweepangle / 10;
            double theta = 0;
            Point3D p0;
            Point3D p1;
            Point3D p2;
            Point3D p3;

            int v0;
            int v1;
            int v2;
            int v3;
            List<Point> borePoints = new List<Point>();
            List<Point> baseTeethPoints = new List<Point>();
            List<Point> topTeethPoints = new List<Point>();
            List<Point> basePoints = new List<Point>();
            while (theta < TwoPi)
            {
                double r1 = baseRadius;
                double r2 = r1 + radoff;
                double r3 = r1 + (toothLength - radoff);
                double r4 = r1 + toothLength;

                for (int i = 0; i < 11; i++)
                {
                    double t = theta + (i * dt);
                    Point borep = CalcPoint(t, boreHoleRadius);
                    borePoints.Add(borep);
                }
                Point t0 = CalcPoint(theta, r1);
                Point t1 = CalcPoint(theta + (1 * dt), r2);
                Point t2 = CalcPoint(theta + (2 * dt), r3);
                Point t3 = CalcPoint(theta + (3 * dt), r4);
                Point t4 = CalcPoint(theta + (4 * dt), r4);
                Point t5 = CalcPoint(theta + (5 * dt), r4);
                Point t6 = CalcPoint(theta + (6 * dt), r3);
                Point t7 = CalcPoint(theta + (7 * dt), r2);
                Point t8 = CalcPoint(theta + (8 * dt), r1);
                Point t9 = CalcPoint(theta + (9 * dt), r1);
                Point t10 = CalcPoint(theta + (10 * dt), r1);

                baseTeethPoints.Add(t0);
                baseTeethPoints.Add(t1);
                baseTeethPoints.Add(t2);
                baseTeethPoints.Add(t3);
                baseTeethPoints.Add(t4);
                baseTeethPoints.Add(t5);
                baseTeethPoints.Add(t6);
                baseTeethPoints.Add(t7);
                baseTeethPoints.Add(t8);
                baseTeethPoints.Add(t9);
                baseTeethPoints.Add(t10);

                // top
                if (bevelled)
                {
                    r1 = baseRadius - gearHeight;
                }
                else
                {
                    r1 = baseRadius;
                }

                r2 = r1 + radoff;
                r3 = r1 + (toothLength - radoff);
                r4 = r1 + toothLength;
                dt = sweepangle / 10;

                t0 = CalcPoint(theta, r1);
                t1 = CalcPoint(theta + (1 * dt), r2);
                t2 = CalcPoint(theta + (2 * dt), r3);
                t3 = CalcPoint(theta + (3 * dt), r4);
                t4 = CalcPoint(theta + (4 * dt), r4);
                t5 = CalcPoint(theta + (5 * dt), r4);
                t6 = CalcPoint(theta + (6 * dt), r3);
                t7 = CalcPoint(theta + (7 * dt), r2);
                t8 = CalcPoint(theta + (8 * dt), r1);
                t9 = CalcPoint(theta + (9 * dt), r1);
                t10 = CalcPoint(theta + (10 * dt), r1);

                topTeethPoints.Add(t0);
                topTeethPoints.Add(t1);
                topTeethPoints.Add(t2);
                topTeethPoints.Add(t3);
                topTeethPoints.Add(t4);
                topTeethPoints.Add(t5);
                topTeethPoints.Add(t6);
                topTeethPoints.Add(t7);
                topTeethPoints.Add(t8);
                topTeethPoints.Add(t9);
                topTeethPoints.Add(t10);
                theta += 11 * dt;
            }

            // triangulate top and bottom
            for (int index = 0; index < baseTeethPoints.Count - 1; index++)
            {
                int j = index + 1;
                if (j >= baseTeethPoints.Count)
                {
                    j = 0;
                }
                p0 = new Point3D(borePoints[index].X, 0, borePoints[index].Y);
                p1 = new Point3D(borePoints[j].X, 0, borePoints[j].Y);
                p2 = new Point3D(baseTeethPoints[index].X, 0, baseTeethPoints[index].Y);
                p3 = new Point3D(baseTeethPoints[j].X, 0, baseTeethPoints[j].Y);
                if (!fillBase)
                {
                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v2, v1);
                    AddFace(v1, v2, v3);
                }

                p0 = new Point3D(borePoints[index].X, topy, borePoints[index].Y);
                p1 = new Point3D(borePoints[j].X, topy, borePoints[j].Y);
                p2 = new Point3D(topTeethPoints[index].X, topy, topTeethPoints[index].Y);
                p3 = new Point3D(topTeethPoints[j].X, topy, topTeethPoints[j].Y);

                v0 = AddVertice(p0);
                v1 = AddVertice(p1);
                v2 = AddVertice(p2);
                v3 = AddVertice(p3);
                AddFace(v0, v1, v2);
                AddFace(v1, v3, v2);
            }

            // close outside
            for (int index = 0; index < baseTeethPoints.Count - 1; index++)
            {
                int j = index + 1;
                if (j >= baseTeethPoints.Count)
                {
                    j = 0;
                }
                p0 = new Point3D(baseTeethPoints[index].X, 0, baseTeethPoints[index].Y);
                p1 = new Point3D(baseTeethPoints[j].X, 0, baseTeethPoints[j].Y);
                p2 = new Point3D(topTeethPoints[index].X, topy, topTeethPoints[index].Y);
                p3 = new Point3D(topTeethPoints[j].X, topy, topTeethPoints[j].Y);

                v0 = AddVertice(p0);
                v1 = AddVertice(p1);
                v2 = AddVertice(p2);
                v3 = AddVertice(p3);
                AddFace(v0, v2, v1);
                AddFace(v1, v2, v3);
            }
            if (boreHoleRadius > 0)
            {
                // close bore
                for (int index = 0; index < borePoints.Count - 1; index++)
                {
                    int j = index + 1;
                    if (j >= borePoints.Count)
                    {
                        j = 0;
                    }
                    p0 = new Point3D(borePoints[index].X, 0, borePoints[index].Y);
                    p1 = new Point3D(borePoints[j].X, 0, borePoints[j].Y);
                    p2 = new Point3D(borePoints[index].X, topy, borePoints[index].Y);
                    p3 = new Point3D(borePoints[j].X, topy, borePoints[j].Y);

                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v1, v2);
                    AddFace(v1, v3, v2);
                }
            }

            if (fillBase)
            {
                double r = baserad + 0.05;
                theta = 0;
                while (theta < TwoPi)
                {
                    Point pb = CalcPoint(theta, r);
                    basePoints.Add(pb);
                    theta += dt;
                }

                for (int index = 0; index < basePoints.Count - 1; index++)
                {
                    int j = index + 1;
                    if (j >= basePoints.Count)
                    {
                        j = 0;
                    }

                    // bottom of base
                    p0 = new Point3D(borePoints[index].X, by, borePoints[index].Y);
                    p1 = new Point3D(borePoints[j].X, by, borePoints[j].Y);
                    p2 = new Point3D(basePoints[index].X, by, basePoints[index].Y);
                    p3 = new Point3D(basePoints[j].X, by, basePoints[j].Y);

                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v2, v1);
                    AddFace(v1, v2, v3);
                    // top of base
                    p0 = new Point3D(basePoints[index].X, 0, basePoints[index].Y);
                    p1 = new Point3D(basePoints[j].X, 0, basePoints[j].Y);
                    p2 = new Point3D(baseTeethPoints[index].X, 0, baseTeethPoints[index].Y);
                    p3 = new Point3D(baseTeethPoints[j].X, 0, baseTeethPoints[j].Y);

                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v2, v1);
                    AddFace(v1, v2, v3);

                    // out side of base
                    p0 = new Point3D(basePoints[index].X, by, basePoints[index].Y);
                    p1 = new Point3D(basePoints[j].X, by, basePoints[j].Y);
                    p2 = new Point3D(basePoints[index].X, 0, basePoints[index].Y);
                    p3 = new Point3D(basePoints[j].X, 0, basePoints[j].Y);

                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v2, v1);
                    AddFace(v1, v2, v3);

                    // fill the extra bit of the bore
                    p0 = new Point3D(borePoints[index].X, by, borePoints[index].Y);
                    p1 = new Point3D(borePoints[j].X, by, borePoints[j].Y);
                    p2 = new Point3D(borePoints[index].X, 0, borePoints[index].Y);
                    p3 = new Point3D(borePoints[j].X, 0, borePoints[j].Y);

                    v0 = AddVertice(p0);
                    v1 = AddVertice(p1);
                    v2 = AddVertice(p2);
                    v3 = AddVertice(p3);
                    AddFace(v0, v1, v2);
                    AddFace(v1, v3, v2);
                }
            }
        }

        public void SetValues(double baseRadius,
                              double gearHeight,
                              double toothLength,
                              int numberOfTeeth,
                              double boreHoleRadius,
                              double baseThickness,
                              bool fillBase,
                              bool bevel)
        {
            this.baseRadius = baseRadius;
            this.gearHeight = gearHeight;
            this.toothLength = toothLength;
            this.numberOfTeeth = numberOfTeeth;
            this.boreHoleRadius = boreHoleRadius;
            this.baseThickness = baseThickness;
            this.fillBase = fillBase;
            this.bevelled = bevel;
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("BaseRadius", 1, 200);
            paramLimits.AddLimit("GearHeight", 1, 200);
            paramLimits.AddLimit("ToothLength", 1, 200);
            paramLimits.AddLimit("NumberOfTeeth", 4, 100);
            paramLimits.AddLimit("BoreHoleRadius", 0, 199);
            paramLimits.AddLimit("BaseThickness", 0, 100);
        }
    }
}