using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MakerLib
{
    public class TorusMaker : MakerBase
    {
        private int curve;
        private double height;
        private double knobFactor;
        private double mainRadius;
        private double ringRadius;
        private double verticalRadius;

        public TorusMaker(double mr, double hr, double vr, int cv, double knobs, double th)
        {
            mainRadius = mr;
            ringRadius = hr;
            verticalRadius = vr;
            curve = cv;
            knobFactor = knobs;

            height = th;
        }

        public void Generate(Point3DCollection pnts, Int32Collection faces)
        {
            pnts.Clear();
            faces.Clear();
            Vertices = pnts;
            Faces = faces;

            double dTheta = 0.1;
            double dPhi = 0.2;
            int mainSteps = (int)((Math.PI * 2.0) / dTheta);
            int subSteps = (int)((Math.PI * 2.0) / dPhi);
            double theta1;
            double theta2;
            for (int i = 0; i < mainSteps; i++)
            {
                int j = i + 1;
                if (j >= mainSteps)
                {
                    j = 0;
                }
                theta1 = (double)i * dTheta;
                theta2 = (double)j * dTheta;

                double phi1;
                double phi2;

                for (int l = 0; l < subSteps; l++)
                {
                    int k = l + 1;
                    if (k >= subSteps)
                    {
                        k = 0;
                    }
                    phi1 = l * dPhi;
                    phi2 = k * dPhi;

                    Point3D p1 = TorusPoint(theta1, phi1, curve, mainRadius, ringRadius, verticalRadius, knobFactor, height);
                    Point3D p2 = TorusPoint(theta1, phi2, curve, mainRadius, ringRadius, verticalRadius, knobFactor, height);
                    Point3D p3 = TorusPoint(theta2, phi2, curve, mainRadius, ringRadius, verticalRadius, knobFactor, height);
                    Point3D p4 = TorusPoint(theta2, phi1, curve, mainRadius, ringRadius, verticalRadius, knobFactor, height);

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

        private Point3D TorusPoint(double t, double p, int curveType, double mr, double hr, double vr, double knobFactor, double height)
        {
            Point3D res = new Point3D(0, 0, 0);
            if (curveType == 0)
            {
                res = TorusPoint0(t, p, mr, hr, height);
            }
            else
            if (curveType == 1)
            {
                res = TorusPoint2(t, p, mr, hr, vr, 3, height);
            }
            else
            if (curveType == 2)
            {
                res = TorusPoint2(t, p, mr, hr, vr, 5, height);
            }
            else
            {
                res = TorusPoint3(t, p, mr, hr, vr, knobFactor, height);
            }

            return res;
        }

        private Point3D TorusPoint0(double t, double p, double mr, double hr, double stretch)
        {
            double r = mr + hr;
            double x = (mr + hr * Math.Cos(p)) * Math.Cos(t);
            double z = (mr + hr * Math.Cos(p)) * Math.Sin(t);
            double y = hr * Math.Sin(p);
            Point3D res = new Point3D(x, y, z);
            return res;
        }

        private Point3D TorusPoint2(double t, double p, double mainRadius, double horizontalRadius, double verticalRadius, double power, double stretch)
        {
            double x = (mainRadius + (horizontalRadius * (Math.Pow(Math.Cos(p), power)))) * Math.Cos(t);
            double z = (mainRadius + (horizontalRadius * (Math.Pow(Math.Cos(p), power)))) * Math.Sin(t);
            double y = stretch * Math.Sin(p);
            Point3D res = new Point3D(x, y, z);
            return res;
        }

        private Point3D TorusPoint3(double t, double p, double mainRadius, double horizontalRadius, double verticalRadius, double knobFactor, double stretch)
        {
            double x = (mainRadius + horizontalRadius * (Math.Cos(p) * Math.Abs(Math.Sin(knobFactor * t)))) * Math.Cos(t);
            double z = (mainRadius + verticalRadius * (Math.Cos(p) * Math.Abs(Math.Sin(knobFactor * t)))) * Math.Sin(t);
            double y = stretch * Math.Sin(p);
            Point3D res = new Point3D(x, y, z);
            return res;
        }
    }
}