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

            polarFrontHome = new PolarCoordinate(1.53, 1.243, homeDistance);
            polarBackHome = new PolarCoordinate(4.568, 1.243, homeDistance);
            polarRightHome = new PolarCoordinate(0.0, 1.483, homeDistance);

            polarLeftHome = new PolarCoordinate(3.130, 1.243, homeDistance);

            polarTopHome = new PolarCoordinate(1.571, 0.01, homeDistance);
            polarBottomHome = new PolarCoordinate(4.728, 3.067, homeDistance);

            distance = homeDistance;
            polarPos = new PolarCoordinate(0, 0, 0);
            Copy(polarFrontHome);
            ConvertPolarTo3D();
        }

        private void Copy(PolarCoordinate p)
        {
            polarPos.Phi = p.Phi;
            polarPos.Theta = p.Theta;
            polarPos.Rho = p.Rho;
            distance = polarPos.Rho;
        }

        private void ConvertPolarTo3D()
        {
            double x = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Cos(polarPos.Theta);
            double z = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Sin(polarPos.Theta);
            double y = polarPos.Rho * Math.Cos(polarPos.Phi);
            cameraPos = new Point3D(x, y, z);
            distance = polarPos.Rho;
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
            double dt = dx * 0.01;
            double dp = dy * 0.01;

            if (Math.Abs(dt) >= 0.01 || Math.Abs(dp) >= 0.01)

            {
                polarPos.Theta += dt;
                polarPos.Phi += dp;
                polarPos.Dump();
                ConvertPolarTo3D();
            }
        }

        internal void HomeFront()
        {
            Copy(polarFrontHome);
            ConvertPolarTo3D();
        }

        internal void HomeBack()
        {
            Copy(polarBackHome);
            ConvertPolarTo3D();
        }

        internal void HomeRight()
        {
            Copy(polarRightHome);
            ConvertPolarTo3D();
        }

        internal void HomeLeft()
        {
            polarPos = polarLeftHome;
            ConvertPolarTo3D();
        }

        internal void HomeTop()
        {
            Copy(polarTopHome);
            ConvertPolarTo3D();
        }

        internal void HomeBottom()
        {
            Copy(polarBottomHome);
            ConvertPolarTo3D();
        }
    }
}