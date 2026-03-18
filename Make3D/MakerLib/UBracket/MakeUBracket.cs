using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class UBracketMaker : MakerBase
    {
        private double legGap;
        private double legHeight;
        private double legWidth;
        private int rotDivisions = 36;
        private double startoffset = 10;
        private double sweepAngle;

        public UBracketMaker()
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
            double halfgap = legGap / 2;
            double cx = startoffset;
            List<Point> outterArch = new List<Point>();
            List<Point> innerArch = new List<Point>();
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
            AddToPolarProfile(cx, 0, halfgap, polarProfile);
            AddToPolarProfile(cx, 0, halfgap + legWidth, polarProfile);
            AddToPolarProfile(cx + legHeight, 0, halfgap + legWidth, polarProfile);

            // tmp point
            double outerrad = halfgap + legWidth;
            double theta = -Math.PI / 2.0;
            double dtheta = (Math.PI / 2.0) / 10.0;
            while (theta < Math.PI / 2.0)
            {
                Point po = CalcPoint(theta, outerrad);
                outterArch.Add(new Point(cx + legHeight + po.X, -po.Y));
                AddToPolarProfile(cx + legHeight + po.X, 0, -po.Y, polarProfile);
                theta += dtheta;
            }

            Point po1 = CalcPoint(Math.PI / 2.0, outerrad);
            outterArch.Add(new Point(cx + legHeight + po1.X, -po1.Y));
            AddToPolarProfile(cx + legHeight + po1.X, 0, -po1.Y, polarProfile);

            AddToPolarProfile(cx + legHeight, 0, -halfgap - legWidth, polarProfile);
            AddToPolarProfile(cx, 0, -halfgap - legWidth, polarProfile);
            AddToPolarProfile(cx, 0, -halfgap, polarProfile);
            AddToPolarProfile(cx + legHeight, 0, -halfgap, polarProfile);

            double innerrad = halfgap;
            theta = Math.PI / 2.0;

            while (theta > -Math.PI / 2.0)
            {
                Point po = CalcPoint(theta, innerrad);
                innerArch.Insert(0, new Point(cx + legHeight + po.X, -po.Y));
                AddToPolarProfile(cx + legHeight + po.X, 0, -po.Y, polarProfile);
                theta -= dtheta;
            }
            po1 = CalcPoint(-Math.PI / 2.0, innerrad);
            innerArch.Add(new Point(cx + legHeight + po1.X, -po1.Y));
            AddToPolarProfile(cx + legHeight + po1.X, 0, -po1.Y, polarProfile);

            AddToPolarProfile(cx + legHeight, 0, halfgap, polarProfile);

            SweepPolarProfileTheta2(polarProfile, cx, 0, sweepAngle, rotDivisions);

            // Close front
            // leg 1
            Point3D p0 = new Point3D(cx, halfgap, 0);
            Point3D p1 = new Point3D(cx, halfgap + legWidth, 0);
            Point3D p2 = new Point3D(cx + legHeight, halfgap + legWidth, 0);
            Point3D p3 = new Point3D(cx + legHeight, halfgap, 0);
            AddFace(p0, p2, p1);
            AddFace(p0, p3, p2);

            //leg2
            Point3D p4 = new Point3D(cx, -halfgap, 0);
            Point3D p5 = new Point3D(cx, -halfgap - legWidth, 0);
            Point3D p6 = new Point3D(cx + legHeight, -halfgap - legWidth, 0);
            Point3D p7 = new Point3D(cx + legHeight, -halfgap, 0);

            AddFace(p4, p5, p6);
            AddFace(p4, p6, p7);
            int j;
            for (int i = 0; i < innerArch.Count; i++)
            {
                j = i + 1;
                if (j == innerArch.Count)
                {
                    j = 0;
                }
                Point3D p8 = new Point3D(innerArch[i].X, innerArch[i].Y, 0);
                Point3D p9 = new Point3D(outterArch[i].X, outterArch[i].Y, 0);
                Point3D p10 = new Point3D(outterArch[j].X, outterArch[j].Y, 0);
                Point3D p11 = new Point3D(innerArch[j].X, innerArch[j].Y, 0);

                AddFace(p8, p10, p9);
                AddFace(p8, p11, p10);
            }

            // Close back
            // leg 1
            p0 = RotateRoundY(p0, sweepAngle);
            p1 = RotateRoundY(p1, sweepAngle);
            p2 = RotateRoundY(p2, sweepAngle);
            p3 = RotateRoundY(p3, sweepAngle);
            AddFace(p0, p1, p2);
            AddFace(p0, p2, p3);

            p4 = RotateRoundY(p4, sweepAngle);
            p5 = RotateRoundY(p5, sweepAngle);
            p6 = RotateRoundY(p6, sweepAngle);
            p7 = RotateRoundY(p7, sweepAngle);
            AddFace(p4, p6, p5);
            AddFace(p4, p7, p6);

            for (int i = 0; i < innerArch.Count; i++)
            {
                j = i + 1;
                if (j == innerArch.Count)
                {
                    j = 0;
                }
                Point3D p8 = new Point3D(innerArch[i].X, innerArch[i].Y, 0);
                p8 = RotateRoundY(p8, sweepAngle);
                Point3D p9 = new Point3D(outterArch[i].X, outterArch[i].Y, 0);
                p9 = RotateRoundY(p9, sweepAngle);
                Point3D p10 = new Point3D(outterArch[j].X, outterArch[j].Y, 0);
                p10 = RotateRoundY(p10, sweepAngle);
                Point3D p11 = new Point3D(innerArch[j].X, innerArch[j].Y, 0);
                p11 = RotateRoundY(p11, sweepAngle);

                AddFace(p8, p9, p10);
                AddFace(p8, p10, p11);
            }
        }

        public void SetValues(double legHeight, double legWidth, double legGap, double sweepAngle)
        {
            this.legHeight = legHeight;
            this.legWidth = legWidth;
            this.legGap = legGap;
            this.sweepAngle = sweepAngle;
        }

        internal void SweepPolarProfileTheta2(List<PolarCoordinate> polarProfile, double cx, double cy, double sweepDegrees, int numSegs)
        {
            double sweepRadians = sweepDegrees * (Math.PI * 2.0) / 360.0;
            double da = sweepRadians / (numSegs - 1);
            for (int i = 0; i < numSegs; i++)
            {
                double a = da * i;
                int j = i + 1;
                if (j == numSegs)
                {
                    if (sweepDegrees == 360)
                    {
                        j = 0;
                    }
                    else
                    {
                        // dont connect end to start if the sweepRadians doesn't go all the way round
                        break;
                    }
                }
                double b = da * j;

                for (int index = 0; index < polarProfile.Count; index++)
                {
                    int index2 = index + 1;
                    if (index2 == polarProfile.Count)
                    {
                        index2 = 0;
                    }
                    PolarCoordinate pc1 = polarProfile[index].Clone();
                    PolarCoordinate pc2 = polarProfile[index2].Clone();
                    PolarCoordinate pc3 = polarProfile[index2].Clone();
                    PolarCoordinate pc4 = polarProfile[index].Clone();
                    pc1.Theta -= a;
                    pc2.Theta -= a;
                    pc3.Theta -= b;
                    pc4.Theta -= b;

                    Point3D p1 = pc1.GetPoint3D();
                    Point3D p2 = pc2.GetPoint3D();
                    Point3D p3 = pc3.GetPoint3D();
                    Point3D p4 = pc4.GetPoint3D();
                    int v1 = AddVertice(p1);
                    int v2 = AddVertice(p2);
                    int v3 = AddVertice(p3);
                    int v4 = AddVertice(p4);
                    Faces.Add(v1);
                    Faces.Add(v2);
                    Faces.Add(v3);

                    Faces.Add(v1);
                    Faces.Add(v3);
                    Faces.Add(v4);
                }
            }
        }

        private Point3D RotateRoundY(Point3D p, double v)
        {
            v = v * (Math.PI * 2.0) / 360;
            double d = Math.Sqrt((p.X * p.X) + (p.Z * p.Z));
            double theta = Math.Atan2(p.Z, p.X);
            theta -= v;
            double x = Math.Cos(theta) * d;
            double y = Math.Sin(theta) * d;

            return new Point3D(x, p.Y, y);
        }

        private void SetLimits()
        {
            paramLimits.AddLimit("LegHeight", 0.25, 200);
            paramLimits.AddLimit("LegWIdth", 0.25, 200);
            paramLimits.AddLimit("LegGap", 1, 200);
            paramLimits.AddLimit("SweepAngle", 1, 90);
        }
    }
}