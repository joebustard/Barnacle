using Barnacle.LineLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class FlexiPathEditorControlViewModel : INotifyPropertyChanged
    {
        private bool canCNVDouble;

        private FlexiPath flexiPath;
        private List<Shape> gridMarkers;
        private double gridX = 0;
        private double gridY = 0;

        private bool lineShape;
        private BitmapImage localImage;
        private bool moving;
        private string pathText;
        private ObservableCollection<FlexiPoint> polyPoints; // SHOULD BE CALLED FLEXIPAHCONTROLPOINTS
        private double scale;
        private DpiScale screenDpi;

        private int selectedPoint;

        private SelectionModeType selectionMode;

        private bool showGrid;
        public bool ShowGrid
        {
             get { return showGrid; }
            set
            {
                showGrid = value;
            }
        }
        public List<Shape> GridMarkers
        {
            get { return gridMarkers;  }
            set
            {
                gridMarkers = value;
            }
        }
        private bool showOrtho;
        public bool ShowOrtho
        {
            get { return showOrtho; }
            set
            {
                if ( showOrtho != value)
                {
                    showOrtho = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Visibility showWidth;

        private bool snap;

        public FlexiPathEditorControlViewModel()
        {
            AddCubicBezierCommand = new RelayCommand(OnAddCubic);
            AddQuadBezierCommand = new RelayCommand(OnAddQuad);
            AddSegmentCommand = new RelayCommand(OnAddSegment);
            CNVDoubleSegCommand = new RelayCommand(OnCnvDoubleSegment);
            DeleteSegmentCommand = new RelayCommand(OnDeleteSegment);
            MovePathCommand = new RelayCommand(OnMovePath);
            PickCommand = new RelayCommand(OnPickSegment);
            ResetPathCommand = new RelayCommand(OnResetPath);
            ShowAllPointsCommand = new RelayCommand(OnShowAllPoints);
            CopyPathCommand = new RelayCommand(OnCopy);
            PastePathCommand = new RelayCommand(OnPaste);

            flexiPath = new FlexiPath();
            polyPoints = flexiPath.FlexiPoints;
            selectedPoint = -1;
            selectionMode = SelectionModeType.SelectSegmentAtPoint;
            scale = 1.0;
            lineShape = false;
            showOrtho = true;
            showWidth = Visibility.Hidden;
            showGrid = true;
            snap = true;
        }

        internal bool MouseUp(MouseButtonEventArgs e, Point position)
        {
            bool updateRequired = false;
            if (selectedPoint != -1 && moving && snap)
            {

                System.Windows.Point positionSnappedToMM = SnapPositionToMM(position);

                polyPoints[selectedPoint].X = position.X;
                polyPoints[selectedPoint].Y = position.Y;

                flexiPath.SetPointPos(selectedPoint, positionSnappedToMM);
                PathText = flexiPath.ToPath();
                updateRequired = true;
                selectedPoint = -1;
            }
            moving = false;
            return updateRequired;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public enum SelectionModeType
        {
            StartPoint,
            AppendPoint,
            SelectSegmentAtPoint,
            SplitLine,
            ConvertToCubicBezier,
            DeleteSegment,
            ConvertToQuadBezier,
            MovePath
        };

        public ICommand AddCubicBezierCommand { get; set; }

        public ICommand AddQuadBezierCommand { get; set; }

        public ICommand AddSegmentCommand { get; set; }

        public bool CanCNVDouble
        {
            get { return canCNVDouble; }
            set
            {
                if (canCNVDouble != value)
                {
                    canCNVDouble = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CNVDoubleVisible");
                }
            }
        }

        public ICommand CNVDoubleSegCommand { get; set; }

        public Visibility CNVDoubleVisible
        {
            get
            {
                if (canCNVDouble)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }

        public ICommand CopyPathCommand { get; set; }

        public ICommand DeleteSegmentCommand { get; set; }

        public ICommand MovePathCommand { get; set; }

        public ICommand PastePathCommand { get; set; }

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

        public ICommand PickCommand { get; set; }

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

        public ICommand ResetPathCommand { get; set; }

        public DpiScale ScreenDpi
        {
            get { return screenDpi; }
            set
            {
                screenDpi = value;
            }
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
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ShowAllPointsCommand { get; set; }

     public List<System.Windows.Point> DisplayPoints
        {
            get { return flexiPath.DisplayPoints(); }
        }

        public bool ShowPolyGrid
        {
            get { return showGrid; }
            set
            {
                if (value != showGrid)
                {
                    showGrid = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Snap
        {
            get { return snap; }
            set
            {
                if (snap != value)
                {
                    snap = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal bool MouseDown(MouseButtonEventArgs e, System.Windows.Point position)
        {
            bool updateRequired = false;
            if (selectionMode == SelectionModeType.StartPoint)
            {
                AddStartPointToPoly(position);
                updateRequired = true;
                e.Handled = true;
            }
            else if (selectionMode == SelectionModeType.AppendPoint)
            {
                AddAnotherPointToPoly(position);
                e.Handled = true;
                updateRequired = true;
            }
            else
            {
                try
                {
                    if (selectedPoint >= 0)
                    {
                        Points[selectedPoint].Selected = false;
                    }
                    SelectedPoint = -1;

                    // do this test here because the othe modes only trigger ifn you click a line
                    if (selectionMode == SelectionModeType.MovePath)
                    {
                        position = SnapPositionToMM(position);
                        MoveWholePath(position);
                        SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        updateRequired = true;
                        e.Handled = true;
                    }
                    else
                    {
                        // dont snap position when selecting points as they  may have been positioned between two
                        // grid points .
                        // just convert position to mm
                        position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));

                        // find the closest point thats within search radius
                        double rad = 3;
                        double minDist = double.MaxValue;
                        for (int i = 0; i < Points.Count; i++)
                        {
                            System.Windows.Point p = Points[i].ToPoint();
                            double dist = Math.Sqrt(((position.X - p.X) * (position.X - p.X)) +
                                                    ((position.Y - p.Y) * (position.Y - p.Y)));
                            if (dist < minDist && dist < rad)
                            {
                                SelectedPoint = i;
                                minDist = dist;
                            }
                        }
                        if (SelectedPoint != -1)
                        {
                            Points[SelectedPoint].Selected = true;
                            moving = true;
                        }
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            e.Handled = true;
                            updateRequired = true;
                        }
                    }
                }
                catch
                {
                }
            }

            return updateRequired;
        }
        private string positionText;
        public String PositionText
        {
            get { return positionText; }
            set
            {
                if (positionText != value)
                {
                    positionText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double GridX {
            get { return gridX; }
            set { gridX = value; } }
        public double GridY
        {
            get { return gridY; }
            set { gridY = value; }
        }

        
        public bool MouseMove(MouseEventArgs e, System.Windows.Point position)
        {
            bool updateRequired = false;
            System.Windows.Point snappedPos = SnapPositionToMM(position);
            PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";
            if (selectedPoint != -1)
            {
                if (e.LeftButton == MouseButtonState.Pressed && moving)
                {
                    polyPoints[selectedPoint].X = position.X;
                    polyPoints[selectedPoint].Y = position.Y;
                    flexiPath.SetPointPos(selectedPoint, snappedPos);
                    PathText = flexiPath.ToPath();
                    updateRequired = true;
                }
                else
                {
                    moving = false;
                }
                
            }
            return updateRequired;
        }
        private void AddAnotherPointToPoly(System.Windows.Point position)
        {
            position = SnapPositionToMM(position);

            if (Math.Abs(position.X - flexiPath.Start.X) < 2 && Math.Abs(position.Y - flexiPath.Start.Y) < 2)
            {
                //  closeFigure = true;
                selectedPoint = -1;
                selectionMode = SelectionModeType.SelectSegmentAtPoint;
                flexiPath.ClosePath();
            }
            else
            {
                flexiPath.AddLine(new System.Windows.Point(position.X, position.Y));

                selectionMode = SelectionModeType.AppendPoint;
                selectedPoint = polyPoints.Count - 1;
                moving = true;
            }
        }

        private void AddStartPointToPoly(System.Windows.Point position)
        {
            position = SnapPositionToMM(position);
            flexiPath.Start = new FlexiPoint(new System.Windows.Point(position.X, position.Y), 0);
            selectionMode = SelectionModeType.AppendPoint;
        }

       

        private void MoveWholePath(System.Windows.Point position)
        {
            flexiPath.MoveTo(position);
        }
        private void OnAddCubic(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToCubicBezier;
        }

        private void OnAddQuad(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToQuadBezier;
        }

        private void OnAddSegment(object obj)
        {
            SelectionMode = SelectionModeType.SplitLine;
        }

        private void OnCnvDoubleSegment(object obj)
        {
        }

        private void OnCopy(object obj)
        {
            PathText = flexiPath.ToPath();
            Clipboard.SetText(PathText);
        }

        private void OnDeleteSegment(object obj)
        {
            SelectionMode = SelectionModeType.DeleteSegment;
        }

        private void OnMovePath(object obj)
        {
        }

        private void OnPaste(object obj)
        {
            string pth = Clipboard.GetText();
            if (pth != null && pth != "" && pth.StartsWith("M"))
            {
                PathText = pth;
                flexiPath.FromTextPath(pth);

                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
        }

        private void OnPickSegment(object obj)
        {
            selectionMode = SelectionModeType.SelectSegmentAtPoint;
        }

        private void OnResetPath(object obj)
        {
            flexiPath.Clear();
            SelectionMode = SelectionModeType.StartPoint;
            PathText = "";
        }

        private void OnShowAllPoints(object obj)
        {
            foreach (FlexiPoint p in polyPoints)
            {
                p.Visible = true;
            }
        }

        private System.Windows.Point SnapPositionToMM(System.Windows.Point pos)
        {
            double gx = pos.X;
            double gy = pos.Y;
            if (snap)
            {
                gx = pos.X / gridX;
                gx = Math.Round(gx) * gridX;
                gy = pos.Y / gridY;
                gy = Math.Round(gy) * gridY;
            }
            return new System.Windows.Point(ToMMX(gx), ToMMY(gy));
        }

        private double ToMMX(double x)
        {

            double res = 25.4 * x / ScreenDpi.PixelsPerInchX;
            return res;
        }

        private double ToMMY(double y)
        {
            double res = 25.4 * y / ScreenDpi.PixelsPerInchY;
            return res;
        }

        private double ToPixelX(double x)
        {
            double res = ScreenDpi.PixelsPerInchX * x / 25.4;
            return res;
        }

        private double ToPixelY(double y)
        {
            double res = ScreenDpi.PixelsPerInchY * y / 25.4;
            return res;
        }
    }
}