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
                    break;

                case 3:
                    MakeCrownRidge(armLength, armAngle, armThickness, ridgeLength, crownRadius);
                    break;
            }
        }

        private double DegRad(double x)
        {
            return (x / 180) * Math.PI;
        }

        private const double PiByTwo = Math.PI / 2.0;

        private void MakeCrownRidge(double armLength, double armAngle, double armThickness, double ridgeLength, double crownRadius)
        {
            double outterCircumference = 2 * Math.PI * (crownRadius + armLength);
            double outerArmSweep = (armThickness / outterCircumference) * Math.PI * 2.0;

            double innerCircumference = 2 * Math.PI * crownRadius;
            double innerArmSweep = (armThickness / innerCircumference) * Math.PI * 2.0;
         //   innerArmSweep = 0.001;
            List<Point> pnts = new List<Point>();
            double armAngleRad = DegRad(armAngle);
            double aaByTwo = armAngleRad / 2;

            double arm1thetaE = PiByTwo - aaByTwo;

            double arm1thetaS = arm1thetaE - innerArmSweep;

            double arm2thetaE = PiByTwo + aaByTwo;

            double arm2thetaS = arm2thetaE + innerArmSweep;


            double outter1 = arm1thetaE - (innerArmSweep / 2);


            double outter1S = outter1 - (outerArmSweep / 2);
            double outter1E = outter1 + (outerArmSweep / 2);
            //arm1thetaS = DegRad(arm1thetaS);
            //   double arm1thetaE = arm1thetaS + innerArmSweep;

            //   double arm2thetaS = arm1thetaE + DegRad(armAngle);
            //    double arm2thetaE = arm2thetaS + innerArmSweep;

            // arm2thetaE += (2.0 * Math.PI);
            // arm2thetaS += (2.0 * Math.PI);

            double dt = Math.PI / 180;
            double t = 0;
            while (t < arm1thetaS)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }

            pnts.Add(CalcPoint(arm1thetaS, crownRadius));
            pnts.Add(CalcPoint(outter1S, crownRadius + armLength));

            pnts.Add(CalcPoint(outter1E , crownRadius + armLength));
            pnts.Add(CalcPoint(arm1thetaE, crownRadius));

            t = arm1thetaE;
           
            while (t < arm2thetaS)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }
            /*
            pnts.Add(CalcPoint(arm2thetaS, crownRadius));
            pnts.Add(CalcPoint(arm2thetaS, crownRadius + armLength));

            pnts.Add(CalcPoint(arm2thetaS + outerArmSweep, crownRadius + armLength));
            pnts.Add(CalcPoint(arm2thetaE, crownRadius));

            t = arm2thetaE;
            */
            while (t < Math.PI * 2)
            {
                //   pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }

            ExtrudeH(pnts, ridgeLength);
        }

        private void MakeFlatRidge(double armLength, double armAngle, double armThickness, double ridgeLength, double flatCrestWidth)
        {
            List<Point> pnts = new List<Point>();
            armAngle = 180 - armAngle;
            double theta = (armAngle / 2) + 90;
            double fcw = flatCrestWidth / 2.0;
            theta = (theta * Math.PI * 2.0) / 360.0;

            pnts.Add(new Point(fcw, 0));
            Point pt = CalcPoint(theta, armLength);
            pnts.Add(new Point(pt.X + fcw, pt.Y));
            pnts.Add(new Point(pnts[1].X, pnts[1].Y - armThickness));
            pnts.Add(new Point(fcw, -armThickness));

            pnts.Add(new Point(-pnts[3].X, pnts[3].Y));
            pnts.Add(new Point(-pnts[2].X, pnts[2].Y));
            pnts.Add(new Point(-pnts[1].X, pnts[1].Y));
            pnts.Add(new Point(-pnts[0].X, pnts[0].Y));
            ExtrudeH(pnts, ridgeLength);
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
            ExtrudeH(pnts, ridgeLength1);
        }
    }
}