using System;

namespace Make3D.Models
{
    internal class PolarCoordinate
    {
        private const double TwoPI = Math.PI * 2.0;
        private double phi;

        public double Phi

        {
            get { return phi; }
            set
            {
                phi = value;
                if (phi < 0.001)
                {
                    phi = phi + TwoPI;
                }
                if (phi > TwoPI)
                {
                    phi = phi - TwoPI;
                }
            }
        }

        private double theta;

        public double Theta
        {
            get { return theta; }
            set
            {
                theta = value;
                if (theta <= 0)
                {
                    theta = theta + TwoPI;
                }
                if (theta > TwoPI)
                {
                    theta = theta - TwoPI;
                }
            }
        }

        public double Rho { get; set; }

        public PolarCoordinate(double t, double p, double r)
        {
            Theta = t;
            Phi = p;
            Rho = r;
        }

        internal void Dump()
        {
            System.Diagnostics.Debug.WriteLine($"Phi {Phi:F3} Theta {Theta:F3} Rho {Rho:F3}");
        }
    }
}