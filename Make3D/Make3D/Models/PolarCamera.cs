using System;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class PolarCamera
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


        private Point3D dragDelta;
        public Point3D DragDelta {  get { return dragDelta; } }
        public PolarCamera()
        {
            homeDistance = 300;

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
            dragDelta = new Point3D(1, 1, 0);
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
            GenerateDragDelta();
        }

        // calculates the direction that move drags on the GUI should move objects
        // based roughly on where the camera is.
        private void GenerateDragDelta()
        {
            if ((polarPos.Theta >= 0.76) && (polarPos.Theta < 2.35))
            {
                // Front
                dragDelta = new Point3D(1, 1, 0);
            }
            if ((polarPos.Theta >= 2.35) && (polarPos.Theta < 4))
            {
                // Left
                dragDelta = new Point3D(0, 1, 1);
            }

            if ((polarPos.Theta >= 4) && (polarPos.Theta < 5.54))
            {
                // Back
                dragDelta = new Point3D(-1, 1, 0);
            }
            if ((polarPos.Theta >= 5.54) && (polarPos.Theta <=6.28))
            {
                // Right
            }
            if ((polarPos.Theta >= 0) && (polarPos.Theta < 0.76))
            {
                // Right
                dragDelta = new Point3D(0, 1, -1);
            }
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
            Copy(polarLeftHome);
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