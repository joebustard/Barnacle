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

using Barnacle.ViewModels;
using FileUtils;
using Microsoft.Win32;
using PolygonLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    public class FlexiPathEditorControlViewModel : INotifyPropertyChanged
    {
        private bool absolutePaths;
        private List<FlexiPath> allPaths;

        private Visibility appendButtonVisible;

        // private BitmapImage backgroundImage;
        private BitmapSource backgroundImage;

        private bool canCNVDouble;
        private ObservableCollection<string> curveNames;
        private string defaultImagePath;
        private String editedPresetText;
        private bool editingHole = false;
        private bool fixedEndPath = false;
        private Point fixedPathEndPoint;
        private Point fixedPathMidPoint;
        private Point fixedPathStartPoint;
        private Point fixedPolarGridCentre;
        private double gridHeight;
        private GridSettings gridSettings;
        private double gridWidth;
        private double gridX = 0;
        private double gridY = 0;
        private string imagePath;
        private bool isEditingEnabled;
        private PenSetting linePen;
        private bool lineShape;
        private bool moving;
        private int nextHoleId = 1;
        private bool openEndedPath;
        private bool orthoLocked;
        private string pathText;
        private List<PenSetting> Pens;
        private PointGrid pointGrid;
        private bool pointsDirty;
        private PolarGrid polarGrid;
        private string positionText;
        private Dictionary<String, Preset> presets;
        private RectangularGrid rectGrid;
        private bool savePresetEnabled;
        private double scale;
        private DpiScale screenDpi;
        private int selectedArcPoint;
        private string selectedCurveName;
        private FlexiPath selectedFlexiPath;
        private ObservableCollection<FlexiPoint> selectedFlexiPathControlPoints; // SHOULD BE CALLED FLEXIPAHCONTROLPOINTS
        private int selectedPoint;
        private string selectedPreset;

        private SelectionModeType selectionMode;

        private Visibility showHoleControls;
        private bool showOrtho;
        private Visibility showPresets;
        private Visibility showSavePresets;
        private Visibility showWidth;
        private bool snap;
        private bool supportsHoles;
        private Visibility swapArcVisible;
        private string toolName;

        public FlexiPathEditorControlViewModel()

        {
            AddCubicBezierCommand = new RelayCommand(OnAddCubic);
            ApplyPresetCommand = new RelayCommand(OnApplyPreset);
            AddQuadBezierCommand = new RelayCommand(OnAddQuad);
            AppendCommand = new RelayCommand(OnAppendPoint);
            SplitQuadBezierCommand = new RelayCommand(OnSplitQuad);
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
            OrthoLockCommand = new RelayCommand(OnToggleOrthoLock);
            LineStyleCommand = new RelayCommand(OnLineStyle);
            SavePresetCommand = new RelayCommand(OnSavePreset);
            HoleCommand = new RelayCommand(OnHoleCommand);
            FlipCommand = new RelayCommand(OnFlip);
            AntiClockwiseArcCommand = new RelayCommand(OnAntiClockwiseArc);
            SwapArcSegmentSizeCommand = new RelayCommand(OnSwapArcSegmentSize);
            SwapArcDirectionCommand = new RelayCommand(OnSwapArcDirection);
            fixedEndPath = false;
            supportsHoles = false;
            showHoleControls = Visibility.Hidden;
            appendButtonVisible = Visibility.Hidden;
            allPaths = new List<FlexiPath>();
            allPaths.Add(new FlexiPath());
            selectedFlexiPath = allPaths[0];
            selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
            curveNames = new ObservableCollection<string>();
            curveNames.Add("Outside");
            selectedCurveName = "Outside";
            ShowPointsStatus = false;
            selectedPoint = -1;
            SelectionMode = SelectionModeType.StartPoint;
            scale = 1.0;
            lineShape = false;
            showOrtho = true;
            snap = true;
            gridSettings = new GridSettings();
            ShowGrid = GridSettings.GridStyle.Rectangular;
            gridSettings.Load();
            imagePath = "";
            pointsDirty = true;
            CreatePen();
            editedPresetText = "";
            ShowPresets = Visibility.Visible;
            SwapArcVisible = Visibility.Hidden;
            presets = new Dictionary<string, Preset>();
            PresetNames = new List<string>();
            ToolName = "";
            IncludeCommonPresets = true;
            LoadPresets();
            isEditingEnabled = true;
        }

        public delegate void FlexiUserAction();

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
            MovePath,
            DraggingPath,
            SplitQuad,
            AntiClockwiseArc,
            ClockwiseArc
        }

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

        public ICommand AddCubicBezierCommand
        {
            get; set;
        }

        public ICommand AddQuadBezierCommand
        {
            get; set;
        }

        public ICommand AddSegmentCommand
        {
            get; set;
        }

        public ICommand AntiClockwiseArcCommand
        {
            get; set;
        }

        public Visibility AppendButtonVisible
        {
            get
            {
                return appendButtonVisible;
            }

            set
            {
                if (value != appendButtonVisible)
                {
                    appendButtonVisible = value;
                    NotifyPropertyChanged("AppendButtonVisible");
                }
            }
        }

        public ICommand AppendCommand
        {
            get; set;
        }

        public RelayCommand ApplyPresetCommand
        {
            get; private set;
        }

        //public BitmapImage BackgroundImage
        public BitmapSource BackgroundImage
        {
            get
            {
                return backgroundImage;
            }

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
            get
            {
                return canCNVDouble;
            }

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

        public ICommand CNVDoubleSegCommand
        {
            get; set;
        }

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

        public bool ContinuousPointsNotify { get; set; } = true;

        public ICommand CopyPathCommand
        {
            get; set;
        }

        public int CurrentPen
        {
            get; set;
        }

        public ObservableCollection<string> CurveNames
        {
            get
            {
                return curveNames;
            }

            set
            {
                if (value != curveNames)
                {
                    curveNames = value;
                    NotifyPropertyChanged();
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
            }
        }

        public ICommand DeleteSegmentCommand
        {
            get; set;
        }

        public List<System.Windows.Point> DisplayOutsidePoints
        {
            get
            {
                return allPaths[0].DisplayPoints();
            }
        }

        public List<System.Windows.Point> DisplayPoints
        {
            get
            {
                return selectedFlexiPath.DisplayPoints();
            }
        }

        public String EditedPresetText
        {
            get
            {
                return editedPresetText;
            }

            set
            {
                if (editedPresetText != value)
                {
                    editedPresetText = value;
                    NotifyPropertyChanged();
                    CheckEnableSavePreset();
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
                    if (fixedEndPath)
                    {
                        supportsHoles = false;
                        FixedEndFlexiPath fep = new FixedEndFlexiPath();
                        if (fixedPathStartPoint != null)
                        {
                            fep.SetBaseSegment(fixedPathStartPoint, fixedPathMidPoint, fixedPathEndPoint);
                            gridSettings.SetPolarCentre(fixedPolarGridCentre.X, fixedPolarGridCentre.Y);
                        }
                        allPaths[0] = fep;
                        selectedFlexiPath = allPaths[0];
                        selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
                        SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        ShowPresets = Visibility.Hidden;
                        ShowSavePresets = Visibility.Hidden;
                    }
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

        public RelayCommand FlipCommand
        {
            get; private set;
        }

        public ICommand GridCommand
        {
            get; set;
        }

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

        public ICommand GridSettingsCommand
        {
            get; set;
        }

        public double GridX
        {
            get
            {
                return gridX;
            }
            set
            {
                gridX = value;
            }
        }

        public double GridY
        {
            get
            {
                return gridY;
            }
            set
            {
                gridY = value;
            }
        }

        public RelayCommand HoleCommand
        {
            get; private set;
        }

        public string ImagePath
        {
            get
            {
                return imagePath;
            }
        }

        public bool IncludeCommonPresets
        {
            get; internal set;
        }

        public bool IsEditingEnabled
        {
            get
            {
                return isEditingEnabled;
            }

            set
            {
                if (value != isEditingEnabled)
                {
                    isEditingEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand LineStyleCommand
        {
            get; private set;
        }

        public RelayCommand LoadImageCommand
        {
            get;
        }

        public ICommand MovePathCommand
        {
            get; set;
        }

        public int NumberOfPaths
        {
            get
            {
                return allPaths.Count;
            }
        }

        public FlexiUserAction OnFlexiUserActive
        {
            get; set;
        }

        public bool OpenEndedPath
        {
            get
            {
                return openEndedPath;
            }

            set
            {
                openEndedPath = value;
                if (selectedFlexiPath != null)
                {
                    selectedFlexiPath.OpenEndedPath = openEndedPath;
                    if (openEndedPath)
                    {
                        ShowPresets = Visibility.Hidden;
                    }
                }
                NotifyPropertyChanged();
            }
        }

        public RelayCommand OrthoLockCommand
        {
            get; set;
        }

        public bool OrthoLocked
        {
            get
            {
                return orthoLocked;
            }

            set
            {
                if (orthoLocked != value)
                {
                    orthoLocked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand PastePathCommand
        {
            get; set;
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

        public ICommand PickCommand
        {
            get; set;
        }

        public ObservableCollection<FlexiPoint> Points
        {
            get
            {
                return selectedFlexiPathControlPoints;
            }

            set
            {
                if (value != selectedFlexiPathControlPoints)
                {
                    selectedFlexiPathControlPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool PointsDirty
        {
            get
            {
                return pointsDirty;
            }

            set
            {
                if (value != pointsDirty)
                {
                    pointsDirty = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand PolarGridCommand
        {
            get; set;
        }

        public String PositionText
        {
            get
            {
                return positionText;
            }

            set
            {
                if (positionText != value)
                {
                    positionText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> PresetNames
        {
            get; set;
        }

        public ICommand ResetPathCommand
        {
            get; set;
        }

        public RelayCommand SavePresetCommand
        {
            get; private set;
        }

        public bool SavePresetEnabled
        {
            get
            {
                return savePresetEnabled;
            }

            set
            {
                if (value != savePresetEnabled)
                {
                    savePresetEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Scale
        {
            get
            {
                return scale;
            }

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
            get
            {
                return screenDpi;
            }

            set
            {
                screenDpi = value;
            }
        }

        public string SelectedCurveName
        {
            get
            {
                return selectedCurveName;
            }

            set
            {
                if (value != selectedCurveName)
                {
                    selectedCurveName = value;
                    NotifyPropertyChanged();
                    SwitchPath(selectedCurveName);
                    NotifyPropertyChanged("Points");
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
                    if (selectedFlexiPathControlPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < selectedFlexiPathControlPoints.Count)
                        {
                            selectedFlexiPathControlPoints[selectedPoint].Selected = true;
                            selectedFlexiPathControlPoints[selectedPoint].Visible = true;
                        }
                    }
                }
            }
        }

        public string SelectedPreset
        {
            get
            {
                return selectedPreset;
            }

            set
            {
                if (value != selectedPreset)
                {
                    selectedPreset = value;
                    NotifyPropertyChanged();
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

        public ICommand ShowAllPointsCommand
        {
            get; set;
        }

        public GridSettings.GridStyle ShowGrid
        {
            get
            {
                return gridSettings.ShowGrid;
            }

            set
            {
                gridSettings.ShowGrid = value;
                if (gridSettings.ShowGrid == GridSettings.GridStyle.Hidden)
                {
                    Snap = false;
                }
                else
                {
                    Snap = true;
                }
                NotifyPropertyChanged();
            }
        }

        public Visibility ShowHoleControls
        {
            get
            {
                return showHoleControls;
            }

            set
            {
                if (showHoleControls != value)
                {
                    showHoleControls = value;
                    NotifyPropertyChanged();
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
                if (showOrtho != value)
                {
                    showOrtho = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowPointsStatus
        {
            get; set;
        }

        public Visibility ShowPresets
        {
            get
            {
                return showPresets;
            }

            set
            {
                if (showPresets != value)
                {
                    showPresets = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowSavePresets
        {
            get
            {
                return showSavePresets;
            }

            set
            {
                if (showSavePresets != value)
                {
                    showSavePresets = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Snap
        {
            get
            {
                return snap;
            }

            set
            {
                if (snap != value)
                {
                    snap = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand SplitQuadBezierCommand
        {
            get; set;
        }

        public bool SupportsHoles
        {
            get
            {
                return supportsHoles;
            }

            set
            {
                if (supportsHoles != value)
                {
                    supportsHoles = value;
                    NotifyPropertyChanged();
                    if (supportsHoles)
                    {
                        ShowHoleControls = Visibility.Visible;
                    }
                    else
                    {
                        ShowHoleControls = Visibility.Hidden;
                    }
                }
            }
        }

        public ICommand SwapArcDirectionCommand
        {
            get; set;
        }

        public ICommand SwapArcSegmentSizeCommand
        {
            get; set;
        }

        public Visibility SwapArcVisible
        {
            get
            {
                return swapArcVisible;
            }

            set
            {
                if (swapArcVisible != value)
                {
                    swapArcVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ToolName
        {
            get
            {
                return toolName;
            }

            set
            {
                if (toolName != value)
                {
                    toolName = value;
                    if (!String.IsNullOrEmpty(toolName))
                    {
                        LoadPresets();
                        if (ShowPresets == Visibility.Visible)
                        {
                            ShowSavePresets = Visibility.Visible;
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        public ICommand ZoomCommand
        {
            get; set;
        }

        public bool ConvertLineAtPointToBezier(System.Windows.Point position, bool cubic)
        {
            bool added = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (selectedFlexiPath.SelectAtPoint(position, true))
            {
                if (cubic)
                {
                    added = selectedFlexiPath.ConvertToCubic(position);
                }
                else
                {
                    added = selectedFlexiPath.ConvertToQuadQuadAtSelected(position);
                }
                if (added)
                {
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    NotifyPropertyChanged("Points");
                }
            }

            return added;
        }

        public bool DeleteSegment(System.Windows.Point position)
        {
            bool deleted = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (selectedFlexiPath.SelectAtPoint(position, true))
            {
                deleted = selectedFlexiPath.DeleteSelectedSegment();

                if (deleted)
                {
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    PointsDirty = true;
                }
            }
            // just in case the segment deletion means the point has gone
            SelectedPoint = -1;
            NotifyPropertyChanged("Points");
            return deleted;
        }

        public PenSetting GetPen()
        {
            return linePen;
        }

        public void LoadImage(string f)
        {
            if (!String.IsNullOrEmpty(f))
            {
                Uri fileUri = new Uri(f);
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = fileUri;
                bmi.EndInit();

                var dpi = screenDpi;
                if (bmi.DpiX == dpi.PixelsPerInchX && bmi.DpiY == dpi.PixelsPerInchY)
                {
                    //use the BitmapImage as it is
                    BackgroundImage = bmi;
                }
                else
                {
                    //create a new BitmapSource and use dpi to set its DPI without changing any pixels
                    PixelFormat pf = PixelFormats.Bgr32;
                    int rawStride = (bmi.PixelWidth * pf.BitsPerPixel + 7) / 8;
                    byte[] rawImage = new byte[rawStride * bmi.PixelHeight];
                    bmi.CopyPixels(rawImage, rawStride, 0);
                    BitmapSource bitmapSource = BitmapSource.Create(bmi.PixelWidth, bmi.PixelHeight,
                    dpi.PixelsPerInchX, dpi.PixelsPerInchY, pf, null, rawImage, rawStride);
                    // dpi.PixelsPerInchX / dpi.DpiScaleX, dpi.PixelsPerInchY / dpi.DpiScaleY, pf,
                    // null, rawImage, rawStride);
                    BackgroundImage = bitmapSource;
                }

                imagePath = f;
                NotifyPropertyChanged("BackgroundImage");
            }
        }

        public void LoadPresets()
        {
            // can have two sets of presets one in the installed data and the other user defined.
            try
            {
                presets.Clear();
                PresetNames.Clear();
                string dataPath = "";
                // some paths will want to show all presets some will only want to show presets of
                // their own
                if (IncludeCommonPresets)
                {
                    dataPath = AppDomain.CurrentDomain.BaseDirectory + "data\\PresetPaths.txt";
                    LoadPresetFile(dataPath, false);

                    dataPath = PathManager.ApplicationDataFolder() + "\\PresetPaths.txt";
                    LoadPresetFile(dataPath, true);
                }
                dataPath = AppDomain.CurrentDomain.BaseDirectory + "data\\" + ToolName + "PresetPaths.txt";
                LoadPresetFile(dataPath, false);
                dataPath = PathManager.UserPresetsPath() + "\\" + ToolName + "PresetPaths.txt";
                LoadPresetFile(dataPath, true);

                foreach (String s in presets.Keys)
                {
                    PresetNames.Add(s);
                }
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogLine(ex.Message);
            }
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
            found = selectedFlexiPath.SelectAtPoint(position, !shift);
            if (!found && allPaths.Count > 1)
            {
                for (int i = 0; i < allPaths.Count; i++)
                {
                    if (allPaths[i] != selectedFlexiPath)
                    {
                        found = allPaths[i].SelectAtPoint(position, !shift);
                        if (found)
                        {
                            SelectedClickedCurve(i);
                        }
                    }
                }
            }
            CanCNVDouble = selectedFlexiPath.HasTwoConsecutiveLineSegmentsSelected();
            return found;
        }

        public void SetOpenEnded(bool open)
        {
            selectedFlexiPath.OpenEndedPath = open;
        }

        public bool SplitLineAtPoint(System.Windows.Point position)
        {
            bool added = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (selectedFlexiPath.SelectAtPoint(position, true))
            {
                if (selectedFlexiPath.SplitSelectedLineSegment(position))
                {
                    added = true;
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    PointsDirty = true;
                    NotifyPropertyChanged("Points");
                }
            }
            return added;
        }

        public bool SplitQuadBezier(System.Windows.Point position)
        {
            bool split = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (selectedFlexiPath.SelectAtPoint(position, true))
            {
                split = selectedFlexiPath.SplitQuadBezier(position);

                if (split)
                {
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    NotifyPropertyChanged("Points");
                }
            }

            return split;
        }

        internal string AbsPathText()
        {
            return selectedFlexiPath.ToPath(true);
        }

        internal Point Centroid()
        {
            return selectedFlexiPath.Centroid();
        }

        internal bool ConvertToArc(Point position, bool clockwise)
        {
            bool converted = false;

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (selectedFlexiPath.SelectAtPoint(position, true))
            {
                converted = selectedFlexiPath.ConvertToArc(position, clockwise);

                if (converted)
                {
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    PointsDirty = true;
                    NotifyPropertyChanged("Points");
                }
            }

            return converted;
        }

        internal void CreateGrid(DpiScale dpiScale, double actualWidth, double actualHeight)
        {
            this.ScreenDpi = dpiScale;
            this.gridWidth = actualWidth;
            this.gridHeight = actualHeight;

            CreateGridMarkers();
        }

        internal List<System.Drawing.PointF> DisplayPointsF()
        {
            return selectedFlexiPath.DisplayPointsF();
        }

        internal FlexiSegment FirstSelectedSegment()
        {
            return selectedFlexiPath.FirstSelectedSegment();
        }

        internal void FromString(string s, bool resetMode = true)
        {
            selectedFlexiPath.FromString(s);
            PathText = selectedFlexiPath.ToPath(absolutePaths);
            LoggerLib.Logger.LogLine($"FlexiPathEditorViewModel::FromString() {selectedFlexiPath.ToOutline()}");
            if (s != "" && resetMode)
            {
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
            PointsDirty = true;
        }

        internal List<Point> GetDisplayPointsForPath(int i)
        {
            List<Point> res = new List<Point>();
            if (i >= 0 && i < allPaths.Count)
            {
                res = allPaths[i].DisplayPoints();
            }
            return res;
        }

        internal string GetPathText(int index)
        {
            string res = "";
            if (index >= 0 && index <= allPaths.Count)
            {
                res = allPaths[index].ToPath(true);
            }
            return res;
        }

        internal void GetSegmentLengthLabel(out string lengthText, out Point labelPos)
        {
            lengthText = "";
            labelPos = new Point(-1, -1);
            FlexiSegment fs = FirstSelectedSegment();
            if (fs != null)
            {
                FlexiPoint ps = selectedFlexiPath.FlexiPoints[fs.Start()];
                FlexiPoint pe = selectedFlexiPath.FlexiPoints[fs.End()];

                double dx = ps.X - pe.X;
                double dy = ps.Y - pe.Y;

                double d = Math.Sqrt(dx * dx + dy * dy);
                lengthText = d.ToString("F3");
                labelPos.X = ToPixelX((ps.X + pe.X) / 2);
                labelPos.Y = ToPixelY((ps.Y + pe.Y) / 2);
            }
        }

        internal bool MouseDown(MouseButtonEventArgs e, System.Windows.Point position)
        {
            bool updateRequired = false;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SwapArcVisible = Visibility.Hidden;

                if (selectionMode == SelectionModeType.StartPoint)
                {
                    AddStartPointToPoly(position);
                    updateRequired = true;
                    e.Handled = true;
                }
                else if (selectionMode == SelectionModeType.AppendPoint)
                {
                    PointsDirty = true;
                    AddAnotherPointToPoly(position);
                    e.Handled = true;
                    updateRequired = true;

                    if (ContinuousPointsNotify)
                    {
                        NotifyPropertyChanged("Points");
                    }
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

                            SelectionMode = SelectionModeType.DraggingPath;
                            updateRequired = true;
                            e.Handled = true;
                        }
                        else
                        {
                            // dont snap position when selecting points as they may have been positioned
                            // between two grid points . just convert position to mm
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
                                    e.Handled = true;
                                }
                            }
                            if (SelectedPoint != -1)
                            {
                                Points[SelectedPoint].Selected = true;
                                moving = true;
                                updateRequired = true;
                                if (Points[SelectedPoint].Mode == FlexiPoint.PointMode.ControlA)
                                {
                                    SwapArcVisible = Visibility.Visible;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerLib.Logger.LogLine(ex.Message);
                    }
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

        internal int PointsInFirstSegment()
        {
            int res = 0;
            if (selectedFlexiPath != null)
            {
                res = selectedFlexiPath.PointsInFirstSegment();
            }
            return res;
        }

        internal void SetFixedEnds(Point fixedPathStartPoint, Point fixedPathMidPoint, Point fixedPathEndPoint)
        {
            this.fixedPathStartPoint = fixedPathStartPoint;
            this.fixedPathMidPoint = fixedPathMidPoint;
            this.fixedPathEndPoint = fixedPathEndPoint;
        }

        internal void SetPath(string v)
        {
            if (v != "")
            {
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }

            selectedFlexiPath.InterpretTextPath(v);
            PathText = selectedFlexiPath.ToPath(absolutePaths);
            PointsDirty = true;
            NotifyPropertyChanged("Points");
        }

        internal void SetPath(string s, int i)
        {
            if (i >= allPaths.Count)
            {
                allPaths.Add(new FlexiPath());
            }
            allPaths[i].FromString(s);
            if (i == 0)
            {
                curveNames.Clear();
                curveNames.Add("Outside");
                nextHoleId = 1;
            }
            else
            {
                curveNames.Add("Hole " + (nextHoleId).ToString());
                nextHoleId++;
            }
        }

        internal void SetPolarGridCentre(Point fixedPolarGridCentre)
        {
            throw new NotImplementedException();
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private void AddAnotherPointToPoly(System.Windows.Point position)
        {
            position = SnapPositionToMM(position);

            if (Math.Abs(position.X - selectedFlexiPath.Start.X) < 2 && Math.Abs(position.Y - selectedFlexiPath.Start.Y) < 2)
            {
                // closeFigure = true;
                selectedPoint = -1;
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                PointsDirty = true;
                selectedFlexiPath.ClosePath();
                NotifyPropertyChanged("Points");
            }
            else
            {
                bool okToSet = true;
                if (orthoLocked)
                {
                    position = selectedFlexiPath.OrthoLockPosition(position);
                    if (editingHole)
                    {
                        okToSet = PointInOutline(position);
                    }
                    if (okToSet)
                    {
                        selectedFlexiPath.AddLine(new System.Windows.Point(position.X, position.Y));
                        moving = false;
                        SelectionMode = SelectionModeType.AppendPoint;
                        selectedPoint = selectedFlexiPathControlPoints.Count - 1;
                        PointsDirty = true;
                    }
                }
                else
                {
                    if (editingHole)
                    {
                        okToSet = PointInOutline(position);
                    }
                    if (okToSet)
                    {
                        selectedFlexiPath.AddLine(new System.Windows.Point(position.X, position.Y));
                        moving = true;
                        SelectionMode = SelectionModeType.AppendPoint;
                        selectedPoint = selectedFlexiPathControlPoints.Count - 1;
                        PointsDirty = true;
                    }
                }
            }
        }

        private void AddStartPointToPoly(System.Windows.Point position)
        {
            position = SnapPositionToMM(position);
            bool okToSet = true;
            if (editingHole)
            {
                okToSet = PointInOutline(position);
            }
            if (okToSet)
            {
                selectedFlexiPath.Start = new FlexiPoint(new System.Windows.Point(position.X, position.Y), 0);
                SelectionMode = SelectionModeType.AppendPoint;
            }
        }

        private void CheckEnableSavePreset()
        {
            bool enable = false;
            if (editedPresetText != "")
            {
                enable = true;
                foreach (string s in presets.Keys)
                {
                    if (s.ToLower() == editedPresetText.ToLower())
                    {
                        enable = false;
                        break;
                    }
                }
            }
            SavePresetEnabled = enable;
        }

        private void ClearAllHoles()
        {
            while (allPaths.Count > 1)
            {
                curveNames.RemoveAt(allPaths.Count - 1);
                allPaths.RemoveAt(allPaths.Count - 1);
            }
            NotifyPropertyChanged("CurveNames");
        }

        private void ClearPointSelections()
        {
            if (selectedFlexiPathControlPoints != null)
            {
                for (int i = 0; i < selectedFlexiPathControlPoints.Count; i++)
                {
                    selectedFlexiPathControlPoints[i].Selected = false;
                    // selectedFlexiPathControlPoints[i].Visible = false;
                }
            }
        }

        private void CreateGridMarkers()
        {
            MakeGrid(gridWidth, gridHeight);
            rectGrid.CreateMarkers(ScreenDpi);
            polarGrid.CreateMarkers(ScreenDpi);
        }

        private void CreatePen()
        {
            Pens = new List<PenSetting>();
            PenSetting ps = new PenSetting((int)gridSettings.LineThickness,
            new SolidColorBrush(gridSettings.LineColour),
            gridSettings.LineOpacity);
            ps.DashPattern.Add(2);
            ps.DashPattern.Add(2);
            linePen = ps;
        }

        private void DeselectAll()
        {
            selectedFlexiPath.DeselectAll();
            foreach (FlexiPoint p in Points)
            {
                p.Selected = false;
                p.Visible = false;
            }
        }

        private bool FixedEndMouseUp(Point position, bool updateRequired)
        {
            if (selectedPoint != -1)
            {
                if (moving)
                {
                    if (selectedPoint == 0)
                    {
                        if (position.Y > ToPixelY(selectedFlexiPathControlPoints[selectedFlexiPathControlPoints.Count - 1].Y))
                        {
                            position.Y = ToPixelY(selectedFlexiPathControlPoints[selectedFlexiPathControlPoints.Count - 1].Y);
                        }
                        position.X = ToPixelX(selectedFlexiPathControlPoints[selectedPoint].X);
                        //polyPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = selectedFlexiPathControlPoints[selectedPoint].X;
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }
                    else if (selectedPoint == selectedFlexiPathControlPoints.Count - 1)
                    {
                        position.X = ToPixelX(selectedFlexiPathControlPoints[selectedPoint].X);
                        if (position.Y < ToPixelY(selectedFlexiPathControlPoints[0].Y))
                        {
                            position.Y = ToPixelY(selectedFlexiPathControlPoints[0].Y);
                        }
                        // polyPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = selectedFlexiPathControlPoints[selectedPoint].X;
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }
                    else
                    {
                        selectedFlexiPathControlPoints[selectedPoint].X = position.X;
                        selectedFlexiPathControlPoints[selectedPoint].Y = position.Y;
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }

                    PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";
                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    updateRequired = true;
                    selectedPoint = -1;
                    NotifyPropertyChanged("Points");
                }
            }
            moving = false;
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
                        if (position.Y > ToPixelY(selectedFlexiPathControlPoints[selectedFlexiPathControlPoints.Count - 1].Y))
                        {
                            position.Y = ToPixelY(selectedFlexiPathControlPoints[selectedFlexiPathControlPoints.Count - 1].Y);
                        }
                        position.X = ToPixelX(selectedFlexiPathControlPoints[selectedPoint].X);

                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = selectedFlexiPathControlPoints[selectedPoint].X;
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }
                    else if (selectedPoint == selectedFlexiPathControlPoints.Count - 1)
                    {
                        position.X = ToPixelX(selectedFlexiPathControlPoints[selectedPoint].X);
                        if (position.Y < ToPixelY(selectedFlexiPathControlPoints[0].Y))
                        {
                            position.Y = ToPixelY(selectedFlexiPathControlPoints[0].Y);
                        }

                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        snappedPos.X = selectedFlexiPathControlPoints[selectedPoint].X;
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }
                    else
                    {
                        System.Windows.Point snappedPos = SnapPositionToMM(position);
                        selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                        PointsDirty = true;
                    }

                    PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";

                    PathText = selectedFlexiPath.ToPath(absolutePaths);
                    updateRequired = true;
                }
                else
                {
                    moving = false;
                }
            }

            return updateRequired;
        }

        private void LoadPresetFile(string dataPath, bool user)
        {
            if (File.Exists(dataPath))
            {
                String[] lines = System.IO.File.ReadAllLines(dataPath);
                if (lines.GetLength(0) > 0)
                {
                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        string[] words = lines[i].Split('=');
                        if (words.GetLength(0) == 2)
                        {
                            Preset p = new Preset();
                            p.Name = words[0];
                            p.Path = words[1];
                            p.User = user;
                            presets[words[0]] = p;
                        }
                    }
                }
            }
        }

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

        private void MoveWholePath(System.Windows.Point position)
        {
            System.Windows.Point offset = selectedFlexiPath.MoveTo(position);
            if (!editingHole)
            {
                for (int i = 1; i < allPaths.Count; i++)
                {
                    allPaths[i].MoveByOffset(offset);
                }
            }
            PointsDirty = true;
        }

        private bool NormalMouseUp(Point position, bool updateRequired)
        {
            if (selectionMode == SelectionModeType.DraggingPath)
            {
                position = SnapPositionToMM(position);
                MoveWholePath(position);
                updateRequired = true;
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
            else
            {
                if (selectedPoint != -1 && moving)
                {
                    selectedArcPoint = -1;
                    SwapArcVisible = Visibility.Hidden;

                    System.Windows.Point positionSnappedToMM = SnapPositionToMM(position);
                    bool okToSet = true;
                    if (editingHole)
                    {
                        okToSet = PointInOutline(positionSnappedToMM);
                    }
                    if (okToSet)
                    {
                        // if the selected point is the control point for an arc
                        // it isn't snapped to the grid but has to follow the arc control line
                        if (Points[selectedPoint].Mode == FlexiPoint.PointMode.ControlA)
                        {
                            Point unsnappedPositionMM = new Point(0, 0);
                            unsnappedPositionMM.X = ToMMX(position.X);
                            unsnappedPositionMM.Y = ToMMY(position.Y);

                            Point snappedPos = selectedFlexiPath.ArcSnap(selectedPoint, unsnappedPositionMM);

                            selectedFlexiPathControlPoints[selectedPoint].X = snappedPos.X;
                            selectedFlexiPathControlPoints[selectedPoint].Y = snappedPos.Y;

                            selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                            selectedArcPoint = selectedPoint;
                            SwapArcVisible = Visibility.Visible;
                        }
                        else
                        {
                            selectedFlexiPathControlPoints[selectedPoint].X = position.X;
                            selectedFlexiPathControlPoints[selectedPoint].Y = position.Y;

                            selectedFlexiPath.SetPointPos(selectedPoint, positionSnappedToMM);
                        }
                        PointsDirty = true;
                        updateRequired = true;
                        selectedPoint = -1;
                        NotifyPropertyChanged("Points");
                    }
                }
            }
            PathText = selectedFlexiPath.ToPath(absolutePaths);
            moving = false;
            return updateRequired;
        }

        private bool NormalPathMove(MouseEventArgs e, Point position, bool updateRequired)
        {
            if (selectionMode == SelectionModeType.DraggingPath)
            {
                position = SnapPositionToMM(position);
                MoveWholePath(position);
                updateRequired = true;
                e.Handled = true;
            }
            else
            {
                System.Windows.Point snappedPos = SnapPositionToMM(position);

                PositionText = $"({position.X.ToString("F3")},{position.Y.ToString("F3")})";
                if (selectedPoint != -1)
                {
                    if (e.LeftButton == MouseButtonState.Pressed && moving)
                    {
                        // if the selected point is the control point for an arc
                        // it isn't snapped to the grid but has to follow the arc control line
                        if (Points[selectedPoint].Mode == FlexiPoint.PointMode.ControlA)
                        {
                            Point unsnappedPositionMM = new Point(0, 0);
                            unsnappedPositionMM.X = ToMMX(position.X);
                            unsnappedPositionMM.Y = ToMMY(position.Y);
                            snappedPos = selectedFlexiPath.ArcSnap(selectedPoint, unsnappedPositionMM);
                            // snappedPos = new Point(ToMMX(snappedPos.X), ToMMY(snappedPos.Y));
                        }
                        // polyPoints[selectedPoint].X = position.X; polyPoints[selectedPoint].Y = position.Y;
                        bool okToSet = true;
                        if (editingHole)
                        {
                            okToSet = PointInOutline(snappedPos);
                        }
                        if (okToSet)
                        {
                            selectedFlexiPath.SetPointPos(selectedPoint, snappedPos);
                            PathText = selectedFlexiPath.ToPath(absolutePaths);
                            updateRequired = true;
                        }
                    }
                    else
                    {
                        moving = false;
                    }
                }
            }
            return updateRequired;
        }

        private void NotifyUserActive()
        {
            if (OnFlexiUserActive != null)
            {
                OnFlexiUserActive();
            }
        }

        private void OnAddCubic(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToCubicBezier;
            NotifyUserActive();
        }

        private void OnAddQuad(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToQuadBezier;
            NotifyUserActive();
        }

        private void OnAddSegment(object obj)
        {
            SelectionMode = SelectionModeType.SplitLine;
            NotifyUserActive();
        }

        private void OnAntiClockwiseArc(object obj)
        {
            SelectionMode = SelectionModeType.AntiClockwiseArc;
            NotifyUserActive();
        }

        private void OnAppendPoint(object obj)
        {
            SelectionMode = SelectionModeType.AppendPoint;
            NotifyUserActive();
        }

        private void OnApplyPreset(object obj)
        {
            NotifyUserActive();
            if (!String.IsNullOrEmpty(selectedPreset))
            {
                if (MessageBox.Show("This will replace the path completely. The old one can't be recovered. Continue", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    FromString(presets[selectedPreset].Path, true);
                    NotifyPropertyChanged("Points");
                }
            }
        }

        private void OnCnvDoubleSegment(object obj)
        {
            NotifyUserActive();
            if (canCNVDouble)
            {
                selectedFlexiPath.ConvertTwoLineSegmentsToQuadraticBezier();
                PathText = selectedFlexiPath.ToPath(absolutePaths);
                CanCNVDouble = false;
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                NotifyPropertyChanged("Points");
            }
        }

        private void OnCopy(object obj)
        {
            NotifyUserActive();
            PathText = selectedFlexiPath.ToPath(absolutePaths);
            System.Windows.Clipboard.SetText(PathText);
        }

        private void OnDeleteSegment(object obj)
        {
            NotifyUserActive();
            SelectionMode = SelectionModeType.DeleteSegment;
        }

        private void OnFlip(object obj)
        {
            NotifyUserActive();
            string cmd = obj.ToString();
            switch (cmd.ToLower())
            {
                case "horizontal":
                    {
                        selectedFlexiPath.FlipHorizontal();
                        pointsDirty = true;
                    }
                    break;

                case "vertical":
                    {
                        selectedFlexiPath.FlipVertical();
                        pointsDirty = true;
                    }
                    break;
            }
            PathText = selectedFlexiPath.ToPath(absolutePaths);
            NotifyPropertyChanged("Points");
        }

        private void OnGrid(object obj)
        {
            NotifyUserActive();
            switch (gridSettings.ShowGrid)
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

        private void OnGridSettings(object obj)
        {
            NotifyUserActive();
            GridSettingsDlg dlg = new GridSettingsDlg();
            dlg.Settings = gridSettings;
            if (dlg.ShowDialog() == true)
            {
                rectGrid.SetGridIntervals(gridSettings.RectangularGridSize, gridSettings.RectangularGridSize);
                polarGrid.SetGridIntervals(gridSettings.PolarGridRadius, gridSettings.PolarGridAngle);
                CreateGridMarkers();
                CreatePen();
                // force a screen refresh
                NotifyPropertyChanged("Points");
                gridSettings.Save();
            }
        }

        private void OnHoleCommand(object obj)
        {
            NotifyUserActive();
            string cmd = obj.ToString();
            switch (cmd.ToLower())
            {
                case "add":
                    {
                        if (supportsHoles)
                        {
                            FlexiPath nfp = new FlexiPath();
                            allPaths.Add(nfp);
                            selectedFlexiPath = nfp;
                            selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
                            curveNames.Add("Hole " + (nextHoleId).ToString());
                            nextHoleId++;
                            selectedCurveName = curveNames[curveNames.Count - 1];
                            NotifyPropertyChanged("CurveNames");
                            NotifyPropertyChanged("SelectedCurveName");
                            selectedPoint = -1;
                            SelectionMode = SelectionModeType.StartPoint;
                            editingHole = true;
                            PointsDirty = true;
                            NotifyPropertyChanged("Points");
                        }
                    }
                    break;

                case "delete":
                    {
                        if (selectedFlexiPath == allPaths[0])
                        {
                            MessageBox.Show("Can't delete the outer path");
                        }
                        else
                        {
                            int target = -1;
                            for (int i = 1; i < allPaths.Count; i++)
                            {
                                if (selectedFlexiPath == allPaths[i])
                                {
                                    target = i;
                                }
                            }
                            if (target != -1)
                            {
                                allPaths.RemoveAt(target);
                                curveNames.RemoveAt(target);
                            }
                            NotifyPropertyChanged("CurveNames");
                            selectedPoint = -1;
                            selectedFlexiPath = allPaths[0];
                            selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
                            selectedCurveName = curveNames[0];
                            NotifyPropertyChanged("CurveNames");
                            NotifyPropertyChanged("SelectedCurveName");
                            editingHole = false;
                            PointsDirty = true;
                            NotifyPropertyChanged("Points");
                        }
                    }
                    break;

                case "deleteall":
                    {
                        while (allPaths.Count > 1)
                        {
                            allPaths.RemoveAt(1);
                        }
                        curveNames.Clear();
                        curveNames.Add("Outline");

                        selectedPoint = -1;
                        selectedFlexiPath = allPaths[0];
                        selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
                        selectedCurveName = curveNames[0];
                        NotifyPropertyChanged("CurveNames");
                        NotifyPropertyChanged("SelectedCurveName");
                        editingHole = false;
                        PointsDirty = true;
                        NotifyPropertyChanged("Points");
                    }
                    break;
            }
        }

        private void OnLineStyle(object obj)
        {
        }

        private void OnLoadImage(object obj)
        {
            NotifyUserActive();
            OpenFileDialog opDlg = new OpenFileDialog();
            if (!String.IsNullOrEmpty(defaultImagePath))
            {
                if (Directory.Exists(defaultImagePath))
                {
                    opDlg.InitialDirectory = defaultImagePath;
                }
            }
            opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
            if (opDlg.ShowDialog() == true)
            {
                try
                {
                    LoadImage(opDlg.FileName);
                }
                catch (Exception ex)
                {
                    LoggerLib.Logger.LogLine(ex.Message);
                }
            }
        }

        private void OnMovePath(object obj)
        {
            NotifyUserActive();
            SelectionMode = SelectionModeType.MovePath;
        }

        private void OnPaste(object obj)
        {
            NotifyUserActive();
            string pth = System.Windows.Clipboard.GetText();
            if (pth != null && pth != "" && pth.StartsWith("M"))
            {
                PathText = pth;
                selectedFlexiPath.InterpretTextPath(pth);
                PointsDirty = true;
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                NotifyPropertyChanged("Points");
            }
        }

        private void OnPickSegment(object obj)
        {
            NotifyUserActive();
            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
        }

        private void OnPolarGrid(object obj)
        {
            NotifyUserActive();
            switch (gridSettings.ShowGrid)
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

        private void OnResetPath(object obj)
        {
            NotifyUserActive();
            bool clear = true;
            if (selectedFlexiPath == allPaths[0] && allPaths.Count > 1)
            {
                MessageBoxResult res = MessageBox.Show("Resetting outline will remove all holes too. Do you want to continue?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    clear = false;
                }
                else
                {
                    ClearAllHoles();
                }
            }
            if (clear)
            {
                selectedFlexiPath.Clear();
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
                PointsDirty = true;
                NotifyPropertyChanged("Points");
            }
        }

        private void OnSavePreset(object obj)
        {
            NotifyUserActive();
            String preset = editedPresetText;
            if (!String.IsNullOrEmpty(preset))
            {
                if (presets.ContainsKey(preset))
                {
                    MessageBox.Show("A preset with that name already exists", "Error");
                }
                else
                {
                    SaveAsPreset(toolName, preset, PathText);
                }
            }
        }

        private void OnShowAllPoints(object obj)
        {
            NotifyUserActive();
            ShowPointsStatus = !ShowPointsStatus;
            foreach (FlexiPath fp in allPaths)
            {
                foreach (FlexiPoint p in fp.FlexiPoints)
                {
                    p.Visible = ShowPointsStatus;
                }
            }

            NotifyPropertyChanged("Points");
        }

        private void OnSplitQuad(object obj)
        {
            NotifyUserActive();
            SelectionMode = SelectionModeType.SplitQuad;
        }

        private void OnSwapArcDirection(object obj)
        {
            if (selectedArcPoint != -1 && selectedFlexiPath != null)
            {
                selectedFlexiPath.ArcSwapDirection(selectedArcPoint);
                pointsDirty = true;
                NotifyPropertyChanged("Points");
            }
            NotifyUserActive();
        }

        private void OnSwapArcSegmentSize(object obj)
        {
            if (selectedArcPoint != -1 && selectedFlexiPath != null)
            {
                selectedFlexiPath.SwapArcSegmentSize(selectedArcPoint);
                pointsDirty = true;
                NotifyPropertyChanged("Points");
            }
            NotifyUserActive();
        }

        private void OnToggleOrthoLock(object obj)
        {
            NotifyUserActive();
            OrthoLocked = !OrthoLocked;
        }

        private void OnZoom(object obj)
        {
            NotifyUserActive();
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

        private bool PointInOutline(Point position)
        {
            bool inside = false;
            ConvexPolygon2D path = new ConvexPolygon2D(allPaths[0].DisplayPoints().ToArray());
            ConvexPolygon2DHelper interceptor = new ConvexPolygon2DHelper();
            if (interceptor.IsPointInsidePoly(position, path))
            {
                inside = true;
            }
            return inside;
        }

        private void SaveAsPreset(string toolName, string preset, string pathText)
        {
            if (!String.IsNullOrEmpty(toolName) && !String.IsNullOrEmpty(preset) && !String.IsNullOrEmpty(pathText))
            {
                string dataPath = PathManager.UserPresetsPath() + "\\" + ToolName + "PresetPaths.txt";
                System.IO.File.AppendAllText(dataPath, $"{preset}={pathText}\n");

                Preset p = new Preset();
                p.Name = preset;
                p.Path = pathText;
                p.User = true;
                presets[preset] = p;
                PresetNames.Clear();
                foreach (String s in presets.Keys)
                {
                    PresetNames.Add(s);
                }
                NotifyPropertyChanged("PresetNames");
            }
        }

        private void SelectedClickedCurve(int i)
        {
            NotifyUserActive();
            selectedFlexiPath = allPaths[i];
            selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
            selectedPoint = -1;
            selectedCurveName = curveNames[i];
            NotifyPropertyChanged("SelectedCurveName");
            NotifyPropertyChanged("Points");
            editingHole = i > 0;
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

        private void SwitchPath(string name)
        {
            ClearPointSelections();
            for (int i = 0; i < curveNames.Count; i++)
            {
                if (curveNames[i] == name)
                {
                    selectedFlexiPath = allPaths[i];
                    PathText = selectedFlexiPath.ToPath(true);
                    selectedFlexiPathControlPoints = selectedFlexiPath.FlexiPoints;
                    selectedPoint = -1;
                    SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                    editingHole = i > 0;
                    break;
                }
            }
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

        private double ToScreenX(double x)
        {
            double res = ScreenDpi.PixelsPerInchX * x / 25.4;
            return res;
        }

        private double ToScreenY(double y)
        {
            double res = ScreenDpi.PixelsPerInchY * y / 25.4;
            return res;
        }

        public struct Preset
        {
            public String Name
            {
                get; set;
            }

            public String Path
            {
                get; set;
            }

            public bool User
            {
                get; set;
            }
        }
    }
}