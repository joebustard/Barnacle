using Make3D.Models;
using Make3D.Object3DLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Make3D.LineLib;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class BezierRingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        public List<Point> ControlPoints;
        public List<Point> Points;
        private const int padding = 40;

        private static SolidColorBrush[] pointColours =
 {
Brushes.Blue,
Brushes.Brown,
Brushes.Yellow,
Brushes.Green
};

        private double bezierDivisions;
        private BezierLine[] bzlines;
        private double cx;
        private double cy;
        private bool linkPoints;
        private double maxRadius;
        private List<double> pointAngles;
        private List<double> pointDistance;
        private double profileHeight;
        private double profileWidth;
        private double ringRadius;
        private int rotDivisions;
        private int selectedControlPoint;
        private int selectedPoint;
        private bool snapPoint;

        private double sweepDegrees;

        public BezierRingDlg()
        {
            InitializeComponent();
            DataContext = this;

            ToolName = "BezierRing";
            Points = new List<Point>();
            ControlPoints = new List<Point>();
            pointAngles = new List<double>();
            pointDistance = new List<double>();
            bzlines = new BezierLine[4];
            snapPoint = false;
            linkPoints = false;
            GenerateDefaultPoints();
            ringRadius = 10;
            profileHeight = 10;
            profileWidth = 10;
            sweepDegrees = 360;
            rotDivisions = 19;
            bezierDivisions = 40;
            ModelGroup = MyModelGroup;
        }

        public double BezierDivisions
        {
            get
            {
                return bezierDivisions;
            }
            set
            {
                bezierDivisions = value;
                NotifyPropertyChanged();
                GenerateRing();
                Redisplay();
            }
        }

        public BezierLine[] BzLines
        {
            get { return bzlines; }
        }

        public List<double> PointAngles
        {
            get { return pointAngles; }
            set { pointAngles = value; }
        }

        public List<double> PointDistance
        {
            get { return pointDistance; }
            set { pointDistance = value; }
        }

        public int RotDivisions
        {
            get
            {
                return rotDivisions;
            }
            set
            {
                rotDivisions = value;
                NotifyPropertyChanged();
                GenerateRing();
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

        public double SweepDegrees
        {
            get
            {
                return sweepDegrees;
            }
            set
            {
                sweepDegrees = value;
                NotifyPropertyChanged();
                GenerateRing();
                Redisplay();
            }
        }

        public void GenerateDefaultPoints()
        {
            Points.Clear();
            ControlPoints.Clear();
            pointAngles.Clear();
            pointDistance.Clear();

            Points.Clear();
            bzlines = new BezierLine[4];
            bzlines[0] = new BezierLine();
            BzLines[0].SetPoints(1.0, 0.0,
                                1.0, 0.5,
                                 .5, 1,
                                 0, 1.0);
            Points.Add(new Point(1.0, 0));
            pointDistance.Add(1);
            ControlPoints.Add(bzlines[0].P1);
            ControlPoints.Add(bzlines[0].P2);

            bzlines[1] = new BezierLine();
            BzLines[1].SetPoints(0, 1.0,
                                 -0.5, 1.0,
                                -1, 0.5,
                                 -1.0, 0);
            Points.Add(new Point(0, 1));
            pointDistance.Add(1);
            ControlPoints.Add(bzlines[1].P1);
            ControlPoints.Add(bzlines[1].P2);

            bzlines[2] = new BezierLine();
            BzLines[2].SetPoints(-1.0, 0,
                                 -1, -0.5,
                                -.5, -1,
                                 0, -1.0);
            Points.Add(new Point(-1.0, 0));
            pointDistance.Add(1);
            ControlPoints.Add(bzlines[2].P1);
            ControlPoints.Add(bzlines[2].P2);

            bzlines[3] = new BezierLine();
            BzLines[3].SetPoints(0, -1.0,
                                 0.5, -1,
                                1, -0.5,
                                 1.0, 0);
            Points.Add(new Point(0, -1));
            pointDistance.Add(1);
            ControlPoints.Add(bzlines[3].P1);
            ControlPoints.Add(bzlines[3].P2);

            selectedControlPoint = -1;
            selectedPoint = -1;
        }

        public void GeneratePoints()
        {
            Points[0] = DistPoint(Points[0], pointDistance[0]);
            Points[1] = DistPoint(Points[1], pointDistance[1]);
            Points[2] = DistPoint(Points[2], pointDistance[2]);
            Points[3] = DistPoint(Points[3], pointDistance[3]);

            bzlines[0].Move(Points[0], Points[1]);
            bzlines[1].Move(Points[1], Points[2]);
            bzlines[2].Move(Points[2], Points[3]);
            bzlines[3].Move(Points[3], Points[0]);
            ControlPoints.Clear();
            ControlPoints.Add(bzlines[0].P1);
            ControlPoints.Add(bzlines[0].P2);

            ControlPoints.Add(bzlines[1].P1);
            ControlPoints.Add(bzlines[1].P2);

            ControlPoints.Add(bzlines[2].P1);
            ControlPoints.Add(bzlines[2].P2);

            ControlPoints.Add(bzlines[3].P1);
            ControlPoints.Add(bzlines[3].P2);
        }

        public void Redraw()
        {
            cx = PointCanvas.ActualWidth / 2.0;
            cy = PointCanvas.ActualHeight / 2.0;

            maxRadius = PointCanvas.ActualWidth / 2;
            if ((PointCanvas.ActualHeight / 2) - padding < maxRadius)
            {
                maxRadius = (PointCanvas.ActualHeight / 2) - padding;
            }
            PointCanvas.Children.Clear();
            DisplayAxis();
            DisplayLines();
            DisplayPoints();

            DrawCentreMark();
        }

        private void AddCircle(double px, double py, double rad, SolidColorBrush br)
        {
            Ellipse el = new Ellipse();

            double x = cx + (px * maxRadius);
            double y = cy + (py * maxRadius);
            Canvas.SetLeft(el, x - rad);
            Canvas.SetTop(el, y - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Fill = br;
            el.Stroke = Brushes.Black;
            PointCanvas.Children.Add(el);
            el.MouseUp += El_MouseUp;
        }

        private void DisplayAxis()
        {
            DoubleCollection coll = new DoubleCollection() { 8, 1, 1, 1, 1, 1 };

            Line vl = new Line();
            vl.Stroke = Brushes.Blue;
            vl.StrokeDashArray = coll;
            vl.X1 = PointCanvas.ActualWidth / 2;
            vl.Y1 = 0;
            vl.X2 = PointCanvas.ActualWidth / 2;
            vl.Y2 = PointCanvas.ActualHeight;
            PointCanvas.Children.Add(vl);

            Line hl = new Line();
            hl.Stroke = Brushes.Blue;
            hl.StrokeDashArray = coll;
            hl.X1 = 0;
            hl.Y1 = PointCanvas.ActualHeight / 2;
            hl.X2 = PointCanvas.ActualWidth;
            hl.Y2 = PointCanvas.ActualHeight / 2;
            PointCanvas.Children.Add(hl);
        }

        private void DisplayLines()
        {
            if (Points != null && Points.Count > 0)
            {
                Polyline pl = new Polyline();
                pl.Points = new PointCollection();

                PointCanvas.Children.Add(pl);
                for (int i = 0; i < bzlines.GetLength(0); i++)
                {
                    for (double t = 0; t < 1; t += 0.01)
                    {
                        Point p = bzlines[i].GetCoord(t, false);
                        pl.Points.Add(new Point(cx + (p.X * maxRadius), cy + (p.Y * maxRadius)));
                    }
                }
                pl.Fill = Brushes.CadetBlue;
            }
        }

        private void DisplayPoints()
        {
            if (bzlines != null)
            {
                double rad = 3;
                int cp = 0;

                for (int i = 0; i < bzlines.GetLength(0); i++)
                {
                    if (selectedPoint == i)
                    {
                        rad = 9;
                    }
                    else
                    {
                        rad = 6;
                    }

                    AddCircle(bzlines[i].P0.X, bzlines[i].P0.Y, rad, pointColours[i]);

                    rad = 5;
                    if (cp == selectedControlPoint)
                    {
                        rad = 7;
                    }

                    AddCircle(bzlines[i].P1.X, bzlines[i].P1.Y, rad, pointColours[i]);
                    cp++;

                    rad = 5;
                    if (cp == selectedControlPoint)
                    {
                        rad = 7;
                    }

                    AddCircle(bzlines[i].P2.X, bzlines[i].P2.Y, rad, pointColours[i]);
                    cp++;
                }
            }
        }

        private Point DistPoint(Point point, double d)
        {
            double x = 0;
            double y = 0;
            if (point.X != 0)
            {
                x = Math.Sign(point.X) * d;
            }
            else
            {
                if (point.Y != 0)
                {
                    y = Math.Sign(point.Y) * d;
                }
            }

            return new Point(x, y);
        }

        private void DrawCentreMark()
        {
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
        }

        private void El_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
            selectedControlPoint = -1;
        }

        private void GenerateRing()
        {
            List<PolarCoordinate> polarProfile = new List<PolarCoordinate>();
            double hr = profileWidth / 2;
            double vr = profileHeight / 2;
            double cx = ringRadius;
            double cy = vr;
            double bstep = 4 / bezierDivisions;
            if (Points != null && Points.Count > 0)
            {
                for (int i = 0; i < bzlines.GetLength(0); i++)
                {
                    for (double t = 0; t < 1; t += bstep)
                    {
                        Point p = bzlines[i].GetCoord(t, false);
                        Point3D p3d = new Point3D(cx + hr * p.X, 0, cy - (vr * p.Y));
                        PolarCoordinate pcol = new PolarCoordinate(0, 0, 0);
                        pcol.SetPoint3D(p3d);
                        polarProfile.Add(pcol);
                    }
                }

                SweepPolarProfileTheta(polarProfile, cx, cy, sweepDegrees, rotDivisions);
            }
        }

        private void MoveSelected(MouseEventArgs e)
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

                GeneratePoints();
                GenerateRing();
                Redraw();
            }
        }

        private void MoveSelectedControlPoint(MouseEventArgs e)
        {
            Point p = e.GetPosition(PointCanvas);

            double x = (p.X - cx) / maxRadius;
            double y = (p.Y - cy) / maxRadius;

            switch (selectedControlPoint)
            {
                case 0:
                    {
                        if (x > 0 && y > 0)
                        {
                            bzlines[0].P1 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 1:
                    {
                        if (x > 0 && y > 0)
                        {
                            bzlines[0].P2 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 2:
                    {
                        if (x < 0 && y > 0)
                        {
                            bzlines[1].P1 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 3:
                    {
                        if (x < 0 && y > 0)
                        {
                            bzlines[1].P2 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 4:
                    {
                        if (x < 0 && y < 0)
                        {
                            bzlines[2].P1 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 5:
                    {
                        if (x < 0 && y < 0)
                        {
                            bzlines[2].P2 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 6:
                    {
                        if (x > 0 && y < 0)
                        {
                            bzlines[3].P1 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;

                case 7:
                    {
                        if (x > 0 && y < 0)
                        {
                            bzlines[3].P2 = new Point(x, y);
                            ControlPoints[selectedControlPoint] = new Point(x, y);
                        }
                    }
                    break;
            }
            GenerateRing();
            Redraw();
        }

        private void PointCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
            //    DistanceLabel.Content = "";
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
                            break;
                        }
                    }
                }
                if (selectedPoint == -1)
                {
                    for (int i = 0; i < ControlPoints.Count; i++)
                    {
                        Point p = ControlPoints[i];
                        double x = cx + (p.X * maxRadius);
                        double y = cy + (p.Y * maxRadius);
                        if (position.X >= x - rad && position.X <= x + rad)
                        {
                            if (position.Y >= y - rad && position.Y <= y + rad)
                            {
                                selectedControlPoint = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void PointCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            selectedPoint = -1;
        }

        private void PointCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
            {
                MoveSelected(e);
            }
            else
            {
                selectedPoint = -1;
                if (selectedControlPoint != -1 && e.LeftButton == MouseButtonState.Pressed)
                {
                    MoveSelectedControlPoint(e);
                }
            }
        }

        private void PointCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedPoint = -1;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyModelGroup.Children.Clear();
            maxRadius = (PointCanvas.ActualWidth / 2) - padding;
            if (PointCanvas.ActualHeight / 2 < maxRadius)
            {
                maxRadius = (PointCanvas.ActualHeight / 2) - padding;
            }
            GenerateRing();
            Redraw();
            Redisplay();
            UpdateCameraPos();
        }
    }
}