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
        public virtual void DeletePoints(List<FlexiPoint> points)
        {
        }

        public virtual void DisplayPoints(List<Point> res, List<FlexiPoint> pnts)
        {
        }

        public virtual int End()
        {
            return -1;
        }

        public virtual void GetSegmentPoints(List<FlexiPoint> res, List<FlexiPoint> pnts)
        {
        }

        public virtual void PointInserted(int v2, int numInserted)
        {
        }

        public virtual int Start()
        {
            return -1;
        }

        public bool Selected { get; set; }
    }
}