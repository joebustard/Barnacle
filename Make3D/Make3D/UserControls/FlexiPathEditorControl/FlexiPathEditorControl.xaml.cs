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
        public FlexiPathChanged OnFlexiPathChanged;

        private string pathText = "";

        private FlexiPathEditorControlViewModel vm;

        public FlexiPathEditorControl()
        {
            InitializeComponent();
            OnFlexiPathChanged = null;
        }

        public delegate void FlexiPathChanged(List<System.Windows.Point> points);
        public bool PathClosed
        {
            get
            {
                bool cl = false;
                if (vm != null && vm.Points != null)
                {
                    cl = (vm.SelectionMode != FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint) &&
                              (vm.SelectionMode != FlexiPathEditorControlViewModel.SelectionModeType.StartPoint) &&
                              (vm.Points.Count > 3);
                }
                return cl;
            }
        }

        private string imagePath;
        public string ImagePath 
        {
            get { return vm?.ImagePath ?? imagePath; }

            set
            {
                if ( imagePath != value)
                {
                    imagePath = value;
                    
                }
            }
        }

        public string PathString { 
            get { return vm?.PathText ?? ""; }
        }

        internal string GetPath()
        {
            return vm.AbsPathText();
        }

        internal void SetPath(string v)
        {
            pathText = v;
            vm?.SetPath(v);
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
            List<System.Windows.Point> points = vm.DisplayPoints;
            if (points != null && points.Count >= 2)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    DrawLine(i, i + 1, points);
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
                    ox = vm.Points[vm.SelectedPoint].X;
                    oy = vm.Points[vm.SelectedPoint].Y;
                }

                double rad = 3;
                System.Windows.Media.Brush br = null;
                for (int i = 0; i < vm.Points.Count; i++)
                {
                    bool ortho = false;

                    System.Windows.Point p = vm.Points[i].ToPoint();
                    if (vm.SelectedPoint == i)
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
                // If we are appending points to the polygon then always draw the start point
                if ((vm.SelectionMode == FlexiPathEditorControlViewModel.SelectionModeType.StartPoint || vm.SelectionMode == FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint) && vm.Points.Count > 0)
                {
                    br = System.Windows.Media.Brushes.Red;
                    var p = vm.Points[0].ToPoint();
                    MakeEllipse(6, br, p);
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

        internal DpiScale GetDpi()
        {
            throw new NotImplementedException();
        }

        private bool absolutePaths;
        public bool AbsolutePaths {
            get { return absolutePaths; }

            set { absolutePaths = value;}
        }

        public object LocalImage
        {
            get { return vm?.BackgroundImage ?? null; }
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

            MainCanvas.Children.Add(ln);
        }

        private void DrawLine(int i, int v, List<System.Windows.Point> points)
        {
            if (v < points.Count)
            {
                SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 255, 255, 5));
                Line ln = new Line();
                ln.Stroke = br;
                ln.StrokeThickness = 6;
                ln.Fill = br;
                ln.X1 = ToPixelX(points[i].X);
                ln.Y1 = ToPixelY(points[i].Y);
                ln.X2 = ToPixelX(points[v].X);
                ln.Y2 = ToPixelY(points[v].Y);
                ln.MouseLeftButtonDown += Ln_MouseLeftButtonDown;
                ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
                MainCanvas.Children.Add(ln);
            }
        }

        private void EnableSelectionModeBorder(Border src)
        {
            DoButtonBorder(src, PickBorder);
            DoButtonBorder(src, AddSegBorder);
            DoButtonBorder(src, AddBezierBorder);
            DoButtonBorder(src, AddQuadBezierBorder);
            DoButtonBorder(src, DelSegBorder);
            DoButtonBorder(src, MovePathBorder);
        }

        private void Ln_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            Line ln = sender as Line;
            bool found = false;
            if (ln != null)
            {
                switch (vm.SelectionMode)
                {
                    case FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint:
                        {
                            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                            found = vm.SelectLineFromPoint(position, shiftDown);
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
                }
            }
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseDown(e, position))
            {
                UpdateDisplay();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseMove(e, position))
            {
                UpdateDisplay();
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(MainCanvas);
            if (vm.MouseUp(e, position))
            {
                UpdateDisplay();
                NotifyPathPointsChanged();
            }
        }

        internal void FromString(string s)
        {
            pathText = s;
            vm?.FromString(s);
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
            //      el.ContextMenu = PointMenu(el);
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
            //   myPolygon.ContextMenu = PointMenu(myPolygon);
            MainCanvas.Children.Add(myPolygon);
            return p;
        }

        private void NotifyPathPointsChanged()
        {
            if (OnFlexiPathChanged != null)
            {
                OnFlexiPathChanged(vm.DisplayPoints);
            }
        }

        public void LoadImage(String fileName)
        {
            vm?.LoadImage(fileName);
            imagePath = fileName;
        }
        private void SetSelectionModeBorderColours()
        {
            // put a clear border around the button associated with active state
            switch (vm.SelectionMode)
            {
                case FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint:
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

                case FlexiPathEditorControlViewModel.SelectionModeType.DeleteSegment:
                    {
                        EnableSelectionModeBorder(DelSegBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.MovePath:
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

        public double ToMMX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = 25.4 * x / sc.PixelsPerInchX;
            return res;
        }
        public double ToMM(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = 25.4 * x / sc.PixelsPerInchX;
            return res;
        }
        public double ToMMY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(MainCanvas);
            double res = 25.4 * y / sc.PixelsPerInchY;
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

                    vm.CreateGrid(VisualTreeHelper.GetDpi(MainCanvas), MainCanvas.ActualWidth, MainCanvas.ActualHeight);
                    ShowGridStatus();
                    vm.AbsolutePaths = absolutePaths;
                    vm.SetPath(pathText);
                    if ( imagePath != null )
                    {
                        vm.LoadImage(imagePath);
                    }
                    if ( pathText != null)
                    {
                        vm.FromString(pathText);
                    }

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

                case "Points":
                case "BackgroundImage":
                    {
                        UpdateDisplay();
                        NotifyPathPointsChanged();
                    }
                    break;
            }
        }
    }
}