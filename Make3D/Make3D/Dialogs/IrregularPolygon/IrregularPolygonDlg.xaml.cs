using Make3D.Models;
using Microsoft.Win32;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for IrregularPolygonDlg.xaml
    /// </summary>
    public partial class PlateletDlg : BaseModellerDialog
    {
        private double height = 10;
        private bool hollowShape;
        private BitmapImage localImage;
        private List<System.Windows.Point> points;
        private double scale;
        private int selectedPoint;
        private bool showOrtho;
        private Visibility showWidth;
        private bool solidShape;
        private int wallWidth;

        public PlateletDlg()
        {
            InitializeComponent();
            points = new List<System.Windows.Point>();

            selectedPoint = -1;
            scale = 1.0;
            wallWidth = 5;
            solidShape = true;
            hollowShape = false;
            showOrtho = true;
            showWidth = Visibility.Hidden;
            InitialisePoints();

            EditorParameters.ToolName = "Platelet";
            DataContext = this;
            SolidShape = true;
            Camera.Distance = Camera.Distance * 3.0;
            ModelGroup = MyModelGroup;
        }

        public bool HollowShape
        {
            get
            {
                return hollowShape;
            }
            set
            {
                if (hollowShape != value)
                {
                    hollowShape = value;
                    if (hollowShape == true)
                    {
                        ShowWidth = Visibility.Visible;
                    }
                    else
                    {
                        ShowWidth = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        public bool ShowOrtho
        {
            get
            {
                return showOrtho;
            }
            set
            {
                if (value != showOrtho)
                {
                    showOrtho = value;

                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public Visibility ShowWidth
        {
            get
            {
                return showWidth;
            }
            set
            {
                if (showWidth != value)
                {
                    showWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool SolidShape
        {
            get
            {
                return solidShape;
            }
            set
            {
                if (solidShape != value)
                {
                    solidShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public int WallWidth
        {
            get
            {
                return wallWidth;
            }
            set
            {
                if (wallWidth != value)
                {
                    wallWidth = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            GeneratePointParams();
            GenerateFaces();
            DialogResult = true;
            Close();
        }

        private void AddLine(int i, int v)
        {
            if (v >= points.Count)
            {
                v = 0;
            }
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 32, 32, 255));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 6;
            ln.Fill = br;
            ln.X1 = points[i].X;
            ln.Y1 = points[i].Y;
            ln.X2 = points[v].X;
            ln.Y2 = points[v].Y;
            ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
            MainCanvas.Children.Add(ln);
        }

        private void AddPointClicked(object sender, RoutedEventArgs e)
        {
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i)
        {
            int v = i + 1;
            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, height);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, height);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
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
            ln.StrokeDashArray.Add(0.2);
            ln.StrokeDashArray.Add(0.2);

            ln.Fill = br;
            ln.X1 = x1;
            ln.Y1 = y1;
            ln.X2 = x2;
            ln.Y2 = y2;

            MainCanvas.Children.Add(ln);
        }

        private void DeletePointClicked(object sender, RoutedEventArgs e)
        {
        }

        private void DisplayLines()
        {
            if (points != null && points.Count > 2)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    AddLine(i, i + 1);
                }
            }
        }

        private void DisplayPoints()
        {
            if (points != null)
            {
                double ox = double.NaN;
                double oy = double.NaN;
                if (selectedPoint != -1)
                {
                    ox = points[selectedPoint].X;
                    oy = points[selectedPoint].Y;
                }

                double rad = 3;
                System.Windows.Media.Brush br = null;
                for (int i = 0; i < points.Count; i++)
                {
                    System.Windows.Point p = points[i];
                    if (selectedPoint == i)
                    {
                        rad = 6;
                        br = System.Windows.Media.Brushes.Green;
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
                            }
                        }
                    }

                    Ellipse el = new Ellipse();

                    Canvas.SetLeft(el, p.X - rad);
                    Canvas.SetTop(el, p.Y - rad);
                    el.Width = 2 * rad;
                    el.Height = 2 * rad;
                    el.Stroke = br;
                    el.Fill = br;
                    el.MouseDown += MainCanvas_MouseDown;
                    el.MouseMove += MainCanvas_MouseMove;
                    el.ContextMenu = PointMenu(el);
                    MainCanvas.Children.Add(el);

                    if (selectedPoint == i && showOrtho)
                    {
                        DashLine(p.X, 0, p.X, MainCanvas.ActualHeight - 1);
                        DashLine(0, p.Y, MainCanvas.ActualWidth - 1, p.Y);
                    }
                }
            }
        }

        private void GenerateFaces()
        {
            if (SolidShape)
            {
                GenerateSolid();
            }
            else
            {
                GenerateHollow();
            }
        }

        private void GenerateHollow()
        {
            List<PointF> outerPolygon = new List<PointF>();
            List<PointF> innerPolygon = new List<PointF>();
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            double top = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Y > top)
                {
                    top = points[i].Y;
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (localImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                else
                {
                    double x = ToMM(points[i].X);
                    double y = ToMM(top - points[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                outerPolygon.Add(new PointF((float)tmp[i].X, (float)tmp[i].Y));
                innerPolygon.Add(new PointF((float)tmp[i].X, (float)tmp[i].Y));
            }
            outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);
            innerPolygon = LineUtils.RemoveCoplanarSegments(innerPolygon);
            outerPolygon = LineUtils.GetEnlargedPolygon(outerPolygon, (float)wallWidth / 2.0F);
            innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallWidth / 2.0F);
            tmp.Clear();
            for (int i = outerPolygon.Count - 1; i >= 0; i--)
            {
                tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }

            tmp.Clear();
            for (int i = 0; i < innerPolygon.Count; i++)
            {
                tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }

            for (int i = 0; i < outerPolygon.Count; i++)
            {
                int j = i + 1;
                if (j == outerPolygon.Count)
                {
                    j = 0;
                }
                int c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                int c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                int c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                int c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                Faces.Add(c0);
                Faces.Add(c3);
                Faces.Add(c2);

                c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, height);
                c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, height);
                c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, height);
                c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, height);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);

                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);
            }

            CentreVertices();
        }

        private void GeneratePointParams()
        {
            String s = "";
            for (int i = 0; i < points.Count; i++)
            {
                s += points[i].X.ToString() + "," + points[i].Y.ToString();
                if (i < points.Count - 1)
                {
                    s += ",";
                }
            }
            EditorParameters.Set("Points", s);
        }

        private void GenerateSolid()
        {
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            double top = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Y > top)
                {
                    top = points[i].Y;
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (localImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                else
                {
                    double x = ToMM(points[i].X);
                    double y = ToMM(top - points[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }

            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in tmp)
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
            CentreVertices();
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
            points.Clear();
            points.Add(new System.Windows.Point(10, 10));
            points.Add(new System.Windows.Point(100, 10));
            points.Add(new System.Windows.Point(100, 100));
            points.Add(new System.Windows.Point(10, 100));
        }

        private ContextMenu LineMenu()
        {
            ContextMenu mn = new ContextMenu();
            MenuItem mni = new MenuItem();
            mni.Header = "Add Point";
            mni.Click += AddPointClicked;
            mn.Items.Add(mni);
            return mn;
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line ln = sender as Line;
            int found = -1;
            if (ln != null)
            {
                System.Windows.Point position = e.GetPosition(MainCanvas);
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].X == ln.X1 && points[i].Y == ln.Y1)
                    {
                        found = i;
                        break;
                    }
                }
                if (found != -1)
                {
                    if (found < points.Count - 1)
                    {
                        points.Insert(found + 1, position);
                    }
                    else
                    {
                        points.Add(position);
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
            MainCanvas.Width = localImage.Width;
            MainCanvas.Height = localImage.Height;
            UpdateDisplay();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
            System.Windows.Point position = e.GetPosition(MainCanvas);

            double rad = 3;

            for (int i = 0; i < points.Count; i++)
            {
                System.Windows.Point p = points[i];
                if (position.X >= p.X - rad && position.X <= p.X + rad)
                {
                    if (position.Y >= p.Y - rad && position.Y <= p.Y + rad)
                    {
                        selectedPoint = i;

                        break;
                    }
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateDisplay();
            }
            else
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (sender is Ellipse)
                {
                    Ellipse el = sender as Ellipse;
                    if (points.Count > 3)
                    {
                        MessageBoxResult res = MessageBox.Show("Delete the point", "Edit", MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.Yes)
                        {
                            points.RemoveAt(selectedPoint);
                            UpdateDisplay();
                        }
                    }
                }
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            PositionLabel.Content = $"({position.X},{position.Y})";
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                points[selectedPoint] = position;
                GenerateFaces();
            }
            else
            {
                //selectedPoint = -1;
            }

            UpdateDisplay();
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void MainCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedPoint != -1)
            {
                bool shift = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
                double d = 1;
                if (shift)
                {
                    d = 0.1;
                }
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.L:
                        {
                            points[selectedPoint] += new Vector(-d, 0);
                        }
                        break;

                    case Key.Right:
                    case Key.R:
                        {
                            points[selectedPoint] += new Vector(d, 0);
                        }
                        break;

                    case Key.U:
                    case Key.Up:
                        {
                            points[selectedPoint] += new Vector(0, -d);
                        }
                        break;

                    case Key.D:
                    case Key.Down:
                        {
                            points[selectedPoint] += new Vector(0, d);
                        }
                        break;
                }
                GenerateFaces();
                UpdateDisplay();
            }
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private ContextMenu PointMenu(object tag)
        {
            ContextMenu mn = new ContextMenu();
            MenuItem mni = new MenuItem();
            mni.Header = "Delete Point";
            mni.Click += DeletePointClicked;
            mni.Tag = tag;
            mn.Items.Add(mni);
            return mn;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            GenerateFaces();
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
                Image image = new System.Windows.Controls.Image();
                image.Source = localImage;
                image.Width = localImage.Width;
                image.Height = localImage.Height;
                MainCanvas.Children.Add(image);
            }
            DisplayLines();
            DisplayPoints();
            GenerateFaces();
            Redisplay();
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
                points.Clear();
                for (int i = 0; i < words.GetLength(0); i += 2)
                {
                    points.Add(new System.Windows.Point(Convert.ToDouble(words[i]), Convert.ToDouble(words[i + 1])));
                }
            }
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateFaces();
            UpdateDisplay();
        }
    }
}