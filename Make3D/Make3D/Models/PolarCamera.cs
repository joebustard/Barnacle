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
            polarFrontHome = new PolarCoordinate(Math.PI / 2, Math.PI / 2, homeDistance);
            polarBackHome = new PolarCoordinate(Math.PI / 2, -Math.PI / 2, homeDistance);
            polarRightHome = new PolarCoordinate(0.0, Math.PI / 2, homeDistance);
            polarLeftHome = new PolarCoordinate(0.0, -Math.PI / 2, homeDistance);
            polarTopHome = new PolarCoordinate(1.571  , 0.01, homeDistance);
            polarBottomHome = new PolarCoordinate(Math.PI/2,-Math.PI /2, homeDistance);
            distance = homeDistance;
            polarPos = polarFrontHome;
            ConvertPolarTo3D();
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
            double dt = dx  * 0.01;
            double dp = dy * 0.01;

            if (Math.Abs(dt) >= 0.01 || Math.Abs(dp) >= 0.01)

            {
                polarPos.Theta += dt;
                polarPos.Phi += dp;
                polarPos.Dump();
                /*
                if (polarPos.Phi < -2.0 * Math.PI)
                {
                    polarPos.Phi += Math.PI * 2.0;
                }
                if (polarPos.Phi > -2.0 * Math.PI)
                {
                    polarPos.Phi -= Math.PI * 2.0;
                }
                */
                ConvertPolarTo3D();
            }
        }

        internal void HomeFront()
        {
            polarPos = polarFrontHome;
            ConvertPolarTo3D();
        }

        internal void HomeBack()
        {
            polarPos = polarBackHome;
            ConvertPolarTo3D();
        }
        internal void HomeRight()
        {
            polarPos = polarRightHome;
            ConvertPolarTo3D();
        }
        internal void HomeLeft()
        {
            polarPos = polarLeftHome;
            ConvertPolarTo3D();
        }
        internal void HomeTop()
        {
            polarPos = polarTopHome;
            ConvertPolarTo3D();
        }
        internal void HomeBottom()
        {
            polarPos = polarBottomHome;
            ConvertPolarTo3D();
        }
    }
}