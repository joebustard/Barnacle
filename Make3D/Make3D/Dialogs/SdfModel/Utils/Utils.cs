using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs.SdfModel
{
    internal class Utils
    {
        internal static Vector3D Abs(Vector3D a)
        {
            return new Vector3D(Math.Abs(a.X), Math.Abs(a.Y), Math.Abs(a.Z));
        }

        internal static double Clamp(double v1, double v2, double v3)
        {
            if (v1 < v2) return v2;
            if (v1 > v3) return v3;
            return v1;
        }

        internal static Vector3D Max(Vector3D a, Vector3D b)
        {
            return new Vector3D(Math.Max(a.X, b.X),
                                 Math.Max(a.Y, b.Y),
                                 Math.Max(a.Z, b.Z));
        }

        internal static Vector3D Min(Vector3D a, Vector3D b)
        {
            return new Vector3D(Math.Min(a.X, b.X),
                                 Math.Min(a.Y, b.Y),
                                 Math.Min(a.Z, b.Z));
        }

        internal static double Mix(double a, double b, double t)
        {
            return (a * (1 - t) + (b * t));
        }

        // smooth max
        internal static double Smax(double a, double b, double k)
        {
            return -Smin(-a, -b, k);
        }

        // smooth min
        internal static double Smin(double a, double b, double k)
        {
            double h = Clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
            return Mix(b, a, h) - k * h * (1.0 - h);
        }

        internal static double Vmax(Vector3D a)
        {
            return Math.Max(Math.Max(a.X, a.Y), a.Z);
        }

        internal static double Vmin(Vector3D a)
        {
            return Math.Min(Math.Min(a.X, a.Y), a.Z);
        }
    }
}