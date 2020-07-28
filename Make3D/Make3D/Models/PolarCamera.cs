using System;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    internal class PolarCamera
    {
        private PolarCoordinate polarFrontHome;
        private PolarCoordinate polarBackHome;
        private PolarCoordinate polarLeftHome;
        private PolarCoordinate polarRightHome;
        private PolarCoordinate polarTopHome;
        private PolarCoordinate polarBottomHome;

        private PolarCoordinate polarPos;
        private double homeDistance;
        private double distance;

        public double Distance
        {
            get { return distance; }
        }

        private Point3D cameraPos;
        public Point3D CameraPos { get { return cameraPos; } }

        public PolarCamera()
        {
            homeDistance = 80;
            polarFrontHome = new PolarCoordinate(0.4, 0.4, homeDistance);
            distance = homeDistance;
            polarPos = polarFrontHome;
            ConvertPolarTo3D();
        }

        private void ConvertPolarTo3D()
        {
            double x = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Cos(polarPos.Theta);
            double y = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Sin(polarPos.Theta);
            double z = polarPos.Rho * Math.Cos(polarPos.Phi);
            cameraPos = new Point3D(x, y, z);
        }

        internal void Zoom(double v)
        {
            double d = polarPos.Rho * v / 100;
            if (polarPos.Rho - d > 0)
            {
                polarPos.Rho -= d;
            }
            distance = polarPos.Rho;
            ConvertPolarTo3D();
        }

        internal void Move(double dx, double dy)
        {
            double dt = 0.0;
            if (dx < -5)
            {
                dt = -0.1;
            }
            if (dx > 5)
            {
                dt = 0.1;
            }

            polarPos.Phi += dt;
            System.Diagnostics.Debug.WriteLine("phi " + polarPos.Phi.ToString());
            if (polarPos.Phi < -Math.PI)
            {
                polarPos.Phi += Math.PI * 2.0;
            }
            ConvertPolarTo3D();
        }
    }
}