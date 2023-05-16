using asdflibrary;
using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

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
                pathXSize = blx - trx;
                pathYSize = bly - triy;

                double mx = trx + pathXSize / 2.0;
                double my = triy + pathYSize / 2.0;
                double px;
                double py;
                pathPoints.Clear();
                foreach (Point p in points)
                {
                    /*
                    if (pathXSize > 0)
                    {
                        px = (p.X - mx) / pathXSize;
                    }
                    else
                    {
                        px = 0.0;
                    }
                    if (pathYSize > 0)
                    {
                        py = (p.Y - my) / pathYSize;
                    }
                    else
                    {
                        py = 0.0;
                    }
                    pathPoints.Add(new Point(px, py));
                    */

                    pathPoints.Add(new Point(p.X, p.Y));
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

        private const double sizeLimit = 0.1;

        private void GenerateShape()
        {
            ClearShape();
            // PathLoftMaker maker = new PathLoftMaker(loftHeight, loftThickness);
            //  maker.Generate(Vertices, Faces);
            if (pathPoints != null && pathPoints.Count > 0)
            {
                DistanceCell2D cell = new DistanceCell2D();
                cell.InitialisePoints();
                DistanceCell2D.OnCalculateDistance = CalculateDistance;
                /*
                cell.SetPoint(DistanceCell2D.TopLeft, -0.6F, 0.6F, CalculateDistance(-0.6F, 0.6F));
                cell.SetPoint(DistanceCell2D.TopRight, 0.6F, 0.6F, CalculateDistance(0.6F, 0.6F));
                cell.SetPoint(DistanceCell2D.BottomLeft, -0.6F, -0.6F, CalculateDistance(-0.6F, -0.6F));
                cell.SetPoint(DistanceCell2D.BottomRight, 0.6F, -0.6F, CalculateDistance(0.6F, -0.6F));
                */
                cell.SetPoint(DistanceCell2D.TopLeft, (float)blx - 1, (float)triy + 1, CalculateDistance((float)blx - 1, (float)triy + 1));
                cell.SetPoint(DistanceCell2D.TopRight, (float)trx + 1, (float)triy + 1, CalculateDistance((float)trx + 1, (float)triy + 1));
                cell.SetPoint(DistanceCell2D.BottomLeft, (float)blx - 1, (float)bly - 1, CalculateDistance((float)blx - 1, (float)bly - 1));
                cell.SetPoint(DistanceCell2D.BottomRight, (float)trx + 1, (float)bly - 1, CalculateDistance((float)trx + 1, (float)bly - 1));
                cell.SetCentre();

                cell.CreateSubCells();
                cell.Dump();
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
                        subdivide = true;
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
                float th = (float)(loftThickness * xRes);
                if (pathXSize > pathYSize)
                {
                    th = (float)(loftThickness / pathXSize);
                }
                else
                {
                    th = (float)(loftThickness / pathYSize);
                }
                th = 1.0F;
                cell.AdjustValues(th);
                DateTime end = DateTime.Now;
                TimeSpan dur = end - start;
                Debug($"Duration {dur}");

                List<Triangle> triangles = new List<Triangle>();
                cell.GenerateWalls(triangles);

                foreach (Triangle t in triangles)
                {
                    /*
                    int p0 = AddVertice(t.p[0].x * pathXSize, -(t.p[0].y * loftHeight), t.p[0].z * pathYSize);
                    int p1 = AddVertice(t.p[1].x * pathXSize, -(t.p[1].y * loftHeight), t.p[1].z * pathYSize);
                    int p2 = AddVertice(t.p[2].x * pathXSize, -(t.p[2].y * loftHeight), t.p[2].z * pathYSize);
*/
                    int p0 = AddVertice(t.p[0].x, (t.p[0].y * loftHeight), t.p[0].z);
                    int p1 = AddVertice(t.p[1].x, (t.p[1].y * loftHeight), t.p[1].z);
                    int p2 = AddVertice(t.p[2].x, (t.p[2].y * loftHeight), t.p[2].z);
                    Faces.Add(p0);
                    Faces.Add(p2);
                    Faces.Add(p1);
                }
                CentreVertices();
                cell = null;
                GC.Collect();
            }
        }

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

            // See if this represents one of the segment's
            // end points or a point in the middle.
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
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
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
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}