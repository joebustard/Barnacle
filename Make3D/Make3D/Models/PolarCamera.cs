using Barnacle.Object3DLib;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Models
{
    public class PolarCamera
    {
        private const double piBy2 = Math.PI / 2.0;
        private Point3D cameraPos;

        private double distance;

        private double fieldOfView;

        private double homeDistance;

        private Orientations horizontalOrientation;
        private PolarCoordinate polarBackHome;
        private PolarCoordinate polarBottomHome;
        private PolarCoordinate polarFrontHome;
        private PolarCoordinate polarLeftHome;
        private Point3D polarOrigin;
        private PolarCoordinate polarPos;
        private PolarCoordinate polarRightHome;
        private PolarCoordinate polarTopHome;
        private Orientations verticalOrientation;

        public PolarCamera()
        {
            homeDistance = 400;
            fieldOfView = 45;
            polarOrigin = new Point3D(0, 0, 0);
            Init();
        }

        public PolarCamera(double d)
        {
            homeDistance = d;
            fieldOfView = 45;
            polarOrigin = new Point3D(0, 0, 0);
            Init();
        }

        public enum Orientations
        {
            Front,
            Back,
            Left,
            Right,
            Top,
            Bottom,
            Unknown
        }

        public Point3D CameraPos
        { get { return cameraPos; } }

        public double Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
                polarPos.Rho = distance;
                ConvertPolarTo3D();
            }
        }

        public double FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                if (fieldOfView != value)
                {
                    fieldOfView = value;
                }
            }
        }
        private Orientations oldHorizontalOrientation=Orientations.Unknown;
        public Orientations HorizontalOrientation
        {
            get
            {
                return horizontalOrientation;
            }
        }

        private Orientations oldVerticalOrientation = Orientations.Unknown;
        public Orientations VerticalOrientation
        {
            get
            {
                return verticalOrientation;
            }
        }

        public void DistanceToFit(double w, double h, double min = 0)
        {
            double l = w;
            if (h > l)
            {
                l = h;
            }
            double fov_radians = FieldOfView * Math.PI / 180.0;
            Distance = (l) / Math.Tan(fov_radians / 2);
            if (Distance < min)
            {
                Distance = min;
            }
            if (Distance < homeDistance)
            {
                Distance = homeDistance;
            }
        }

        internal void HomeBack()
        {
            Copy(polarBackHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void HomeBottom()
        {
            Copy(polarBottomHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void HomeFront()
        {
            Copy(polarFrontHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void HomeLeft()
        {
            Copy(polarLeftHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void HomeRight()
        {
            Copy(polarRightHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void HomeTop()
        {
            Copy(polarTopHome);
            polarOrigin = new Point3D(0, 0, 0);
            ConvertPolarTo3D();
        }

        internal void LookAt(Point3D pnt)
        {
            polarOrigin = pnt;
        }

        internal void LooktoCenter()
        {
            polarOrigin = new Point3D(0, 0, 0);
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

                ConvertPolarTo3D();
               
            }
        }

        internal void RotateDegrees(double dt,double dp)
        {
            polarPos.Theta += (dt * Math.PI) / 180.0;
            polarPos.Phi += (dp * Math.PI) / 180.0;

            ConvertPolarTo3D();
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
            NotificationManager.Notify("CameraMoved", null);
        }

        private void ConvertPolarTo3D()
        {
            double x = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Cos(polarPos.Theta);
            double z = polarPos.Rho * Math.Sin(polarPos.Phi) * Math.Sin(polarPos.Theta);
            double y = polarPos.Rho * Math.Cos(polarPos.Phi);
            cameraPos = new Point3D(x + polarOrigin.X, y + polarOrigin.Y, z + polarOrigin.Z);
            distance = polarPos.Rho; 
            NotificationManager.Notify("CameraMoved", null);
            SetOrientation();
        }

        private void Copy(PolarCoordinate p)
        {
            polarPos.Phi = p.Phi;
            polarPos.Theta = p.Theta;
            polarPos.Rho = p.Rho;
            distance = polarPos.Rho;
        }

        private void Init()
        {
            polarFrontHome = new PolarCoordinate(1.53, 1.243, homeDistance);
            polarBackHome = new PolarCoordinate(4.568, 1.243, homeDistance);
            polarRightHome = new PolarCoordinate(0.0, 1.243, homeDistance);
            polarLeftHome = new PolarCoordinate(3.130, 1.243, homeDistance);
            polarTopHome = new PolarCoordinate(1.571, 0.01, homeDistance);
            polarBottomHome = new PolarCoordinate(4.728, 3.067, homeDistance);

            distance = homeDistance;
            polarPos = new PolarCoordinate(0, 0, 0);
            Copy(polarFrontHome);
            ConvertPolarTo3D();
            horizontalOrientation = Orientations.Front;
            verticalOrientation = Orientations.Top;
        }

        // calculates the direction that move drags on the GUI should move objects
        // based roughly on where the camera is.
        private void SetOrientation()
        {
            horizontalOrientation = Orientations.Unknown;
            if ((polarPos.Theta >= 0.76) && (polarPos.Theta < 2.35))
            {
                // Front
                horizontalOrientation = Orientations.Front;
            }
            if ((polarPos.Theta >= 2.35) && (polarPos.Theta < 4))
            {
                // Left
                horizontalOrientation = Orientations.Left;
            }

            if ((polarPos.Theta >= 4) && (polarPos.Theta < 5.54))
            {
                // Back
                horizontalOrientation = Orientations.Back;
            }
            if ((polarPos.Theta >= 5.54) && (polarPos.Theta <= 6.2831856))
            {
                // Right
                horizontalOrientation = Orientations.Right;
            }
            if ((polarPos.Theta >= 0) && (polarPos.Theta < 0.76))
            {
                // Right
                horizontalOrientation = Orientations.Right;
            }

            verticalOrientation = Orientations.Unknown;
            if ((polarPos.Phi >= 0) && (polarPos.Phi <= piBy2))
            {
                verticalOrientation = Orientations.Top;
            }
            else
            {
                verticalOrientation = Orientations.Bottom;
            }
            if ( ( oldHorizontalOrientation != horizontalOrientation) || (oldVerticalOrientation != verticalOrientation))
            {
                NotificationManager.Notify("CameraOrientation", null);
            }
            oldHorizontalOrientation = horizontalOrientation;
            oldVerticalOrientation = verticalOrientation;
        }
    }
}