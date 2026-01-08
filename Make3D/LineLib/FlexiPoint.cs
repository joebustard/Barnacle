using System;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiPoint
    {
        private const double tolerance = 0.0000001;

        public FlexiPoint(System.Windows.Point p)
        {
            X = p.X;
            Y = p.Y;
            Mode = PointMode.Data;
            Visible = false;
            Selected = false;
        }

        public FlexiPoint(double x, double y)
        {
            X = x;
            Y = y;
            Mode = PointMode.Data;
            Visible = false;
            Selected = false;
        }

        public FlexiPoint(System.Windows.Point p, int i)
        {
            X = p.X;
            Y = p.Y;
            Id = i;
            Mode = PointMode.Data;
            Visible = false;
            Selected = false;
        }

        public FlexiPoint(Point p, int i, PointMode m) : this(p, i)
        {
            Mode = m;
            Visible = false;
            Selected = false;
        }

        public enum PointMode
        {
            Data,
            Control1,
            Control2,
            ControlQ,
            ControlA
        }

        public int Id
        {
            get; set;
        }

        public PointMode Mode
        {
            get; set;
        }

        public bool Selected
        {
            get; set;
        }

        public bool Visible
        {
            get; set;
        }

        // public System.Windows.Point Point { get; set; }
        public double X
        {
            get; set;
        }

        public double Y
        {
            get; set;
        }

        public bool Compare(FlexiPoint b)
        {
            bool res = false;
            if (b != null)
            {
                if (Math.Abs(X - b.X) < tolerance &&
                 Math.Abs(Y - b.Y) < tolerance)
                {
                    res = true;
                }
            }
            return res;
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public override string ToString()
        {
            string s = "";
            s += Id.ToString() + ",";
            s += X.ToString() + ",";
            s += Y.ToString() + ",";
            s += Mode.ToString();
            return s;
        }

        internal void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }
    }
}