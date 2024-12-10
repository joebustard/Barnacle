// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.LineLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for FlexiPathEditorControl.xaml
    /// </summary>
    public partial class FlexiPathEditorControl : UserControl
    {
        private bool absolutePaths;
        private bool continuosPointsNotify;

        private string defaultImagePath;

        private bool fixedEndPath = false;

        private Point fixedPathEndPoint;

        private Point fixedPathMidPoint;

        private Point fixedPathStartPoint;

        private Point fixedPolarGridCentre;

        private string imagePath;

        private List<string> initialPaths;

        private LengthLabel lengthLabel = null;

        private bool openEndedPath;

        private string pathText = "";

        private GridSettings.GridStyle showGrid;

        private string toolName;

        private FlexiPathEditorControlViewModel vm;

        public FlexiPathEditorControl()
        {
            InitializeComponent();
            OnFlexiPathChanged = null;
            fixedEndPath = false;
            ShowAppend = false;
            showGrid = GridSettings.GridStyle.Rectangular;
            fixedPathStartPoint = new Point(0, 10);
            fixedPathMidPoint = new Point(20, 30);
            fixedPathEndPoint = new Point(0, 40);
            fixedPolarGridCentre = new Point(0, 50);
            initialPaths = new List<string>();
            IncludeCommonPresets = true;
        }

        public delegate void FlexiImageChanged(String imagePath);

        public delegate void FlexiPathChanged(List<System.Windows.Point> points);

        public delegate void FlexiPathTextChanged(string pathText);

        public delegate void FlexiUserAction();

        public bool AbsolutePaths
        {
            get
            {
                return absolutePaths;
            }

            set
            {
                absolutePaths = value;
            }
        }

        public string AbsolutePathString
        {
            get
            {
                pathText = vm?.AbsPathText() ?? "";
                return pathText;
            }
        }

        public bool ContinuousPointsNotify
        {
            get
            {
                if (vm != null)
                {
                    return vm.ContinuousPointsNotify;
                }
                else
                {
                    return continuosPointsNotify;
                }
            }

            set
            {
                if (vm != null)
                {
                    continuosPointsNotify = value;
                    vm.ContinuousPointsNotify = value;
                }
                else
                {
                    continuosPointsNotify = value;
                }
            }
        }

        public string DefaultImagePath
        {
            get
            {
                return defaultImagePath;
            }

            set
            {
                defaultImagePath = value;
                if (vm != null)
                {
                    vm.DefaultImagePath = DefaultImagePath;
                }
            }
        }

        public bool FixedEndPath
        {
            get
            {
                return fixedEndPath;
            }

            set
            {
                if (fixedEndPath != value)
                {
                    fixedEndPath = value;
                }
            }
        }

        public Point FixedPathEndPoint
        {
            get
            {
                return fixedPathEndPoint;
            }

            set
            {
                if (value != fixedPathEndPoint)
                {
                    fixedPathEndPoint = value;
                }
            }
        }

        public Point FixedPathMidPoint
        {
            get
            {
                return fixedPathMidPoint;
            }

            set
            {
                if (value != fixedPathMidPoint)
                {
                    fixedPathMidPoint = value;
                }
            }
        }

        public Point FixedPathStartPoint
        {
            get
            {
                return fixedPathStartPoint;
            }

            set
            {
                if (value != fixedPathStartPoint)
                {
                    fixedPathStartPoint = value;
                }
            }
        }

        public Point FixedPolarGridCentre
        {
            get
            {
                return FixedPolarGridCentre;
            }

            set
            {
                if (value != fixedPolarGridCentre)
                {
                    fixedPolarGridCentre = value;
                }
            }
        }

        public bool HasPresets
        {
            get; internal set;
        }

        public string ImagePath
        {
            get
            {
                return vm?.ImagePath ?? imagePath;
            }

            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                }
            }
        }

        public bool IncludeCommonPresets
        {
            get; internal set;
        }

        public object LocalImage
        {
            get
            {
                return vm?.BackgroundImage ?? null;
            }
        }

        public int NumberOfPaths
        {
            get
            {
                if (vm != null)
                {
                    return vm.NumberOfPaths;
                }
                else
                {
                    return 0;
                }
            }
        }

        public FlexiImageChanged OnFlexiImageChanged
        {
            get; set;
        }

        public FlexiPathChanged OnFlexiPathChanged
        {
            get; set;
        }

        public FlexiPathTextChanged OnFlexiPathTextChanged
        {
            get; set;
        }

        public FlexiUserAction OnFlexiUserActive
        {
            get; set;
        }

        public bool OpenEndedPath
        {
            get
            {
                if (vm != null)
                {
                    return vm.OpenEndedPath;
                }
                else
                {
                    return openEndedPath;
                }
            }

            set
            {
                if (vm != null)
                {
                    vm.OpenEndedPath = value;
                }
                else
                {
                    openEndedPath = value;
                }
            }
        }

        public bool PathClosed
        {
            get
            {
                bool cl = false;
                if (vm != null && vm.Points != null)
                {
                    if (fixedEndPath)
                    {
                        cl = (vm.Points.Count > 2);
                    }
                    else
                    {
                        cl = (vm.SelectionMode != FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint) &&
                                  (vm.SelectionMode != FlexiPathEditorControlViewModel.SelectionModeType.StartPoint) &&
                                  (vm.Points.Count >= 3);
                    }
                }
                return cl;
            }
        }

        public string PathString
        {
            get
            {
                return vm?.PathText ?? "";
            }
        }

        public List<String> PresetNames
        {
            get
            {
                if (vm != null)
                {
                    return vm.PresetNames;
                }
                else
                    return null;
            }
        }

        public DpiScale ScreenDpi
        {
            get; set;
        }

        public bool ShowAppend
        {
            get; internal set;
        }

        public GridSettings.GridStyle ShowGrid
        {
            get
            {
                return showGrid;
            }

            set
            {
                showGrid = value;
                if (vm != null)
                {
                    vm.ShowGrid = showGrid;
                }
            }
        }

        public bool SupportsHoles
        {
            get; internal set;
        }

        public string ToolName
        {
            get
            {
                return toolName;
            }

            set
            {
                toolName = value;
                if (vm != null)
                {
                    vm.ToolName = toolName;
                }
            }
        }

        public void Busy()
        {
            if (vm != null)
            {
                vm.IsEditingEnabled = false;
            }
        }

        public List<Point> GetOutsidePoints()
        {
            if (vm != null)
            {
                return vm.DisplayOutsidePoints;
            }
            else
            {
                return new List<Point>();
            }
        }

        public List<Point> GetPathPoints(int i)
        {
            if (vm != null)
            {
                return vm.GetDisplayPointsForPath(i);
            }
            else
            {
                return new List<Point>();
            }
        }

        public String GetPathText(int index)
        {
            return vm?.GetPathText(index);
        }

        public List<Point> GetPoints()
        {
            return vm?.DisplayPoints;
        }

        public void LoadImage(String fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                vm?.LoadImage(fileName);
            }
            imagePath = fileName;
            NotifyImageChanged();
        }

        public double ToMM(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = (25.4 * x / sc.PixelsPerInchX); // * sc.DpiScaleX;
            return res;
        }

        public double ToMMX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = (25.4 * x / sc.PixelsPerInchX); // * sc.DpiScaleX;
            return res;
        }

        public double ToMMY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = (25.4 * y / sc.PixelsPerInchY); // * sc.DpiScaleY; ;
            return res;
        }

        public double ToPixelX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = sc.PixelsPerInchX * x / 25.4;
            return res;
        }

        public double ToPixelY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = sc.PixelsPerInchY * y / 25.4;
            return res;
        }

        internal Point Centroid()
        {
            if (vm != null)
            {
                return vm.Centroid();
            }
            else
                return new Point(0, 0);
        }

        internal List<System.Drawing.PointF> DisplayPointsF()
        {
            return vm?.DisplayPointsF();
        }

        internal void FromString(string s, bool resetMode = true)
        {
            pathText = s;
            vm?.FromString(s, resetMode);
        }

        internal void FromTextPath(string v)
        {
            pathText = v;
            if (vm != null)
            {
                vm.FromString(pathText);
            }
        }

        internal DpiScale GetDpi()
        {
            throw new NotImplementedException();
        }

        internal string GetPath()
        {
            if (vm == null)
            {
                return "";
            }
            else
            {
                return vm.AbsPathText();
            }
        }

        internal bool HasHoles()
        {
            if (vm != null)
            {
                return vm.NumberOfPaths > 1;
            }
            else
            {
                return false;
            }
        }

        internal void NotBusy()
        {
            if (vm != null)
            {
                vm.IsEditingEnabled = true;
            }
        }

        internal int PointsInFirstSegment()
        {
            int res = 0;
            if (vm != null)
            {
                res = vm.PointsInFirstSegment();
            }

            return res;
        }

        internal void SetPath(string v)
        {
            if (pathText != v)
            {
                pathText = v;
                vm?.SetPath(v);
            }
        }

        internal void SetPath(string s, int i)
        {
            if (vm != null)
            {
                vm.SetPath(s, i);
            }
            else
            {
                initialPaths.Add(s);
            }
        }

        internal string ToPath(bool v)
        {
            if (vm != null)
            {
                if (v)
                {
                    return vm.AbsPathText();
                }
                else
                {
                    return vm.PathText;
                }
            }
            return "";
        }

        internal void TurnOffGrid()
        {
            ShowGrid = GridSettings.GridStyle.Hidden;
        }

        internal void UpdatePath(string v)
        {
            pathText = v;
        }

        private void CheckLengthLabel(MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (vm.Points.Count > 1 && vm.SelectionMode == FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint)
                {
                    double d = LastSegLength();
                    lengthLabel = new LengthLabel();
                    lengthLabel.Content = d.ToString("F3");
                    lengthLabel.Position = new Point(position.X, position.Y);
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
            ln.MouseMove += MainCanvas_MouseMove;
            ln.MouseUp += MainCanvas_MouseUp;
            ln.MouseDown += MainCanvas_MouseDown;
            MainCanvas.Children.Add(ln);
        }

        private void DisplayLines()
        {
            for (int l = 0; l < vm.NumberOfPaths; l++)
            {
                List<System.Windows.Point> points = vm.GetDisplayPointsForPath(l);
                if (points != null && points.Count >= 2)
                {
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        DrawLine(i, i + 1, points);
                    }
                }
            }
        }

        private void DisplayPoints()
        {
            if (vm.Points != null)
            {
                double ox = double.NaN;
                double oy = double.NaN;
                if (vm.SelectedPoint != -1)
                {
                    if (vm.SelectedPoint < vm.Points.Count)

                    {
                        ox = vm.Points[vm.SelectedPoint].X;
                        oy = vm.Points[vm.SelectedPoint].Y;
                    }
                    else
                    {
                        vm.SelectedPoint = -1;
                    }
                }

                double rad = 6;
                System.Windows.Media.Brush br = null;
                for (int i = 0; i < vm.Points.Count; i++)
                {
                    bool ortho = false;

                    System.Windows.Point p = vm.Points[i].ToPoint();
                    if (vm.SelectedPoint == i)
                    {
                        rad = 8;
                        br = System.Windows.Media.Brushes.LightGreen;
                    }
                    else
                    {
                        rad = 6;
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

                    // only show the points if they are marked as visible OR they are orthogonal to
                    // the selected one
                    if (vm.Points[i].Visible || (ortho && vm.ShowOrtho))
                    {
                        if (vm.Points[i].Mode == FlexiPoint.PointMode.Data)
                        {
                            p = MakeEllipse(rad, br, p);
                        }
                        if (vm.Points[i].Mode == FlexiPoint.PointMode.Control1 ||
                            vm.Points[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            p = MakeRect(rad, br, p);
                        }
                        if (vm.Points[i].Mode == FlexiPoint.PointMode.Control2)
                        {
                            p = MakeTri(rad, br, p);
                        }

                        if (vm.SelectedPoint == i && vm.ShowOrtho)
                        {
                            DashLine(ToPixelX(p.X), 0, ToPixelX(p.X), MainCanvas.ActualHeight - 1);
                            DashLine(0, ToPixelY(p.Y), MainCanvas.ActualWidth - 1, ToPixelY(p.Y));
                        }
                    }
                }
                if (!vm.FixedEndPath)
                {
                    // If we are appending points to the polygon then always draw the start point
                    if ((vm.SelectionMode == FlexiPathEditorControlViewModel.SelectionModeType.StartPoint || vm.SelectionMode == FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint) && vm.Points.Count > 0)
                    {
                        br = System.Windows.Media.Brushes.Red;
                        var p = vm.Points[0].ToPoint();
                        MakeEllipse(8, br, p);
                    }
                }
                // now draw any control connectors
                for (int i = 0; i < vm.Points.Count; i++)
                {
                    if (vm.Points[i].Visible)
                    {
                        if (vm.Points[i].Mode == FlexiPoint.PointMode.Control1 ||
                            vm.Points[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i - 1;
                            if (j < 0)
                            {
                                j = vm.Points.Count - 1;
                            }
                            DrawControlLine(vm.Points[i].ToPoint(), vm.Points[j].ToPoint());
                        }
                        if (vm.Points[i].Mode == FlexiPoint.PointMode.Control2 ||
                            vm.Points[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i + 1;
                            if (j == vm.Points.Count)
                            {
                                j = 0;
                            }
                            DrawControlLine(vm.Points[i].ToPoint(), vm.Points[j].ToPoint());
                        }
                    }
                }
            }
        }

        private void DoButtonBorder(Border src, Border trg)
        {
            if (src == trg)
            {
                trg.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                trg.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
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
            ln.X1 = ToPixelX(p1.X);
            ln.Y1 = ToPixelY(p1.Y);
            ln.X2 = ToPixelX(p2.X);
            ln.Y2 = ToPixelY(p2.Y);
            ln.MouseUp += MainCanvas_MouseUp;
            MainCanvas.Children.Add(ln);
        }

        private void DrawLine(int i, int v, List<System.Windows.Point> points)
        {
            if (v < points.Count)
            {
                PenSetting ps = vm.GetPen();
                SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 255, 255, 5));
                Line ln = new Line();
                ln.Stroke = ps.Brush;
                ln.StrokeThickness = ps.StrokeThickness;
                ln.Opacity = ps.Opacity;
                ln.Fill = ps.Brush;
                ln.StrokeDashArray = ps.DashPattern;
                ln.X1 = ToPixelX(points[i].X);
                ln.Y1 = ToPixelY(points[i].Y);
                ln.X2 = ToPixelX(points[v].X);
                ln.Y2 = ToPixelY(points[v].Y);
                ln.MouseLeftButtonDown += Ln_MouseLeftButtonDown;
                ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
                ln.MouseUp += MainCanvas_MouseUp;
                MainCanvas.Children.Add(ln);
            }
        }

        private void EnableSelectionModeBorder(Border src)
        {
            DoButtonBorder(src, PickBorder);
            DoButtonBorder(src, AddSegBorder);
            DoButtonBorder(src, AddBezierBorder);
            DoButtonBorder(src, AddQuadBezierBorder);
            DoButtonBorder(src, SplitQuadBezierBorder);
            DoButtonBorder(src, DelSegBorder);
            DoButtonBorder(src, MovePathBorder);
            DoButtonBorder(src, AppendBorder);
        }

        private void FlexiUserActive()
        {
            NotifyUserActive();
        }

        private double LastSegLength()
        {
            double res = 0;
            if (vm.Points.Count > 1)
            {
                FlexiPoint p1 = vm.Points[vm.Points.Count - 2];
                FlexiPoint p2 = vm.Points[vm.Points.Count - 1];
                double dx = p1.X - p2.X;
                double dy = p1.Y - p2.Y;
                res = Math.Sqrt(dx * dx + dy * dy);
            }
            return res;
        }

        private void Ln_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            Line ln = sender as Line;
            bool found = false;
            if (ln != null)
            {
                lengthLabel = null;
                switch (vm.SelectionMode)
                {
                    case FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint:
                        {
                            lengthLabel = null;
                            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                            found = vm.SelectLineFromPoint(position, shiftDown);
                            if (!shiftDown && found)
                            {
                                string lengthText = "";
                                Point labelPos;
                                vm.GetSegmentLengthLabel(out lengthText, out labelPos);
                                if (labelPos.X != -1)
                                {
                                    lengthLabel = new LengthLabel();
                                    lengthLabel.Content = lengthText;
                                    lengthLabel.Position = labelPos;
                                }
                            }
                        }
                        break;

                    case FlexiPathEditorControlViewModel.SelectionModeType.SplitLine:
                        {
                            found = vm.SplitLineAtPoint(position);
                            vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToCubicBezier:
                        {
                            found = vm.ConvertLineAtPointToBezier(position, true);
                            vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToQuadBezier:
                        {
                            found = vm.ConvertLineAtPointToBezier(position, false);
                            vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case FlexiPathEditorControlViewModel.SelectionModeType.SplitQuad:
                        {
                            found = vm.SplitQuadBezier(position);
                            vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case FlexiPathEditorControlViewModel.SelectionModeType.DeleteSegment:
                        {
                            found = vm.DeleteSegment(position);

                            vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;
                }
                if (found)
                {
                    e.Handled = true;
                    UpdateDisplay();
                    NotifyUserActive();
                }
            }
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lengthLabel = null;
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseDown(e, position))
            {
                CheckLengthLabel(e, position);
                UpdateDisplay();
                NotifyUserActive();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseMove(e, position))
            {
                CheckLengthLabel(e, position);

                UpdateDisplay();
                NotifyUserActive();
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseUp(e, position))
            {
                CheckLengthLabel(e, position);
                UpdateDisplay();
                NotifyPathPointsChanged();
                NotifyUserActive();
            }
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            vm?.CreateGrid(VisualTreeHelper.GetDpi(MainCanvas), MainCanvas.ActualWidth, MainCanvas.ActualHeight);
            UpdateDisplay();
        }

        private System.Windows.Point MakeEllipse(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            Ellipse el = new Ellipse();

            Canvas.SetLeft(el, ToPixelX(p.X) - rad);
            Canvas.SetTop(el, ToPixelY(p.Y) - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Stroke = br;
            el.Fill = br;
            el.MouseDown += MainCanvas_MouseDown;
            el.MouseMove += MainCanvas_MouseMove;
            el.MouseUp += MainCanvas_MouseUp;
            el.ToolTip = $"{p.X.ToString("F2")},{p.Y.ToString("F2")}";
            // el.ContextMenu = PointMenu(el);
            MainCanvas.Children.Add(el);
            return p;
        }

        private System.Windows.Point MakeRect(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            Rectangle el = new Rectangle();

            Canvas.SetLeft(el, ToPixelX(p.X) - rad);
            Canvas.SetTop(el, ToPixelY(p.Y) - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Stroke = br;
            el.Fill = br;
            el.MouseDown += MainCanvas_MouseDown;
            el.MouseMove += MainCanvas_MouseMove;
            el.MouseUp += MainCanvas_MouseUp;
            el.ToolTip = $"{p.X.ToString("F2")},{p.Y.ToString("F2")}";
            // el.ContextMenu = PointMenu(el);
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
            Canvas.SetLeft(myPolygon, ToPixelX(p.X) - rad);
            Canvas.SetTop(myPolygon, ToPixelY(p.Y) - rad);
            myPolygon.Width = 2 * rad;
            myPolygon.Height = 2 * rad;
            myPolygon.Stroke = br;
            myPolygon.Fill = br;
            myPolygon.MouseDown += MainCanvas_MouseDown;
            myPolygon.MouseMove += MainCanvas_MouseMove;
            myPolygon.MouseUp += MainCanvas_MouseUp;
            myPolygon.ToolTip = $"{p.X.ToString("F2")},{p.Y.ToString("F2")}";
            // myPolygon.ContextMenu = PointMenu(myPolygon);
            MainCanvas.Children.Add(myPolygon);
            return p;
        }

        private void NotifyImageChanged()
        {
            if (OnFlexiImageChanged != null && vm != null)
            {
                OnFlexiImageChanged(vm.ImagePath);
            }
        }

        private void NotifyPathPointsChanged()
        {
            if (OnFlexiPathChanged != null)
            {
                if (vm.PointsDirty)
                {
                    OnFlexiPathChanged(vm.DisplayPoints);
                    vm.PointsDirty = false;
                }
            }
        }

        private void NotifyPathTextChanged()
        {
            if (OnFlexiPathTextChanged != null)
            {
                OnFlexiPathTextChanged(vm.PathText);
            }
        }

        private void NotifyUserActive()
        {
            if (OnFlexiUserActive != null)
            {
                OnFlexiUserActive();
            }
        }

        private void SetSelectionModeBorderColours()
        {
            // put a clear border around the button associated with active state
            switch (vm.SelectionMode)
            {
                case FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint:
                    {
                        if (ShowAppend)
                        {
                            EnableSelectionModeBorder(AppendBorder);
                        }
                        else
                        {
                            EnableSelectionModeBorder(PickBorder);
                        }
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.StartPoint:
                case FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint:
                    {
                        EnableSelectionModeBorder(PickBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.SplitLine:
                    {
                        EnableSelectionModeBorder(AddSegBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToCubicBezier:
                    {
                        EnableSelectionModeBorder(AddBezierBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToQuadBezier:
                    {
                        EnableSelectionModeBorder(AddQuadBezierBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.SplitQuad:
                    {
                        EnableSelectionModeBorder(SplitQuadBezierBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.DeleteSegment:
                    {
                        EnableSelectionModeBorder(DelSegBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.MovePath:
                case FlexiPathEditorControlViewModel.SelectionModeType.DraggingPath:
                    {
                        EnableSelectionModeBorder(MovePathBorder);
                    }
                    break;
            }
        }

        private void ShowGridStatus()
        {
            if (vm.ShowGrid == GridSettings.GridStyle.Rectangular)
            {
                GridBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                GridBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }

            if (vm.ShowGrid == GridSettings.GridStyle.Polar)
            {
                PolarGridBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                PolarGridBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }
        }

        private void ShowOrthoLockedStatus()
        {
            if (vm.OrthoLocked == true)
            {
                OrthoLockBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                OrthoLockBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }
        }

        private void ShowPointsStatus()
        {
            if (vm.ShowPointsStatus)
            {
                ShowPointsBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                ShowPointsBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }
        }

        private void UpdateDisplay()
        {
            MainCanvas.Children.Clear();

            // this is the grid on the path edit
            if (vm != null)
            {
                if (vm.BackgroundImage != null)
                {
                    Image image = new System.Windows.Controls.Image();
                    image.Source = vm.BackgroundImage;
                    image.Width = vm.BackgroundImage.Width;
                    image.Height = vm.BackgroundImage.Height;
                    MainCanvas.Children.Add(image);
                    image.MouseUp += MainCanvas_MouseUp;
                }
                if (vm.ShowGrid != GridSettings.GridStyle.Hidden)
                {
                    foreach (Shape sh in vm.GridMarkers)
                    {
                        MainCanvas.Children.Add(sh);
                    }
                }
                DisplayLines();
                DisplayPoints();
                if (lengthLabel != null)
                {
                    MainCanvas.Children.Add(lengthLabel.TextBox);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                vm = DataContext as FlexiPathEditorControlViewModel;
                if (vm != null)
                {
                    vm.PropertyChanged += Vm_PropertyChanged;
                    vm.SetFixedEnds(fixedPathStartPoint, fixedPathMidPoint, fixedPathEndPoint);
                    vm.IncludeCommonPresets = IncludeCommonPresets;
                    vm.LoadPresets();
                    vm.FixedPolarGridCentre = fixedPolarGridCentre;
                    vm.FixedEndPath = fixedEndPath;
                    vm.OpenEndedPath = openEndedPath;
                    vm.SupportsHoles = SupportsHoles;
                    vm.OnFlexiUserActive = FlexiUserActive;
                    if (ShowAppend)
                    {
                        vm.AppendButtonVisible = Visibility.Visible;
                    }
                    else
                    {
                        vm.AppendButtonVisible = Visibility.Hidden;
                    }
                    ScreenDpi = VisualTreeHelper.GetDpi(MainCanvas);
                    vm.CreateGrid(ScreenDpi, MainCanvas.ActualWidth, MainCanvas.ActualHeight);
                    vm.ShowGrid = showGrid;
                    ShowGridStatus();
                    ShowPointsStatus();
                    vm.AbsolutePaths = absolutePaths;
                    vm.SetPath(pathText);
                    vm.DefaultImagePath = defaultImagePath;
                    vm.ContinuousPointsNotify = continuosPointsNotify;
                    if (imagePath != null)
                    {
                        vm.LoadImage(imagePath);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(vm.ImagePath))
                        {
                            vm.LoadImage(vm.ImagePath);
                        }
                    }

                    vm.SelectedPoint = -1;
                    vm.ToolName = ToolName;
                    vm.ShowPresets = Visibility.Hidden;
                    vm.ShowSavePresets = Visibility.Hidden;
                    if (HasPresets)
                    {
                        vm.ShowPresets = Visibility.Visible;
                        if (!String.IsNullOrEmpty(toolName))
                        {
                            vm.ShowSavePresets = Visibility.Visible;
                        }
                    }
                    if (initialPaths.Count > 0)
                    {
                        for (int i = 0; i < initialPaths.Count; i++)
                        {
                            vm.SetPath(initialPaths[i], i);
                        }
                        vm.PointsDirty = true;
                        vm.SelectionMode = FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint;
                        vm.SelectedCurveName = "Outside";
                    }
                    SetSelectionModeBorderColours();
                    NotifyPathPointsChanged();
                }
            }
            UpdateDisplay();
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectionMode":
                    {
                        SetSelectionModeBorderColours();
                    }
                    break;

                case "ShowGrid":
                    {
                        ShowGridStatus();
                        UpdateDisplay();
                    }
                    break;

                case "OrthoLocked":
                    {
                        ShowOrthoLockedStatus();
                        //UpdateDisplay();
                    }
                    break;

                case "Points":
                    {
                        ShowPointsStatus();
                        UpdateDisplay();
                        NotifyPathPointsChanged();
                    }
                    break;

                case "BackgroundImage":
                    {
                        UpdateDisplay();
                        NotifyPathPointsChanged();
                        NotifyImageChanged();
                    }
                    break;

                case "PathText":
                    {
                        NotifyPathTextChanged();
                    }
                    break;
            }
        }
    }
}