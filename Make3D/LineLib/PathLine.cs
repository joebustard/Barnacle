using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
{
    public class PathLine
    {
        private List<PathPoint> points;
        private List<PathSegment> segs;
        /*
                public PathLine()
                {
                    segs = new List<PathSegment>();
                }

                public void AddCurve(Point p0, Point p1, Point p2, Point p3)
                {
                    int i = AddPathPoint(p0);
                    AddPathPoint(p1);
                    AddPathPoint(p2);
                    AddPathPoint(p3);
                    BezierLine bl = new BezierLine(i,i+1,i+2,i+3);
                    segs.Add(bl);
                }
                private int AddPathPoint(Point p0)
                {
                    points.Add(new PathPoint(p0,points.Count));
                    return points.Count - 1;
                }

                public void AddLine(Point p0, Point p1)
                {
                    LineSegment ls = new LineSegment(p0, p1);
                    segs.Add(ls);
                }

                public void AppendCurve(Point p1, Point p2, Point p3)
                {
                    BezierLine bl;
                    if (segs.Count > 0)
                    {
                        bl = new BezierLine(segs[segs.Count - 1].End(), new PathPoint(p1), new PathPoint(p2), new PathPoint(p3));

                        segs.Add(bl);
                    }
                }

                public void AppendLine(Point p1)
                {
                    LineSegment ls;
                    if (segs.Count > 0)
                    {
                        ls = new LineSegment(segs[segs.Count - 1].End(), new PathPoint(p1);
                        segs.Add(ls);
                    }
                }

                public void InsertCurve(int index)
                {
                    BezierLine bl;
                    if (segs.Count > 1 && index >= 0 && index < segs.Count - 1)
                    {
                        bl = new BezierLine(segs[index].End(), segs[index].Start());
                        bl.SetControlPoints(List < PathPoint > points);

                        segs.Insert(index, bl);
                    }
                }

                public void InsertLine(int index)
                {
                    LineSegment bl;
                    if (segs.Count > 1 && index >= 0 && index < segs.Count - 1)
                    {
                        bl = new LineSegment(segs[index].End(), segs[index].Start());

                        segs.Insert(index, bl);
                    }
                }
                */
    }
}