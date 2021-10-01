using System;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public class PointUtils
    {
        public static bool equals(Point3D p, double x, double y, double z)
        {
            return equals(p.X, x) && equals(p.Y, y) && equals(p.Z, z);
        }

        public static bool equals(double v1, double v2)
        {
            if (Math.Abs(v1 - v2) < 0.000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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
    }
}