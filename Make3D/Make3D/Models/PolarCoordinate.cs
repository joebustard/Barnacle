namespace Make3D.Models
{
    internal class PolarCoordinate
    {
        public double Phi { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }

        public PolarCoordinate(double t, double p, double r)
        {
            Theta = t;
            Phi = p;
            Rho = r;
        }
    }
}