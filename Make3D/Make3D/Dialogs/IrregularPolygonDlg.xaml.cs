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
    public partial class IrregularPolygonDlg : BaseModellerDialog
    {
        private double height = 10;
        private BitmapImage localImage;
        private List<System.Windows.Point> points;
        private double scale;
        private int selectedPoint;
        private WriteableBitmap wbx;

        public IrregularPolygonDlg()
        {
            InitializeComponent();
            points = new List<System.Windows.Point>();

            selectedPoint = -1;
            scale = 1.0;
            InitialisePoints();

            EditorParameters.ToolName = "IrregularPolygon";
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
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
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

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
                Image image = new System.Windows.Controls.Image();
                image.Source = localImage;
                image.Width = localImage.Width;
                image.Height = localImage.Height;
                MainCanvas.Children.Add(image);
            }
            DisplayLines();
            DisplayPoints();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainCanvas.ContextMenu = LineMenu();
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
            UpdateDisplay();
        }
    }
}