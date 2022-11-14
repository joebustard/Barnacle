using Barnacle.Dialogs;
using Barnacle.LineLib;

using Barnacle.ViewModels;
using Microsoft.Win32;
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
        private BitmapImage backgroundImage;
        private bool canCNVDouble;

        private FlexiPath flexiPath;
        private List<Shape> gridMarkers;
        private double gridX = 0;
        private double gridY = 0;

        private bool lineShape;
        private bool moving;
        private string pathText;
        private ObservableCollection<FlexiPoint> polyPoints; // SHOULD BE CALLED FLEXIPAHCONTROLPOINTS
        private string positionText;
        private double scale;
        private DpiScale screenDpi;

        private int selectedPoint;

        private SelectionModeType selectionMode;

        private bool absolutePaths;

        public bool AbsolutePaths
        {
            get { return absolutePaths; }
            set
            {
                absolutePaths = value;
            }
        }

        private bool showOrtho;

        private Visibility showWidth;

        private bool snap;

        private GridSettings.GridStyle showGrid;

        public FlexiPathEditorControlViewModel()
        {
            AddCubicBezierCommand = new RelayCommand(OnAddCubic);
            AddQuadBezierCommand = new RelayCommand(OnAddQuad);
            AddSegmentCommand = new RelayCommand(OnAddSegment);
            CNVDoubleSegCommand = new RelayCommand(OnCnvDoubleSegment);
            DeleteSegmentCommand = new RelayCommand(OnDeleteSegment);
            GridCommand = new RelayCommand(OnGrid);
            GridSettingsCommand = new RelayCommand(OnGridSettings);
            PolarGridCommand = new RelayCommand(OnPolarGrid);
            MovePathCommand = new RelayCommand(OnMovePath);
            PickCommand = new RelayCommand(OnPickSegment);
            ResetPathCommand = new RelayCommand(OnResetPath);
            ShowAllPointsCommand = new RelayCommand(OnShowAllPoints);
            CopyPathCommand = new RelayCommand(OnCopy);
            PastePathCommand = new RelayCommand(OnPaste);
            ZoomCommand = new RelayCommand(OnZoom);
            LoadImageCommand = new RelayCommand(OnLoadImage);
            fixedEndPath = false;
            flexiPath = new FlexiPath();
            polyPoints = flexiPath.FlexiPoints;
            selectedPoint = -1;
            selectionMode = SelectionModeType.StartPoint;
            scale = 1.0;
            lineShape = false;
            showOrtho = true;
            ShowGrid = GridSettings.GridStyle.Rectangular;
            snap = true;
            gridSettings = new GridSettings();
            imagePath = "";
        }

        private void OnGridSettings(object obj)
        {
            GridSettingsDlg dlg = new GridSettingsDlg();
            dlg.Settings = gridSettings;
            if (dlg.ShowDialog() == true)
            {
                rectGrid.SetGridIntervals(gridSettings.RectangularGridSize, gridSettings.RectangularGridSize);
                polarGrid.SetGridIntervals(gridSettings.PolarGridRadius, gridSettings.PolarGridAngle);
                CreateGridMarkers();
                // force a screen refresh
                NotifyPropertyChanged("Points");
            }
        }

        private bool fixedEndPath = false;

        public bool FixedEndPath
        {
            get { return fixedEndPath; }
            set
            {
                if (fixedEndPath != value)
                {
                    fixedEndPath = value;
                    if (fixedEndPath)
                    {
                        FixedEndFlexiPath fep = new FixedEndFlexiPath();
                        if (fixedPathStartPoint != null)
                        {
                            fep.SetBaseSegment(fixedPathStartPoint, fixedPathMidPoint, fixedPathEndPoint);
                            gridSettings.SetPolarCentre(fixedPolarGridCentre.X, fixedPolarGridCentre.Y);
                        }
                        flexiPath = fep;
                        polyPoints = flexiPath.FlexiPoints;
                        SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                    }
                }
            }
        }

        private Point fixedPolarGridCentre;

        public Point FixedPolarGridCentre
        {
            get { return FixedPolarGridCentre; }
            set
            {
                if (value != fixedPolarGridCentre)
                {
                    fixedPolarGridCentre = value;
                }
            }
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

        public BitmapImage BackgroundImage
        {
            get { return backgroundImage; }

            set
            {
                if (value != backgroundImage)
                {
                    backgroundImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public List<System.Windows.Point> DisplayPoints
        {
            get { return flexiPath.DisplayPoints(); }
        }

        public ICommand GridCommand { get; set; }
        public ICommand GridSettingsCommand { get; set; }
        public ICommand PolarGridCommand { get; set; }

        public List<Shape> GridMarkers
        {
            get
            {
                if (pointGrid != null)
                {
                    return pointGrid.GridMarkers;
                }
                else
                {
                    return null;
                }
            }
        }

        public double GridX
        {
            get { return gridX; }
            set { gridX = value; }
        }

        public double GridY
        {
            get { return gridY; }
            set { gridY = value; }
        }

        public RelayCommand LoadImageCommand { get; }

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

        public ICommand ResetPathCommand { get; set; }

        public double Scale
        {
            get { return scale; }
            set
            {
                if (value != scale)
                {
                    scale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DpiScale ScreenDpi
        {
            get { return screenDpi; }
            set
            {
                screenDpi = value;
            }
        }

        private double gridWidth;
        private double gridHeight;

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

        public GridSettings.GridStyle ShowGrid
        {
            get { return showGrid; }
            set
            {
                showGrid = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowOrtho
        {
            get { return showOrtho; }
            set
            {
                if (showOrtho != value)
                {
                    showOrtho = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /*
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
        */

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

        public ICommand ZoomCommand { get; set; }

        private string imagePath;

        public string ImagePath
        { get { return imagePath; } }

        public bool ConvertLineAtPointToBezier(System.Windows.Point position, bool cubic)
        {
            bool added = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position, true))
            {
                if (cubic)
                {
                    added = flexiPath.ConvertToCubic(position);
                }
                else
                {
                    added = flexiPath.ConvertToQuadQuadAtSelected(position);
                }
                if (added)
                {
                    PathText = flexiPath.ToPath(absolutePaths);
                    NotifyPropertyChanged("Points");
                }
            }

            return added;
        }

        public bool DeleteSegment(System.Windows.Point position)
        {
            bool deleted = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position, true))
            {
                deleted = flexiPath.DeleteSelectedSegment();

                if (deleted)
                {
                    PathText = flexiPath.ToPath(absolutePaths);
                }
            }
            // just in case the segment deletion means the point has gone
            SelectedPoint = -1;
            NotifyPropertyChanged("Points");
            return deleted;
        }

        public bool MouseMove(MouseEventArgs e, System.Windows.Point position)
        {
            bool updateRequired = false;
            if (fixedEndPath)
            {
                updateRequired = FixedEndPathMove(e, position, updateRequired);
            }
            else
            {
                updateRequired = NormalPathMove(e, position, updateRequired);
            }
            return updateRequired;
        }

        private bool NormalPathMove(MouseEventArgs e, Point position, bool updateRequired)
        {
            System.Windows.Point snappedPos = SnapPositionToMM(position);
            PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";
            if (selectedPoint != -1)
            {
                if (e.LeftButton == MouseButtonState.Pressed && moving)
                {
                    // polyPoints[selectedPoint].X = position.X;
                    // polyPoints[selectedPoint].Y = position.Y;
                    flexiPath.SetPointPos(selectedPoint, snappedPos);
                    PathText = flexiPath.ToPath(absolutePaths);
                    updateRequired = true;
                }
                else
                {
                    moving = false;
                }
            }

            return updateRequired;
        }

        private bool FixedEndPathMove(MouseEventArgs e, Point position, bool updateRequired)
        {
            if (selectedPoint != -1)
            {
                if (e.LeftButton == MouseButtonState.Pressed && moving)
                {
                    if (selectedPoint == 0)
                    {
                        if (position.Y > ToPixelY(polyPoints[polyPoints.Count - 1].Y))
                        {
                            position.Y = ToPixelY(polyPoints[polyPoints.Count - 1].Y);
                        }
                        position.X = ToPixelX(polyPoints[selectedPoint].X);

                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = polyPoints[selectedPoint].X;
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }
                    else if (selectedPoint == polyPoints.Count - 1)
                    {
                        position.X = ToPixelX(polyPoints[selectedPoint].X);
                        if (position.Y < ToPixelY(polyPoints[0].Y))
                        {
                            position.Y = ToPixelY(polyPoints[0].Y);
                        }

                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = polyPoints[selectedPoint].X;
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }
                    else
                    {
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }

                    PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";

                    PathText = flexiPath.ToPath(absolutePaths);
                    updateRequired = true;
                }
                else
                {
                    moving = false;
                }
            }

            return updateRequired;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool SelectLineFromPoint(System.Windows.Point position, bool shift)
        {
            bool found;
            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            found = flexiPath.SelectAtPoint(position, !shift);
            CanCNVDouble = flexiPath.HasTwoConsecutiveLineSegmentsSelected();
            return found;
        }

        public bool SplitLineAtPoint(System.Windows.Point position)
        {
            bool added = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position, true))
            {
                if (flexiPath.SplitSelectedLineSegment(position))
                {
                    added = true;
                    PathText = flexiPath.ToPath(absolutePaths);
                    NotifyPropertyChanged("Points");
                }
            }
            return added;
        }

        internal void FromString(string s)
        {
            flexiPath.FromString(s);
            PathText = flexiPath.ToPath(absolutePaths);
        }

        private PointGrid pointGrid;
        private RectangularGrid rectGrid;
        private PolarGrid polarGrid;
        private GridSettings gridSettings;

        private void MakeGrid(double actualWidth, double actualHeight)
        {
            if (rectGrid == null)
            {
                rectGrid = new RectangularGrid();
            }
            rectGrid.SetActualSize(ScreenDpi, actualWidth, actualHeight);
            rectGrid.SetGridIntervals(gridSettings.RectangularGridSize, gridSettings.RectangularGridSize);

            if (polarGrid == null)
            {
                polarGrid = new PolarGrid();
                polarGrid.SetGridIntervals(gridSettings.PolarGridRadius, gridSettings.PolarGridAngle);
            }
            if (!fixedEndPath)
            {
                gridSettings.Centre = new Point(ToMMX(actualWidth / 2), ToMMY(actualHeight / 2));
            }
            polarGrid.SetActualSize(ScreenDpi, actualWidth, actualHeight);
            polarGrid.SetPolarCentre(gridSettings.Centre);

            if (pointGrid == null)
            {
                pointGrid = rectGrid;
            }
        }

        internal void CreateGrid(DpiScale dpiScale, double actualWidth, double actualHeight)
        {
            this.ScreenDpi = dpiScale;
            this.gridWidth = actualWidth;
            this.gridHeight = actualHeight;

            CreateGridMarkers();
        }

        private void CreateGridMarkers()
        {
            MakeGrid(gridWidth, gridHeight);
            rectGrid.CreateMarkers(ScreenDpi);
            polarGrid.CreateMarkers(ScreenDpi);
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
                NotifyPropertyChanged("Points");
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

        internal bool MouseUp(MouseButtonEventArgs e, Point position)
        {
            bool updateRequired = false;
            if (fixedEndPath)
            {
                updateRequired = FixedEndMouseUp(position, updateRequired);
            }
            else
            {
                updateRequired = NormalMouseUp(position, updateRequired);
            }
            return updateRequired;
        }

        private bool NormalMouseUp(Point position, bool updateRequired)
        {
            if (selectedPoint != -1 && moving)
            {
                System.Windows.Point positionSnappedToMM = SnapPositionToMM(position);

                polyPoints[selectedPoint].X = position.X;
                polyPoints[selectedPoint].Y = position.Y;

                flexiPath.SetPointPos(selectedPoint, positionSnappedToMM);
                PathText = flexiPath.ToPath(absolutePaths);
                updateRequired = true;
                selectedPoint = -1;
                NotifyPropertyChanged("Points");
            }
            moving = false;
            return updateRequired;
        }

        private bool FixedEndMouseUp(Point position, bool updateRequired)
        {
            if (selectedPoint != -1)
            {
                if (moving)
                {
                    if (selectedPoint == 0)
                    {
                        if (position.Y > ToPixelY(polyPoints[polyPoints.Count - 1].Y))
                        {
                            position.Y = ToPixelY(polyPoints[polyPoints.Count - 1].Y);
                        }
                        position.X = ToPixelX(polyPoints[selectedPoint].X);
                        //polyPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = polyPoints[selectedPoint].X;
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }
                    else if (selectedPoint == polyPoints.Count - 1)
                    {
                        position.X = ToPixelX(polyPoints[selectedPoint].X);
                        if (position.Y < ToPixelY(polyPoints[0].Y))
                        {
                            position.Y = ToPixelY(polyPoints[0].Y);
                        }
                        //  polyPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = polyPoints[selectedPoint].X;
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }
                    else
                    {
                        polyPoints[selectedPoint].X = position.X;
                        polyPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        flexiPath.SetPointPos(selectedPoint, snappedPos);
                    }

                    PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";
                    PathText = flexiPath.ToPath(absolutePaths);
                    updateRequired = true;
                    selectedPoint = -1;
                    NotifyPropertyChanged("Points");
                }
            }
            moving = false;
            return updateRequired;
        }

        internal void SetPolarGridCentre(Point fixedPolarGridCentre)
        {
            throw new NotImplementedException();
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

        public void LoadImage(string f)
        {
            Uri fileUri = new Uri(f);
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = fileUri;
            bmi.EndInit();
            BackgroundImage = bmi;
            imagePath = f;
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
            if (canCNVDouble)
            {
                flexiPath.ConvertTwoLineSegmentsToQuadraticBezier();
                PathText = flexiPath.ToPath(absolutePaths);
                CanCNVDouble = false;
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                NotifyPropertyChanged("Points");
            }
        }

        internal string AbsPathText()
        {
            return flexiPath.ToPath(true);
        }

        private void OnCopy(object obj)
        {
            PathText = flexiPath.ToPath(absolutePaths);
            System.Windows.Clipboard.SetText(PathText);
        }

        internal void SetPath(string v)
        {
            if (v != "")
            {
                selectionMode = SelectionModeType.SelectSegmentAtPoint;
            }

            flexiPath.FromTextPath(v);
            PathText = flexiPath.ToPath(absolutePaths);
            NotifyPropertyChanged("Points");
        }

        private void OnDeleteSegment(object obj)
        {
            SelectionMode = SelectionModeType.DeleteSegment;
        }

        private Point fixedPathStartPoint;
        private Point fixedPathMidPoint;
        private Point fixedPathEndPoint;

        internal void SetFixedEnds(Point fixedPathStartPoint, Point fixedPathMidPoint, Point fixedPathEndPoint)
        {
            this.fixedPathStartPoint = fixedPathStartPoint;
            this.fixedPathMidPoint = fixedPathMidPoint;
            this.fixedPathEndPoint = fixedPathEndPoint;
        }

        private void OnGrid(object obj)
        {
            switch (showGrid)
            {
                case GridSettings.GridStyle.Hidden:
                case GridSettings.GridStyle.Polar:
                    {
                        ShowGrid = GridSettings.GridStyle.Rectangular;
                        pointGrid = rectGrid;
                        snap = true;
                    }
                    break;

                case GridSettings.GridStyle.Rectangular:
                    {
                        ShowGrid = GridSettings.GridStyle.Hidden;
                        snap = false;
                    }
                    break;
            }
        }

        private void OnPolarGrid(object obj)
        {
            switch (showGrid)
            {
                case GridSettings.GridStyle.Hidden:
                case GridSettings.GridStyle.Rectangular:
                    {
                        pointGrid = polarGrid;
                        ShowGrid = GridSettings.GridStyle.Polar;
                        snap = true;
                    }
                    break;

                case GridSettings.GridStyle.Polar:
                    {
                        ShowGrid = GridSettings.GridStyle.Hidden;
                        snap = false;
                    }
                    break;
            }
        }

        private void OnLoadImage(object obj)
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
        }

        private void OnMovePath(object obj)
        {
            SelectionMode = SelectionModeType.MovePath;
        }

        private void OnPaste(object obj)
        {
            string pth = System.Windows.Clipboard.GetText();
            if (pth != null && pth != "" && pth.StartsWith("M"))
            {
                PathText = pth;
                flexiPath.FromTextPath(pth);

                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
        }

        private void OnPickSegment(object obj)
        {
            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
        }

        private void OnResetPath(object obj)
        {
            flexiPath.Clear();
            SelectedPoint = -1;
            if (!fixedEndPath)
            {
                SelectionMode = SelectionModeType.StartPoint;
            }
            else
            {
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
            PathText = "";
            NotifyPropertyChanged("Points");
        }

        private void OnShowAllPoints(object obj)
        {
            foreach (FlexiPoint p in polyPoints)
            {
                p.Visible = true;
            }
            NotifyPropertyChanged("Points");
        }

        private void OnZoom(object obj)
        {
            string p = obj.ToString();
            switch (p)
            {
                case "In":
                    {
                        Scale = scale * 1.1;
                    }
                    break;

                case "Out":
                    {
                        Scale = scale * 0.9;
                    }
                    break;

                case "Reset":
                    {
                        Scale = 1;
                    }
                    break;
            }
        }

        private System.Windows.Point SnapPositionToMM(System.Windows.Point pos)
        {
            Point result = new Point(0, 0);
            if (pointGrid != null && snap)
            {
                result = pointGrid.SnapPositionToMM(pos);
            }
            else
            {
                result = new Point(ToMMX(pos.X), ToMMY(pos.Y));
            }
            return result;
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