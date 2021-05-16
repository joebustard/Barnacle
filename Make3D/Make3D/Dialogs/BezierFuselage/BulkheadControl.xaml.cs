using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for BulkheadControl.xaml
    /// </summary>
    public partial class BulkheadControl : UserControl
    {
        public delegate void PerformAction(int id, string cmd);

        public PerformAction OnPerformAction;

        public delegate void PointsChanged(List<Point> pnts);

        public PointsChanged OnPointsChanged;
        public int IdNumber { get; set; }
        public int NumberOfPoints { get; set; }
        public FuselageBulkhead FuselageBulkHead { get; set; }
        public List<Point> Points;
        public List<Point> ControlPoints;
        private double cx;
        private double cy;
        private double maxRadius;

        private List<double> pointAngles;

        public List<double> PointAngles
        {
            get { return pointAngles; }
            set { pointAngles = value; }
        }

        private List<double> pointDistance;

        public List<double> PointDistance
        {
            get { return pointDistance; }
            set { pointDistance = value; }
        }

        private double rotationDegrees;
        private double maxRotation;
        private int selectedPoint;
        private int selectedControlPoint;
        private bool snapPoint;
        private bool linkPoints=false;
        private BezierLine[] bzlines;

        public BezierLine[] BzLines
        {
            get { return bzlines; }
        }

        public BulkheadControl()
        {
            InitializeComponent();
            Width = 240;
            Height = 240;
            NumberOfPoints = 4;
            maxRadius = 1;
            OnPointsChanged = null;
            Points = new List<Point>();
            ControlPoints = new List<Point>();
            pointAngles = new List<double>();
            pointDistance = new List<double>();
            bzlines = new BezierLine[4];
            snapPoint = false;
            GenerateDefaultPoints();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            maxRadius = PointCanvas.ActualWidth / 2;
            if (PointCanvas.ActualHeight / 2 < maxRadius)
            {
                maxRadius = PointCanvas.ActualHeight / 2;
            }
            Redraw();
            Notify("Refresh");
        }

        public void Redraw()
        {
            cx = PointCanvas.ActualWidth / 2.0;
            cy = PointCanvas.ActualHeight / 2.0;
            PositionX.Text = FuselageBulkHead.OffsetX.ToString();
            PositionY.Text = FuselageBulkHead.OffsetY.ToString();
            PositionZ.Text = FuselageBulkHead.OffsetZ.ToString();
            Number.Content = IdNumber.ToString();
            maxRadius = PointCanvas.ActualWidth / 2;
            if (PointCanvas.ActualHeight / 2 < maxRadius)
            {
                maxRadius = PointCanvas.ActualHeight / 2;
            }
            PointCanvas.Children.Clear();
            DisplayAxis();
            DisplayLines();
            DisplayPoints();

            DrawCentreMark();
            if (OnPointsChanged != null)
            {
                OnPointsChanged(Points);
            }
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

        internal double GetDistance()
        {
            return FuselageBulkHead.OffsetX;
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

        internal void Set(List<Point> pnts, List<Point> ctrls, List<double> dists)
        {
            Points.Clear();
            for (int i = 0; i < pnts.Count; i++)
            {
                Points.Add(new Point(pnts[i].X, pnts[i].Y));
            }
            ControlPoints.Clear();
            for (int i = 0; i < ctrls.Count; i++)
            {
                ControlPoints.Add(new Point(ctrls[i].X, ctrls[i].Y));
            }
            pointDistance.Clear();
            for (int i = 0; i < dists.Count; i++)
            {
                double d = dists[i];
                pointDistance.Add(d);
            }
            if (bzlines[0] == null)
            {
                bzlines[0] = new BezierLine();
                bzlines[1] = new BezierLine();
                bzlines[2] = new BezierLine();
                bzlines[3] = new BezierLine();
            }
            bzlines[0].Move(Points[0], Points[1]);
            bzlines[1].Move(Points[1], Points[2]);
            bzlines[2].Move(Points[2], Points[3]);
            bzlines[3].Move(Points[3], Points[0]);

            bzlines[0].P1 = ControlPoints[0];
            bzlines[0].P2 = ControlPoints[1];

            bzlines[1].P1 = ControlPoints[2];
            bzlines[1].P2 = ControlPoints[3];

            bzlines[2].P1 = ControlPoints[4];
            bzlines[2].P2 = ControlPoints[5];

            bzlines[3].P1 = ControlPoints[6];
            bzlines[3].P2 = ControlPoints[7];
            selectedControlPoint = -1;
            selectedPoint = -1;
            Redraw();
            Notify("Refresh");
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

        private static SolidColorBrush[] pointColours =
        {
Brushes.Blue,
Brushes.Brown,
Brushes.Yellow,
Brushes.Green
};

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
                    double dx = bzlines[i].P0.X * FuselageBulkHead.Depth;
                    double dy = bzlines[i].P0.Y * FuselageBulkHead.Height;
                    string s = $"{dx},{dy}";

                    AddCircle(bzlines[i].P0.X, bzlines[i].P0.Y, rad, pointColours[i], s);

                    rad = 5;
                    if (cp == selectedControlPoint)
                    {
                        rad = 7;
                    }
                    dx = bzlines[i].P1.X * FuselageBulkHead.Depth;
                    dy = bzlines[i].P1.Y * FuselageBulkHead.Height;
                    s = $"{dx},{dy}";
                    AddCircle(bzlines[i].P1.X, bzlines[i].P1.Y, rad, pointColours[i], s);
                    cp++;

                    rad = 5;
                    if (cp == selectedControlPoint)
                    {
                        rad = 7;
                    }
                    dx = bzlines[i].P2.X * FuselageBulkHead.Depth;
                    dy = bzlines[i].P2.Y * FuselageBulkHead.Height;
                    s = $"{dx},{dy}";
                    AddCircle(bzlines[i].P2.X, bzlines[i].P2.Y, rad, pointColours[i], s);
                    cp++;
                }
            }
        }

        private void AddCircle(double px, double py, double rad, SolidColorBrush br, string s)
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
            if (s != "")
            {
                el.ToolTip = s;
            }
        }

        private void El_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Notify("BulkheadFocused");
            selectedPoint = -1;
            selectedControlPoint = -1;
            Notify("Refresh");
            Notify("");
        }

        private void PointCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Notify("BulkheadFocused");
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
            Redraw();
        }

        internal void MoveToTop()
        {
            double offset = 1.0 - Points[1].Y;
            FuselageBulkHead.OffsetY = -(offset * FuselageBulkHead.Height);
            Redraw();
        }

        private double GetDouble(XmlNode nd, string v)
        {
            double res = 0;
            if ((nd as XmlElement).HasAttribute(v))
            {
                res = Convert.ToDouble((nd as XmlElement).GetAttribute(v));
            }
            return res;
        }

        private int GetInt(XmlNode nd, string v)
        {
            int res = 0;
            if ((nd as XmlElement).HasAttribute(v))
            {
                res = Convert.ToInt32((nd as XmlElement).GetAttribute(v));
            }
            return res;
        }

        internal void Read(XmlDocument doc, XmlNode nd)
        {
            FuselageBulkhead fb = new FuselageBulkhead();
            IdNumber = GetInt(nd, "Id");
            fb.Depth = GetDouble(nd, "D");
            fb.Height = GetDouble(nd, "H");
            fb.OffsetX = GetDouble(nd, "X");
            fb.OffsetY = GetDouble(nd, "Y");
            fb.OffsetZ = GetDouble(nd, "Z");

            FuselageBulkHead = fb;

            List<Point> pnts = new List<Point>();
            XmlNodeList pntList = nd.SelectNodes("Point");
            foreach (XmlNode pn in pntList)
            {
                Point p = new Point(GetDouble(pn, "X"), GetDouble(pn, "Y"));
                pnts.Add(p);
            }

            List<Point> ctrls = new List<Point>();
            XmlNodeList ctrlList = nd.SelectNodes("Control");
            foreach (XmlNode pn in ctrlList)
            {
                Point p = new Point(GetDouble(pn, "X"), GetDouble(pn, "Y"));
                ctrls.Add(p);
            }

            List<double> dists = new List<double>();
            XmlNodeList dList = nd.SelectNodes("Distance");
            foreach (XmlNode pn in dList)
            {
                dists.Add(GetDouble(pn, "V"));
            }
            Points = new List<Point>();
            ControlPoints = new List<Point>();
            pointDistance = new List<double>();
            Set(pnts, ctrls, dists);
        }

        internal void Write(XmlDocument doc, XmlElement docNode)
        {
            XmlElement nd = doc.CreateElement("Bulkhead");
            nd.SetAttribute("Id", IdNumber.ToString());
            nd.SetAttribute("D", FuselageBulkHead.Depth.ToString());
            nd.SetAttribute("H", FuselageBulkHead.Height.ToString());
            nd.SetAttribute("X", FuselageBulkHead.OffsetX.ToString());
            nd.SetAttribute("Y", FuselageBulkHead.OffsetY.ToString());
            nd.SetAttribute("Z", FuselageBulkHead.OffsetZ.ToString());
            foreach (Point p in Points)
            {
                XmlElement pnt = doc.CreateElement("Point");
                pnt.SetAttribute("X", p.X.ToString());
                pnt.SetAttribute("Y", p.Y.ToString());
                nd.AppendChild(pnt);
            }
            foreach (Point p in ControlPoints)
            {
                XmlElement pnt = doc.CreateElement("Control");
                pnt.SetAttribute("X", p.X.ToString());
                pnt.SetAttribute("Y", p.Y.ToString());
                nd.AppendChild(pnt);
            }
            foreach (double d in pointDistance)
            {
                XmlElement dt = doc.CreateElement("Distance");
                dt.SetAttribute("V", d.ToString());

                nd.AppendChild(dt);
            }
            docNode.AppendChild(nd);
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
                Redraw();
            }
        }

        private void Notify(string s)
        {
            if (OnPerformAction != null)
            {
                OnPerformAction(IdNumber, s);
            }
        }

        private void PointCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Notify("BulkheadFocused");
            selectedPoint = -1;
            Notify("Refresh");
        }

        private void PointCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            selectedPoint = -1;
            Notify("Refresh"); //       DistanceLabel.Content = "";
        }

        private void PointCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void PositionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double pos = Convert.ToDouble(PositionX.Text);
                FuselageBulkHead.OffsetX = pos;
                Notify("Refresh");
            }
            catch
            {
            }
        }

        private void PositionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double pos = Convert.ToDouble(PositionY.Text);
                FuselageBulkHead.OffsetY = pos;
                SetBezierOffsets();
                Notify("Refresh");
            }
            catch
            {
            }
        }

        private void PositionZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double pos = Convert.ToDouble(PositionZ.Text);
                FuselageBulkHead.OffsetZ = pos;
                SetBezierOffsets();
                Notify("Refresh");
            }
            catch
            {
            }
        }

        private void SetBezierOffsets()
        {
            for (int i = 0; i < bzlines.GetLength(0); i++)
            {
                bzlines[i].SetOffset(FuselageBulkHead.OffsetZ / FuselageBulkHead.Depth, FuselageBulkHead.OffsetY / FuselageBulkHead.Height);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateDefaultPoints();
            Redraw();
            Notify("Refresh");
        }

        private void CopyToNextButton_Click(object sender, RoutedEventArgs e)
        {
            Notify("CopyToNext");
            Notify("Refresh");
        }

        private void ScaleMinus_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < pointDistance.Count; i++)
            {
                if (pointDistance[i] * 0.9 >= 0.01)
                {
                    pointDistance[i] *= 0.9;
                }
            }
            for (int i = 0; i < BzLines.Length; i++)

            {
                BzLines[i].P1 = ScalePoint(BzLines[i].P1, 0.9);
                BzLines[i].P2 = ScalePoint(BzLines[i].P2, 0.9);
            }

            GeneratePoints();
            Redraw();
        }

        private Point ScalePoint(Point p1, double v)
        {
            return new Point(p1.X * v, p1.Y * v);
        }

        private void ScalePlus_Click(object sender, RoutedEventArgs e)
        {
            bool canEnlarge = true;
            for (int i = 0; i < pointDistance.Count; i++)
            {
                if (pointDistance[i] * 1.1 >= 1)
                {
                    canEnlarge = false;
                }
            }
            if (canEnlarge)
            {
                for (int i = 0; i < pointDistance.Count; i++)
                {
                    pointDistance[i] *= 1.1;
                }
                for (int i = 0; i < BzLines.Length; i++)

                {
                    BzLines[i].P1 = ScalePoint(BzLines[i].P1, 1.1);
                    BzLines[i].P2 = ScalePoint(BzLines[i].P2, 1.1);
                }
            }
            GeneratePoints();
            Redraw();
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Notify("BulkheadFocused");
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        private void LeftRight_Click(object sender, RoutedEventArgs e)
        {
            pointDistance[0] = pointDistance[2];
            Points[0] = new Point(-Points[2].X, Points[2].Y);
            bzlines[0].P0 = new Point(-Points[2].X, Points[2].Y); ;
            bzlines[0].P2 = new Point(-bzlines[1].P1.X, bzlines[1].P1.Y);
            bzlines[0].P1 = new Point(-bzlines[1].P2.X, bzlines[1].P2.Y);
            ControlPoints[0] = new Point(-ControlPoints[3].X, ControlPoints[3].Y);
            ControlPoints[1] = new Point(-ControlPoints[2].X, ControlPoints[2].Y);

            bzlines[3].P2 = new Point(-bzlines[2].P1.X, bzlines[2].P1.Y);
            bzlines[3].P1 = new Point(-bzlines[2].P2.X, bzlines[2].P2.Y);
            bzlines[3].P3 = new Point(-Points[2].X, Points[2].Y);
            ControlPoints[6] = new Point(-ControlPoints[5].X, ControlPoints[5].Y);
            ControlPoints[7] = new Point(-ControlPoints[4].X, ControlPoints[4].Y);
            Redraw();
        }

        private void BottomUp_Click(object sender, RoutedEventArgs e)
        {
            pointDistance[3] = pointDistance[1];
            Points[3] = new Point(Points[1].X, -Points[1].Y);
            bzlines[3].P0 = new Point(Points[1].X, -Points[1].Y);

            bzlines[3].P2 = new Point(bzlines[0].P1.X, -bzlines[0].P1.Y);
            bzlines[3].P1 = new Point(bzlines[0].P2.X, -bzlines[0].P2.Y);
            bzlines[2].P3 = new Point(Points[1].X, -Points[1].Y);
            ControlPoints[6] = new Point(ControlPoints[1].X, -ControlPoints[1].Y);
            ControlPoints[7] = new Point(ControlPoints[0].X, -ControlPoints[0].Y);

            bzlines[2].P2 = new Point(bzlines[1].P1.X, -bzlines[1].P1.Y);
            bzlines[2].P1 = new Point(bzlines[1].P2.X, -bzlines[1].P2.Y);

            ControlPoints[4] = new Point(ControlPoints[3].X, -ControlPoints[3].Y);
            ControlPoints[5] = new Point(ControlPoints[2].X, -ControlPoints[2].Y);

            Redraw();
        }

        // This is specifically for saving as an editor parameters
        public String ToEditorParameters()
        {
            string res = "";

            res += "Id=" + IdNumber.ToString() + ",";
            res += "D=" + FuselageBulkHead.Depth.ToString() + ",";
            res += "H=" + FuselageBulkHead.Height.ToString() + ",";
            res += "X=" + FuselageBulkHead.OffsetX.ToString() + ",";
            res += "Y=" + FuselageBulkHead.OffsetY.ToString() + ",";
            res += "Z=" + FuselageBulkHead.OffsetZ.ToString() + ",";
            foreach (Point p in Points)
            {
                res += "P=" + p.X.ToString() + "!" + p.Y.ToString() + ",";
            }
            foreach (Point p in ControlPoints)
            {
                res += "C=" + p.X.ToString() + "!" + p.Y.ToString() + ",";
            }
            foreach (double d in pointDistance)
            {
                res += "V=" + d.ToString() + ",";
            }
            return res;
        }
    }
}