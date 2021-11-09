using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.LineLib
{
    public class LineSegment : FlexiSegment
    {
        public LineSegment(int p0, int p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public int P0 { get; set; }
        public int P1 { get; set; }

        public override void DeletePoints(ObservableCollection<FlexiPoint> points)
        {
            points.RemoveAt(P1);
        }

        public override void Deselect(ObservableCollection<FlexiPoint> points)
        {
            Selected = false;
            points[P0].Selected = false;
            points[P1].Selected = false;
            points[P0].Visible = false;
            points[P1].Visible = false;
        }

        public override void DisplayPoints(List<Point> res, ObservableCollection<FlexiPoint> pnts)
        {
            if (P1 < pnts.Count())
            {
                res.Add(new Point(pnts[P1].X, pnts[P1].Y));
            }
        }

        public override void DisplayPointsF(List<System.Drawing.PointF> res, ObservableCollection<FlexiPoint> pnts)
        {
            if (P1 < pnts.Count())
            {
                res.Add(new System.Drawing.PointF((float)pnts[P1].X, (float)pnts[P1].Y));
            }
        }

        public override double DistToPoint(Point position, ObservableCollection<FlexiPoint> points)
        {
            double dist = DistToLine.FindDistanceToLine(position, points[P0].ToPoint(), points[P1].ToPoint());
            return dist;
        }

        public override int End()
        {
            return P1;
        }

        public Point GetCoord(double t, ObservableCollection<FlexiPoint> points)
        {
            double x = points[P0].X + t * (points[P1].X - points[P0].X);
            double y = points[P0].Y + t * (points[P1].Y - points[P0].Y);

            return new Point(x, y);
        }

        public override void GetSegmentPoints(List<FlexiPoint> res, ObservableCollection<FlexiPoint> pnts)
        {
            res.Add(pnts[P1]);
        }

        public override int NumberOfPoints()
        {
            return 2;
        }

        public override void PointInserted(int v2, int n)
        {
            if (P0 >= v2)
            {
                P0 += n;
            }

            if (P1 >= v2)
            {
                P1 += n;
            }
        }

        public override void PointsRemoved(int n)
        {
            P0 -= n;
            P1 -= n;
        }

        public override void Select(ObservableCollection<FlexiPoint> points)
        {
            Selected = true;
            points[P0].Selected = true;
            points[P0].Visible = true;
            points[P1].Selected = true;
            points[P1].Visible = true;
        }

        public override int Start()
        {
            return P0;
        }

        public override string ToString()
        {
            string s = "L," + P0.ToString() + "," + P1.ToString();

            return s;
        }

        internal override string ToPath(ObservableCollection<FlexiPoint> points, ref double ox, ref double oy)
        {
            string res = "";

            if (points[P1].X == ox)
            {
                // relative vertical
                res = $"RV {points[P1].Y - oy} ";
            }
            else
            {
                if (points[P1].Y == oy)
                {
                    // relative horizontal
                    res = $"RH {points[P1].X - ox} ";
                }
                else
                {
                    res = $"RL {points[P1].X - ox},{points[P1].Y - oy} ";
                }
            }
            ox = points[P1].X;
            oy = points[P1].Y;
            return res;
        }
    }
}