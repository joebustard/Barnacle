using Barnacle.Models;
using Microsoft.Win32;
using Barnacle.LineLib;

using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for IrregularPolygonDlg.xaml
    /// </summary>
    public partial class PlateletDlg : BaseModellerDialog
    {
        private FlexiPath flexiPath;
        private double height = 10;
        private bool hollowShape;
        private bool lineShape;
        private BitmapImage localImage;
        private bool moving;
        private string pathText;
        private ObservableCollection<FlexiPoint> polyPoints;
        private double scale;
        private int selectedPoint;
        private SelectionModeType selectionMode;
        private bool showOrtho;
        private Visibility showWidth;
        private bool solidShape;
        private int wallWidth;

        public PlateletDlg()
        {
            InitializeComponent();
            //polyPoints = new ObservableCollection<PolyPoint>();
            flexiPath = new FlexiPath();
            polyPoints = flexiPath.FlexiPoints;
            selectedPoint = -1;
            selectionMode = SelectionModeType.SelectPoint;
            scale = 1.0;
            wallWidth = 5;
            solidShape = true;
            hollowShape = false;
            lineShape = false;
            showOrtho = true;
            showWidth = Visibility.Hidden;

            InitialisePoints();

            EditorParameters.ToolName = "Platelet";
            DataContext = this;
            SolidShape = true;
            Camera.Distance = Camera.Distance * 3.0;
            ModelGroup = MyModelGroup;
            moving = false;
            SetButtonBorderColours();
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

        public bool LineShape
        {
            get
            {
                return lineShape;
            }
            set
            {
                if (lineShape != value)
                {
                    lineShape = value;
                    if (lineShape == true)
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

        public string PathText
        {
            get
            {
                return pathText;
            }
            set
            {
                if (pathText != value)
                {
                    pathText = value;
                    NotifyPropertyChanged();
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

                    //GetRawFlexiPoints();
                    PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
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
                PathText = flexiPath.ToPath();
            }

            return added;
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
                    // if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    // {
                    //      InsertCurveSegment(found, position);
                    //  }
                    //else
                    //      {
                    InsertLineSegment(found, position);
                    //       }

                    PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                else
                {
                    flexiPath.AddLine(position);
                    PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                added = true;
                PathText = flexiPath.ToPath();
            }

            return added;
        }

        private void AddPointClicked(object sender, RoutedEventArgs e)
        {
        }

        private void AddQuadBezierButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.AddQuadBezier;
        }

        private void AddSegButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.AddLine;
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

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            PathText = flexiPath.ToPath();
            Clipboard.SetText(PathText);
        }

        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
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

                    //GetRawFlexiPoints();
                    PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                else
                {
                    flexiPath.AddLine(position);
                    PointGrid.ItemsSource = Points;
                    CollectionViewSource.GetDefaultView(Points).Refresh();
                }
                added = true;
                PathText = flexiPath.ToPath();
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
                        AddLine(i, i + 1, points, !LineShape);
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

        private void GenerateFaces()
        {
            if (SolidShape)
            {
                GenerateSolid();
            }
            else
            {
                if (HollowShape)
                {
                    GenerateHollow();
                }
                else
                {
                    GenerateLine();
                }
            }
        }

        private void GenerateHollow()
        {
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
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
            // outerPolygon = LineUtils.GetEnlargedPolygon(outerPolygon, (float)wallWidth / 2.0F);
            innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallWidth);
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

        private void GenerateLine()
        {
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
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
            // outerPolygon = LineUtils.GetEnlargedPolygon(outerPolygon, (float)wallWidth / 2.0F);
            innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallWidth);
            System.Windows.Point opf = Perpendicular(new System.Windows.Point(outerPolygon[0].X, outerPolygon[0].Y), new System.Windows.Point(outerPolygon[1].X, outerPolygon[1].Y), 0, wallWidth);
            innerPolygon[0] = new PointF((float)opf.X, (float)opf.Y);
            opf = Perpendicular(new System.Windows.Point(outerPolygon[outerPolygon.Count - 2].X, outerPolygon[outerPolygon.Count - 2].Y), new System.Windows.Point(outerPolygon[outerPolygon.Count - 1].X, outerPolygon[outerPolygon.Count - 1].Y), 1, -wallWidth);
            innerPolygon[innerPolygon.Count - 1] = new PointF((float)opf.X, (float)opf.Y);
            //  outerPolygon = LineUtils.GetEnlargedPolygon(outerPolygon, (float)wallWidth / 2.0F);
            // innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallWidth / 2.0F);
            tmp.Clear();
            for (int i = outerPolygon.Count - 1; i >= 0; i--)
            {
                tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i, false);
            }

            tmp.Clear();
            for (int i = 0; i < innerPolygon.Count; i++)
            {
                tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i, false);
            }
            int c0, c1, c2, c3;
            for (int i = 0; i < outerPolygon.Count - 1; i++)
            {
                int j = i + 1;
                if (j == outerPolygon.Count)
                {
                    j = 0;
                }
                c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
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
            c0 = AddVertice(outerPolygon[0].X, outerPolygon[0].Y, 0.0);
            c1 = AddVertice(innerPolygon[0].X, innerPolygon[0].Y, 0.0);
            c2 = AddVertice(innerPolygon[0].X, innerPolygon[0].Y, height);
            c3 = AddVertice(outerPolygon[0].X, outerPolygon[0].Y, height);
            Faces.Add(c0);
            Faces.Add(c1);
            Faces.Add(c2);

            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c3);
            int l = outerPolygon.Count - 1;
            c0 = AddVertice(outerPolygon[l].X, outerPolygon[l].Y, 0.0);
            c1 = AddVertice(innerPolygon[l].X, innerPolygon[l].Y, 0.0);
            c2 = AddVertice(innerPolygon[l].X, innerPolygon[l].Y, height);
            c3 = AddVertice(outerPolygon[l].X, outerPolygon[l].Y, height);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
            CentreVertices();
        }

        private void GeneratePointParams()
        {
            String s = flexiPath.ToString();
            EditorParameters.Set("Points", s);
        }

        private void GenerateSolid()
        {
            ClearShape();
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
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
            flexiPath.Clear();
            flexiPath.Start = new FlexiPoint(new System.Windows.Point(10, 10), 0);
            flexiPath.AddLine(new System.Windows.Point(100, 10));
            flexiPath.AddLine(new System.Windows.Point(100, 100));
            flexiPath.AddLine(new System.Windows.Point(10, 100));
        }

        private void InsertCurveSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.ConvertLineCurveSegment(startIndex, position);
            PathText = flexiPath.ToPath();
        }

        private void InsertLineSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.InsertLineSegment(startIndex, position);
            PathText = flexiPath.ToPath();
        }

        private void InsertQuadCurveSegment(int startIndex, System.Windows.Point position)
        {
            flexiPath.ConvertLineQuadCurveSegment(startIndex, position);
            PathText = flexiPath.ToPath();
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
            EditorParameters.Set("ImagePath", f);
            MainCanvas.Width = localImage.Width;
            MainCanvas.Height = localImage.Height;
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
                // do this test here because the othe modes only trigger ifn you click a line
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
            catch 
            {
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            PositionLabel.Content = $"({position.X},{position.Y})";
            if (selectedPoint != -1)
            {
                if ( e.LeftButton == MouseButtonState.Pressed && moving)
                {
                    polyPoints[selectedPoint].X = position.X;
                    polyPoints[selectedPoint].Y = position.Y;
                    flexiPath.SetPointPos(selectedPoint, position);
                    PathText = flexiPath.ToPath();
                    GenerateFaces();
                    UpdateDisplay();
                }
                else
                {
                    moving = false;
                }
            }
            
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
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
                            polyPoints[selectedPoint].X -= d;
                        }
                        break;

                    case Key.Right:
                    case Key.R:
                        {
                            polyPoints[selectedPoint].X += d;
                        }
                        break;

                    case Key.U:
                    case Key.Up:
                        {
                            polyPoints[selectedPoint].Y -= d;
                        }
                        break;

                    case Key.D:
                    case Key.Down:
                        {
                            polyPoints[selectedPoint].Y += d;
                        }
                        break;
                }
                GenerateFaces();
                UpdateDisplay();
            }
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
            el.ContextMenu = PointMenu(el);
            MainCanvas.Children.Add(el);
            return p;
        }

        private System.Windows.Point MakeRect(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            Rectangle el = new Rectangle();

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
            myPolygon.ContextMenu = PointMenu(myPolygon);
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
            GenerateFaces();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        private System.Windows.Point Perpendicular(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double x;
            double y;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (Math.Abs(p1.X - p2.X) < 0.00001)
            {
                if (dy > 0)
                {
                    x = p1.X - distanceFromLine;
                }
                else
                {
                    x = p1.X + distanceFromLine;
                }
                y = p1.Y + t * (p2.Y - p1.Y);
            }
            else if (Math.Abs(p1.Y - p2.Y) < 0.00001)
            {
                x = p1.X + t * (p2.X - p1.X);
                if (dx > 0)
                {
                    y = p1.Y + distanceFromLine;
                }
                else
                {
                    y = p1.Y - distanceFromLine;
                }
            }
            else
            {
                double grad = dy / dx;
                double perp = -1.0 / grad;

                double sgn = Math.Sign(distanceFromLine);
                distanceFromLine = Math.Abs(distanceFromLine);
                System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
                x = tp.X + sgn * Math.Sqrt((distanceFromLine * distanceFromLine) / (1.0 + (1.0 / (grad * grad))));
                y = tp.Y + perp * (x - tp.X);
            }
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            selectionMode = SelectionModeType.SelectPoint;
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
            PathText = flexiPath.ToPath();
        }

        private void ResetPathButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();
            scale = 1;

            selectedPoint = -1;
            UpdateDisplay();
            PathText = flexiPath.ToPath();
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

        private void SetPointIds()
        {
            for (int i = 0; i < polyPoints.Count; i++)
            {
                polyPoints[i].Id = i + 1;
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
                flexiPath.FromString(s);
                //GetRawFlexiPoints();
            }

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateFaces();
            UpdateDisplay();
            PathText = flexiPath.ToPath();
        }
    }
}