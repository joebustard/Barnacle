using Microsoft.Win32;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
        private double height = 10;
        private BitmapImage localImage;
        private List<System.Windows.Point> editingPolygon;
        private List<System.Windows.Point> outerPolygon;
        private List<System.Windows.Point> innerPolygon;
        private double scale;
        private int selectedPoint;
        private WriteableBitmap wbx;

        private ObservableCollection<String> trackTypes;
        public ObservableCollection<String> TrackTypes
        {
            get
            {
                return trackTypes;
            }
            set
            {
                if ( trackTypes != value)
                {
                    trackTypes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string selectedTrackType;
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
                }
            }
        }
        private int noOfLinks;
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
                    GenerateTrackPath();
                    
                }
            }
        }

        private List<System.Windows.Point> trackPath;
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
            SelectedTrackType = "Simple";
            NoOfLinks = 100;
            trackPath = new List<System.Windows.Point>();
            outerPolygon = new List<System.Windows.Point>();
            innerPolygon = new List<System.Windows.Point>();
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

        private void AddTrackLine(List<System.Windows.Point> pnts,int i, int v)
        {
            if (v >= pnts.Count)
            {
                v = 0;
            }
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 32, 32));
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

        private void CreateOutsideFace(List<System.Windows.Point> ply,int i)
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

        private void DisplayTrack2D()
        {
            if (trackPath != null)
            {
               switch (SelectedTrackType)
                {
                    case "Simple":
                        GenerateSimpleTrack();
                        break;
                }

                DisplayTrackPolygon();
            }
        }

        private void DisplayTrackPolygon()
        {
            if ( outerPolygon.Count > 2 )
            {
                for (int i = 0; i < outerPolygon.Count; i++)
                {
                    AddTrackLine(outerPolygon,i, i + 1);
                }
            }
            if (innerPolygon.Count > 2)
            {
                for (int i = 0; i < innerPolygon.Count; i++)
                {
                   AddTrackLine(innerPolygon, i, i + 1);
                }
            }
        }

        private void GenerateSimpleTrack()
        {
            outerPolygon.Clear();
            innerPolygon.Clear();
            bool firstCall = true;
            for( int i = 0; i < trackPath.Count; i ++)
            {
                int j = i + 1;
                if ( j >= trackPath.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = trackPath[i];
                System.Windows.Point p2 = trackPath[j];
                SimpleLinkPolygon(p1, p2, ref outerPolygon, ref innerPolygon, firstCall);
                firstCall = false;
            }
        }

        private void GenerateFaces()
        {
            Faces.Clear();
            Vertices.Clear();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
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
                    tmp.Insert(0, new System.Windows.Point(outerPolygon[i].X, top - outerPolygon[i].Y));
                }
                else
                {
                    double x = ToMM(outerPolygon[i].X);
                    double y = ToMM(top - outerPolygon[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }
            
            // generate side triangles so original points are already in list
            for (int i = 0; i < outerPolygon.Count; i++)
            {
                CreateOutsideFace(tmp,i);
            }
            tmp.Clear();
            for (int i = 0; i < innerPolygon.Count; i++)
            {
                if (localImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(innerPolygon[i].X, top - innerPolygon[i].Y));
                }
                else
                {
                    double x = ToMM(innerPolygon[i].X);
                    double y = ToMM(top - innerPolygon[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }
            
            for (int i = 0; i < innerPolygon.Count; i++)
            {
                CreateInnerFace(tmp,i);
            }
            /*
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in editingPolygon)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, height);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, height);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, height);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
            */
        }

        private void GeneratePointParams()
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
            editingPolygon.Add(new System.Windows.Point(10, 10));
            editingPolygon.Add(new System.Windows.Point(100, 10));
            editingPolygon.Add(new System.Windows.Point(100, 100));
            editingPolygon.Add(new System.Windows.Point(10, 100));
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
            }
            else
            {
                selectedPoint = -1;
            }
            UpdateDisplay();
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GenerateFaces();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            UpdateDisplay();
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
            DisplayTrackPath();
            DisplayTrack2D();
            DisplayLines();
            DisplayPoints();
            DisplayShape();
        }
        private void DisplayShape()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();
                
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);
            }
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
            Camera.Distance = 200;
            UpdateCameraPos();
            UpdateDisplay();
            
        }

        /// <summary>
        /// Generates the basic path created by the user defined polygon points
        /// </summary>
        private void GenerateTrackPath()
        {
            if (NoOfLinks > 0 && trackPath != null)
            {
                trackPath.Clear();
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
                if ( runningDist < targetDistance && (runningDist + dist >= targetDistance))
                {
                    double overHang = (targetDistance - runningDist)/dist;
                    res = new System.Windows.Point( p1.X + ( p2.X -p1.X)*overHang, 
                                                    p1.Y + (p2.Y - p1.Y) * overHang);
                    found = true;
                }
                runningDist += dist;

            }
            return res;
        }

        private double PolygonLength(List<System.Windows.Point> points)
        {
            double res = 0;
            for( int i = 0; i < points.Count; i ++)
            {
                int j = i + 1;
                if ( j == points.Count)
                {
                    j = 0;
                }
                System.Windows.Point p1 = points[i];
                System.Windows.Point p2 = points[j];
                res += Distance(p1, p2);
            }
            return res;

        }

        private double Distance(System.Windows.Point p1, System.Windows.Point p2)
        {
            double d = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) +
                                   (p2.Y - p1.Y) * (p2.Y - p1.Y));

            return d;
        }

        private void SimpleLinkPolygon(System.Windows.Point p1, System.Windows.Point p2, ref List<System.Windows.Point> outter, ref List<System.Windows.Point> inner, bool firstCall)
        {

            double grad = 0;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double thickness = 6;
            double spikeSize = 4;
            if ( dx == 0 && dy != 0)
            {
                if (p2.Y > p1.Y)
                {
                    // vertical downwards
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y));
                    outter.Add(new System.Windows.Point(p1.X + thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X + thickness - spikeSize, p1.Y + 0.87 * dy));
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
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y));
                    outter.Add(new System.Windows.Point(p1.X - thickness, p1.Y + 0.75 * dy));
                    outter.Add(new System.Windows.Point(p1.X - thickness - spikeSize, p1.Y + 0.87 * dy));
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
                if (p2.X - p1.X>0)
                {
                    // Horizontal Left to right
                    outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness - spikeSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

                    if (firstCall)
                    {
                        inner.Add(new System.Windows.Point(p1.X, p1.Y + thickness));
                    }
                    inner.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y + thickness));
                    inner.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y + thickness ));
                    inner.Add(new System.Windows.Point(p2.X, p2.Y + thickness));
                }
                else
                {
                    // Horizontal right to Left
                    outter.Add(new System.Windows.Point(p1.X, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.75 * dx, p1.Y - thickness));
                    outter.Add(new System.Windows.Point(p1.X + 0.87 * dx, p1.Y - thickness + spikeSize));
                    outter.Add(new System.Windows.Point(p2.X, p2.Y - thickness));

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
                System.Windows.Point o1 = Perpendicular(p1, p2, 0.0, -sign *thickness);
                System.Windows.Point o2 = Perpendicular(p1, p2, 0.75, -sign * thickness);
                System.Windows.Point o3 = Perpendicular(p1, p2, 0.87, -sign * (thickness +spikeSize));
                System.Windows.Point o4 = Perpendicular(p1, p2, 1.0, -sign * thickness);
                outter.Add(o1);
                outter.Add(o2);
                outter.Add(o3);
                outter.Add(o4);

                
                System.Windows.Point i1 = Perpendicular(p1, p2, 0.0, sign*thickness);
                System.Windows.Point i2 = Perpendicular(p1, p2, 0.75, sign*thickness);
                System.Windows.Point i3 = Perpendicular(p1, p2, 0.87, sign*thickness);
                System.Windows.Point i4 = Perpendicular(p1, p2, 1.0, sign*thickness);
                if (firstCall)
                {
                    inner.Add(i1);
                }
                inner.Add(i2);
                inner.Add(i3);
                inner.Add(i4);
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
        private System.Windows.Point Perpendicular(System.Windows.Point p1, System.Windows.Point p2, double t, double d)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double grad = dy/ dx;
            double perp = -1.0 / grad;
            double sgn = Math.Sign(d);
            d = Math.Abs(d);
            System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
            double x = tp.X + sgn * Math.Sqrt((d * d) / (1.0 + (1.0 / (grad * grad))));
            double y = tp.Y + perp * (x - tp.X);
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            GeneratePointParams();
            GenerateFaces();
            DialogResult = true;
            Close();
        }
    }
}
