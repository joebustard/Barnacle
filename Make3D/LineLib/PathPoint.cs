using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class PathPoint
    {
        public PathPoint(Point p, int i)
        {
            X = p.X;
            Y = p.Y;
            Id = i;
        }

        public PathPoint(double x0, double y0, int i)
        {
            X = x0;
            Y = y0;
            Id = i;
        }

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }
}