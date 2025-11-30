using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net.Configuration;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiPath
    {
        protected ObservableCollection<FlexiPoint> flexiPoints;
        protected List<FlexiSegment> segs;
        private const double selectionDistance = 2;
        private double brx;
        private double bry;
        private bool clipAgainstBounds;
        private bool closed;
        private bool openEndedPath;

        private double pathHeight;
        private double pathWidth;

        private double tlx;
        private double tly;

        public FlexiPath()
        {
            segs = new List<FlexiSegment>();
            flexiPoints = new ObservableCollection<FlexiPoint>();
            closed = true;
            openEndedPath = false;
            clipAgainstBounds = false;
        }

        public bool ClipAgainstBounds
        {
            get
            {
                return clipAgainstBounds;
            }

            set
            {
                clipAgainstBounds = value;
            }
        }

        public bool Closed
        {
            get
            {
                return closed;
            }
            set
            {
                closed = value;
            }
        }

        public ObservableCollection<FlexiPoint> FlexiPoints
        {
            get
            {
                return flexiPoints;
            }
            set
            {
                flexiPoints = value;
            }
        }

        public bool OpenEndedPath
        {
            get
            {
                return openEndedPath;
            }
            set
            {
                openEndedPath = value;
            }
        }

        public FlexiPoint Start
        {
            get
            {
                if (flexiPoints != null && flexiPoints.Count > 0 && flexiPoints[0] != null)
                {
                    return flexiPoints[0];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (flexiPoints.Count == 0)
                {
                    flexiPoints.Add(value);
                }
                else
                {
                    flexiPoints[0] = value;
                }
            }
        }

        public void AddClosingArcCurve(System.Windows.Point p1, bool clockwise, double radius)
        {
            int i = flexiPoints.Count - 1;
            flexiPoints.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.ControlA));

            FlexiArcSegment bl = new FlexiArcSegment(i, i + 1, 0);
            bl.Radius = radius;
            bl.Clockwise = clockwise;
            segs.Add(bl);
        }

        public void AddClosingQCurve(System.Windows.Point p1)
        {
            int i = flexiPoints.Count - 1;
            flexiPoints.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));

            FlexiQuadBezier bl = new FlexiQuadBezier(i, i + 1, 0);
            segs.Add(bl);
        }

        public void AddCurve(System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3)
        {
            int i = flexiPoints.Count - 1;
            flexiPoints.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));
            flexiPoints.Add(new FlexiPoint(p2, i + 2, FlexiPoint.PointMode.Control2));
            flexiPoints.Add(new FlexiPoint(p3, i + 3));
            FlexiCubicBezier bl = new FlexiCubicBezier(i, i + 1, i + 2, i + 3);
            segs.Add(bl);
        }

        public void AddLine(System.Windows.Point p1)
        {
            int i = flexiPoints.Count - 1;
            flexiPoints.Add(new FlexiPoint(p1, i + 1));
            LineSegment ls = new LineSegment(i, i + 1);
            segs.Add(ls);
        }

        public void AddOrthoLockedLine(System.Windows.Point position)
        {
            int i = flexiPoints.Count - 1;
            double oldX = flexiPoints[i].X;
            double oldY = flexiPoints[i].Y;
            double dx = Math.Abs(position.X - oldX);
            double dy = Math.Abs(position.Y - oldY);
            if (dx >= dy)
            {
                position.Y = oldY;
            }
            else
            {
                position.X = oldX;
            }
            flexiPoints.Add(new FlexiPoint(position, i + 1));
            LineSegment ls = new LineSegment(i, i + 1);
            segs.Add(ls);
        }

        public void AddQCurve(System.Windows.Point p1, System.Windows.Point p2)
        {
            int i = flexiPoints.Count - 1;
            flexiPoints.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));
            flexiPoints.Add(new FlexiPoint(p2, i + 2));

            FlexiQuadBezier bl = new FlexiQuadBezier(i, i + 1, i + 2);
            segs.Add(bl);
        }

        public void AppendClosingCurveSegment()
        {
            // get current last point
            FlexiPoint lp = flexiPoints[flexiPoints.Count - 1];
            FlexiPoint fp = flexiPoints[0];
            double cx1 = lp.X + 0.25 * (fp.X - lp.X);
            double cy1 = lp.Y + 0.25 * (fp.Y - lp.Y);
            double cx2 = lp.X + 0.75 * (fp.X - lp.X);
            double cy2 = lp.Y + 0.75 * (fp.Y - lp.Y);
            AddCurve(new System.Windows.Point(cx1, cy1), new System.Windows.Point(cx2, cy2), fp.ToPoint());
        }

        public void AppendClosingQuadCurveSegment()
        {
            // get current last point
            FlexiPoint lp = flexiPoints[flexiPoints.Count - 1];
            FlexiPoint fp = flexiPoints[0];
            double cx1 = lp.X + 0.5 * (fp.X - lp.X);
            double cy1 = lp.Y + 0.5 * (fp.Y - lp.Y);

            AddClosingQCurve(new System.Windows.Point(cx1, cy1));
        }

        public System.Windows.Point ArcSnap(int selectedPoint, System.Windows.Point position)
        {
            System.Windows.Point p = position;
            foreach (FlexiSegment fs in segs)
            {
                if (fs is FlexiArcSegment)
                {
                    FlexiArcSegment fa = fs as FlexiArcSegment;
                    if (fa.P1 == selectedPoint)
                    {
                        p = fa.Snap(p, flexiPoints);
                        break;
                    }
                }
            }
            return p;
        }

        public void ArcSwapDirection(int selectedPoint)
        {
            foreach (FlexiSegment fs in segs)
            {
                if (fs is FlexiArcSegment)
                {
                    FlexiArcSegment fa = fs as FlexiArcSegment;
                    if (fa.P1 == selectedPoint)
                    {
                        fa.Clockwise = !fa.Clockwise;
                        break;
                    }
                }
            }
        }

        public void CalculatePathBounds()
        {
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            var pnts = DisplayPointsF();
            double pathLength = 0;
            for (int i = 0; i < pnts.Count; i++)
            {
                if (i < pnts.Count - 1)
                {
                    pathLength += Distance(pnts[i], pnts[i + 1]);
                }
                if (pnts[i].X < tlx)
                {
                    tlx = pnts[i].X;
                }

                if (pnts[i].Y < tly)
                {
                    tly = pnts[i].Y;
                }

                if (pnts[i].X > brx)
                {
                    brx = pnts[i].X;
                }

                if (pnts[i].Y > bry)
                {
                    bry = pnts[i].Y;
                }
            }
            pathWidth = brx - tlx;
            pathHeight = bry - tly;
        }

        public void CalculatePathBounds(List<PointF> pnts)
        {
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;

            double pathLength = 0;
            for (int i = 0; i < pnts.Count; i++)
            {
                if (i < pnts.Count - 1)
                {
                    pathLength += Distance(pnts[i], pnts[i + 1]);
                }
                if (pnts[i].X < tlx)
                {
                    tlx = pnts[i].X;
                }

                if (pnts[i].Y < tly)
                {
                    tly = pnts[i].Y;
                }

                if (pnts[i].X > brx)
                {
                    brx = pnts[i].X;
                }

                if (pnts[i].Y > bry)
                {
                    bry = pnts[i].Y;
                }
            }
        }

        public System.Windows.Point Centroid()
        {
            double x = 0;
            double y = 0;
            int count = 0;

            foreach (FlexiPoint p in flexiPoints)
            {
                x += p.X;
                y += p.Y;
                count++;
            }
            if (count > 0)
            {
                return new System.Windows.Point(x / count, y / count);
            }
            else
            {
                return new System.Windows.Point(0, 0);
            }
        }

        public void ChangeSize(double sd)
        {
            double minx = double.MaxValue;
            double miny = double.MaxValue;

            if (flexiPoints.Count > 2)
            {
                FlexiPoint origin = flexiPoints[0];
                minx = Math.Min(minx, origin.X);
                miny = Math.Min(miny, origin.Y);
                for (int i = 1; i < flexiPoints.Count; i++)
                {
                    double dx = flexiPoints[i].X - origin.X;
                    double dy = flexiPoints[i].Y - origin.Y;
                    dx = dx + (sd * dx);
                    dy = dy + (sd * dy);
                    flexiPoints[i].X = dx + origin.X;
                    flexiPoints[i].Y = dy + origin.Y;

                    minx = Math.Min(minx, flexiPoints[i].X);
                    miny = Math.Min(miny, flexiPoints[i].Y);
                }

                if (minx < 0)
                {
                    minx = Math.Abs(minx);
                    for (int i = 0; i < flexiPoints.Count; i++)
                    {
                        flexiPoints[i].X += minx;
                    }
                }
                if (miny < 0)
                {
                    miny = Math.Abs(miny);
                    for (int i = 0; i < flexiPoints.Count; i++)
                    {
                        flexiPoints[i].Y += miny;
                    }
                }
            }
        }

        public virtual void Clear()
        {
            segs.Clear();
            flexiPoints.Clear();
        }

        public void ClosePath()
        {
            if (segs.Count >= 2 && openEndedPath == false)
            {
                // create a separate segment that goes back to point zero
                LineSegment sq = new LineSegment(flexiPoints.Count - 1, 0);
                segs.Add(sq);
            }
        }

        public void ConvertLineCurveSegment(int index, System.Windows.Point position)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int c1 = index;
                    int c2 = c1 + 1;
                    int c3 = c2 + 1;
                    int c4 = c3 + 1;

                    flexiPoints.Insert(index + 1, new FlexiPoint(position, c3, FlexiPoint.PointMode.Control2));
                    flexiPoints.Insert(index + 1, new FlexiPoint(position, c2, FlexiPoint.PointMode.Control1));

                    FlexiCubicBezier ls = new FlexiCubicBezier(c1, c2, c3, c4);
                    // starting at segment i +1, if the point id is c2 or more
                    PointInserted(segs, i + 1, c2, 2);
                    segs[i] = ls;
                    ls.ResetControlPoints(flexiPoints);
                    DeselectAll();
                    ls.Select(flexiPoints);
                    break;
                }
            }
        }

        public void ConvertLineQuadCurveSegment(int index, System.Windows.Point position, bool centreControl = true)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int c1 = index;
                    int c2 = c1 + 1;
                    int c3 = c2 + 1;

                    flexiPoints.Insert(index + 1, new FlexiPoint(position, c2, FlexiPoint.PointMode.ControlQ));

                    FlexiQuadBezier ls = new FlexiQuadBezier(c1, c2, c3);
                    // starting at segment i +1, if the point id is c2 or more
                    PointInserted(segs, i + 1, c2, 1);
                    segs[i] = ls;
                    if (centreControl)
                    {
                        ls.ResetControlPoints(flexiPoints);
                    }
                    DeselectAll();
                    ls.Select(flexiPoints);
                    break;
                }
            }
        }

        public bool ConvertToArc(System.Windows.Point position, bool clockwise)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is LineSegment)
                    {
                        int start = segs[i].Start();
                        int end = segs[i].End();
                        // closing segment which goes back to 0 is a special case
                        if (end != 0)
                        {
                            ConvertLineArcSegment(start, position, clockwise);
                        }
                        else
                        {
                            // Delete the last dummy segment
                            segs.RemoveAt(segs.Count - 1);
                            // Append a new curve
                            AppendClosingArcSegment(clockwise);

                            segs[segs.Count - 1].Select(flexiPoints);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public bool ConvertToCubic(System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is LineSegment)
                    {
                        int start = segs[i].Start();
                        int end = segs[i].End();
                        // closing segment which goes back to 0 is a special case
                        if (end != 0)
                        {
                            ConvertLineCurveSegment(start, position);
                        }
                        else
                        {
                            // Delete the last dummy segment
                            segs.RemoveAt(segs.Count - 1);
                            // Append a new curve
                            AppendClosingCurveSegment();

                            segs[segs.Count - 1].Select(flexiPoints);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public bool ConvertToQuadQuadAtSelected(System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is LineSegment)
                    {
                        int start = segs[i].Start();
                        int end = segs[i].End();
                        // closing segment which goes back to 0 is a special case
                        if (end != 0)
                        {
                            ConvertLineQuadCurveSegment(start, position);
                        }
                        else
                        {
                            // Delete the last dummy segment
                            segs.RemoveAt(segs.Count - 1);
                            // Append a new curve
                            AppendClosingQuadCurveSegment();

                            segs[segs.Count - 1].Select(flexiPoints);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public void ConvertTwoLineSegmentsToQuadraticBezier()
        {
            // have we really got two consecutive line segs selected?
            int firstSeg = TwoConsecutiveLineSegmentsSelected();
            if (firstSeg > -1)
            {
                int secondSeg = firstSeg + 1;
                // collect the details of the points in case we need em later
                int seg1P0 = segs[firstSeg].Start();
                System.Windows.Point P0 = flexiPoints[seg1P0].ToPoint();

                int seg2P0 = segs[secondSeg].Start();
                System.Windows.Point targetPoint = flexiPoints[seg2P0].ToPoint();

                int seg2P1 = segs[secondSeg].End();
                System.Windows.Point P2 = flexiPoints[seg2P1].ToPoint();

                double t = DistToLine.FindTOfClosestToLine(targetPoint, P0, P2);
                if (t != double.MinValue && t != 0)
                {
                    // Now it gets nasty delete the second segment
                    DeleteSegmentStartingAt(seg1P0);
                    // calculate where the control point should be so cuve goes through target point
                    System.Windows.Point P1 = new System.Windows.Point(0, 0);
                    P1.X = (targetPoint.X - (1.0 - t) * t * P0.X - ((t * t) * P2.X)) / (2.0 * (1 - t) * t);
                    P1.Y = (targetPoint.Y - (1.0 - t) * t * P0.Y - ((t * t) * P2.Y)) / (2.0 * (1 - t) * t);
                    // convert seg1 to a quadratic bezier
                    ConvertLineQuadCurveSegment(seg1P0, P1, false);
                }
            }
        }

        public void DeleteLastSegment()
        {
            if (segs.Count > 0)
            {
                segs[segs.Count - 1].DeletePoints(flexiPoints);
                segs.RemoveAt(segs.Count - 1);
            }
        }

        public void DeleteSegment(int index)
        {
            if (index >= 0 && index < segs.Count && segs.Count > 3)
            {
                int numPointsRemoved = segs[index].NumberOfPoints() - 1;
                segs[index].DeletePoints(flexiPoints);
                segs.RemoveAt(index);
                PointsRemoved(segs, index, numPointsRemoved);
            }
        }

        public void DeleteSegmentStartingAt(int index)
        {
            if (segs.Count >= 3)
            {
                for (int i = 0; i < segs.Count; i++)
                {
                    if (segs[i].Start() == index)
                    {
                        int numPointsRemoved = segs[i].NumberOfPoints() - 1;
                        segs[i].DeletePoints(flexiPoints);
                        segs.RemoveAt(i);

                        PointsRemoved(segs, i, numPointsRemoved);

                        DeselectAll();
                        if (i < segs.Count)
                        {
                            segs[i].Select(flexiPoints);
                        }
                        break;
                    }
                }
            }
        }

        public bool DeleteSelectedSegment()
        {
            bool result = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    // if its not the closing segment go ahead an do a simple delete
                    if (segs[i].End() != 0)
                    {
                        DeleteSegment(i);
                        result = true;
                    }

                    break;
                }
            }
            return result;
        }

        public void DeselectAll()
        {
            foreach (FlexiSegment sg in segs)
            {
                sg.Deselect(flexiPoints);
            }
        }

        public List<System.Windows.Point> DisplayPoints()
        {
            // display points are NOT the same as the raw FlexiPoints; Any curves etc may generate
            // more intermediate display points
            List<System.Windows.Point> res = new List<System.Windows.Point>();
            if (Start != null)
            {
                res.Add(Start.ToPoint());
                foreach (FlexiSegment sg in segs)
                {
                    sg.DisplayPoints(res, flexiPoints);
                }
            }
            bool anticlockwise = SignedPolygonArea(res) > 0;
            if (anticlockwise)
            {
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                for (int i = 0; i < res.Count; i++)
                {
                    tmp.Insert(0, res[i]);
                }
                res = tmp;
            }
            return res;
        }

        public List<System.Drawing.PointF> DisplayPointsF()
        {
            // display points are NOT the same as the raw FlexiPoints; Any curves etc may generate
            // more intermediate display points
            List<System.Drawing.PointF> res = new List<System.Drawing.PointF>();
            if (Start != null)
            {
                res.Add(new System.Drawing.PointF((float)Start.ToPoint().X, (float)Start.ToPoint().Y));
                foreach (FlexiSegment sg in segs)
                {
                    sg.DisplayPointsF(res, flexiPoints);
                }
            }
            // We want a consistent orientation of the final points, no matter how the user has drawn
            // his curve
            bool anticlockwise = SignedPolygonArea(res) > 0;
            if (anticlockwise)
            {
                List<System.Drawing.PointF> tmp = new List<System.Drawing.PointF>();
                for (int i = 0; i < res.Count; i++)
                {
                    tmp.Insert(0, res[i]);
                }
                res = tmp;
            }
            return res;
        }

        public FlexiSegment FirstSelectedSegment()
        {
            FlexiSegment res = null;
            foreach (FlexiSegment fl in segs)
            {
                if (fl.Selected)
                {
                    res = fl;
                    break;
                }
            }
            return res;
        }

        public void FlipHorizontal()
        {
            double low = double.MaxValue;
            double high = double.MinValue;
            for (int i = 0; i < flexiPoints.Count; i++)
            {
                low = Math.Min(low, flexiPoints[i].X);
                high = Math.Max(high, flexiPoints[i].X);
            }
            for (int i = 0; i < flexiPoints.Count; i++)
            {
                flexiPoints[i].X = low + (high - flexiPoints[i].X);
            }
        }

        public void FlipVertical()
        {
            double low = double.MaxValue;
            double high = double.MinValue;
            for (int i = 0; i < flexiPoints.Count; i++)
            {
                low = Math.Min(low, flexiPoints[i].Y);
                high = Math.Max(high, flexiPoints[i].Y);
            }
            for (int i = 0; i < flexiPoints.Count; i++)
            {
                flexiPoints[i].Y = low + (high - flexiPoints[i].Y);
            }
        }

        public void FromString(string s)
        {
            InterpretTextPath(s);
        }

        public List<FlexiPoint> GetSegmentPoints()
        {
            List<FlexiPoint> res = new List<FlexiPoint>();
            res.Add(Start);
            foreach (FlexiSegment sg in segs)
            {
                sg.GetSegmentPoints(res, flexiPoints);
            }
            return res;
        }

        public Dimension GetUpperAndLowerPoints(double x, bool autoclose = true)
        {
            Dimension res = new Dimension();
            res.Lower = double.MaxValue;
            res.Upper = double.MinValue;

            var pnts = DisplayPointsF();
            if (autoclose)
            {
                if ((pnts[0].X != pnts[pnts.Count - 1].X) || (pnts[0].Y != pnts[pnts.Count - 1].Y))
                {
                    pnts.Add(new PointF(pnts[0].X, pnts[0].Y));
                }
            }
            else
            {
                bool more;
                do
                {
                    more = false;
                    if (pnts.Count > 4)
                    {
                        double dx = Math.Abs(pnts[0].X - pnts[pnts.Count - 1].X);
                        double dy = Math.Abs(pnts[0].Y - pnts[pnts.Count - 1].Y);
                        if (dx < 0.00001 && dy < 0.00001)
                        {
                            pnts.RemoveAt(pnts.Count - 1);
                            more = true;
                        }
                    }
                } while (more);
            }
            CalculatePathBounds(pnts);
            double px = (x * pathWidth) + tlx;
            for (int i = 0; i < pnts.Count; i++)
            {
                int j = i + 1;
                if (j == pnts.Count)
                {
                    j = 0;
                }
                double y = 0;
                double sx, sy, ex, ey;
                if (pnts[i].X <= pnts[j].X)
                {
                    sx = pnts[i].X;
                    sy = pnts[i].Y;
                    ex = pnts[j].X;
                    ey = pnts[j].Y;
                }
                else
                {
                    ex = pnts[i].X;
                    ey = pnts[i].Y;
                    sx = pnts[j].X;
                    sy = pnts[j].Y;
                }
                if (px >= sx && px <= ex)
                {
                    if (Math.Abs(ex - sx) > 0.000001)
                    {
                        double t = (px - sx) / (ex - sx);
                        y = sy + t * (ey - sy);
                        res.X = px;
                        res.Lower = Math.Min(y, res.Lower);
                        res.Upper = Math.Max(y, res.Upper);
                    }
                    else
                    {
                        res.X = px;
                        res.Lower = Math.Min(sy, res.Lower);
                        res.Upper = Math.Max(sy, res.Upper);
                        res.Lower = Math.Min(ey, res.Lower);
                        res.Upper = Math.Max(ey, res.Upper);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Does the path have two consecutive line segments selected Used to enable related buttons
        /// </summary>
        /// <returns></returns>
        public bool HasTwoConsecutiveLineSegmentsSelected()
        {
            bool result = false;
            if (TwoConsecutiveLineSegmentsSelected() != -1)
            {
                result = true;
            }
            return result;
        }

        public void InsertCurveSegment(int index, System.Windows.Point position)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int end = segs[i].End();
                    flexiPoints.Insert(index + 1, new FlexiPoint(position, index + 3, FlexiPoint.PointMode.Control2));
                    flexiPoints.Insert(index + 1, new FlexiPoint(position, index + 2, FlexiPoint.PointMode.Control1));
                    flexiPoints.Insert(index + 1, new FlexiPoint(position, index + 1));

                    FlexiCubicBezier ls = new FlexiCubicBezier(index + 1, index + 2, index + 3, index + 4);
                    PointInserted(segs, i + 1, index + 1, 3);
                    segs.Insert(i + 1, ls);
                    ls.ResetControlPoints(flexiPoints);
                    break;
                }
            }
        }

        public void InsertLineSegment(int pointIndex, System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == pointIndex)
                {
                    //int end = segs[i].End();
                    flexiPoints.Insert(pointIndex + 1, new FlexiPoint(position, pointIndex + 1));

                    LineSegment ls = new LineSegment(pointIndex + 1, pointIndex + 2);
                    PointInserted(segs, i + 1, pointIndex + 1, 1);
                    segs.Insert(i + 1, ls);
                    DeselectAll();
                    segs[i + 1].Select(flexiPoints);
                    found = true;
                    break;
                }
            }
            if (!found && closed)
            {
                // special case, trying to split the imaginary line that connects the last point
                // back to the first
                if (segs[segs.Count - 1].End() == pointIndex)
                {
                    FlexiPoint np = new FlexiPoint(position, flexiPoints.Count);
                    flexiPoints.Add(np);
                    LineSegment ls = new LineSegment(segs[segs.Count - 1].End(), np.Id);
                    segs.Add(ls);
                    DeselectAll();
                    ls.Select(flexiPoints);
                }
            }
        }

        public bool InterpretTextPath(string s)
        {
            List<FlexiPoint> pnts = new List<FlexiPoint>();
            List<FlexiSegment> segments = new List<FlexiSegment>();
            bool valid = true;
            string tr = s.Trim().ToUpper();
            tr = tr.Replace("  ", " ");
            string[] blks = tr.Split(' ');
            double x = 0;
            double y = 0;
            if (blks.GetLength(0) > 1)
            {
                int i = 0;
                bool foundStart = false;
                while (i < blks.GetLength(0) && valid)
                {
                    switch (blks[i])
                    {
                        case "M":
                            {
                                if (!foundStart)
                                {
                                    foundStart = true;
                                    valid = GetCoord(blks[i + 1], ref x, ref y);
                                    FlexiPoint fp = new FlexiPoint(x, y);
                                    pnts.Add(fp);
                                    Start = fp;
                                    i++;
                                }
                                else
                                {
                                    valid = false;
                                }
                            }
                            break;

                        case "L":
                            {
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
                                i++;
                            }
                            break;

                        case "H":
                            {
                                valid = GetNumber(blks[i + 1], ref x);
                                y = pnts[pnts.Count - 1].Y;
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
                                i++;
                            }
                            break;

                        case "RH":
                            {
                                valid = GetNumber(blks[i + 1], ref x);
                                if (Math.Abs(x) >= 0.001)
                                {
                                    x = x + pnts[pnts.Count - 1].X;
                                    y = pnts[pnts.Count - 1].Y;
                                    FlexiPoint fp = new FlexiPoint(x, y);
                                    pnts.Add(fp);
                                    LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                    segments.Add(sq);
                                }
                                i++;
                            }
                            break;

                        case "V":
                            {
                                valid = GetNumber(blks[i + 1], ref y);
                                x = pnts[pnts.Count - 1].X;
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
                                i++;
                            }
                            break;

                        case "RV":
                            {
                                valid = GetNumber(blks[i + 1], ref y);
                                if (Math.Abs(y) >= 0.001)
                                {
                                    x = pnts[pnts.Count - 1].X;
                                    y = y + pnts[pnts.Count - 1].Y;
                                    FlexiPoint fp = new FlexiPoint(x, y);
                                    pnts.Add(fp);
                                    LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                    segments.Add(sq);
                                }
                                i++;
                            }
                            break;

                        case "RL":
                            {
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                if (Math.Abs(x) >= 0.001 && Math.Abs(y) >= 0.001)
                                {
                                    x += pnts[pnts.Count - 1].X;
                                    y += pnts[pnts.Count - 1].Y;
                                    FlexiPoint fp = new FlexiPoint(x, y);
                                    pnts.Add(fp);
                                    LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                    segments.Add(sq);
                                }
                                i++;
                            }
                            break;

                        case "Q":
                            {
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                FlexiPoint fp = new FlexiPoint(x, y);
                                fp.Mode = FlexiPoint.PointMode.ControlQ;
                                pnts.Add(fp);
                                if (valid)
                                {
                                    valid = GetCoord(blks[i + 2], ref x, ref y);
                                    FlexiPoint fp2 = new FlexiPoint(x, y);
                                    fp2.Mode = FlexiPoint.PointMode.Data;
                                    pnts.Add(fp2);

                                    FlexiQuadBezier sq = new FlexiQuadBezier(pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                                    segments.Add(sq);
                                }
                                i += 2;
                            }
                            break;

                        case "RQ":
                            {
                                double ox = pnts[pnts.Count - 1].X;
                                double oy = pnts[pnts.Count - 1].Y;
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                FlexiPoint fp = new FlexiPoint(x + ox, y + oy);
                                fp.Mode = FlexiPoint.PointMode.ControlQ;
                                pnts.Add(fp);
                                if (valid)
                                {
                                    valid = GetCoord(blks[i + 2], ref x, ref y);
                                    FlexiPoint fp2 = new FlexiPoint(x + ox, y + oy);
                                    fp2.Mode = FlexiPoint.PointMode.Data;
                                    pnts.Add(fp2);

                                    FlexiQuadBezier sq = new FlexiQuadBezier(pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                                    segments.Add(sq);
                                }
                                i += 2;
                            }
                            break;

                        case "C":
                            {
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                FlexiPoint fp = new FlexiPoint(x, y);
                                fp.Mode = FlexiPoint.PointMode.Control1;
                                pnts.Add(fp);
                                if (valid)
                                {
                                    valid = GetCoord(blks[i + 2], ref x, ref y);
                                    FlexiPoint fp2 = new FlexiPoint(x, y);
                                    fp2.Mode = FlexiPoint.PointMode.Control2;
                                    pnts.Add(fp2);
                                    if (valid)
                                    {
                                        valid = GetCoord(blks[i + 3], ref x, ref y);
                                        FlexiPoint fp3 = new FlexiPoint(x, y);
                                        fp3.Mode = FlexiPoint.PointMode.Control2;
                                        pnts.Add(fp3);
                                        FlexiCubicBezier sq = new FlexiCubicBezier(pnts.Count - 4, pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                                        segments.Add(sq);
                                    }
                                }

                                i += 3;
                            }
                            break;

                        case "RC":
                            {
                                double ox = pnts[pnts.Count - 1].X;
                                double oy = pnts[pnts.Count - 1].Y;
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                FlexiPoint fp = new FlexiPoint(x + ox, y + oy);
                                fp.Mode = FlexiPoint.PointMode.Control1;
                                pnts.Add(fp);
                                if (valid)
                                {
                                    valid = GetCoord(blks[i + 2], ref x, ref y);
                                    FlexiPoint fp2 = new FlexiPoint(x + ox, y + oy);
                                    fp2.Mode = FlexiPoint.PointMode.Control2;
                                    pnts.Add(fp2);
                                    if (valid)
                                    {
                                        valid = GetCoord(blks[i + 3], ref x, ref y);
                                        FlexiPoint fp3 = new FlexiPoint(x + ox, y + oy);
                                        fp3.Mode = FlexiPoint.PointMode.Control2;
                                        pnts.Add(fp3);
                                        FlexiCubicBezier sq = new FlexiCubicBezier(pnts.Count - 4, pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                                        segments.Add(sq);
                                    }
                                }

                                i += 3;
                            }
                            break;

                        case "A":
                            {
                                bool clockwise = false;
                                bool isLarge = false;
                                valid = LoadArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "AC":
                            {
                                bool clockwise = true;
                                bool isLarge = false;
                                valid = LoadArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "AL":
                            {
                                bool clockwise = false;
                                bool isLarge = true;
                                valid = LoadArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "ACL":
                            {
                                bool clockwise = true;
                                bool isLarge = true;
                                valid = LoadArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "RA":
                            {
                                bool clockwise = false;
                                bool isLarge = false;
                                valid = LoadRArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "RAC":
                            {
                                bool clockwise = true;
                                bool isLarge = false;
                                valid = LoadRArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "RAL":
                            {
                                bool clockwise = false;
                                bool isLarge = true;
                                valid = LoadRArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;

                        case "RACL":
                            {
                                bool clockwise = true;
                                bool isLarge = true;
                                valid = LoadRArc(pnts, segments, blks, ref x, ref y, i, clockwise, isLarge);
                                i += 2;
                            }
                            break;
                    }
                    i++;
                }
            }
            if (valid && pnts.Count >= 2)
            {
                EnsureInBounds(pnts);
                flexiPoints.Clear();
                int i = 0;
                foreach (FlexiPoint f in pnts)
                {
                    f.Id = i;
                    flexiPoints.Add(f);
                    i++;
                }

                // auto close
                if (flexiPoints[0].X != flexiPoints[i - 1].X || flexiPoints[0].Y != flexiPoints[i - 1].Y)
                {
                    if (openEndedPath == false)
                    {
                        // create a separate segment that goes back to point zero
                        LineSegment sq = new LineSegment(pnts.Count - 1, 0);
                        segments.Add(sq);
                    }
                }
                segs.Clear();
                segs.AddRange(segments);
            }
            return valid;
        }

        public void MoveByOffset(System.Windows.Point offset)
        {
            foreach (FlexiPoint p in flexiPoints)
            {
                p.X += offset.X;
                p.Y += offset.Y;
            }
        }

        public virtual System.Windows.Point MoveTo(System.Windows.Point position)
        {
            double cx = 0;
            double cy = 0;
            foreach (FlexiPoint p in flexiPoints)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= flexiPoints.Count;
            cy /= flexiPoints.Count;
            double dx = position.X - cx;
            double dy = position.Y - cy;
            foreach (FlexiPoint p in flexiPoints)
            {
                p.X += dx;
                p.Y += dy;
            }
            return new System.Windows.Point(dx, dy);
        }

        public System.Windows.Point OrthoLockPosition(System.Windows.Point position)
        {
            int i = flexiPoints.Count - 1;
            double oldX = flexiPoints[i].X;
            double oldY = flexiPoints[i].Y;
            double dx = Math.Abs(position.X - oldX);
            double dy = Math.Abs(position.Y - oldY);
            if (dx >= dy)
            {
                position.Y = oldY;
            }
            else
            {
                position.X = oldX;
            }

            return position;
        }

        public int PointsInFirstSegment()
        {
            int res = 0;
            if (segs != null)
            {
                res = segs[0].DisplayPointsInSegment();
            }
            return res;
        }

        public bool SelectAtPoint(System.Windows.Point position, bool clear = true)
        {
            bool found = false;
            if (clear)
            {
                DeselectAll();
            }
            if (segs != null && segs.Count > 0)
            {
                double minDistance = double.MaxValue;
                FlexiSegment closest = null;
                foreach (FlexiSegment sg in segs)
                {
                    double d = sg.DistToPoint(position, flexiPoints);
                    if (d < selectionDistance)
                    {
                        if (d < minDistance)
                        {
                            minDistance = d;
                            closest = sg;
                            found = true;
                        }
                    }
                }

                // should the imaginary segment fro the last point back to the first be included
                if (closed)
                {
                    LineSegment ls = new LineSegment(flexiPoints.Count - 1, 0);
                    double d = ls.DistToPoint(position, flexiPoints);
                    if (d < selectionDistance)
                    {
                        if (d < minDistance)
                        {
                            minDistance = d;
                            closest = ls;
                            found = true;
                        }
                    }
                }
                if (closest != null)
                {
                    closest.Select(flexiPoints);
                    closest.Selected = true;
                }
            }
            return found;
        }

        public void SetPointPos(int index, System.Windows.Point position)
        {
            // if the path is a normal closed polygon one then it may have seperate start and
            // ends points that are actually always meant to be coincident

            if (index >= 0 && index < flexiPoints.Count)
            {
                int partner = -1;
                if (closed && !openEndedPath && flexiPoints.Count > 2)
                {
                    if (flexiPoints[0].Compare(flexiPoints[flexiPoints.Count - 1]))
                    {
                        if (index == 0)
                        {
                            partner = flexiPoints.Count - 1;
                        }
                        else if (index == flexiPoints.Count - 1)
                        {
                            partner = 0;
                        }
                    }
                }

                flexiPoints[index].X = position.X;
                flexiPoints[index].Y = position.Y;
                if (partner != -1)
                {
                    flexiPoints[partner].X = position.X;
                    flexiPoints[partner].Y = position.Y;
                }
            }
        }

        /// <summary>
        /// SplitQuadCubic splits a quad bezier into to seperate ones
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool SplitQuadBezier(System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is FlexiQuadBezier)
                    {
                        FlexiQuadBezier quad = segs[i] as FlexiQuadBezier;

                        double splitT = quad.FindT(position, flexiPoints);
                        int startIndex = quad.Start();
                        int controlIndex = quad.P1;
                        int endIndex = quad.End();

                        System.Windows.Point p0 = flexiPoints[startIndex].ToPoint();
                        System.Windows.Point p1 = flexiPoints[controlIndex].ToPoint();
                        System.Windows.Point p2 = flexiPoints[endIndex].ToPoint();

                        // calculate points for lower curve
                        double x, y;
                        System.Windows.Point lnp0 = p0;
                        x = (1 - splitT) * p0.X + (splitT * p1.X);
                        y = (1 - splitT) * p0.Y + (splitT * p1.Y);
                        System.Windows.Point lnp1 = new System.Windows.Point(x, y);

                        x = Math.Pow((1 - splitT), 2) * p0.X + (2.0 * (1 - splitT) * splitT * p1.X) + (splitT * splitT * p1.X);
                        y = Math.Pow((1 - splitT), 2) * p0.Y + (2.0 * (1 - splitT) * splitT * p1.Y) + (splitT * splitT * p1.Y);
                        // System.Windows.Point lnp2 = new System.Windows.Point(x, y);
                        System.Windows.Point lnp2 = position;

                        // calculate points for higher curve
                        System.Windows.Point hnp0 = lnp2;
                        x = (1 - splitT) * p1.X + (splitT * p2.X);
                        y = (1 - splitT) * p1.Y + (splitT * p2.Y);
                        System.Windows.Point hnp1 = new System.Windows.Point(x, y);

                        System.Windows.Point hnp2 = p2;

                        // insert two new plexipoints
                        FlexiPoints.Insert(startIndex + 1, new FlexiPoint(lnp1.X, lnp1.Y));
                        FlexiPoints.Insert(startIndex + 2, new FlexiPoint(lnp2.X, lnp2.Y));

                        FlexiQuadBezier lowerQuad = new FlexiQuadBezier();
                        lowerQuad.P0 = startIndex;
                        lowerQuad.P1 = startIndex + 1;
                        lowerQuad.P2 = startIndex + 2;

                        FlexiQuadBezier upperQuad = new FlexiQuadBezier();
                        upperQuad.P0 = startIndex + 2;

                        upperQuad.P1 = startIndex + 3;
                        flexiPoints[startIndex + 3].X = hnp1.X;
                        flexiPoints[startIndex + 3].Y = hnp1.Y;

                        upperQuad.P2 = startIndex + 4;

                        for (int j = i + 1; j < segs.Count; j++)
                        {
                            segs[j].PointInserted(endIndex, 2);
                        }

                        segs[i] = lowerQuad;
                        segs.Insert(i + 1, upperQuad);
                        if (upperQuad.P2 == flexiPoints.Count)
                        {
                            upperQuad.P2 = 0;
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public virtual bool SplitSelectedLineSegment(System.Windows.Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Selected)
                {
                    if (segs[i] is LineSegment)
                    {
                        int start = segs[i].Start();
                        int end = segs[i].End();
                        // closing segment which goes back to 0 is a special case
                        if (end != 0)
                        {
                            InsertLineSegment(start, position);
                        }
                        else
                        {
                            // add a new flexipoint at the given position
                            FlexiPoint fx = new FlexiPoint(position.X, position.Y);
                            flexiPoints.Add(fx);
                            // Make the existing segment refr to this point
                            (segs[i] as LineSegment).P1 = flexiPoints.Count - 1;
                            segs[i].Deselect(flexiPoints);
                            // now reclose the path to linkback up to the first path
                            ClosePath();
                            segs[i + 1].Select(flexiPoints);
                        }
                        found = true;
                    }
                    else
                    {
                        if (segs[i] is FlexiQuadBezier)
                        {
                            // insert a line segment between the
                            // start point of the bezier and the clicked point
                            int pointIndex = segs[i].Start();
                            flexiPoints.Insert(pointIndex + 1, new FlexiPoint(position, pointIndex + 1));

                            LineSegment ls = new LineSegment(pointIndex, pointIndex + 1);
                            PointInserted(segs, i, pointIndex, 1);
                            segs.Insert(i, ls);
                            DeselectAll();

                            // move the start point of the bezier to clicked point
                        }
                    }
                    break;
                }
            }
            return found;
        }

        public void SwapArcSegmentSize(int selectedPoint)
        {
            foreach (FlexiSegment fs in segs)
            {
                if (fs is FlexiArcSegment)
                {
                    FlexiArcSegment fa = fs as FlexiArcSegment;
                    if (fa.P1 == selectedPoint)
                    {
                        fa.IsLargeArc = !fa.IsLargeArc;
                        break;
                    }
                }
            }
        }

        public string ToOutline()
        {
            string result = "";
            double ox;
            double oy;
            if (flexiPoints.Count > 1)
            {
                ox = flexiPoints[0].X;
                oy = flexiPoints[0].Y;

                result = $"M {ox:F3},{oy:F3} ";
                foreach (FlexiSegment sq in segs)
                {
                    result += sq.ToOutline(flexiPoints);
                }
            }
            return result;
        }

        public string ToPath(bool absolute = false)
        {
            string result = "";
            double ox;
            double oy;
            if (flexiPoints.Count > 1)
            {
                ox = flexiPoints[0].X;
                oy = flexiPoints[0].Y;

                if (absolute)
                {
                    result = $"M {ox:F3},{oy:F3} "; ;
                }
                else
                {
                    result = "M 0,0 ";
                }
                foreach (FlexiSegment sq in segs)
                {
                    result += sq.ToPath(flexiPoints, ref ox, ref oy, absolute);
                }
            }
            return result;
        }

        public override string ToString()
        {
            string s = "";
            foreach (FlexiPoint p in flexiPoints)
            {
                s += p.ToString() + "\n";
            }
            s += "|";
            foreach (FlexiSegment sgs in segs)
            {
                s += sgs.ToString() + "\n";
            }

            return s;
        }

        /// <summary>
        /// Look for the first selected items If its two consecutive linesegments return the index
        /// of the first one Else return -1 Used to decide if two sements can be converted to a
        /// quadratic bezier
        /// </summary>
        /// <returns></returns>
        public int TwoConsecutiveLineSegmentsSelected()
        {
            int res = -1;

            int segmentindex = 0;
            bool foundSelected = false;
            while (foundSelected == false && segmentindex < segs.Count - 1)
            {
                if (segs[segmentindex].Selected)
                {
                    foundSelected = true;
                    if (segs[segmentindex] is LineSegment)
                    {
                        if (segs[segmentindex + 1].Selected)
                        {
                            if (segs[segmentindex + 1] is LineSegment)
                            {
                                res = segmentindex;
                            }
                        }
                    }
                }
                segmentindex++;
            }
            return res;
        }

        private void AppendClosingArcSegment(bool clockwise)
        {
            // get current last point
            FlexiPoint lp = flexiPoints[flexiPoints.Count - 1];
            FlexiPoint fp = flexiPoints[0];
            double cx1 = lp.X + 0.5 * (fp.X - lp.X);
            double cy1 = lp.Y + 0.5 * (fp.Y - lp.Y);
            double radius = Distance(lp, fp) / 2.0;
            AddClosingArcCurve(new System.Windows.Point(cx1, cy1), clockwise, radius);
        }

        private void CheckClip(FlexiPoint f)
        {
            if (clipAgainstBounds)
            {
                if (f.X < 0)
                {
                    f.X = 0;
                }
                if (f.Y < 0)
                {
                    f.Y = 0;
                }
            }
        }

        private void ConvertLineArcSegment(int index, System.Windows.Point position, bool clockwise, bool centreControl = true)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int c1 = index;
                    int c2 = c1 + 1;
                    int c3 = c2 + 1;
                    FlexiPoint lp = flexiPoints[c1];
                    FlexiPoint fp = flexiPoints[c1 + 1];
                    double cx1 = lp.X + 0.5 * (fp.X - lp.X);
                    double cy1 = lp.Y + 0.5 * (fp.Y - lp.Y);
                    double radius = Distance(lp, fp) / 2;
                    flexiPoints.Insert(index + 1, new FlexiPoint(new System.Windows.Point(cx1, cy1), c2, FlexiPoint.PointMode.ControlA));

                    FlexiArcSegment ls = new FlexiArcSegment(c1, c2, c3);
                    ls.Clockwise = clockwise;
                    ls.Radius = radius;
                    // starting at segment i +1, if the point id is c2 or more
                    PointInserted(segs, i + 1, c2, 1);
                    segs[i] = ls;
                    if (centreControl)
                    {
                        ls.ResetControlPoints(flexiPoints);
                    }
                    DeselectAll();
                    ls.Select(flexiPoints);
                    break;
                }
            }
        }

        private void CoordsFromString(string coordPart)
        {
            flexiPoints.Clear();
            string[] coords = coordPart.Split('\n');
            for (int i = 0; i < coords.GetLength(0); i++)
            {
                if (coords[i] != null && coords[i] != "")
                {
                    string[] words = coords[i].Split(',');
                    int id = Convert.ToInt32(words[0]);
                    double x = Convert.ToDouble(words[1]);
                    double y = Convert.ToDouble(words[2]);
                    FlexiPoint.PointMode m = FlexiPoint.PointMode.Data;
                    switch (words[3])
                    {
                        case "Control1":
                            {
                                m = FlexiPoint.PointMode.Control1;
                            }
                            break;

                        case "Control2":
                            {
                                m = FlexiPoint.PointMode.Control2;
                            }
                            break;

                        case "ControlQ":
                            {
                                m = FlexiPoint.PointMode.ControlQ;
                            }
                            break;
                    }

                    FlexiPoint fp = new FlexiPoint(new System.Windows.Point(x, y), id, m);
                    flexiPoints.Add(fp);
                }
            }
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private double Distance(FlexiPoint point1, FlexiPoint point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private void EnsureInBounds(List<FlexiPoint> pnts)
        {
            double minx = double.MaxValue;
            double miny = double.MaxValue;
            foreach (FlexiPoint f in pnts)
            {
                if (f.X < minx)
                {
                    minx = f.X;
                }
                if (f.Y < miny)
                {
                    miny = f.Y;
                }
            }
            bool move = false;
            if (minx < 0)
            {
                // Move by the distance we are out of bounds by plus a little extra so point isn't exactly on edge of control
                minx = -minx + 5;
                move = true;
            }
            else
            {
                minx = 0;
            }
            if (miny < 0)
            {
                miny = -miny + 5;
                move = true;
            }
            else
            {
                miny = 0;
            }
            if (move)
            {
                foreach (FlexiPoint f in pnts)
                {
                    f.X += minx;
                    f.Y += miny;
                }
            }
        }

        private bool GetCoord(string v, ref double x, ref double y)
        {
            bool res = false;
            try
            {
                string[] words = v.Split(',');
                if (words.GetLength(0) == 2)
                {
                    words[0] = words[0].Trim();
                    words[1] = words[1].Trim();
                    x = Convert.ToDouble(words[0]);
                    y = Convert.ToDouble(words[1]);
                    res = true;
                }
            }
            catch
            {
            }
            return res;
        }

        private bool GetNumber(string v, ref double x)
        {
            bool res = false;
            try
            {
                x = Convert.ToDouble(v.Trim());
                res = true;
            }
            catch
            {
            }
            return res;
        }

        private bool LoadArc(List<FlexiPoint> pnts, List<FlexiSegment> segments, string[] blks, ref double x, ref double y, int i, bool clockwise, bool isLarge)
        {
            bool valid = GetCoord(blks[i + 1], ref x, ref y);
            FlexiPoint fp = new FlexiPoint(x, y);
            fp.Mode = FlexiPoint.PointMode.ControlA;
            pnts.Add(fp);
            if (valid)
            {
                valid = GetCoord(blks[i + 2], ref x, ref y);
                FlexiPoint fp2 = new FlexiPoint(x, y);
                fp2.Mode = FlexiPoint.PointMode.Data;
                pnts.Add(fp2);

                FlexiArcSegment sq = new FlexiArcSegment(pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                sq.IsLargeArc = isLarge;
                sq.Clockwise = clockwise;
                sq.Radius = Distance(fp, fp2);
                segments.Add(sq);
            }

            return valid;
        }

        private bool LoadRArc(List<FlexiPoint> pnts, List<FlexiSegment> segments, string[] blks, ref double x, ref double y, int i, bool clockwise, bool isLarge)
        {
            double ox = pnts[pnts.Count - 1].X;
            double oy = pnts[pnts.Count - 1].Y;
            bool valid = GetCoord(blks[i + 1], ref x, ref y);
            FlexiPoint fp = new FlexiPoint(x + ox, y + oy);
            fp.Mode = FlexiPoint.PointMode.ControlA;
            pnts.Add(fp);
            if (valid)
            {
                valid = GetCoord(blks[i + 2], ref x, ref y);
                FlexiPoint fp2 = new FlexiPoint(x + ox, y + oy);
                fp2.Mode = FlexiPoint.PointMode.Data;
                pnts.Add(fp2);

                FlexiArcSegment sq = new FlexiArcSegment(pnts.Count - 3, pnts.Count - 2, pnts.Count - 1);
                sq.IsLargeArc = isLarge;
                sq.Clockwise = clockwise;
                sq.Radius = Distance(fp, fp2);
                segments.Add(sq);
            }

            return valid;
        }

        private void PointInserted(List<FlexiSegment> segs, int startSegment, int v2, int numInserted)
        {
            for (int i = startSegment; i < segs.Count; i++)
            {
                segs[i].PointInserted(v2, numInserted);
            }

            for (int i = 0; i < flexiPoints.Count; i++)
            {
                flexiPoints[i].Id = i;
            }
        }

        private void PointsRemoved(List<FlexiSegment> segs, int index, int numPointsRemoved)
        {
            for (int i = index; i < segs.Count; i++)
            {
                segs[i].PointsRemoved(numPointsRemoved);
            }
        }

        private void SegsFromString(string segpart)
        {
            string[] strokes = segpart.Split('\n');
            segs.Clear();
            for (int i = 0; i < strokes.GetLength(0); i++)
            {
                if (strokes[i] != null && strokes[i] != "")
                {
                    string[] words = strokes[i].Split(',');
                    if (words[0] == "L")
                    {
                        int p0 = Convert.ToInt32(words[1]);
                        int p1 = Convert.ToInt32(words[2]);
                        LineSegment ls = new LineSegment(p0, p1);
                        segs.Add(ls);
                    }
                    if (words[0] == "Q")
                    {
                        int p0 = Convert.ToInt32(words[1]);
                        int p1 = Convert.ToInt32(words[2]);
                        int p2 = Convert.ToInt32(words[3]);

                        FlexiQuadBezier ls = new FlexiQuadBezier(p0, p1, p2);
                        segs.Add(ls);
                    }
                    if (words[0] == "C")
                    {
                        int p0 = Convert.ToInt32(words[1]);
                        int p1 = Convert.ToInt32(words[2]);
                        int p2 = Convert.ToInt32(words[3]);
                        int p3 = Convert.ToInt32(words[4]);
                        FlexiCubicBezier ls = new FlexiCubicBezier(p0, p1, p2, p3);
                        segs.Add(ls);
                    }
                }
            }
        }

        // Return the polygon's area in "square units." Add the areas of the trapezoids defined by
        // the polygon's edges dropped to the X-axis. When the program considers a bottom edge of a
        // polygon, the calculation gives a negative area so the space between the polygon and the
        // axis is subtracted, leaving the polygon's area. This method gives odd results for
        // non-simple polygons.
        //
        // The value will be negative if the polygon is oriented clockwise.
        private float SignedPolygonArea(List<System.Drawing.PointF> fpoints)
        {
            // Get the areas.
            float area = 0;
            // Add the first point to the end.
            int num_points = fpoints.Count - 1;

            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (fpoints[i + 1].X - fpoints[i].X) *
                    (fpoints[i + 1].Y + fpoints[i].Y) / 2;
            }

            // Return the result.
            return area;
        }

        private double SignedPolygonArea(List<System.Windows.Point> fpoints)
        {
            // Get the areas.
            double area = 0;
            // Add the first point to the end.
            int num_points = fpoints.Count - 1;

            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (fpoints[i + 1].X - fpoints[i].X) *
                    (fpoints[i + 1].Y + fpoints[i].Y) / 2;
            }

            // Return the result.
            return area;
        }

        public struct Dimension
        {
            public double Lower;
            public double Upper;
            public double X;
        }
    }
}