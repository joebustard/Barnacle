using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiSegment
    {
        public bool Selected
        {
            get; set;
        }

        public virtual void DeletePoints(ObservableCollection<FlexiPoint> points)
        {
        }

        public virtual void Deselect(ObservableCollection<FlexiPoint> points)
        {
        }

        public void DeselectHide(int v, ObservableCollection<FlexiPoint> points)
        {
            if (v >= 0 && v < points.Count)
            {
                points[v].Selected = false;
                // points[v].Visible = false;
            }
        }

        public virtual void DisplayPoints(List<Point> res, ObservableCollection<FlexiPoint> pnts)
        {
        }

        public virtual void DisplayPointsF(List<System.Drawing.PointF> res, ObservableCollection<FlexiPoint> pnts)
        {
        }

        public virtual int DisplayPointsInSegment()
        {
            return 0;
        }

        public virtual double DistToPoint(Point position, ObservableCollection<FlexiPoint> res)
        {
            return double.MaxValue;
        }

        public virtual int End()
        {
            return -1;
        }

        public virtual void GetSegmentPoints(List<FlexiPoint> res, ObservableCollection<FlexiPoint> pnts)
        {
        }

        public virtual int NumberOfPoints()
        {
            return 0;
        }

        public virtual void PointInserted(int v2, int numInserted)
        {
        }

        public virtual void PointsRemoved(int n)
        {
        }

        public virtual void Select(ObservableCollection<FlexiPoint> points)
        {
        }

        public virtual int Start()
        {
            return -1;
        }

        internal virtual string ToOutline(ObservableCollection<FlexiPoint> flexiPoints)
        {
            return " ";
        }

        internal virtual string ToPath(ObservableCollection<FlexiPoint> points, ref double ox, ref double oy)
        {
            return " ";
        }

        protected void AddDisplayPoint(List<Point> res, double x, double y)
        {
            if (res != null)
            {
                if (res.Count > 0)
                {
                    if (x != res[res.Count - 1].X || y != res[res.Count - 1].Y)
                    {
                        res.Add(new Point(x, y));
                    }
                }
                else
                {
                    res.Add(new Point(x, y));
                }
            }
        }
    }
}