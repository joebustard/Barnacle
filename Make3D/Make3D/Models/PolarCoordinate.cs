using System;

namespace Make3D.Models
{
    internal class PolarCoordinate
    {
        const double TwoPI = Math.PI * 2.0;
        double phi;
        public double Phi
            
        {
            get {return phi; }
            set
            {
                phi = value;
                if (phi < 0.001)
                {
                    phi =0.001 ;
                }
                if (phi > TwoPI)
                {
                    phi = TwoPI;
                }
            }
        }
        public double Theta { get; set; }
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