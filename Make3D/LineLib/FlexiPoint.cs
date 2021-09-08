using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class FlexiPoint
    {
        public FlexiPoint(System.Windows.Point p)
        {
            X = p.X;
            Y = p.Y;
            Mode = PointMode.Data;
            Visible = false;
        }

        public FlexiPoint(System.Windows.Point p, int i)
        {
            X = p.X;
            Y = p.Y;
            Id = i;
            Mode = PointMode.Data;
            Visible = false;
        }

        public FlexiPoint(Point p, int i, PointMode m) : this(p, i)
        {
            Mode = m;
            Visible = false;
        }

        public enum PointMode
        {
            Data,
            Control1,
            Control2
        }

        public int Id { get; set; }
        public PointMode Mode { get; set; }
       // public System.Windows.Point Point { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool Selected { get; set; }
        public bool Visible { get; set; }

        public override string ToString()
        {
            string s = "";
            s += Id.ToString() + ",";
            s += X.ToString() + ",";
            s += Y.ToString() + ",";
            s += Mode.ToString();
            return s;
        }
        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }
}