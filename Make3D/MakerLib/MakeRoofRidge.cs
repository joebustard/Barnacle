using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class RoofRidgeMaker : MakerBase
    {
        private double armLength;
        private double armAngle;
        private double armThickness;
        private double ridgeLength;
        private double crownRadius;
        private double flatCrestWidth;
        private int shape;

        public RoofRidgeMaker(double armLength, double armAngle, double armThickness, double ridgeLength, double crownRadius, double flatCrestWidth, int shape)
        {
            this.armLength = armLength;
            this.armAngle = armAngle;
            this.armThickness = armThickness;
            this.ridgeLength = ridgeLength;
            this.crownRadius = crownRadius;
            this.flatCrestWidth = flatCrestWidth;
            this.shape = shape;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;
            switch (shape)
            {
                case 0:
                    MakeCrownless(armLength, armAngle, armThickness, ridgeLength);
                    break;

                case 1:
                    MakeFlatRidge(armLength, armAngle, armThickness, ridgeLength, flatCrestWidth);
                    break;

                case 2:
                    MakeCrownRidge(armLength, armAngle, armThickness, ridgeLength, crownRadius);
                    break;
            }
        }

        private double DegRad(double x)
        {
            return (Math.PI / 180.0) * x;
        }

        private const double PiByTwo = Math.PI / 2.0;

        private void MakeCrownRidge(double armLength, double armAngle, double armThickness, double ridgeLength, double crownRadius)
        {
            double outterCircumference = 2 * Math.PI * (crownRadius + armLength);
            double outerArmSweep = (armThickness / outterCircumference) * Math.PI * 2.0;

            double innerCircumference = 2 * Math.PI * crownRadius;
            double innerArmSweep = (armThickness / innerCircumference) * Math.PI * 2.0;

            double arm1Angle = 90 - armAngle / 2;
            double arm2Angle = 90 + armAngle / 2;
           
            List<Point> pnts = new List<Point>();
            double arm1AngleRad = DegRad(arm1Angle);
            double arm2AngleRad = DegRad(arm2Angle);

            double dt = Math.PI / 180;
            double t = 0;
            while (t < arm1AngleRad)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }

            pnts.Add(CalcPoint(arm1AngleRad - innerArmSweep / 2, crownRadius));
            pnts.Add(CalcPoint(arm1AngleRad - outerArmSweep / 2, crownRadius + armLength));

            pnts.Add(CalcPoint(arm1AngleRad + outerArmSweep / 2, crownRadius + armLength));
            pnts.Add(CalcPoint(arm1AngleRad + innerArmSweep / 2, crownRadius));

            t = arm1AngleRad + .0001;

            while (t < arm2AngleRad)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }

            pnts.Add(CalcPoint(arm2AngleRad - innerArmSweep / 2, crownRadius));
            pnts.Add(CalcPoint(arm2AngleRad - outerArmSweep / 2, crownRadius + armLength));

            pnts.Add(CalcPoint(arm2AngleRad + outerArmSweep / 2, crownRadius + armLength));
            pnts.Add(CalcPoint(arm2AngleRad + innerArmSweep / 2, crownRadius));

            t = arm2AngleRad + 0.0001;

            while (t < Math.PI * 2)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }
            ExtrudepsideDown(ridgeLength, pnts);
        }

        private void ExtrudepsideDown(double ridgeLength, List<Point> pnts, bool invert = false)
        {
            List<Point> tmp = new List<Point>();
            foreach (Point p in pnts)
            {
                tmp.Add(new Point(p.X, -p.Y));
            }
            ExtrudeH(tmp, ridgeLength, invert);
        }

        private void MakeFlatRidge(double armLength, double armAngle, double armThickness, double ridgeLength, double flatCrestWidth)
        {
            List<Point> pnts = new List<Point>();
            
            double theta1 = 90-(armAngle / 2);
            double fcw = flatCrestWidth / 2.0;
            theta1 = DegRad(theta1);

            pnts.Add(new Point(fcw, 0));
            Point pt = CalcPoint(theta1, armLength);
            pnts.Add(new Point(pt.X + fcw, pt.Y));
            pnts.Add(new Point(pnts[1].X, pnts[1].Y + armThickness));
            pnts.Add(new Point(fcw, armThickness));

            pnts.Add(new Point(-pnts[3].X, pnts[3].Y));
            pnts.Add(new Point(-pnts[2].X, pnts[2].Y));
            pnts.Add(new Point(-pnts[1].X, pnts[1].Y));
            pnts.Add(new Point(-pnts[0].X, pnts[0].Y));
            ExtrudepsideDown(ridgeLength, pnts);
        }

        private void MakeCrownless(double armLength, double armAngle, double armThickness, double ridgeLength1)
        {
            List<Point> pnts = new List<Point>();
            armAngle = 180 - armAngle;
            double theta = (armAngle / 2) + 90;

            theta = (theta * Math.PI * 2.0) / 360.0;

            pnts.Add(new Point(0, 0));
            pnts.Add(CalcPoint(theta, armLength));
            pnts.Add(new Point(pnts[1].X, pnts[1].Y - armThickness));
            pnts.Add(new Point(0, -armThickness));
            pnts.Add(new Point(-pnts[1].X, pnts[1].Y - armThickness));
            pnts.Add(new Point(-pnts[1].X, pnts[1].Y));

            ExtrudepsideDown(ridgeLength, pnts,false);
        }
    }
}