using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiSegment
    {
        public bool Selected { get; set; }

        public virtual void DeletePoints(ObservableCollection<FlexiPoint> points)
        {
        }

        public virtual void Deselect(ObservableCollection<FlexiPoint> points)
        {
        }

        public virtual void DisplayPoints(List<Point> res, ObservableCollection<FlexiPoint> pnts)
        {
        }

        public virtual void DisplayPointsF(List<System.Drawing.PointF> res, ObservableCollection<FlexiPoint> pnts)
        {
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
    }
}