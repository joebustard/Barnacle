using Make3D.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for TrackDialog.xaml
    /// </summary>
    public partial class TrackDialog : BaseModellerDialog
    {
        public string selectedTrackType;
        private bool cf;
        private List<System.Windows.Point> editingPolygon;
        private double height = 10;
        private List<System.Windows.Point> innerPolygon;
        private BitmapImage localImage;
        private int noOfLinks;
        private List<System.Windows.Point> outerPolygon;
        private double scale;
        private int selectedPoint;
        private double spudSize;
        private double guideSize;
        private double thickness;
        private List<System.Windows.Point> trackPath;
        private ObservableCollection<String> trackTypes;
        private WriteableBitmap wbx;
        private bool showLinks;
        private bool showOutline;

        public TrackDialog()
        {
            InitializeComponent();
            editingPolygon = new List<System.Windows.Point>();

            selectedPoint = -1;
            scale = 1.0;
            InitialisePoints();
            ToolName = "TankTrack";
            DataContext = this;
            TrackTypes = new ObservableCollection<string>();
            TrackTypes.Add("Simple");
            TrackTypes.Add("Centre Guide");
            SelectedTrackType = "Simple";
            NoOfLinks = 100;
            trackPath = new List<System.Windows.Point>();
            outerPolygon = new List<System.Windows.Point>();
            innerPolygon = new List<System.Windows.Point>();
            Thickness = 6;
            SpudSize = 4;
            GuideSize = 4;
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

        private void UpdateTrack()
        {
            GenerateTrackPath();
            GenerateTrack();
            GenerateFaces();
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
                    NotifyPropertyChanged();
                    UpdateTrack();
                    UpdateDisplay();
                }
            }
        }

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

        public void TestPerps()
        {
            System.Windows.Point p1 = new System.Windows.Point(1, 1);
            System.Windows.Point p2 = new System.Windows.Point(2, 2);
            System.Windows.Point p3 = Perpendicular(p1, p2, 0, Math.Sqrt(2));

            String s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 0 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);

            p3 = Perpendicular(p1, p2, 0.5, Math.Sqrt(2));
            s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 0.5 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);

            p3 = Perpendicular(p1, p2, 1, Math.Sqrt(2));
            s = $"{p1.X},{p1.Y},{p2.X},{p2.Y} at 1 = {p3.X},{p3.Y}";
            System.Diagnostics.Debug.WriteLine(s);
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();
            GenerateFaces();
            DialogResult = true;
            Close();
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

        private void CreateInnerFace(List<System.Windows.Point> ply, int i)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(ply[i].X, ply[i].Y, height);
            int c2 = AddVertice(ply[v].X, ply[v].Y, height);
            int c3 = AddVertice(ply[v].X, ply[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
        }

        private void CreateOutsideFace(List<System.Windows.Point> ply, int i)
        {
            int v = i + 1;
            if (v == ply.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(ply[i].X, ply[i].Y, 0.0);
            int c1 = AddVertice(ply[i].X, ply[i].Y, height);
            int c2 = AddVertice(ply[v].X, ply[v].Y, height);
            int c3 = AddVertice(ply[v].X, ply[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void DisplayLines()
        {
            if (editingPolygon != null && editingPolygon.Count > 2)
            {
                for (int i = 0; i < editingPolygon.Count; i++)
                {
                    AddEditingLineToDisplayList(i, i + 1);
                }
            }
        }

        private void DisplayPoints()
        {
            if (editingPolygon != null)
            {
                double rad = 3;
                for (int i = 0; i < editingPolygon.Count; i++)
                {
                    if (selectedPoint == i)
                    {
                        rad = 6;
                    }
                    else
                    {
                        rad = 3;
                    }
                    System.Windows.Point p = editingPolygon[i];
                    Ellipse el = new Ellipse();

                    Canvas.SetLeft(el, p.X - rad);
                    Canvas.SetTop(el, p.Y - rad);
                    el.Width = 2 * rad;
                    el.Height = 2 * rad;
                    el.Fill = System.Windows.Media.Brushes.Red;
                    el.MouseDown += MainCanvas_MouseDown;
                    el.MouseMove += MainCanvas_MouseMove;
                    MainCanvas.Children.Add(el);
                }
            }
        }

        private void DisplayShape()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();
                if (floor != null)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
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

        private double Distance(System.Windows.Point p1, System.Windows.Point p2)
        {
            double d = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) +
                                   (p2.Y - p1.Y) * (p2.Y - p1.Y));

            return d;
        }

        private void GenerateFaces()
        {
            Faces.Clear();
            Vertices.Clear();
            List<System.Windows.Point> otmp = new List<System.Windows.Point>();
            List<System.Windows.Point> itmp = new List<System.Windows.Point>();
            if (outerPolygon != null)
            {
                double top = 0;
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (outerPolygon[i].Y > top)
                    {
                        top = outerPolygon[i].Y;
                    }
                }
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    if (localImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        otmp.Insert(0, new System.Windows.Point(outerPolygon[i].X, top - outerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(outerPolygon[i].X);
                        double y = ToMM(top - outerPolygon[i].Y);
                        otmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                // generate side triangles so original points are already in list
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    CreateOutsideFace(otmp, i);
                }
                itmp.Clear();
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    if (localImage == null)
                    {
                        // flipping coordinates so have to reverse polygon too
                        itmp.Insert(0, new System.Windows.Point(innerPolygon[i].X, top - innerPolygon[i].Y));
                    }
                    else
                    {
                        double x = ToMM(innerPolygon[i].X);
                        double y = ToMM(top - innerPolygon[i].Y);
                        itmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    CreateInnerFace(itmp, i);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, 0.0);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, 0.0);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, 0.0);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, 0.0);
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);

                    Faces.Add(c0);
                    Faces.Add(c3);
                    Faces.Add(c2);
                }

                for (int i = 0; i < innerPolygon.Count; i++)
                {
                    int v = i + 1;
                    if (v == innerPolygon.Count)
                    {
                        v = 0;
                    }

                    int c0 = AddVertice(itmp[i].X, itmp[i].Y, height);
                    int c1 = AddVertice(otmp[i].X, otmp[i].Y, height);
                    int c2 = AddVertice(otmp[v].X, otmp[v].Y, height);
                    int c3 = AddVertice(itmp[v].X, itmp[v].Y, height);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);

                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c3);
                }
                CentreVertices();
            }
        }

        private void GenerateSimpleTrack()
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall);
                    firstCall = false;
                }
            }
        }

        private void GenerateCentreGuideTrack()
        {
            if (outerPolygon != null)
            {
                outerPolygon.Clear();
                innerPolygon.Clear();
                bool firstCall = true;
                for (int i = 0; i < trackPath.Count; i++)
                {
                    int j = i + 1;
                    if (j >= trackPath.Count)
                    {
                        j = 0;
                    }
                    System.Windows.Point p1 = trackPath[i];
                    System.Windows.Point p2 = trackPath[j];
                    CentreGuideLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall);
                    firstCall = false;
                }
            }
        }

        private void GenerateTrack()
        {
            if (trackPath != null)
            {
                switch (SelectedTrackType)
                {
                    case "Simple":
                        GenerateSimpleTrack();
                        break;

                    case "Centre Guide":
                        GenerateCentreGuideTrack();
                        break;
                }
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
                if (editingPolygon != null)
                {
                    double totalDist = PolygonLength(editingPolygon);
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
            editingPolygon.Clear();

            editingPolygon.Add(new System.Windows.Point(16.0308899744799, 97.118354607299));
            editingPolygon.Add(new System.Windows.Point(18.9995733030873, 88.2123046214768));
            editingPolygon.Add(new System.Windows.Point(25.1348951574346, 79.2295608223482));
            editingPolygon.Add(new System.Windows.Point(36.3875756563591, 74.6411808335573));
            editingPolygon.Add(new System.Windows.Point(43.1020597048191, 73.285088073382));
            editingPolygon.Add(new System.Windows.Point(52.0791875361411, 73.7929855968123));
            editingPolygon.Add(new System.Windows.Point(254.797849118189, 97.118354607299));
            editingPolygon.Add(new System.Windows.Point(278.97141336542, 100.087037935906));
            editingPolygon.Add(new System.Windows.Point(294.242196571272, 107.916125947681));
            editingPolygon.Add(new System.Windows.Point(296.783513337065, 119.171430762668));
            editingPolygon.Add(new System.Windows.Point(293.80634805609, 128.289529557677));
            editingPolygon.Add(new System.Windows.Point(281.515999075655, 136.55943311594));
            editingPolygon.Add(new System.Windows.Point(262.855703867266, 142.920897391527));
            editingPolygon.Add(new System.Windows.Point(196.571272454067, 152.995014001776));
            editingPolygon.Add(new System.Windows.Point(94.8329656568214, 150.549540451472));
            editingPolygon.Add(new System.Windows.Point(84.6785978257837, 148.783563437378));
            editingPolygon.Add(new System.Windows.Point(44.2592719076565, 133.870637251554));
            editingPolygon.Add(new System.Windows.Point(31.2984042358894, 128.07748074849));
            editingPolygon.Add(new System.Windows.Point(22.3923542500672, 122.140114091276));
            editingPolygon.Add(new System.Windows.Point(16.8790852112249, 110.265380776846));
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line ln = sender as Line;
            int found = -1;
            if (ln != null)
            {
                System.Windows.Point position = e.GetPosition(MainCanvas);
                for (int i = 0; i < editingPolygon.Count; i++)
                {
                    if (editingPolygon[i].X == ln.X1 && editingPolygon[i].Y == ln.Y1)
                    {
                        found = i;
                        break;
                    }
                }
                if (found != -1)
                {
                    if (found < editingPolygon.Count - 1)
                    {
                        editingPolygon.Insert(found + 1, position);
                    }
                    else
                    {
                        editingPolygon.Add(position);
                    }
                }
            }
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
            selectedPoint = -1;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double rad = 3;
                System.Windows.Point position = e.GetPosition(MainCanvas);
                for (int i = 0; i < editingPolygon.Count; i++)
                {
                    System.Windows.Point p = editingPolygon[i];
                    if (position.X >= p.X - rad && position.X <= p.X + rad)
                    {
                        if (position.Y >= p.Y - rad && position.Y <= p.Y + rad)
                        {
                            selectedPoint = i;
                            break;
                        }
                    }
                }
            }
            UpdateDisplay();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            PositionLabel.Content = $"({position.X},{position.Y})";
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                editingPolygon[selectedPoint] = position;
                GenerateTrackPath();
                GenerateTrack();
            }
            else
            {
                selectedPoint = -1;
            }
            UpdateDisplay();
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GenerateTrackPath();
            GenerateTrack();
            GenerateFaces();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private System.Windows.Point Perpendicular(System.Windows.Point p1, System.Windows.Point p2, double t, double d)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double grad = dy / dx;
            double perp = -1.0 / grad;
            double sgn = Math.Sign(d);
            d = Math.Abs(d);
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + sgn * Math.Sqrt((d * d) / (1.0 + (1.0 / (grad * grad))));
            double y = tp.Y + perp * (x - tp.X);
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        private double PolygonLength(List<System.Windows.Point> points)
        {
            double res = 0;
            for (int i = 0; i < points.Count; i++)
            {
                int j = i + 1;
                if (j == points.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = points[i];
                System.Windows.Point p2 = points[j];
                res += Distance(p1, p2);
            }
            return res;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            UpdateDisplay();
        }

        private void SaveEditorParameters()
        {
            String s = "";
            for (int i = 0; i < editingPolygon.Count; i++)
            {
                s += editingPolygon[i].X.ToString() + "," + editingPolygon[i].Y.ToString();
                if (i < editingPolygon.Count - 1)
                {
                    s += ",";
                }
            }
            EditorParameters.Set("Points", s);
            EditorParameters.Set("NoOfLinks", NoOfLinks.ToString());
            EditorParameters.Set("TrackType", SelectedTrackType);
        }

        private void SimpleLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p2.Y));
                }
                else
                {
                    // vertical upwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p2.Y));
                }
            }
            else
            if (dy == 0)
            {
                if (p2.X - p1.X > 0)
                {
                    // Horizontal Left to right
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - +thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness + spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y + thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y - thickness));
                }
            }
            else
            {
                double sign = -1;
                if (dx > 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx > 0 && dy > 0)
                {
                    sign = -1;
                }
                else
                if (dx < 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx < 0 && dy > 0)
                {
                    sign = -1;
                }
                System.Windows.Point o1 = Perpendicular(p1, p2, 0.0, -sign * thickness);
                System.Windows.Point o2 = Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o3 = Perpendicular(p1, p2, 0.87, -sign * (thickness + spudSize));
                System.Windows.Point o4 = Perpendicular(p1, p2, 1.0, -sign * thickness);
                if (firstCall)
                {
                    outter.Add(o1);
                }
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);

                System.Windows.Point i1 = Perpendicular(p1, p2, 0.0, sign * thickness);
                System.Windows.Point i2 = Perpendicular(p1, p2, 0.75, sign * thickness);
                System.Windows.Point i3 = Perpendicular(p1, p2, 0.87, sign * thickness);
                System.Windows.Point i4 = Perpendicular(p1, p2, 1.0, sign * thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
            }
        }

        private void CentreGuideLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.2 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.3 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.5 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.6 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.2 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness - guideSize, p1.Y + 0.3 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness - guideSize, p1.Y + 0.5 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.6 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X - thickness, p2.Y));
                }
                else
                {
                    // vertical upwards
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    }
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.2 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.3 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.5 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.6 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spudSize, p1.Y + 0.87 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p2.Y));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    }
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.2 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.3 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.5 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.6 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.87 * dy));
                    inner.Add(new System.Windows.Point(p1.X + thickness, p2.Y));
                }
            }
            else
            if (dy == 0)
            {
                if (p2.X - p1.X > 0)
                {
                    // Horizontal Left to right
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.4 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    if (firstCall)
                    {
                        outter.Add(new System.Windows.Point(p1.X, p1.Y - +thickness));
                    }
                    outter.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness + spudSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y + thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.2 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.3 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.5 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.6 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y - thickness));
                }
            }
            else
            {
                double sign = -1;
                if (dx > 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx > 0 && dy > 0)
                {
                    sign = -1;
                }
                else
                if (dx < 0 && dy < 0)
                {
                    sign = 1;
                }
                else
                if (dx < 0 && dy > 0)
                {
                    sign = -1;
                }
                System.Windows.Point o1 = Perpendicular(p1, p2, 0.0, -sign * thickness);
                System.Windows.Point o2 = Perpendicular(p1, p2, 0.2, -sign * thickness);
                System.Windows.Point o3 = Perpendicular(p1, p2, 0.3, -sign * thickness);
                System.Windows.Point o4 = Perpendicular(p1, p2, 0.5, -sign * thickness);
                System.Windows.Point o5 = Perpendicular(p1, p2, 0.6, -sign * thickness);
                System.Windows.Point o6 = Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o7 = Perpendicular(p1, p2, 0.87, -sign * (thickness + spudSize));
                System.Windows.Point o8 = Perpendicular(p1, p2, 1.0, -sign * thickness);
                if (firstCall)
                {
                    outter.Add(o1);
                }
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);
                outter.Add(o5);
                outter.Add(o6);
                outter.Add(o7);
                outter.Add(o8);

                System.Windows.Point i1 = Perpendicular(p1, p2, 0.0, sign * thickness);
                System.Windows.Point i2 = Perpendicular(p1, p2, 0.2, sign * thickness);
                System.Windows.Point i3 = Perpendicular(p1, p2, 0.3, sign * (thickness + guideSize));
                System.Windows.Point i4 = Perpendicular(p1, p2, 0.5, sign * (thickness + guideSize));
                System.Windows.Point i5 = Perpendicular(p1, p2, 0.6, sign * thickness);
                System.Windows.Point i6 = Perpendicular(p1, p2, 0.75, sign * thickness);
                System.Windows.Point i7 = Perpendicular(p1, p2, 0.87, sign * thickness);
                System.Windows.Point i8 = Perpendicular(p1, p2, 1.0, sign * thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
                inner.Add(i5);
                inner.Add(i6);
                inner.Add(i7);
                inner.Add(i8);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                LoadImage(imageName);
            }
            String s = EditorParameters.Get("Points");
            if (s != "")
            {
                string[] words = s.Split(',');
                editingPolygon.Clear();
                for (int i = 0; i < words.GetLength(0); i += 2)
                {
                    editingPolygon.Add(new System.Windows.Point(Convert.ToDouble(words[i]), Convert.ToDouble(words[i + 1])));
                }
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
            Camera.Distance = 200;
            GenerateTrackPath();
            GenerateTrack();
            GenerateFaces();
            UpdateCameraPos();
            UpdateDisplay();
        }
    }
}