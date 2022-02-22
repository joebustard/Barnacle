using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public class PolarCoordinate
    {
        private const double TwoPI = Math.PI * 2.0;
        private double phi;

        private double theta;

        public PolarCoordinate(double t, double p, double r)
        {
            Theta = t;
            Phi = p;
            Rho = r;
        }

        public double Phi

        {
            get
            {
                return phi;
            }
            set
            {
                phi = value;
                if (phi < 0)
                {
                    phi = phi + TwoPI;
                }
                if (phi > TwoPI)
                {
                    phi = phi - TwoPI;
                }
            }
        }

        public double Rho { get; set; }

        public double Theta
        {
            get
            {
                return theta;
            }
            set
            {
                theta = value;
                if (theta < 0)
                {
                    theta = theta + TwoPI;
                }
                if (theta > TwoPI)
                {
                    theta = theta - TwoPI;
                }
            }
        }

        public PolarCoordinate Clone()
        {
            PolarCoordinate res = new PolarCoordinate(Theta, Phi, Rho);
            return res;
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine($"Phi {Phi:F3} Theta {Theta:F3} Rho {Rho:F3}");
        }

        public Point3D GetPoint3D()
        {
            double x = Rho * Math.Sin(Phi) * Math.Cos(Theta);
            double z = Rho * Math.Sin(Phi) * Math.Sin(Theta);
            double y = Rho * Math.Cos(Phi);
            if (Double.IsNaN(x))
            {
                return (new Point3D(0, 0, 0));
            }
            else
            {
                return (new Point3D(x, y, z));
            }
        }

        public void SetPoint3D(Point3D p)
        {
            Rho = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
            Phi = Math.Acos(p.Z / Rho);
            Theta = Math.Acos(p.X / Math.Sqrt(p.X * p.X + p.Y * p.Y)) * (p.Y < 0 ? -1.0 : 1.0);
        }
    }
}