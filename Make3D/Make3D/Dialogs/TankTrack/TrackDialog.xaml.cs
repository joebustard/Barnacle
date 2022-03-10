using Barnacle.LineLib;
using Barnacle.Models;
using Microsoft.Win32;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TrackDialog.xaml
    /// </summary>
    public partial class TrackDialog : BaseModellerDialog
    {
        public string selectedTrackType;
        private bool cf;
        private List<System.Windows.Point> editingPolygon;
        private ExternalLinks externalLinks;
        private FlexiPath flexiPath;
        private double guideSize;
        private List<System.Windows.Point> innerPolygon;
        private BitmapImage localImage;
        private bool moving;
        private int noOfLinks;
        private List<System.Windows.Point> outerPolygon;
        private ObservableCollection<FlexiPoint> polyPoints;
        private double scale;
        private int selectedPoint;
        private SelectionModeType selectionMode;
        private Visibility showGuideSize;
        private bool showLinks;
        private bool showOrtho = true;
        private bool showOutline;
        private double spudSize;
        private double thickness;
        private List<System.Windows.Point> trackPath;
        private ObservableCollection<String> trackTypes;
        private double trackWidth = 10;

        public TrackDialog()
        {
            InitializeComponent();
            editingPolygon = new List<System.Windows.Point>();
            flexiPath = new FlexiPath();
            polyPoints = flexiPath.FlexiPoints;
            selectedPoint = -1;
            scale = 1.0;
            InitialisePoints();
            ToolName = "TankTrack";
            DataContext = this;
            TrackTypes = new ObservableCollection<string>();
            TrackTypes.Add("Simple");

            TrackTypes.Add("M1");

            NoOfLinks = 50;
            trackPath = new List<System.Windows.Point>();
            outerPolygon = new List<System.Windows.Point>();
            innerPolygon = new List<System.Windows.Point>();
            Thickness = 6;
            TrackWidth = 10;
            SpudSize = 1;
            GuideSize = 1;
            ModelGroup = MyModelGroup;
            moving = false;
            externalLinks = new ExternalLinks();
            bool containsBasic = false;
            foreach (Link ln in externalLinks.Links)
            {
                if (!TrackTypes.Contains(ln.Name))
                {
                    TrackTypes.Add(ln.Name);
                }
                if (ln.Name == "Basic")
                {
                    containsBasic = true;
                }
            }

            // if there is a link type called basic then use it as the default;
            if (containsBasic)
            {
                SelectedTrackType = "Basic";
            }
        }

        public enum SelectionModeType
        {
            SelectPoint,
            AddLine,
            AddBezier,
            DeleteSegment,
            AddQuadBezier,
            MovePath
        };

        public double GuideSize
        {
            get
            {
                return guideSize;
            }
            set
            {
                if (guideSize != value)
                {
                    guideSize = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public int NoOfLinks
        {
            get
            {
                return noOfLinks;
            }
            set
            {
                if (noOfLinks != value)
                {
                    noOfLinks = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<FlexiPoint> Points
        {
            get
            {
                return polyPoints;
            }
            set
            {
                if (value != polyPoints)
                {
                    polyPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SelectedPoint
        {
            get
            {
                return selectedPoint;
            }
            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    NotifyPropertyChanged();
                    ClearPointSelections();
                    if (polyPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < polyPoints.Count)
                        {
                            polyPoints[selectedPoint].Selected = true;
                            polyPoints[selectedPoint].Visible = true;
                        }
                    }
                    UpdateDisplay();
                }
            }
        }

        public string SelectedTrackType
        {
            get
            {
                return selectedTrackType;
            }
            set
            {
                if (selectedTrackType != value)
                {
                    selectedTrackType = value;
                    if ((selectedTrackType == "Centre Guide") ||
                        (selectedTrackType == "M1"))
                    {
                        ShowGuideSize = Visibility.Visible;
                    }
                    else
                    {
                        ShowGuideSize = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public SelectionModeType SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                if (value != selectionMode)
                {
                    selectionMode = value;
                    SetButtonBorderColours();
                }
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
                    UpdateDisplay();
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
                    UpdateDisplay();
                }
            }
        }

        public Visibility ShowGuideSize
        {
            get
            {
                return showGuideSize;
            }
            set
            {
                if (value != showGuideSize)
                {
                    showGuideSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowLinkMarkers
        {
            get
            {
                return showLinks;
            }

            set
            {
                if (showLinks != value)
                {
                    showLinks = value;
                    UpdateDisplay();
                }
            }
        }

        public bool ShowOutline
        {
            get
            {
                return showOutline;
            }

            set
            {
                if (showOutline != value)
                {
                    showOutline = value;
                    UpdateDisplay();
                }
            }
        }

        public double SpudSize
        {
            get
            {
                return spudSize;
            }
            set
            {
                if (spudSize != value)
                {
                    spudSize = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> TrackTypes
        {
            get
            {
                return trackTypes;
            }
            set
            {
                if (trackTypes != value)
                {
                    trackTypes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TrackWidth
        {
            get
            {
                return trackWidth;
            }
            set
            {
                if (trackWidth != value)
                {
                    trackWidth = value;
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

        public void TestPerps()
        {
            System.Windows.Point p1 = new System.Windows.Point(1, 1);
            System.Windows.Point p2 = new System.Windows.Point(2, 2);
            System.Windows.Point p3 = TankTrackUtils.Perpendicular(p1, p2, 0, Math.Sqrt(2));

            String s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 0 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);

            p3 = TankTrackUtils.Perpendicular(p1, p2, 0.5, Math.Sqrt(2));
            s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 0.5 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);

            p3 = TankTrackUtils.Perpendicular(p1, p2, 1, Math.Sqrt(2));
            s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 1 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();
            GenerateTrack();
            DialogResult = true;
            Close();
        }

        private void AddBezierButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.AddBezier;
        }

        private bool AddBezierFromPoint(MouseButtonEventArgs e, Line ln, bool cubic)
        {
            int found = -1;
            bool added = false;
            System.Windows.Point position = e.GetPosition(MainCanvas);
            for (int i = 0; i < polyPoints.Count; i++)
            {
                if (Math.Abs(polyPoints[i].X - ln.X1) < 0.0001 && Math.Abs(polyPoints[i].Y - ln.Y1) < 0.0001)
                {
                    found = i;
                    break;
                }
            }
            if (found != -1)
            {
                if (found < polyPoints.Count - 1)
                {
                    if (cubic)
                    {
                        InsertCurveSegment(found, position);
                    }
                    else
                    {
                        InsertQuadCurveSegment(found, position);
                    }
                }
                else
                {
                    // trying to convert last segment into a bezier
                    // We actually need to append a new one
                    if (cubic)
                    {
                        flexiPath.AppendClosingCurveSegment();
                    }
                    else
                    {
                        flexiPath.AppendClosingQuadCurveSegment();
                    }
                }
                added = true;
            }

            return added;
        }

        private void AddEditingLineToDisplayList(int i, int v)
        {
            if (v >= editingPolygon.Count)
            {
                v = 0;
            }
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 32, 32, 255));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 6;
            ln.Fill = br;
            ln.X1 = editingPolygon[i].X;
            ln.Y1 = editingPolygon[i].Y;
            ln.X2 = editingPolygon[v].X;
            ln.Y2 = editingPolygon[v].Y;
            ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
            MainCanvas.Children.Add(ln);
        }

        private void AddLine(int i, int v, List<System.Windows.Point> points, bool joinLast)
        {
            if (v >= points.Count)
            {
                if (joinLast)
                {
                    v = 0;
                }
                else
                {
                    return;
                }
            }
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 255, 255, 5));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 6;
            ln.Fill = br;
            ln.X1 = points[i].X;
            ln.Y1 = points[i].Y;
            ln.X2 = points[v].X;
            ln.Y2 = points[v].Y;
            ln.MouseLeftButtonDown += Ln_MouseLeftButtonDown;
            ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
            MainCanvas.Children.Add(ln);
        }

        private bool AddLineFromPoint(MouseButtonEventArgs e, Line ln)
        {
            int found = -1;
            bool added = false;
            System.Windows.Point position = e.GetPosition(MainCanvas);
            for (int i = 0; i < polyPoints.Count; i++)
            {
                if (Math.Abs(polyPoints[i].X - ln.X1) < 0.0001 && Math.Abs(polyPoints[i].Y - ln.Y1) < 0.0001)
                {
                    found = i;
                    break;
                }
            }
            if (found != -1)
            {
                if (found < polyPoints.Count - 1)
                {
                    InsertLineSegment(found, position);
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                else
                {
                    flexiPath.AddLine(position);
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                added = true;
            }

            return added;
        }

        private void AddQuadBezierButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.AddQuadBezier;
        }

        private void AddSegButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.AddLine;
        }

        private void AddTrackLine(List<System.Windows.Point> pnts, int i, int v)
        {
            cf = !cf;
            if (v >= pnts.Count)
            {
                v = 0;
            }
            SolidColorBrush br;
            if (cf)
            {
                br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 32, 32));
            }
            else
            {
                br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 128, 93, 32));
            }
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 1;
            ln.Fill = br;
            ln.X1 = pnts[i].X;
            ln.Y1 = pnts[i].Y;
            ln.X2 = pnts[v].X;
            ln.Y2 = pnts[v].Y;

            MainCanvas.Children.Add(ln);
        }

        private void ClearPointSelections()
        {
            if (polyPoints != null)
            {
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    polyPoints[i].Selected = false;
                    polyPoints[i].Visible = false;
                }
            }
        }

        private void DashLine(double x1, double y1, double x2, double y2)
        {
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 2;
            ln.StrokeDashArray = new DoubleCollection();
            ln.StrokeDashArray.Add(0.5);
            ln.StrokeDashArray.Add(0.5);
            ln.Fill = br;
            ln.X1 = x1;
            ln.Y1 = y1;
            ln.X2 = x2;
            ln.Y2 = y2;

            MainCanvas.Children.Add(ln);
        }

        private bool DeleteSegment(MouseButtonEventArgs e, Line ln)
        {
            int found = -1;
            bool added = false;
            System.Windows.Point position = e.GetPosition(MainCanvas);
            for (int i = 0; i < polyPoints.Count; i++)
            {
                if (Math.Abs(polyPoints[i].X - ln.X1) < 0.0001 && Math.Abs(polyPoints[i].Y - ln.Y1) < 0.0001)
                {
                    found = i;
                    break;
                }
            }
            if (found != -1)
            {
                if (found < polyPoints.Count - 1)
                {
                    flexiPath.DeleteSegmentStartingAt(found);

                    // PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                else
                {
                    flexiPath.AddLine(position);
                    // PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                added = true;
            }

            return added;
        }

        private void DelSegButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.DeleteSegment;
        }

        private void DisplayLines()
        {
            if (flexiPath != null)
            {
                List<System.Windows.Point> points = flexiPath.DisplayPoints();
                if (points != null && points.Count > 2)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        AddLine(i, i + 1, points, true);
                    }
                }
            }
        }

        private void DisplayPoints()
        {
            if (polyPoints != null)
            {
                double ox = double.NaN;
                double oy = double.NaN;
                if (selectedPoint != -1)
                {
                    ox = polyPoints[selectedPoint].X;
                    oy = polyPoints[selectedPoint].Y;
                }

                double rad = 3;
                System.Windows.Media.Brush br = null;
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    bool ortho = false;

                    System.Windows.Point p = polyPoints[i].ToPoint();
                    if (selectedPoint == i)
                    {
                        rad = 6;
                        br = System.Windows.Media.Brushes.LightGreen;
                    }
                    else
                    {
                        rad = 3;
                        br = System.Windows.Media.Brushes.Red;
                        if (ox != double.NaN)
                        {
                            if (Math.Abs(p.X - ox) < 0.1 || Math.Abs(p.Y - oy) < 0.1)
                            {
                                br = System.Windows.Media.Brushes.Blue;
                                ortho = true;
                            }
                        }
                    }

                    // only show the points if they are marked as visible
                    // OR they are orthogonal to the selected one
                    if (polyPoints[i].Visible || (ortho && showOrtho))
                    {
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Data)
                        {
                            p = MakeEllipse(rad, br, p);
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control1 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            p = MakeRect(rad, br, p);
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control2)
                        {
                            p = MakeTri(rad, br, p);
                        }

                        if (selectedPoint == i && showOrtho)
                        {
                            DashLine(p.X, 0, p.X, MainCanvas.ActualHeight - 1);
                            DashLine(0, p.Y, MainCanvas.ActualWidth - 1, p.Y);
                        }
                    }
                }

                // now draw any control connectors
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    if (polyPoints[i].Visible)
                    {
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control1 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i - 1;
                            if (j < 0)
                            {
                                j = polyPoints.Count - 1;
                            }
                            DrawControlLine(polyPoints[i].ToPoint(), polyPoints[j].ToPoint());
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control2 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i + 1;
                            if (j == polyPoints.Count)
                            {
                                j = 0;
                            }
                            DrawControlLine(polyPoints[i].ToPoint(), polyPoints[j].ToPoint());
                        }
                    }
                }
            }
        }

        private void DisplayShape()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();
                if (floor != null && showFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);
            }
        }

        private void DisplayTrack2D()
        {
            DisplayTrackPolygon();
        }

        private void DisplayTrackPath()
        {
            if (trackPath != null)
            {
                double rad = 3;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    System.Windows.Point p = trackPath[i];
                    Ellipse el = new Ellipse();

                    Canvas.SetLeft(el, p.X - rad);
                    Canvas.SetTop(el, p.Y - rad);
                    el.Width = 2 * rad;
                    el.Height = 2 * rad;
                    el.Fill = System.Windows.Media.Brushes.Blue;

                    MainCanvas.Children.Add(el);
                }
            }
        }

        private void DisplayTrackPolygon()
        {
            if (outerPolygon != null && outerPolygon.Count > 2)
            {
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    AddTrackLine(outerPolygon, i, i + 1);
                }
            }
            if (innerPolygon != null && innerPolygon.Count > 2)
            {
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    AddTrackLine(innerPolygon, i, i + 1);
                }
            }
        }

        private void DrawControlLine(System.Windows.Point p1, System.Windows.Point p2)
        {
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 1;
            ln.StrokeDashArray = new DoubleCollection();
            ln.StrokeDashArray.Add(0.5);
            ln.StrokeDashArray.Add(0.5);
            ln.Fill = br;
            ln.X1 = p1.X;
            ln.Y1 = p1.Y;
            ln.X2 = p2.X;
            ln.Y2 = p2.Y;

            MainCanvas.Children.Add(ln);
        }

        private void GenerateTrack()
        {
            BusyCon.Visibility = Visibility.Visible;
            //    var result = await Task.Run(() => GenerationTask());
            var result = GenerationTask();
            Vertices.Clear();
            Faces.Clear();
            foreach (Point3D p in result.Vertices)
            {
                Vertices.Add(new Point3D(p.X, p.Y, p.Z));
            }
            foreach (int i in result.Faces)
            {
                Faces.Add(i);
            }

            BusyCon.Visibility = Visibility.Hidden;
        }

        private void GenerateTrackFomLink(Link ln, Point3DCollection verts, Int32Collection facs)
        {
            facs.Clear();
            verts.Clear();
            bool firstCall = true;
            double top = double.MinValue;
            for (int i = 0; i < trackPath.Count; i++)
            {
                if (trackPath[i].Y > top)
                {
                    top = trackPath[i].Y;
                }
            }
            for (int i = 0; i < trackPath.Count; i++)
            {
                int j = i + 1;
                if (j >= trackPath.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = new System.Windows.Point(ToMM(trackPath[i].X), ToMM(top - trackPath[i].Y));
                System.Windows.Point p2 = new System.Windows.Point(ToMM(trackPath[j].X), ToMM(top - trackPath[j].Y));
                GenerateLinkPart(p1, p2, verts, facs, firstCall, trackWidth, thickness, ln);
                firstCall = false;
            }
        }

        /// <summary>
        /// Generates the basic path created by the user defined polygon points
        /// </summary>
        private void GenerateTrackPath()
        {
            if (NoOfLinks > 0 && trackPath != null)
            {
                trackPath.Clear();
                editingPolygon = flexiPath.DisplayPoints();
                if (editingPolygon != null)
                {
                    double totalDist = TankTrackUtils.PolygonLength(editingPolygon);
                    double t;
                    double dt = 1.0 / NoOfLinks;
                    for (t = 0; t < 1; t += dt)
                    {
                        System.Windows.Point p = GetPathPoint(editingPolygon, t, totalDist);
                        trackPath.Add(p);
                    }
                }
            }
        }

        private Genresult GenerationTask()
        {
            Genresult result;
            Point3DCollection verts = new Point3DCollection();
            Int32Collection facs = new Int32Collection();
            result.Vertices = verts;
            result.Faces = facs;

            if (trackPath != null && SelectedTrackType != null)
            {
                switch (SelectedTrackType)
                {
                    case "Simple":
                        GenerateSimpleTrack(0, verts, facs);
                        GenerateFaces(verts, facs);
                        break;

                    case "Simple 2":
                        GenerateSimpleTrack(1, verts, facs);
                        GenerateFaces(verts, facs);
                        break;
                    /*
                                        case "Centre Guide":
                                            GenerateCentreGuideTrack();
                                            GenerateFaces(verts, facs);
                                            break;
                    */
                    case "M1":
                        GenerateM1Track(verts, facs);
                        CentreVertices(verts, facs);
                        break;

                    default:
                        {
                            if (externalLinks != null)
                            {
                                foreach (Link ln in externalLinks.Links)
                                {
                                    if (ln.Name == SelectedTrackType)
                                    {
                                        GenerateTrackFomLink(ln, verts, facs);
                                        CentreVertices(verts, facs);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return result;
        }

        private void GetEditorParameters()
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                LoadImage(imageName);
            }
            String s = EditorParameters.Get("Points");
            if (s != "")
            {
                flexiPath.FromString(s);
            }
            s = EditorParameters.Get("NoOfLinks");
            if (s != "")
            {
                NoOfLinks = Convert.ToInt16(s);
            }

            s = EditorParameters.Get("TrackType");
            if (s != "")
            {
                SelectedTrackType = s;
            }

            s = EditorParameters.Get("Thickness");
            if (s != "")
            {
                Thickness = Convert.ToDouble(s);
            }

            s = EditorParameters.Get("SpudSize");
            if (s != "")
            {
                SpudSize = Convert.ToDouble(s); ;
            }
            s = EditorParameters.Get("GuideSize");
            if (s != "")
            {
                GuideSize = Convert.ToDouble(s); ;
            }
            s = EditorParameters.Get("TrackWidth");
            if (s != "")
            {
                TrackWidth = Convert.ToDouble(s); ;
            }
        }

        private System.Windows.Point GetPathPoint(List<System.Windows.Point> points, double t, double totalDist)
        {
            System.Windows.Point res = points[0];
            bool found = false;
            double targetDistance = t * totalDist;

            double runningDist = 0;
            double dist = 0;
            for (int i = 0; i < points.Count && found == false; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = points[i];
                System.Windows.Point p2 = points[j];
                dist = Distance(p1, p2);
                if (runningDist < targetDistance && (runningDist + dist >= targetDistance))
                {
                    double overHang = (targetDistance - runningDist) / dist;
                    res = new System.Windows.Point(p1.X + (p2.X - p1.X) * overHang,
                                                    p1.Y + (p2.Y - p1.Y) * overHang);
                    found = true;
                }
                runningDist += dist;
            }
            return res;
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
            if (opDlg.ShowDialog() == true)
            {
                try
                {
                    LoadImage(opDlg.FileName);
                }
                catch
                {
                }
            }

            UpdateDisplay();
        }

        private void InButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 1.1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private void InitialisePoints()
        {
            flexiPath.Clear();

            flexiPath.Start = new FlexiPoint(new System.Windows.Point(16.0308899744799, 97.118354607299));
            flexiPath.AddLine(new System.Windows.Point(18.9995733030873, 88.2123046214768));
            flexiPath.AddLine(new System.Windows.Point(25.1348951574346, 79.2295608223482));
            flexiPath.AddLine(new System.Windows.Point(36.3875756563591, 74.6411808335573));
            flexiPath.AddLine(new System.Windows.Point(43.1020597048191, 73.285088073382));
            flexiPath.AddLine(new System.Windows.Point(52.0791875361411, 73.7929855968123));
            flexiPath.AddLine(new System.Windows.Point(254.797849118189, 97.118354607299));
            flexiPath.AddLine(new System.Windows.Point(278.97141336542, 100.087037935906));
            flexiPath.AddLine(new System.Windows.Point(294.242196571272, 107.916125947681));
            flexiPath.AddLine(new System.Windows.Point(296.783513337065, 119.171430762668));
            flexiPath.AddLine(new System.Windows.Point(293.80634805609, 128.289529557677));
            flexiPath.AddLine(new System.Windows.Point(281.515999075655, 136.55943311594));
            flexiPath.AddLine(new System.Windows.Point(262.855703867266, 142.920897391527));
            flexiPath.AddLine(new System.Windows.Point(196.571272454067, 152.995014001776));
            flexiPath.AddLine(new System.Windows.Point(94.8329656568214, 150.549540451472));
            flexiPath.AddLine(new System.Windows.Point(84.6785978257837, 148.783563437378));
            flexiPath.AddLine(new System.Windows.Point(44.2592719076565, 133.870637251554));
            flexiPath.AddLine(new System.Windows.Point(31.2984042358894, 128.07748074849));
            flexiPath.AddLine(new System.Windows.Point(22.3923542500672, 122.140114091276));
            flexiPath.AddLine(new System.Windows.Point(16.8790852112249, 110.265380776846));
        }

        private void InsertCurveSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.ConvertLineCurveSegment(startIndex, position);
        }

        private void InsertLineSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.InsertLineSegment(startIndex, position);
        }

        private void InsertQuadCurveSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.ConvertLineQuadCurveSegment(startIndex, position);
        }

        private void Ln_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line ln = sender as Line;
            bool found = false;
            if (ln != null)
            {
                switch (selectionMode)
                {
                    case SelectionModeType.SelectPoint:
                        {
                            found = SelectLineFromPoint(e);
                        }
                        break;

                    case SelectionModeType.AddLine:
                        {
                            found = AddLineFromPoint(e, ln);
                            SelectionMode = SelectionModeType.SelectPoint;
                        }
                        break;

                    case SelectionModeType.AddBezier:
                        {
                            found = AddBezierFromPoint(e, ln, true);
                            SelectionMode = SelectionModeType.SelectPoint;
                        }
                        break;

                    case SelectionModeType.AddQuadBezier:
                        {
                            found = AddBezierFromPoint(e, ln, false);
                            SelectionMode = SelectionModeType.SelectPoint;
                        }
                        break;

                    case SelectionModeType.DeleteSegment:
                        {
                            found = DeleteSegment(e, ln);
                            SelectionMode = SelectionModeType.SelectPoint;
                        }
                        break;
                }
                if (found)
                {
                    UpdateDisplay();
                }
            }
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
     
        }

        private void LoadImage(string f)
        {
            Uri fileUri = new Uri(f);
            localImage = new BitmapImage();
            localImage.BeginInit();
            localImage.UriSource = fileUri;
            //    localImage.DecodePixelWidth = 800;
            localImage.EndInit();
            MainCanvas.Width = localImage.Width;
            MainCanvas.Height = localImage.Height;
            EditorParameters.Set("ImagePath", f);
            UpdateDisplay();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (selectedPoint >= 0)
                {
                    Points[selectedPoint].Selected = false;
                }
                SelectedPoint = -1;

                System.Windows.Point position = e.GetPosition(MainCanvas);
                if (selectionMode == SelectionModeType.MovePath)
                {
                    MoveWholePath(position);
                    SelectionMode = SelectionModeType.SelectPoint;
                    UpdateDisplay();
                }
                else
                {
                    double rad = 3;

                    for (int i = 0; i < polyPoints.Count; i++)
                    {
                        System.Windows.Point p = polyPoints[i].ToPoint();
                        if (position.X >= p.X - rad && position.X <= p.X + rad)
                        {
                            if (position.Y >= p.Y - rad && position.Y <= p.Y + rad)
                            {
                                SelectedPoint = i;
                                Points[i].Selected = true;
                                moving = true;
                                break;
                            }
                        }
                    }
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        UpdateDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            PositionLabel.Content = $"({position.X},{position.Y})";
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed && moving)
            {
                polyPoints[selectedPoint].X = position.X;
                polyPoints[selectedPoint].Y = position.Y;
                flexiPath.SetPointPos(selectedPoint, position);

                UpdateDisplay();
            }
            else
            {
                moving = false;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GenerateTrackPath();
            GenerateTrack();
            moving = false;
        }

        private System.Windows.Point MakeEllipse(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            Ellipse el = new Ellipse();

            Canvas.SetLeft(el, p.X - rad);
            Canvas.SetTop(el, p.Y - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Stroke = br;
            el.Fill = br;
            el.MouseDown += MainCanvas_MouseDown;
            el.MouseMove += MainCanvas_MouseMove;
            //   el.ContextMenu = PointMenu(el);
            MainCanvas.Children.Add(el);
            return p;
        }

        private System.Windows.Point MakeRect(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            System.Windows.Shapes.Rectangle el = new System.Windows.Shapes.Rectangle();

            Canvas.SetLeft(el, p.X - rad);
            Canvas.SetTop(el, p.Y - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Stroke = br;
            el.Fill = br;
            el.MouseDown += MainCanvas_MouseDown;
            el.MouseMove += MainCanvas_MouseMove;
            //   el.ContextMenu = PointMenu(el);
            MainCanvas.Children.Add(el);
            return p;
        }

        private System.Windows.Point MakeTri(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new System.Windows.Point(0.5, 0));
            myPointCollection.Add(new System.Windows.Point(0, 1));
            myPointCollection.Add(new System.Windows.Point(1, 1));

            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = br;

            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            myPolygon.StrokeThickness = 2;
            Canvas.SetLeft(myPolygon, p.X - rad);
            Canvas.SetTop(myPolygon, p.Y - rad);
            myPolygon.Width = 2 * rad;
            myPolygon.Height = 2 * rad;
            myPolygon.Stroke = br;
            myPolygon.Fill = br;
            myPolygon.MouseDown += MainCanvas_MouseDown;
            myPolygon.MouseMove += MainCanvas_MouseMove;
            // myPolygon.ContextMenu = PointMenu(myPolygon);
            MainCanvas.Children.Add(myPolygon);
            return p;
        }

        private void MovePathButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.MovePath;
        }

        private void MoveWholePath(System.Windows.Point position)
        {
            flexiPath.MoveTo(position);
            GenerateTrack();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private System.Windows.Point Perpendicular2(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            Vector v1 = new Vector(dx, dy);
            v1.Normalize();
            Vector v2 = new Vector(-v1.Y, v1.X);
            v2.Normalize();
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + distanceFromLine * v2.X;
            double y = tp.Y + distanceFromLine * v2.Y;
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.SelectPoint;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            GenerateTrack();
            UpdateDisplay();
        }

        private void ResetPathButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;

            selectedPoint = -1;
            UpdateDisplay();
        }

        private void SaveEditorParameters()
        {
            String s = flexiPath.ToString();
            EditorParameters.Set("Points", s);
            EditorParameters.Set("NoOfLinks", NoOfLinks.ToString());
            EditorParameters.Set("TrackType", SelectedTrackType);
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("SpudSize", SpudSize.ToString());
            EditorParameters.Set("GuideSize", GuideSize.ToString());
            EditorParameters.Set("TrackWidth", TrackWidth.ToString());
        }

        private bool SelectLineFromPoint(MouseButtonEventArgs e)
        {
            bool found;
            System.Windows.Point position = e.GetPosition(MainCanvas);
            found = flexiPath.SelectAtPoint(position);

            if (found)
            {
                // GetRawFlexiPoints();
                PointGrid.ItemsSource = Points;
                CollectionViewSource.GetDefaultView(Points).Refresh();
                Redisplay();
            }

            return found;
        }

        private void SetButtonBorderColours()
        {
            switch (selectionMode)
            {
                case SelectionModeType.SelectPoint:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddLine:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddBezier:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddQuadBezier:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.DeleteSegment:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.MovePath:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                    }
                    break;
            }
        }

        private double ToMM(double x)
        {
            double res = x;
            if (localImage != null)
            {
                res = 25.4 * x / localImage.DpiX;
            }
            return res;
        }

        private void UpdateDisplay()
        {
            MainCanvas.Children.Clear();

            if (localImage != null)
            {
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = localImage;
                image.Width = localImage.Width;
                image.Height = localImage.Height;
                MainCanvas.Children.Add(image);
            }
            if (showLinks)
            {
                DisplayTrackPath();
            }

            if (showOutline)
            {
                DisplayTrack2D();
            }
            DisplayLines();
            DisplayPoints();
            DisplayShape();
        }

        private void UpdateTrack()
        {
            GenerateTrackPath();
            GenerateTrack();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetEditorParameters();
            Camera.Distance = 200;
            GenerateTrackPath();
            GenerateTrack();

            UpdateCameraPos();
            UpdateDisplay();
        }

        private struct Genresult
        {
            public Int32Collection Faces;
            public Point3DCollection Vertices;
        }
    }
}