using asdflibrary;
using Barnacle.Models.BufferedPolyline;
using EarClipperLib;
using MakerLib;
using OctTreeLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using Triangle = PolygonTriangulationLib.Triangle;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PathLoft.xaml
    /// </summary>
    public partial class PathLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minloftHeight = 0.1;
        private const double maxloftHeight = 200;
        private double loftHeight;

        public double LoftHeight
        {
            get
            {
                return loftHeight;
            }

            set
            {
                if (loftHeight != value)
                {
                    if (value >= minloftHeight && value <= maxloftHeight)
                    {
                        loftHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftHeightToolTip
        {
            get
            {
                return $"LoftHeight must be in the range {minloftHeight} to {maxloftHeight}";
            }
        }

        private const double minloftThickness = 0.1;
        private const double maxloftThickness = 200;
        private double loftThickness;

        private List<Point> pathPoints;

        public double LoftThickness

        {
            get
            {
                return loftThickness;
            }

            set
            {
                if (loftThickness != value)
                {
                    if (value >= minloftThickness && value <= maxloftThickness)
                    {
                        loftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftThicknessToolTip
        {
            get
            {
                return $"LoftThickness must be in the range {minloftThickness} to {maxloftThickness}";
            }
        }

        public PathLoftDlg()
        {
            InitializeComponent();
            ToolName = "PathLoft";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            pathPoints = new List<Point>();
            PathEditor.ToolName = ToolName;
            PathEditor.HasPresets = false;
        }

        private double pathXSize;
        private double pathYSize;
        private double trx = 0;
        private double triy = 0;
        private double blx = 0;
        private double bly = 0;
        private double xRes = 0.25;
        private double yRes = 0.25;

        private void PathPointsChanged(List<Point> points)
        {
            trx = 0;
            triy = 0;
            blx = 0;
            bly = 0;
            Get2DBounds(points, ref blx, ref bly, ref trx, ref triy);
            if (trx < double.MaxValue)
            {
                pathXSize = trx - blx;
                pathYSize = bly - triy;

                double mx = trx + pathXSize / 2.0;
                double my = triy + pathYSize / 2.0;
                // double px; double py;
                pathPoints.Clear();
                foreach (Point p in points)
                {
                    pathPoints.Insert(0, new Point(p.X, p.Y));
                }
                if (pathXSize > 0)
                {
                    xRes = 0.25 / pathXSize;
                }
                else
                {
                    xRes = 0.1;
                }
                if (pathYSize > 0)
                {
                    yRes = 0.25 / pathYSize;
                }
                else
                {
                    yRes = 0.1;
                }
                GenerateShape();
                Redisplay();
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }

            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }

            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public string WarningText
        {
            get
            {
                return warningText;
            }

            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        public int AddVerticeOctTree(double x, double y, double z)
        {
            int res = -1;
            if (octTree != null)
            {
                Point3D v = new Point3D(x, y, z);
                res = octTree.PointPresent(v);

                if (res == -1)
                {
                    res = Vertices.Count;
                    octTree.AddPoint(res, v);
                }
            }
            return res;
        }

        private OctTree octTree;

        private void GenerateShape()
        {
            ClearShape();
            if (pathPoints != null && pathPoints.Count > 0)
            {
                BufferedPolyline bl = new BufferedPolyline(pathPoints);
                bl.BufferRadius = loftThickness / 2;
                List<Point> outline = bl.GenerateBufferOutline();
                if (outline != null && outline.Count > 3)
                {
                    List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                    double top = 0;
                    for (int i = 0; i < outline.Count; i++)
                    {
                        if (outline[i].Y > top)
                        {
                            top = outline[i].Y;
                        }
                    }

                    /*
                    for (int i = 0; i < outline.Count; i++)
                    {
                        double x = PathEditor.ToMM(outline[i].X);
                        double y = PathEditor.ToMM(top - outline[i].Y);
                        //tmp.Insert(0, new System.Windows.Point(x, y));
                        tmp.Add(new System.Windows.Point(x, y));
                    }
                    */
                    for (int i = 0; i < outline.Count; i++)
                    {
                        tmp.Add(new Point(outline[i].X, top - outline[i].Y));
                    }
                    double lx, rx, ty, by;
                    CalculateExtents(tmp, out lx, out rx, out ty, out by);

                    octTree = CreateOctree(new Point3D(-lx, -by, -1),
                                            new Point3D(+rx, +ty, loftHeight + 1));

                    for (int i = 0; i < tmp.Count; i++)
                    {
                        CreateSideFace(tmp, i);
                    }

                    // triangulate the basic polygon
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (System.Windows.Point p in tmp)
                    {
                        pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, 0.0);
                        int c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, 0.0);
                        int c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, 0.0);
                        Faces.Add(c0);
                        Faces.Add(c2);
                        Faces.Add(c1);

                        c0 = AddVerticeOctTree(t.Points[0].X, t.Points[0].Y, loftHeight);
                        c1 = AddVerticeOctTree(t.Points[1].X, t.Points[1].Y, loftHeight);
                        c2 = AddVerticeOctTree(t.Points[2].X, t.Points[2].Y, loftHeight);
                        Faces.Add(c0);
                        Faces.Add(c1);
                        Faces.Add(c2);
                    }

                    CentreVertices();
                }
            }
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }

            int c0 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVerticeOctTree(pnts[i].X, pnts[i].Y, loftHeight);
            int c2 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, loftHeight);
            int c3 = AddVerticeOctTree(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        protected OctTree CreateOctree(Point3D minPoint, Point3D maxPoint)
        {
            octTree = new OctTree(Vertices, minPoint, maxPoint, 200);
            return octTree;
        }

        private double xExtent;
        private double yExtent;

        private void CalculateExtents(List<Point> tmp, out double lx, out double rx, out double ty, out double by)
        {
            lx = double.MaxValue;
            rx = double.MinValue;
            ty = double.MinValue;
            by = double.MaxValue;
            for (int i = 0; i < tmp.Count; i++)
            {
                lx = Math.Min(tmp[i].X, lx);
                rx = Math.Max(tmp[i].X, rx);
                by = Math.Min(tmp[i].Y, by);
                ty = Math.Max(tmp[i].Y, ty);
            }
            xExtent = rx - lx;
            yExtent = ty - by;
        }

        private const double sizeLimit = 0.05;
        /*
                private void GenerateShape()
                {
                    ClearShape();
                    // PathLoftMaker maker = new PathLoftMaker(loftHeight, loftThickness);
                    // maker.Generate(Vertices, Faces);
                    if (pathPoints != null && pathPoints.Count > 0)
                    {
                        DistanceCell2D cell = new DistanceCell2D();
                        cell.InitialisePoints();
                        DistanceCell2D.OnCalculateDistance = CalculateDistance;
                        float th = (float)(loftThickness / 2.0);
                        cell.SetPoint(DistanceCell2D.TopLeft, (float)blx - th - 1, (float)triy + th + 1, CalculateDistance((float)blx - th - 1, (float)triy + th + 1));
                        cell.SetPoint(DistanceCell2D.TopRight, (float)trx + th + 1, (float)triy + th + 1, CalculateDistance((float)trx + th + 1, (float)triy + th + 1));
                        cell.SetPoint(DistanceCell2D.BottomLeft, (float)blx - th - 1, (float)bly - th - 1, CalculateDistance((float)blx - th - 1, (float)bly - th - 1));
                        cell.SetPoint(DistanceCell2D.BottomRight, (float)trx + th + 1, (float)bly - th - 1, CalculateDistance((float)trx + th + 1, (float)bly - th - 1));
                        cell.SetCentre();

                        cell.CreateSubCells();
                        // cell.Dump();
                        List<DistanceCell2D> queue = new List<DistanceCell2D>();
                        queue.Add(cell.SubCells[0]);
                        queue.Add(cell.SubCells[1]);
                        queue.Add(cell.SubCells[2]);
                        queue.Add(cell.SubCells[3]);
                        DateTime start = DateTime.Now;
                        bool subdivide = false;
                        while (queue.Count > 0)
                        {
                            DistanceCell2D cn = queue[0];
                            queue.RemoveAt(0);
                            subdivide = false;
                            if (cn.Size() > sizeLimit)
                            {
                                if (cn.Size() > th / 2.0)
                                {
                                    subdivide = true;
                                }
                                else
                                {
                                    // always subdivide if big and any point is in cell
                                    foreach (Point p in pathPoints)
                                    {
                                        if (cn.Contains(p.X, p.Y))
                                        {
                                            subdivide = true;
                                            break;
                                        }
                                    }

                                    //if no points are the box we may still need to subdivide
                                    // if any path segment intercepts the ends of the box;
                                    if (!subdivide)
                                    {
                                        for (int i = 0; i < pathPoints.Count - 1; i++)
                                        {
                                            if (cn.InterceptedBy(pathPoints[i], pathPoints[i + 1]))
                                            {
                                                subdivide = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (subdivide)
                            {
                                cn.CreateSubCells();
                                queue.Add(cn.SubCells[0]);
                                queue.Add(cn.SubCells[1]);
                                queue.Add(cn.SubCells[2]);
                                queue.Add(cn.SubCells[3]);
                            }
                        }

                        cell.AdjustValues(th);
                        DateTime end = DateTime.Now;
                        TimeSpan dur = end - start;
                        Debug($"Duration {dur}");

                        List<Triangle> triangles = new List<Triangle>();
                        cell.GenerateWalls(triangles);

                        foreach (Triangle t in triangles)
                        {
                            int p0 = AddVertice(t.p[0].x, (t.p[0].y * loftHeight), t.p[0].z);
                            int p1 = AddVertice(t.p[1].x, (t.p[1].y * loftHeight), t.p[1].z);
                            int p2 = AddVertice(t.p[2].x, (t.p[2].y * loftHeight), t.p[2].z);
                            Faces.Add(p0);
                            Faces.Add(p2);
                            Faces.Add(p1);
                        }

                        List<PathEdge> pathEdges = new List<PathEdge>(); ;
                        foreach (Triangle t in triangles)
                        {
                            // Debug($"tri p0={t.p[0].x},{t.p[0].y},{t.p[0].z}
                            // p1={t.p[1].x},{t.p[1].y},{t.p[1].z} p2={t.p[2].x},{t.p[2].y},{t.p[2].z}");
                            if ((Math.Abs(t.p[0].y) < 0.0001) && (Math.Abs(t.p[1].y) < 0.0001))
                            {
                                PathEdge pe = new PathEdge(t.p[0], t.p[1]);
                                // Debug($" p0={t.p[0].x},{t.p[0].y},{t.p[0].z}
                                // p1={t.p[1].x},{t.p[1].y},{t.p[1].z} ");
                                pathEdges.Add(pe);
                            }
                            if ((Math.Abs(t.p[0].y) < 0.0001) && (Math.Abs(t.p[2].y) < 0.0001))
                            {
                                PathEdge pe = new PathEdge(t.p[0], t.p[2]);
                                // Debug($"p0={t.p[0].x},{t.p[0].y},{t.p[0].z} p2={t.p[2].x},{t.p[2].y},{t.p[2].z}");
                                pathEdges.Add(pe);
                            }
                            if ((Math.Abs(t.p[1].y) < 0.0001) && (Math.Abs(t.p[2].y) < 0.0001))
                            {
                                // Debug($"p1={t.p[1].x},{t.p[1].y},{t.p[1].z} p2={t.p[2].x},{t.p[2].y},{t.p[2].z}");
                                PathEdge pe = new PathEdge(t.p[1], t.p[2]);
                                pathEdges.Add(pe);
                            }
                        }
                        if (pathEdges.Count > 0)
                        {
                            List<System.Drawing.PointF> polyLine = new List<System.Drawing.PointF>();
                            polyLine.Add(pathEdges[0].P1);
                            polyLine.Add(pathEdges[0].P2);
                            pathEdges.RemoveAt(0);
                            bool done = false;
                            int pend = 0;
                            while (!done)
                            {
                                pend = polyLine.Count - 1;
                                // Debug($" {pathEdges.Count} edges, Looking for either
                                // {polyLine[0].X},{polyLine[0].Y} or {polyLine[pend].X},{polyLine[pend].Y}");
                                int i = 0;
                                bool match = false;
                                System.Drawing.PointF sp = polyLine[0];
                                System.Drawing.PointF ep = polyLine[pend];

                                while (i < pathEdges.Count && match == false)
                                {
                                    // does the current edge start at the end of the poly
                                    if (pathEdges[i].StartEquals(ep))
                                    {
                                        polyLine.Add(pathEdges[i].P2);

                                        match = true;
                                    }
                                    else
                                    // does the current edge end at the end of the poly if so add it reverse
                                    if (pathEdges[i].EndEquals(ep))
                                    {
                                        polyLine.Add(pathEdges[i].P2);

                                        match = true;
                                    }
                                    else
                                    // does the current edge end at the start of the poly
                                    if (pathEdges[i].EndEquals(sp))
                                    {
                                        polyLine.Insert(0, pathEdges[i].P1);

                                        match = true;
                                    }
                                    else
                                    // does the current edge start at the start of the poly if so
                                    // add it reverse
                                    if (pathEdges[i].StartEquals(sp))
                                    {
                                        polyLine.Insert(0, pathEdges[i].P2);

                                        match = true;
                                    }
                                    if (match)
                                    {
                                        pathEdges.RemoveAt(i);
                                    }
                                    i++;
                                }
                                if (pathEdges.Count == 0 || match == false)
                                {
                                    done = true;
                                }
                            }
                            if (polyLine.Count >= 3)
                            {
                                bool reverse = false;
                                for (float py = 0; py <= loftHeight; py += (float)loftHeight)
                                {
                                    EarClipping earClipping = new EarClipping();
                                    List<Vector3m> rootPoints = new List<Vector3m>();

                                    foreach (System.Drawing.PointF rp in polyLine)
                                    {
                                        rootPoints.Insert(0, new Vector3m(rp.X, py, rp.Y));
                                    }

                                    earClipping.SetPoints(rootPoints);

                                    earClipping.Triangulate();
                                    var surface = earClipping.Result;
                                    for (int i = 0; i < surface.Count; i += 3)
                                    {
                                        int v1 = AddVertice(surface[i].X, surface[i].Y, surface[i].Z);
                                        int v2 = AddVertice(surface[i + 1].X, surface[i + 1].Y, surface[i + 1].Z);
                                        int v3 = AddVertice(surface[i + 2].X, surface[i + 2].Y, surface[i + 2].Z);
                                        if (reverse)
                                        {
                                            Faces.Add(v1);
                                            Faces.Add(v3);
                                            Faces.Add(v2);
                                        }
                                        else
                                        {
                                            Faces.Add(v1);
                                            Faces.Add(v2);
                                            Faces.Add(v3);
                                        }
                                    }
                                    reverse = !reverse;
                                }
                            }
                        }
                        CentreVertices();
                        cell = null;
                        GC.Collect();
                    }
                }
        */

        private float CalculateDistance(float x, float y)
        {
            Point closest = new Point(0, 0);

            float res = DistToPoly(pathPoints, x, y, ref closest);

            return res;
        }

        private float DistToPoly(List<Point> pnts, float x, float y, ref Point closest)
        {
            float v = float.MaxValue;
            float d = 0;

            Point cl = new Point(0, 0);
            for (int i = 0; i < pnts.Count - 1; i++)
            {
                d = FindClosestToLine(x, y, pnts[i], pnts[i + 1], out cl);
                if (d < v)
                {
                    v = d;
                    closest.X = cl.X;
                    closest.Y = cl.Y;
                }
            }
            return v;
        }

        public float FindClosestToLine(
          float x, float y, Point p1, Point p2, out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = x - p1.X;
                dy = y - p1.Y;
                return (float)Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((x - p1.X) * dx + (y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = x - p1.X;
                dy = y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = x - p2.X;
                dy = y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = x - closest.X;
                dy = y - closest.Y;
            }

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            LoftHeight = EditorParameters.GetDouble("LoftHeight", 10);
            LoftThickness = EditorParameters.GetDouble("LoftThickness", 5);
            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
            EditorParameters.Set("LoftThickness", LoftThickness.ToString());
            EditorParameters.Set("Path", PathEditor.AbsolutePathString);
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            PathEditor.ContinuousPointsNotify = false;
            PathEditor.OpenEndedPath = true;
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            // should flexi control give us live point updates while lines are dragged. Computing
            // new line costs too much so , no, instead wait until mouse up

            UpdateDisplay();
        }
    }
}