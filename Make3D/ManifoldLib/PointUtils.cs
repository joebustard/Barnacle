using System;
using System.Windows.Media.Media3D;

namespace ManifoldLib
{
    internal class PointUtils
    {
        private static readonly double EqualityTolerance = 1e-8f;

        public static void MinMax(Point3DCollection pnts, ref Point3D min, ref Point3D max)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double minZ = double.MaxValue;

            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double maxZ = double.MinValue;
            foreach (Point3D pd in pnts)
            {
                if (pd.X < minX)
                {
                    minX = pd.X;
                }
                if (pd.Y < minY)
                {
                    minY = pd.Y;
                }
                if (pd.Z < minZ)
                {
                    minZ = pd.Z;
                }

                if (pd.X > maxX)
                {
                    maxX = pd.X;
                }

                if (pd.Y > maxY)
                {
                    maxY = pd.Y;
                }

                if (pd.Z > maxZ)
                {
                    maxZ = pd.Z;
                }
            }
            min = new Point3D(minX, minY, minZ);
            max = new Point3D(maxX, maxY, maxZ);
        }

        public static bool equals(Point3D p, double x, double y, double z)
        {
            return equals(p.X, x) && equals(p.Y, y) && equals(p.Z, z);
        }
        public static bool equals(Point3D p1, Point3D p2)
        {
            return equals(p1.X, p2.X) && equals(p1.Y, p2.Y) && equals(p1.Z, p2.Z);
        }
        public static bool equals(double v1, double v2)
        {
            if (Math.Abs(v1 - v2) < EqualityTolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}