using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SimpleSmoothLib
{
    internal class VertexAverage
    {
        public double X;
        public double Y;
        public double Z;
        public int N;

        public VertexAverage()
        {
            X = 0;
            Y = 0;
            Z = 0;
            N = 0;
        }

        public void Add(Point3D pnt)
        {
            X += pnt.X;
            Y += pnt.Y;
            Z += pnt.Z;
            N++;
        }

        public Point3D GetAverage()
        {
            Point3D pnt = new Point3D(0, 0, 0);
            if (N > 0)
            {
                pnt.X = X / N;
                pnt.Y = Y / N;
                pnt.Z = Z / N;
            }
            return pnt;
        }
    }
}