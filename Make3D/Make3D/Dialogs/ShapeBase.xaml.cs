using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ShapeBase : UserControl
    {
        public delegate void PointsChanged(List<Point> pnts);

        public PointsChanged OnPointsChanged;
        public string Header { get; set; }
        public int NumberOfPoints { get; set; }

        public List<Point> Points;
        private double cx;
        private double cy;
        private double maxRadius;

        private List<double> pointAngles;
        private List<double> pointDistance;

        private double rotation;
        private double rotationDegrees;
        public double RotationDegrees
        {

            get
            {
                return rotationDegrees;
            }
            set
            {
                rotationDegrees = value;
            }
        }
        private double maxRotation;
        private int selectedPoint;
        private bool snapPoint;
        private bool linkPoints;

        public ShapeBase()
        {
            InitializeComponent();
            Header = "";
            NumberOfPoints = 4;
            maxRadius = 1;
            OnPointsChanged = null;
            rotation = 0;
            rotationDegrees = 0;
            snapPoint = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderLabel.Content = Header;
            NumberOfPointsBox.MinimumValue = 3;
            NumberOfPointsBox.MaximumValue = 30;
            NumberOfPointsBox.StartValue = NumberOfPoints;
            NumberOfPointsBox.OnValueChanged += OnNumberOfPointsChanged;
            maxRadius = PointCanvas.ActualWidth / 2;
            if (PointCanvas.ActualHeight / 2 < maxRadius)
            {
                maxRadius = PointCanvas.ActualHeight / 2;
            }

            cx = PointCanvas.ActualWidth / 2.0;
            cy = PointCanvas.ActualHeight / 2.0;
            GenerateDefaultPoints();
            Redraw();
        }

        private void OnNumberOfPointsChanged(double i)
        {
            if (i >= NumberOfPointsBox.MinimumValue && i <= NumberOfPointsBox.MaximumValue)
            {
                rotation = 0;
                NumberOfPoints = (int)i;
                GenerateDefaultPoints();
                Redraw();
            }
        }

        private void Redraw()
        {
            maxRadius = PointCanvas.ActualWidth / 2;
            if (PointCanvas.ActualHeight / 2 < maxRadius)
            {
                maxRadius = PointCanvas.ActualHeight / 2;
            }
            PointCanvas.Children.Clear();

            DisplayLines();
            DisplayPoints();

            double rad = 4;

            cx = PointCanvas.ActualWidth / 2.0;
            cy = PointCanvas.ActualHeight / 2.0;

            Ellipse el = new Ellipse();
            Canvas.SetLeft(el, cx - rad);
            Canvas.SetTop(el, cy - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Fill = Brushes.Black;

            PointCanvas.Children.Add(el);
            if (OnPointsChanged != null)
            {
                OnPointsChanged(Points);
            }
        }

        private void DisplayLines()
        {
            if (Points != null && Points.Count > 0)
            {
                Polyline pl = new Polyline();
                pl.Points = new PointCollection();

                PointCanvas.Children.Add(pl);
                foreach (Point p in Points)
                {
                    pl.Points.Add(new Point(cx + (p.X * maxRadius), cy + (p.Y * maxRadius)));
                }
                pl.Fill = Brushes.CadetBlue;
            }
        }

        public void GenerateDefaultPoints()
        {
            Points = new List<Point>();
            pointAngles = new List<double>();
            pointDistance = new List<double>();
            double dTheta = (Math.PI * 2) / NumberOfPoints;
            maxRotation = dTheta / 2;
            for (double theta = 0; theta < (Math.PI * 2); theta += dTheta)
            {
                pointAngles.Add(theta);
                pointDistance.Add(0.5);
                double x = 0.5 * Math.Cos(theta);
                double y = 0.5 * Math.Sin(theta);
                Point p = new Point(x, y);
                Points.Add(p);
            }
        }

        private void GeneratePoints()
        {
            Points.Clear();
            for (int i = 0; i < pointAngles.Count; i++)
            {
                double theta = pointAngles[i];
                double x = pointDistance[i] * Math.Cos(theta);
                double y = pointDistance[i] * Math.Sin(theta);
                Point p = new Point(x, y);
                Points.Add(p);
            }
        }

        private void DisplayPoints()
        {
            if (Points != null)
            {
                double rad = 3;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (selectedPoint == i)
                    {
                        rad = 6;
                    }
                    else
                    {
                        rad = 3;
                    }
                    Point p = Points[i];
                    Ellipse el = new Ellipse();
                    double x = cx + (p.X * maxRadius);
                    double y = cy + (p.Y * maxRadius);
                    Canvas.SetLeft(el, x - rad);
                    Canvas.SetTop(el, y - rad);
                    el.Width = 2 * rad;
                    el.Height = 2 * rad;
                    el.Fill = Brushes.Red;

                    PointCanvas.Children.Add(el);
                }
            }
        }

        private void NumberOfPointsBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RotateMinus_Click(object sender, RoutedEventArgs e)
        {
            RotatePoints(-1);
            GeneratePoints();
            Redraw();
        }

        private void RotatePoints(double v)
        {
            if (v != 0)
            {
                double d = v / 180.0 * Math.PI;
                if (Math.Abs(rotation + d) < maxRotation)
                {
                    for (int i = 0; i < pointAngles.Count; i++)
                    {
                        pointAngles[i] += d;
                    }
                    rotation += d;
                    rotationDegrees += v;
                }
            }
        }

        private void RotatePlus_Click_1(object sender, RoutedEventArgs e)
        {
            RotatePoints(1);
            GeneratePoints();
            Redraw();
        }

        private void RotateZero_Click(object sender, RoutedEventArgs e)
        {
            RotatePoints(-rotationDegrees);
            GeneratePoints();
            Redraw();
        }

        private void PointCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
            DistanceLabel.Content = "";
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double rad = 3;
                Point position = e.GetPosition(PointCanvas);
                for (int i = 0; i < Points.Count; i++)
                {
                    Point p = Points[i];
                    double x = cx + (p.X * maxRadius);
                    double y = cy + (p.Y * maxRadius);
                    if (position.X >= x - rad && position.X <= x + rad)
                    {
                        if (position.Y >= y - rad && position.Y <= y + rad)
                        {
                            selectedPoint = i;
                            DistanceLabel.Content = pointDistance[i].ToString("F3");
                            break;
                        }
                    }
                }
            }
        }

        internal string GetDistances()
        {
            string res = "";
            for (int i = 0; i < pointAngles.Count; i++)
            {
                res += pointDistance[i].ToString();
                res += ",";
            }
            return res;
        }

        internal string GetEditorParameters()
        {
            string res = "";

            return res;
        }

        private void PointCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(PointCanvas);
                double dist = Math.Sqrt((cx - position.X) * (cx - position.X) +
                                        (cy - position.Y) * (cy - position.Y));
                dist = dist / maxRadius;
                if (dist <= 1 && dist >= 0.1)
                {
                    if (snapPoint)
                    {
                        dist = 10 * dist;
                        int d = (int)(Math.Round(dist));
                        dist = (double)d / 10;
                    }
                    if (!linkPoints)
                    {
                        pointDistance[selectedPoint] = dist;
                    }
                    else
                    {
                        for (int i = 0; i < Points.Count; i++)
                        {
                            pointDistance[i] = dist;
                        }
                    }
                    DistanceLabel.Content = pointDistance[selectedPoint].ToString("F3");
                    GeneratePoints();
                    Redraw();
                }
            }
            else
            {
                selectedPoint = -1;
            }
        }

        private void PointCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
            DistanceLabel.Content = "";
        }

        private void PointCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            selectedPoint = -1;
            DistanceLabel.Content = "";
        }

        private void PointCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            snapPoint = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            snapPoint = false;
        }

        private void LinkBox_Checked(object sender, RoutedEventArgs e)
        {
            linkPoints = true;
        }

        private void LinkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            linkPoints = false;
        }
    }
}