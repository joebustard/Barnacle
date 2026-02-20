using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
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

        public int P0
        {
            get; set;
        }

        public int P1
        {
            get; set;
        }

        public override void DeletePoints(ObservableCollection<FlexiPoint> points)
        {
            points.RemoveAt(P1);
        }

        public override void Deselect(ObservableCollection<FlexiPoint> points)
        {
            Selected = false;
            DeselectHide(P0, points);
            DeselectHide(P1, points);
        }

        public override void DisplayPoints(List<System.Windows.Point> res, ObservableCollection<FlexiPoint> pnts)
        {
            if (P1 < pnts.Count())
            {
                AddDisplayPoint(res, pnts[P1].X, pnts[P1].Y);
            }
        }

        public override void DisplayPointsF(List<System.Drawing.PointF> res, ObservableCollection<FlexiPoint> pnts)
        {
            if (P1 < pnts.Count())
            {
                if (res.Count > 0)
                {
                    if (((float)pnts[P1].X != res[res.Count - 1].X) ||
                         ((float)pnts[P1].Y != res[res.Count - 1].Y))
                    {
                        res.Add(new System.Drawing.PointF((float)pnts[P1].X, (float)pnts[P1].Y));
                    }
                }
                else
                {
                    res.Add(new System.Drawing.PointF((float)pnts[P1].X, (float)pnts[P1].Y));
                }
            }
        }

        public override int DisplayPointsInSegment()
        {
            return 2;
        }

        public override double DistToPoint(System.Windows.Point position, ObservableCollection<FlexiPoint> points)
        {
            double dist = DistToLine.FindDistanceToLine(position, points[P0].ToPoint(), points[P1].ToPoint());
            return dist;
        }

        public override int End()
        {
            return P1;
        }

        public System.Windows.Point GetCoord(double t, ObservableCollection<FlexiPoint> points)
        {
            double x = points[P0].X + t * (points[P1].X - points[P0].X);
            double y = points[P0].Y + t * (points[P1].Y - points[P0].Y);

            return new System.Windows.Point(x, y);
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
            // if this segment is pointing back to the start of the poly dont move its end
            if (P1 != 0)
            {
                P1 -= n;
            }
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

        internal override void Expand(List<FlexiSegment> segs, ObservableCollection<FlexiPoint> flexiPoints)
        {
            int np0 = P0 + 1;
            int np1 = np0 + 1;
            int np2 = np1 + 1;
            FlexiPoint fp0 = new FlexiPoint(flexiPoints[P0].X, flexiPoints[P0].Y);
            FlexiPoint fp1 = new FlexiPoint(flexiPoints[P1].X, flexiPoints[P1].Y);
            flexiPoints.Insert(P1, fp1);
            flexiPoints.Insert(P1, fp0);
            int index = segs.IndexOf(this);
            if (index >= 0 && index < segs.Count)
            {
                for (int i = index + 1; i < segs.Count; i++)
                {
                    segs[i].PointInserted(P1, 2);
                }
                LineSegment preSeg = new LineSegment(P0, np0);
                LineSegment postSeg = new LineSegment(np1, np2);

                segs.Insert(index + 1, postSeg);
                segs.Insert(index, preSeg);
                P0 = np0;
                P1 = np1;
            }
            // renumber the points
            // might as well just do all of them
            for (int ind = 0; ind < flexiPoints.Count; ind++)
            {
                flexiPoints[ind].Id = ind;
            }
        }

        internal override void MoveSegment(ObservableCollection<FlexiPoint> points, double dx, double dy)
        {
            points[P0].Offset(dx, dy);
            points[P1].Offset(dx, dy);
        }

        internal override string ToOutline(ObservableCollection<FlexiPoint> flexiPoints)
        {
            return $"L {flexiPoints[P1].X:F3},{flexiPoints[P1].Y:F3} ";
        }

        internal override string ToPath(ObservableCollection<FlexiPoint> points, ref double ox, ref double oy, bool absolute)
        {
            string res = "";

            if (absolute)
            {
                res = $"L {points[P1].X:F07},{points[P1].Y:F07} ";
            }
            else
            {
                if (points[P1].X == ox)
                {
                    // relative vertical
                    res = $"RV {points[P1].Y - oy:F07} ";
                }
                else
                {
                    if (points[P1].Y == oy)
                    {
                        // relative horizontal
                        res = $"RH {points[P1].X - ox:F07} ";
                    }
                    else
                    {
                        res = $"RL {points[P1].X - ox:F07},{points[P1].Y - oy:F07} ";
                    }
                }
            }
            ox = points[P1].X;
            oy = points[P1].Y;
            return res;
        }
    }
}