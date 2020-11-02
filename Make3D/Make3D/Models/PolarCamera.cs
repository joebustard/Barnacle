using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Models
{
    public class PolarCamera
    {
        public enum Orientations
        {
            Front,
            Back,
            Left,
            Right,
            Top,
            Bottom
        }

        private Orientations orientation;

        public Orientations Orientation
        {
            get
            {
                return orientation;
            }
        }

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
            homeDistance = 300;

            Init();
        }

        public PolarCamera(double d)
        {
            homeDistance = d;

            Init();
        }

        private void Init()
        {
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
            orientation = Orientations.Front;
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
            SetOrientation();
        }

        // calculates the direction that move drags on the GUI should move objects
        // based roughly on where the camera is.
        private void SetOrientation()
        {
            if ((polarPos.Theta >= 0.76) && (polarPos.Theta < 2.35))
            {
                // Front

                orientation = Orientations.Front;
            }
            if ((polarPos.Theta >= 2.35) && (polarPos.Theta < 4))
            {
                // Left

                orientation = Orientations.Left;
            }

            if ((polarPos.Theta >= 4) && (polarPos.Theta < 5.54))
            {
                // Back

                orientation = Orientations.Back;
            }
            if ((polarPos.Theta >= 5.54) && (polarPos.Theta <= 6.2831856))
            {
                // Right
                orientation = Orientations.Right;
            }
            if ((polarPos.Theta >= 0) && (polarPos.Theta < 0.76))
            {
                // Right

                orientation = Orientations.Right;
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
            double dt = 0;
            double dp = 0;

            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = (int)dpiXProperty.GetValue(null, null);
            var dpiY = (int)dpiYProperty.GetValue(null, null);
            double mmx = (dx * 25.4) / dpiX;
            if (Math.Abs(mmx) > 0.00001)
            {
                double theta = mmx / distance;
                dt = Math.Atan(theta);
            }
            double mmy = (dy * 25.4) / dpiY;
            if (Math.Abs(mmy) > 0.00001)
            {
                double theta = mmy / distance;
                dp = Math.Atan(theta);
            }
            if (Math.Abs(dt) >= 0.001 || Math.Abs(dp) >= 0.001)
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