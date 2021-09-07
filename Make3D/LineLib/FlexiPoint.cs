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
            Point = p;
            Mode = PointMode.Data;
        }

        public FlexiPoint(System.Windows.Point p, int i)
        {
            Point = p;
            Id = i;
            Mode = PointMode.Data;
        }

        public FlexiPoint(Point p, int i, PointMode m) : this(p, i)
        {
            Mode = m;
        }

        public enum PointMode
        {
            Data,
            Control1,
            Control2
        }

        public int Id { get; set; }
        public PointMode Mode { get; set; }
        public System.Windows.Point Point { get; set; }

        public bool Selected { get; set; }

        public override string ToString()
        {
            string s = "";
            s += Id.ToString() + ",";
            s += Point.X.ToString() + ",";
            s += Point.Y.ToString() + ",";
            s += Mode.ToString();
            return s;
        }
    }
}