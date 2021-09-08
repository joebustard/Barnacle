using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class FlexiSegment
    {
        public bool Selected { get; set; }

        public virtual void DeletePoints(List<FlexiPoint> points)
        {
        }

        public virtual void Deselect(List<FlexiPoint> points)
        {
        }

        public virtual void DisplayPoints(List<Point> res, List<FlexiPoint> pnts)
        {
        }

        public virtual double DistToPoint(Point position, List<FlexiPoint> res)
        {
            return double.MaxValue;
        }

        public virtual int End()
        {
            return -1;
        }

        public virtual void GetSegmentPoints(List<FlexiPoint> res, List<FlexiPoint> pnts)
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

        public virtual void Select(List<FlexiPoint> points)
        {
        }

        public virtual int Start()
        {
            return -1;
        }
    }
}