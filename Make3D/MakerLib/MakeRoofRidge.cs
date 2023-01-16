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

        public RoofRidgeMaker(double armLength, double armAngle,double armThickness, double ridgeLength, double crownRadius, double flatCrestWidth,  int shape)
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
            switch ( shape)
            {
                case 0:
                    MakeCrownless(armLength, armAngle,armThickness, ridgeLength);
                    break;
                case 1:
                    MakeFlatRidge(armLength, armAngle, armThickness, ridgeLength, flatCrestWidth);
                    break;
                case 2:
                    break;
                case 3:
                    MakeCrownRidge(armLength, armAngle, armThickness, ridgeLength,crownRadius);
                    break;
            }
        }

        private void MakeCrownRidge(double armLength, double armAngle, double armThickness, double ridgeLength, double crownRadius)
        {
            double outterCircumference = 2* Math.PI * (crownRadius+armLength);
            double outerArmSweep = (armThickness / outterCircumference) * (Math.PI * 2);

            double innerCircumference = 2 * Math.PI * crownRadius ;
            double innerArmSweep = (armThickness / innerCircumference) * (Math.PI * 2);

            List<Point> pnts = new List<Point>();
            armAngle = 180 - armAngle;
          //  double theta = (armAngle / 2) +180;
            double theta = (armAngle / 2) + 90;
            double theta2 = theta + 360 - armAngle;
            theta = (theta * Math.PI * 2.0) / 360.0;
            theta2 = (theta2 * Math.PI * 2.0) / 360.0;
            
            double dt =(theta2 - theta) / 50;
            double t = theta;
            pnts.Add(CalcPoint(t, crownRadius));
            pnts.Add(CalcPoint(t, crownRadius + armLength));
           
            pnts.Add(CalcPoint(t+outerArmSweep, crownRadius + armLength));
            pnts.Add(CalcPoint(t+ innerArmSweep, crownRadius));
            t += innerArmSweep;
            while (t < theta2)
            {
                pnts.Add(CalcPoint(t, crownRadius));
                t += dt;
            }
           
            pnts.Add(CalcPoint(t, crownRadius));
            pnts.Add(CalcPoint(t, crownRadius+armLength));
            
            pnts.Add(CalcPoint(t+outerArmSweep, crownRadius + armLength));
            pnts.Add(CalcPoint(t+innerArmSweep, crownRadius));
            

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
            pnts.Add(new Point( pt.X + fcw, pt.Y));
            pnts.Add(new Point(pnts[1].X, pnts[1].Y - armThickness));
            pnts.Add(new Point(fcw, -armThickness));

            pnts.Add(new Point(-pnts[3].X, pnts[3].Y ));
            pnts.Add(new Point(-pnts[2].X, pnts[2].Y));
            pnts.Add(new Point(-pnts[1].X, pnts[1].Y));
            pnts.Add(new Point(-pnts[0].X, pnts[0].Y));
            ExtrudeH(pnts, ridgeLength);
        }

        private void MakeCrownless(double armLength, double armAngle, double armThickness, double ridgeLength1)
        {
            List<Point> pnts = new List<Point>();
            armAngle = 180 - armAngle;
            double theta = (armAngle / 2)+90 ;
            
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
