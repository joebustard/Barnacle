using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Make3D.LineLib
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

        public ObservableCollection<FlexiPoint> FlexiPoints
        {
            get { return points; }
            set
            {
                points = value;
            }
        }

        public void AddCurve(Point p1, Point p2, Point p3)
        {
            int i = points.Count - 1;
            points.Add(new FlexiPoint(p1, i + 1, FlexiPoint.PointMode.Control1));
            points.Add(new FlexiPoint(p2, i + 2, FlexiPoint.PointMode.Control2));
            points.Add(new FlexiPoint(p3, i + 3));
            FlexiBezier bl = new FlexiBezier(i, i + 1, i + 2, i + 3);
            segs.Add(bl);
        }

        public void AddLine(Point p1)
        {
            int i = points.Count - 1;
            points.Add(new FlexiPoint(p1, i + 1));
            LineSegment ls = new LineSegment(i, i + 1);
            segs.Add(ls);
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

                    FlexiBezier ls = new FlexiBezier(c1, c2, c3, c4);
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

        public void DeleteLastSegment()
        {
            if (segs.Count > 0)
            {
                segs[segs.Count - 1].DeletePoints(points);
                segs.RemoveAt(segs.Count - 1);
            }
        }

        public void DeleteSegmentStartingAt(int index)
        {
            if (segs.Count > 2)
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
            res.Add(Start.ToPoint());
            foreach (FlexiSegment sg in segs)
            {
                sg.DisplayPoints(res, points);
            }

            return res;
        }

        public void FromString(string s)
        {
            string coordPart;
            string segpart;
            int index = s.IndexOf("|");
            if (index > -1)
            {
                coordPart = s.Substring(0, index);
                CoordsFromString(coordPart);
                segpart = s.Substring(index + 1);
                SegsFromString(segpart);
            }
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

                    FlexiBezier ls = new FlexiBezier(index + 1, index + 2, index + 3, index + 4);
                    PointInserted(segs, i + 1, index + 1, 3);
                    segs.Insert(i + 1, ls);
                    ls.ResetControlPoints(points);
                    break;
                }
            }
        }

        public void InsertLineSegment(int index, Point position)
        {
            bool found = false;
            for (int i = 0; i < segs.Count; i++)
            {
                if (segs[i].Start() == index)
                {
                    int end = segs[i].End();
                    points.Insert(index + 1, new FlexiPoint(position, index + 1));

                    LineSegment ls = new LineSegment(index + 1, index + 2);
                    PointInserted(segs, i + 1, index + 1, 1);
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
                if (segs[segs.Count - 1].End() == index)
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

        public bool SelectAtPoint(Point position)
        {
            bool found = false;

            DeselectAll();
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
                    }

                    FlexiPoint fp = new FlexiPoint(new Point(x, y), id, m);
                    points.Add(fp);
                }
            }
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
                    if (words[0] == "C")
                    {
                        int p0 = Convert.ToInt32(words[1]);
                        int p1 = Convert.ToInt32(words[2]);
                        int p2 = Convert.ToInt32(words[3]);
                        int p3 = Convert.ToInt32(words[4]);
                        FlexiBezier ls = new FlexiBezier(p0, p1, p2, p3);
                        segs.Add(ls);
                    }
                }
            }
        }
    }
}