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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for IrregularPolygonDlg.xaml
    /// </summary>
    public partial class IrregularPolygonDlg : Window
    {
        private List<System.Windows.Point> points;
        private int selectedPoint;
        private int selectedLine = -1;
        private double height = 10;
        private BitmapImage localImage;
        private WriteableBitmap wbx;
        private Point3DCollection vertices;

        public Point3DCollection Vertices
        {
            get
            {
                return vertices;
            }
        }

        private Int32Collection triangles;

        public Int32Collection Triangles
        {
            get
            {
                return triangles;
            }
        }

        public IrregularPolygonDlg()
        {
            InitializeComponent();
            points = new List<System.Windows.Point>();
            vertices = new Point3DCollection();
            triangles = new Int32Collection();
            selectedPoint = -1;
            InitialisePoints();
        }

        private void LoadImage(string f)
        {
            Uri fileUri = new Uri(f);
            localImage = new BitmapImage();
            localImage.BeginInit();
            localImage.UriSource = fileUri;
            //    localImage.DecodePixelWidth = 800;
            localImage.EndInit();
            UpdateDisplay();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double rad = 3;
                System.Windows.Point position = e.GetPosition(MainCanvas);
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
            }
            UpdateDisplay();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point position = e.GetPosition(MainCanvas);

                points[selectedPoint] = position;
            }
            else
            {
                selectedPoint = -1;
            }
            UpdateDisplay();
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

        private void AddPointClicked(object sender, RoutedEventArgs e)
        {
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void InitialisePoints()
        {
            points.Clear();
            points.Add(new System.Windows.Point(10, 10));
            points.Add(new System.Windows.Point(100, 10));
            points.Add(new System.Windows.Point(100, 100));
            points.Add(new System.Windows.Point(10, 100));
        }

        private void DisplayLines()
        {
            if (points != null && points.Count > 2)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    AddLine(i, i + 1);
                }
                /*
                 Polyline pl = new Polyline();
                 pl.Points = new PointCollection();

                 MainCanvas.Children.Add(pl);
                 foreach (Point p in points)
                 {
                     pl.Points.Add(new Point(p.X, p.Y));
                 }
                 pl.Fill = Brushes.CadetBlue;
                 */
            }
        }

        private void AddLine(int i, int v)
        {
            if (v >= points.Count)
            {
                v = 0;
            }

            Line ln = new Line();
            ln.Stroke = System.Windows.Media.Brushes.CadetBlue;
            ln.StrokeThickness = 6;
            ln.Fill = System.Windows.Media.Brushes.CadetBlue;
            ln.X1 = points[i].X;
            ln.Y1 = points[i].Y;
            ln.X2 = points[v].X;
            ln.Y2 = points[v].Y;
            ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
            MainCanvas.Children.Add(ln);
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

        private void DisplayPoints()
        {
            if (points != null)
            {
                double rad = 3;
                for (int i = 0; i < points.Count; i++)
                {
                    if (selectedPoint == i)
                    {
                        rad = 6;
                    }
                    else
                    {
                        rad = 3;
                    }
                    System.Windows.Point p = points[i];
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.ContextMenu = LineMenu();
            UpdateDisplay();
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
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            UpdateDisplay();
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateFaces();
            DialogResult = true;
            Close();
        }

        private void GenerateFaces()
        {
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
            points = tmp;
            // generate side triangles so original points are already in list
            for (int i = 0; i < points.Count; i++)
            {
                CreateSideFace(i);
            }
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in points)
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
                triangles.Add(c0);
                triangles.Add(c2);
                triangles.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, height);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, height);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, height);
                triangles.Add(c0);
                triangles.Add(c1);
                triangles.Add(c2);
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

        private void CreateSideFace(int i)
        {
            int v = i + 1;
            if (v == points.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(points[i].X, points[i].Y, 0.0);
            int c1 = AddVertice(points[i].X, points[i].Y, height);
            int c2 = AddVertice(points[v].X, points[v].Y, height);
            int c3 = AddVertice(points[v].X, points[v].Y, 0.0);
            triangles.Add(c0);
            triangles.Add(c2);
            triangles.Add(c1);

            triangles.Add(c0);
            triangles.Add(c3);
            triangles.Add(c2);
        }

        private int AddVertice(double x, double y, double z)
        {
            int res = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (equals(x, vertices[i].X) && equals(y, vertices[i].Y) && equals(z, vertices[i].Z))
                {
                    res = i;
                    break;
                }
            }

            if (res == -1)
            {
                vertices.Add(new Point3D(x, y, z));
                res = vertices.Count - 1;
            }
            return res;
        }

        private bool equals(double v1, double v2)
        {
            if (Math.Abs(v1 - v2) < 0.0000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}