using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Barnacle.LineLib
{
    public class FlexiPath
    {
        private const double selectionDistance = 2;
        private bool closed;
        private ObservableCollection<FlexiPoint> points;
        private List<FlexiSegment> segs;

        public FlexiPath()
        {
            segs = new List<FlexiSegment>();
            points = new ObservableCollection<FlexiPoint>();
            closed = true;
        }

        public bool Closed
        {
            get { return closed; }
            set { closed = value; }
        }

        public ObservableCollection<FlexiPoint> FlexiPoints
        {
            get { return points; }
            set { points = value; }
        }

        public FlexiPoint Start
        {
            get
            {
                if (points != null && points.Count > 0 && points[0] != null)
                {
                    return points[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (points.Count == 0)
                {
                    points.Add(value);
                }
                else
                {
                    points[0] = value;
                }
            }
        }

        public void AddCurve(Point p1, Point p2, Point p3)
        {
            int i = points.Count - 1;
            points.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));
            points.Add(new FlexiPoint(p2, i + 2, FlexiPoint.PointMode.Control2));
            points.Add(new FlexiPoint(p3, i + 3));
            FlexiCubicBezier bl = new FlexiCubicBezier(i, i + 1, i + 2, i + 3);
            segs.Add(bl);
        }

        public void AddLine(Point p1)
        {
            int i = points.Count - 1;
            points.Add(new FlexiPoint(p1, i + 1));
            LineSegment ls = new LineSegment(i, i + 1);
            segs.Add(ls);
        }

        public void AddQCurve(Point p1, Point p2)
        {
            int i = points.Count - 1;
            points.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));
            points.Add(new FlexiPoint(p2, i + 2));

            FlexiQuadBezier bl = new FlexiQuadBezier(i, i + 1, i + 2);
            segs.Add(bl);
        }

        public void AppendClosingCurveSegment()
        {
            // get current last point
            FlexiPoint lp = points[points.Count - 1];
            FlexiPoint fp = points[0];
            double cx1 = lp.X + 0.25 * (fp.X - lp.X);
            double cy1 = lp.Y + 0.25 * (fp.Y - lp.Y);
            double cx2 = lp.X + 0.75 * (fp.X - lp.X);
            double cy2 = lp.Y + 0.75 * (fp.Y - lp.Y);
            AddCurve(new Point(cx1, cy1), new Point(cx2, cy2), fp.ToPoint());
        }

        public void AppendClosingQuadCurveSegment()
        {
            // get current last point
            FlexiPoint lp = points[points.Count - 1];
            FlexiPoint fp = points[0];
            double cx1 = lp.X + 0.5 * (fp.X - lp.X);
            double cy1 = lp.Y + 0.5 * (fp.Y - lp.Y);

            AddQCurve(new Point(cx1, cy1), fp.ToPoint());
        }

        public void Clear()
        {
            segs.Clear();
            points.Clear();
        }

        public void ConvertLineCurveSegment(int index, Point position)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int c1 = index;
                    int c2 = c1 + 1;
                    int c3 = c2 + 1;
                    int c4 = c3 + 1;

                    points.Insert(index + 1, new FlexiPoint(position, c3, FlexiPoint.PointMode.Control2));
                    points.Insert(index + 1, new FlexiPoint(position, c2, FlexiPoint.PointMode.Control1));

                    FlexiCubicBezier ls = new FlexiCubicBezier(c1, c2, c3, c4);
                    // starting at segment i +1, if the point id is c2 or more
                    PointInserted(segs, i + 1, c2, 2);
                    segs[i] = ls;
                    ls.ResetControlPoints(points);
                    DeselectAll();
                    ls.Select(points);
                    break;
                }
            }
        }

        public void ConvertLineQuadCurveSegment(int index, Point position, bool centreControl = true)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int c1 = index;
                    int c2 = c1 + 1;
                    int c3 = c2 + 1;

                    points.Insert(index + 1, new FlexiPoint(position, c2, FlexiPoint.PointMode.ControlQ));

                    FlexiQuadBezier ls = new FlexiQuadBezier(c1, c2, c3);
                    // starting at segment i +1, if the point id is c2 or more
                    PointInserted(segs, i + 1, c2, 1);
                    segs[i] = ls;
                    if (centreControl)
                    {
                        ls.ResetControlPoints(points);
                    }
                    DeselectAll();
                    ls.Select(points);
                    break;
                }
            }
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
                Point P0 = points[seg1P0].ToPoint();

                int seg2P0 = segs[secondSeg].Start();
                Point targetPoint = points[seg2P0].ToPoint();

                int seg2P1 = segs[secondSeg].End();
                Point P2 = points[seg2P1].ToPoint();

                double t = DistToLine.FindTOfClosestToLine(targetPoint, P0, P2);
                if (t != double.MinValue && t != 0)
                {
                    // Now it gets nasty
                    // delete the second segment
                    DeleteSegmentStartingAt(seg1P0);
                    // calculate where the control point should be so cuve goes through target point
                    Point P1 = new Point(0, 0);
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
                segs[segs.Count - 1].DeletePoints(points);
                segs.RemoveAt(segs.Count - 1);
            }
        }

        public void DeleteSegment(int index)
        {
            if (index >= 0 && index < segs.Count && segs.Count > 3)
            {
                int numPointsRemoved = segs[index].NumberOfPoints() - 1;
                segs[index].DeletePoints(points);
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
                        segs[i].DeletePoints(points);
                        segs.RemoveAt(i);

                        PointsRemoved(segs, i, numPointsRemoved);

                        DeselectAll();
                        if (i < segs.Count)
                        {
                            segs[i].Select(points);
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
                sg.Deselect(points);
            }
        }

        public List<Point> DisplayPoints()
        {
            // display points are NOT the same as the raw FlexiPoints;
            // Any curves etc may generate more intermediate display points
            List<Point> res = new List<Point>();
            if (Start != null)
            {
                res.Add(Start.ToPoint());
                foreach (FlexiSegment sg in segs)
                {
                    sg.DisplayPoints(res, points);
                }
            }
            return res;
        }

        public List<System.Drawing.PointF> DisplayPointsF()
        {
            // display points are NOT the same as the raw FlexiPoints;
            // Any curves etc may generate more intermediate display points
            List<System.Drawing.PointF> res = new List<System.Drawing.PointF>();
            if (Start != null)
            {
                res.Add(new System.Drawing.PointF((float)Start.ToPoint().X, (float)Start.ToPoint().Y));
                foreach (FlexiSegment sg in segs)
                {
                    sg.DisplayPointsF(res, points);
                }
            }
            //  We want a consistent orientaion of the final points, no matter how the user
            // has drawn his curve
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

        // Return the polygon's area in "square units."
        // Add the areas of the trapezoids defined by the
        // polygon's edges dropped to the X-axis. When the
        // program considers a bottom edge of a polygon, the
        // calculation gives a negative area so the space
        // between the polygon and the axis is subtracted,
        // leaving the polygon's area. This method gives odd
        // results for non-simple polygons.
        //
        // The value will be negative if the polygon is
        // oriented clockwise.
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

        public void FromString(string s)
        {
            string coordPart;
            string segpart;
            if (!s.StartsWith("M"))
            {
                int index = s.IndexOf("|");
                if (index > -1)
                {
                    coordPart = s.Substring(0, index);
                    CoordsFromString(coordPart);
                    segpart = s.Substring(index + 1);
                    SegsFromString(segpart);
                }
            }
            else
            {
                FromTextPath(s);
            }
        }

        public void ClosePath()
        {
            if (segs.Count > 2)
            {
                // create a separate segment that goes back to point zero
                LineSegment sq = new LineSegment(points.Count - 1, 0);
                segs.Add(sq);
            }
        }

        public bool FromTextPath(string s)
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
                                x = x + pnts[pnts.Count - 1].X;
                                y = pnts[pnts.Count - 1].Y;
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
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
                                x = pnts[pnts.Count - 1].X;
                                y = y + pnts[pnts.Count - 1].Y;
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
                                i++;
                            }
                            break;

                        case "RL":
                            {
                                valid = GetCoord(blks[i + 1], ref x, ref y);
                                x += pnts[pnts.Count - 1].X;
                                y += pnts[pnts.Count - 1].Y;
                                FlexiPoint fp = new FlexiPoint(x, y);
                                pnts.Add(fp);
                                LineSegment sq = new LineSegment(pnts.Count - 2, pnts.Count - 1);
                                segments.Add(sq);
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
                    }
                    i++;
                }
            }
            if (valid && pnts.Count > 2)
            {
                points.Clear();
                int i = 0;
                foreach (FlexiPoint f in pnts)
                {
                    f.Id = i;
                    points.Add(f);
                    i++;
                }

                // auto close
                if (points[0].X != points[i - 1].X || points[0].Y != points[i - 1].Y)
                {
                    // create a separate segment that goes back to point zero
                    LineSegment sq = new LineSegment(pnts.Count - 1, 0);
                    segments.Add(sq);
                }
                segs.Clear();
                segs.AddRange(segments);
            }
            return valid;
        }

        public bool ConvertToQuadQuadAtSelected(Point position)
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
                            AppendClosingCurveSegment();

                            segs[segs.Count - 1].Select(points);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public bool ConvertToCubic(Point position)
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

                            segs[segs.Count - 1].Select(points);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public List<FlexiPoint> GetSegmentPoints()
        {
            List<FlexiPoint> res = new List<FlexiPoint>();
            res.Add(Start);
            foreach (FlexiSegment sg in segs)
            {
                sg.GetSegmentPoints(res, points);
            }
            return res;
        }

        /// <summary>
        /// Does the path have two consecutive line segments selected
        /// Used to enable related buttons
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

        public void InsertCurveSegment(int index, Point position)
        {
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int end = segs[i].End();
                    points.Insert(index + 1, new FlexiPoint(position, index + 3, FlexiPoint.PointMode.Control2));
                    points.Insert(index + 1, new FlexiPoint(position, index + 2, FlexiPoint.PointMode.Control1));
                    points.Insert(index + 1, new FlexiPoint(position, index + 1));

                    FlexiCubicBezier ls = new FlexiCubicBezier(index + 1, index + 2, index + 3, index + 4);
                    PointInserted(segs, i + 1, index + 1, 3);
                    segs.Insert(i + 1, ls);
                    ls.ResetControlPoints(points);
                    break;
                }
            }
        }

        public void InsertLineSegment(int pointIndex, Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == pointIndex)
                {
                    int end = segs[i].End();
                    points.Insert(pointIndex + 1, new FlexiPoint(position, pointIndex + 1));

                    LineSegment ls = new LineSegment(pointIndex + 1, pointIndex + 2);
                    PointInserted(segs, i + 1, pointIndex + 1, 1);
                    segs.Insert(i + 1, ls);
                    DeselectAll();
                    segs[i + 1].Select(points);
                    found = true;
                    break;
                }
            }
            if (!found && closed)
            {
                // special case, trying to split the imaginary line that connects
                // the last point back to the first
                if (segs[segs.Count - 1].End() == pointIndex)
                {
                    FlexiPoint np = new FlexiPoint(position, points.Count);
                    points.Add(np);
                    LineSegment ls = new LineSegment(segs[segs.Count - 1].End(), np.Id);
                    segs.Add(ls);
                    DeselectAll();
                    ls.Select(points);
                }
            }
        }

        public bool SplitSelectedLineSegment(Point position)
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
                            points.Add(fx);
                            // Make the existing segment refr to this point
                            (segs[i] as LineSegment).P1 = points.Count - 1;
                            segs[i].Deselect(points);
                            // now reclose the path to linkback up to the first path
                            ClosePath();
                            segs[i + 1].Select(points);
                        }
                        found = true;
                    }
                    break;
                }
            }
            return found;
        }

        public void MoveTo(Point position)
        {
            double cx = 0;
            double cy = 0;
            foreach (FlexiPoint p in points)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= points.Count;
            cy /= points.Count;
            double dx = position.X - cx;
            double dy = position.Y - cy;
            foreach (FlexiPoint p in points)
            {
                p.X += dx;
                p.Y += dy;
            }
        }

        public bool SelectAtPoint(Point position, bool clear = true)
        {
            bool found = false;
            if (clear)
            {
                DeselectAll();
            }
            double minDistance = double.MaxValue;
            FlexiSegment closest = null;
            foreach (FlexiSegment sg in segs)
            {
                double d = sg.DistToPoint(position, points);
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
                LineSegment ls = new LineSegment(points.Count - 1, 0);
                double d = ls.DistToPoint(position, points);
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
                closest.Select(points);
                closest.Selected = true;
            }
            return found;
        }

        public void SetPointPos(int index, Point position)
        {
            if (index >= 0 && index < points.Count)
            {
                points[index].X = position.X;
                points[index].Y = position.Y;
            }
        }

        public string ToPath(bool absolute = false)
        {
            string result = "";
            double ox;
            double oy;
            if (points.Count > 1)
            {
                ox = points[0].X;
                oy = points[0].Y;

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
                    result += sq.ToPath(points, ref ox, ref oy);
                }
            }
            return result;
        }

        public override string ToString()
        {
            string s = "";
            foreach (FlexiPoint p in points)
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
        /// Look for the first selected items
        /// If its two consecutive linesegments return the index of the first one
        /// Else return -1
        /// Used to decide if two sements can be converted to a quadratic bezier
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

        private void CoordsFromString(string coordPart)
        {
            points.Clear();
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

                    FlexiPoint fp = new FlexiPoint(new Point(x, y), id, m);
                    points.Add(fp);
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

        private void PointInserted(List<FlexiSegment> segs, int startSegment, int v2, int numInserted)
        {
            for (int i = startSegment; i < segs.Count; i++)
            {
                segs[i].PointInserted(v2, numInserted);
            }

            for (int i = 0; i < points.Count; i++)
            {
                points[i].Id = i;
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
    }
}