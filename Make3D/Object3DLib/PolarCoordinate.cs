using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public class PolarCoordinate
    {
        private const double TwoPI = Math.PI * 2.0;
        private double elevation;
        private double azimuth;

        public PolarCoordinate(double t, double p, double r)
        {
            Azimuth = t;
            Elevation = p;
            Radius = r;
        }

        public double Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                if (elevation < 0)
                {
                    elevation = elevation + TwoPI;
                }
                if (elevation > TwoPI)
                {
                    elevation = elevation - TwoPI;
                }
            }
        }

        public double Radius
        {
            get; set;
        }

        public double Azimuth
        {
            get
            {
                return azimuth;
            }
            set
            {
                azimuth = value;
                if (azimuth < 0)
                {
                    azimuth = azimuth + TwoPI;
                }
                if (azimuth > TwoPI)
                {
                    azimuth = azimuth - TwoPI;
                }
            }
        }

        public PolarCoordinate Clone()
        {
            PolarCoordinate res = new PolarCoordinate(Azimuth, Elevation, Radius);
            return res;
        }

        public void Dump()
        {
            System.Diagnostics.Debug.WriteLine($"Elevation {Elevation:F7} Azimuth {Azimuth:F7} Radius {Radius:F7}");
        }

        public Point3D GetPoint3D()
        {
            double x = Radius * Math.Sin(Elevation) * Math.Cos(Azimuth);
            double y = Radius * Math.Sin(Elevation) * Math.Sin(Azimuth);
            double z = Radius * Math.Cos(Elevation);
            if (Double.IsNaN(x))
            {
                return (new Point3D(0, 0, 0));
            }
            else
            {
                return (new Point3D(x, y, z));
            }
        }

        public void SetPoint(double x, double y, double z)
        {
            Radius = Math.Sqrt(x * x + y * y + z * z);
            Elevation = Math.Acos(z / Radius);
            Azimuth = Math.Acos(x / Math.Sqrt(x * x + y * y)) * (y < 0 ? -1.0 : 1.0);
        }

        public void SetPoint3D(Point3D p)
        {
            Radius = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
            Elevation = Math.Acos(p.Z / Radius);
            Azimuth = Math.Acos(p.X / Math.Sqrt(p.X * p.X + p.Y * p.Y)) * (p.Y < 0 ? -1.0 : 1.0);
            // Azimuth = Math.Atan2(p.Z, p.X);
        }
    }
}