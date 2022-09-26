using Barnacle.Dialogs;
using Barnacle.Models.Mru;
using Barnacle.ViewModel.BuildPlates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Barnacle.ViewModels
{
    internal class DefaultViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public List<ToolDef> aircraftToolsToShow;
        public List<ToolDef> decorativeToolsToShow;
        public List<ToolDef> loftedToolsToShow;
        public List<ToolDef> parametricToolsToShow;
        public List<ToolDef> vehicleToolsToShow;
        public List<ToolDef> buildingToolsToShow;

        private static string statusBlockText1;
        private static string statusBlockText2;
        private static string statusBlockText3;
        private static Control subView;
        private ObservableCollection<FontFamily> _systemFonts = new ObservableCollection<FontFamily>();
        private bool bezierRingEnabled;
        private bool boldChecked;
        private BuildPlateManager buildPlateManager;
        private List<String> buildPlateNames;
        private string caption;
        private bool centerTextAlignment;
        private string currentViewName;
        private bool doughnutEnabled;
        private string fontSize;
        private bool fuselageEnabled;
        private bool irregularEnabled;
        private bool italicChecked;
        private bool leftTextAlignment;
        private ImageSource libraryImageSource;
        private Visibility libraryVisibility;
        private bool linearEnabled;
        private Ribbon MainRibbon;
        private bool profileFuselageEnabled;
        private bool rightTextAlignment;
        private String selectedBuildPlate;
        private string selectedObjectName;
        private bool settingsLoaded;
        private bool showAxiesChecked;
        private bool showBuildPlate;
        private bool showFloorChecked;
        private bool showFloorMarkerChecked;
        private bool snapMarginChecked;
        private bool spurGearEnabled;
        private bool stadiumEnabled;
        private SubViewManager subViewMan;
        private bool tankTrackEnabled;
        private Visibility toolPaletteVisible;
        private bool tubeEnabled;
        private bool twoShapeEnabled;
        private bool underLineChecked;
        private bool wingEnabled;
        private bool editingActive;
        private bool canSlice;
        public bool CanSlice
        {
            get { return canSlice; }
            set
            {
                if (canSlice != value)
                {
                    canSlice = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DefaultViewModel()
        {
            settingsLoaded = false;
            subViewMan = new SubViewManager();
            NewCommand = new RelayCommand(OnNew);
            NewProjectCommand = new RelayCommand(OnNewProject);
            OpenProjectCommand = new RelayCommand(OnOpenProject);
            OpenCommand = new RelayCommand(OnOpen);
            OpenRecentFileCommand = new RelayCommand(OnOpenRecent);
            SaveCommand = new RelayCommand(OnSave);
            SaveAsCommand = new RelayCommand(OnSaveAs);
            InsertCommand = new RelayCommand(OnInsert);
            ReferenceCommand = new RelayCommand(OnReference);
            ManifoldCommand = new RelayCommand(OnManifoldTest);
            UndoCommand = new RelayCommand(OnUndo);
            //   RedoCommand = new RelayCommand(OnRedo);
            CopyCommand = new RelayCommand(OnCopy);
            PasteCommand = new RelayCommand(OnPaste);
            PasteAtCommand = new RelayCommand(OnPasteAt);
            MultiPasteCommand = new RelayCommand(OnMultiPaste);
            CircularPasteCommand = new RelayCommand(OnCircularPaste);
            CutCommand = new RelayCommand(OnCut);

            AddCommand = new RelayCommand(OnAdd);

            ZoomInCommand = new RelayCommand(OnZoomIn);
            ZoomOutCommand = new RelayCommand(OnZoomOut);
            Zoom100Command = new RelayCommand(onZoomReset);

            AlignCommand = new RelayCommand(OnAlignment);
            MarkerCommand = new RelayCommand(OnMarker);
            DistributeCommand = new RelayCommand(OnDistribute);
            FlipCommand = new RelayCommand(OnFlip);
            SizeCommand = new RelayCommand(OnSize);
            GroupCommand = new RelayCommand(OnGroup);
            SelectCommand = new RelayCommand(OnSelect);
            ImportCommand = new RelayCommand(OnImport);
            ExportCommand = new RelayCommand(OnExport);
            ExportPartsCommand = new RelayCommand(OnExportParts);
            SliceCommand = new RelayCommand(OnSlice);
            SplitCommand = new RelayCommand(OnSplit);
            BendCommand = new RelayCommand(OnBend);
            FoldCommand = new RelayCommand(OnFold);
            SettingsCommand = new RelayCommand(OnSettings);
            TextAlignmentCommand = new RelayCommand(OnTextAlignment);
            ToolCommand = new RelayCommand(OnTool);

            TwoShapeCommand = new RelayCommand(OnTwoShape);
            SpurGearCommand = new RelayCommand(OnSpurGear);
            TankTrackCommand = new RelayCommand(OnTankTrack);
            MeshEditCommand = new RelayCommand(OnMeshEdit);
            MeshHullCommand = new RelayCommand(OnHullEdit);
            DupVertexCommand = new RelayCommand(OnDupVertex);
            UnrefVertexCommand = new RelayCommand(OnUnrefVertex);
            MeshSmoothCommand = new RelayCommand(OnLoopSmooth);
            ResetOriginCommand = new RelayCommand(OnReorigin);
            ViewCommand = new RelayCommand(OnView);
            AboutCommand = new RelayCommand(OnAbout);
            showFloorChecked = false;

            ExitCommand = new RelayCommand(OnExit);

            Caption = BaseViewModel.Document.Caption;

            FillColor = Brushes.White;
            SelectedObjectName = "";
            LoadSystemFonts();
            SelectedFont = "Arial";
            FontSize = "14";
            snapMarginChecked = true;
            StatusBlockText1 = "Status Text 1";
            StatusBlockText2 = "V" + SoftwareVersion;

            buildPlateNames = new List<string>();
            buildPlateManager = new BuildPlateManager();
            buildPlateManager.Initialise();

            foreach (BuildPlate bp in buildPlateManager.BuildPlates)
            {
                buildPlateNames.Add(bp.PrinterName);
            }

            BaseViewModel.Document.PropertyChanged += Document_PropertyChanged;

            NotificationManager.Subscribe("SetFontName", SetFontName);
            NotificationManager.Subscribe("SetFontSize", SetFontSize);
            NotificationManager.Subscribe("SetFontBold", SetFontBold);
            NotificationManager.Subscribe("SetFontItalic", SetFontItalic);
            NotificationManager.Subscribe("SetFontUnderLine", SetFontUnderline);
            NotificationManager.Subscribe("SetTextAlignment", SetTextAlignment);
            NotificationManager.Subscribe("SetStatusText1", SetStatusText1);
            NotificationManager.Subscribe("SetStatusText3", SetStatusText3);
            NotificationManager.Subscribe("SetToolsVisibility", SetToolVisibility);
            NotificationManager.Subscribe("SetSingleToolsVisible", SetSingleToolVisible);
            NotificationManager.Subscribe("ObjectNamesChanged", ObjectNamesChanged);
            NotificationManager.Subscribe("Loading", LoadingNewFile);
            NotificationManager.Subscribe("SuspendEditing", SuspendEditing);

            SubView = subViewMan.GetView("editor");
            CreateToolMenus();
            editingActive = true;
            EnableAllTools(true);

            NotificationManager.Notify("ProjectChanged", Project);

            LoadShowSettings();
            LoadPartLibrary();
        }

        private void SuspendEditing(object param)
        {
            bool b = Convert.ToBoolean(param);
            EditingActive = !b;
            EnableAllTools(editingActive);
        }

        public ICommand AboutCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand AddPageCommand { get; set; }

        public List<ToolDef> AircraftToolsToShow
        {
            get
            {
                return aircraftToolsToShow;
            }
            set
            {
                if (aircraftToolsToShow != value)
                {
                    aircraftToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand AlignCommand { get; set; }

        public ICommand BendCommand { get; set; }

        public bool BezierRingEnabled
        {
            get
            {
                return bezierRingEnabled;
            }
            set
            {
                if (bezierRingEnabled != value)
                {
                    bezierRingEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EditingActive
        {
            get
            {
                return editingActive;
            }
            set
            {
                if (editingActive != value)
                {
                    editingActive = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool BoldChecked
        {
            get
            {
                return boldChecked;
            }
            set
            {
                if (value != boldChecked)
                {
                    boldChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Bold", boldChecked);
                }
            }
        }

        public List<String> BuildPlateNames
        {
            get
            {
                return buildPlateNames;
            }
            set
            {
                if (buildPlateNames != value)
                {
                    buildPlateNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CenterTextAlignment
        {
            get
            {
                return centerTextAlignment;
            }
            set
            {
                if (value != centerTextAlignment)
                {
                    centerTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand CircularPasteCommand { get; set; }

        public ICommand CopyCommand { get; set; }

        public ICommand CutCommand { get; set; }

        public List<ToolDef> DecorativeToolsToShow
        {
            get
            {
                return decorativeToolsToShow;
            }
            set
            {
                if (decorativeToolsToShow != value)
                {
                    decorativeToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand DistributeCommand { get; set; }

        public ICommand DoNothingCommand { get; set; }

        public ICommand DoughnutCommand { get; set; }

        public bool DoughnutEnabled
        {
            get
            {
                return doughnutEnabled;
            }
            set
            {
                if (doughnutEnabled != value)
                {
                    doughnutEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand DupVertexCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand ExportPartsCommand { get; set; }

        public ICommand FlipCommand { get; set; }

        public String FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FontSize", fontSize);
                }
            }
        }

        public ICommand FuselageCommand { get; set; }

        public bool FuselageEnabled
        {
            get
            {
                return fuselageEnabled;
            }
            set
            {
                if (fuselageEnabled != value)
                {
                    fuselageEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand GroupCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand InsertCommand { get; set; }

        public bool IrregularEnabled
        {
            get
            {
                return irregularEnabled;
            }
            set
            {
                if (irregularEnabled != value)
                {
                    irregularEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ItalicChecked
        {
            get
            {
                return italicChecked;
            }
            set
            {
                if (value != italicChecked)
                {
                    italicChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Italic", italicChecked);
                }
            }
        }

        public bool LeftTextAlignment
        {
            get
            {
                return leftTextAlignment;
            }
            set
            {
                if (value != leftTextAlignment)
                {
                    leftTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource LibraryImageSource
        {
            get
            {
                return libraryImageSource;
            }
            set
            {
                if (libraryImageSource != value)
                {
                    libraryImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility LibraryVisibility
        {
            get
            {
                return libraryVisibility;
            }
            set
            {
                if (libraryVisibility != value)
                {
                    libraryVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand LinearCommand { get; set; }

        public bool LinearEnabled
        {
            get
            {
                return linearEnabled;
            }

            set
            {
                if (linearEnabled != value)
                {
                    linearEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<ToolDef> LoftedToolsToShow
        {
            get
            {
                return loftedToolsToShow;
            }
            set
            {
                if (loftedToolsToShow != value)
                {
                    loftedToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ManifoldCommand { get; set; }

        public ICommand MarkerCommand { get; set; }

        public ICommand MeshEditCommand { get; set; }

        public ICommand MeshHullCommand { get; set; }

        public ICommand MeshSmoothCommand { get; set; }

        public ICommand MultiPasteCommand { get; set; }

        public ICommand NewCommand { get; set; }
        public ICommand NewProjectCommand { get; set; }

        public ObservableCollection<string> ObjectNames
        {
            get
            {
                ObservableCollection<string> res = null;
                if (document != null)
                {
                    res = document.GetObjectNames();
                }
                return res;
            }
        }

        public ICommand OpenCommand { get; set; }

        public ICommand OpenProjectCommand { get; set; }

        public ICommand OpenRecentFileCommand { get; set; }

        public ICommand PageCommand { get; set; }

        public List<ToolDef> ParametricToolsToShow
        {
            get
            {
                return parametricToolsToShow;
            }
            set
            {
                if (parametricToolsToShow != value)
                {
                    parametricToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand PasteAtCommand { get; set; }

        public ICommand PasteCommand { get; set; }
        public ICommand FoldCommand { get; set; }

        public ICommand PrintCommand { get; set; }

        public ICommand PrintPreviewCommand { get; set; }

        public bool ProfileFuselageEnabled
        {
            get
            {
                return profileFuselageEnabled;
            }
            set
            {
                if (profileFuselageEnabled != value)
                {
                    profileFuselageEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<MruEntry> RecentFilesList
        {
            get
            {
                return RecentlyUsedManager.RecentFilesList;
            }
        }

        public ICommand RedoCommand { get; set; }

        public ICommand ReferenceCommand { get; set; }

        public ICommand ResetOriginCommand { get; set; }

        public bool RightTextAlignment
        {
            get
            {
                return rightTextAlignment;
            }
            set
            {
                if (value != rightTextAlignment)
                {
                    rightTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand SaveAsCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand SelectCommand { get; set; }

        public String SelectedBuildPlate
        {
            get
            {
                return selectedBuildPlate;
            }
            set
            {
                if (selectedBuildPlate != value)
                {
                    selectedBuildPlate = value;
                    NotifyPropertyChanged();
                    SelectedBuildPlateUpdated();
                    if (settingsLoaded)
                    {
                        SaveShowSettings();
                    }
                }
            }
        }

        public string SelectedObjectName
        {
            get
            {
                return selectedObjectName;
            }

            set
            {
                if (selectedObjectName != value)
                {
                    selectedObjectName = value;
                    NotifyPropertyChanged();

                    if (selectedObjectName != "")
                    {
                        NotificationManager.Notify("SelectObjectName", selectedObjectName);
                    }
                }
            }
        }

        public ICommand SettingsCommand { get; set; }

        public bool ShowAxiesChecked
        {
            get
            {
                return showAxiesChecked;
            }
            set
            {
                if (showAxiesChecked != value)
                {
                    showAxiesChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ShowAxies", showAxiesChecked);
                }
            }
        }

        public bool ShowBuildPlateChecked
        {
            get
            {
                return showBuildPlate;
            }
            set
            {
                if (showBuildPlate != value)
                {
                    showBuildPlate = value;
                    NotifyPropertyChanged();

                    NotificationManager.Notify("ShowBuildPlate", showBuildPlate);
                    if (settingsLoaded)
                    {
                        SaveShowSettings();
                    }
                }
            }
        }

        public bool ShowFloorChecked
        {
            get
            {
                return showFloorChecked;
            }
            set
            {
                if (showFloorChecked != value)
                {
                    showFloorChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ShowFloor", showFloorChecked);
                    if (settingsLoaded)
                    {
                        SaveShowSettings();
                    }
                }
            }
        }

        public bool ShowFloorMarkerChecked
        {
            get
            {
                return showFloorMarkerChecked;
            }
            set
            {
                if (showFloorMarkerChecked != value)
                {
                    showFloorMarkerChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ShowFloorMarker", showFloorMarkerChecked);
                    if (settingsLoaded)
                    {
                        SaveShowSettings();
                    }
                }
            }
        }

        public ICommand SizeCommand { get; set; }

        public ICommand SliceCommand { get; set; }

        public bool SnapMarginChecked
        {
            get
            {
                return snapMarginChecked;
            }
            set
            {
                if (snapMarginChecked != value)
                {
                    snapMarginChecked = value;
                    if (snapMarginChecked)
                    {
                        StatusBlockText2 = "Snap Margin On";
                    }
                    else
                    {
                        StatusBlockText2 = "Snap Margin Off";
                    }
                    NotifyPropertyChanged();
                    NotificationManager.Notify("SnapMargin", snapMarginChecked);
                }
            }
        }

        public ICommand SplitCommand { get; set; }

        public ICommand SpurGearCommand { get; set; }

        public bool SpurGearEnabled
        {
            get
            {
                return spurGearEnabled;
            }

            set
            {
                if (spurGearEnabled != value)
                {
                    spurGearEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool StadiumEnabled
        {
            get
            {
                return stadiumEnabled;
            }
            set
            {
                if (stadiumEnabled != value)
                {
                    stadiumEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String StatusBlockText1
        {
            get
            {
                return statusBlockText1;
            }

            set
            {
                if (statusBlockText1 != value)
                {
                    statusBlockText1 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String StatusBlockText2
        {
            get
            {
                return statusBlockText2;
            }

            set
            {
                if (statusBlockText2 != value)
                {
                    statusBlockText2 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String StatusBlockText3
        {
            get
            {
                return statusBlockText3;
            }

            set
            {
                if (statusBlockText3 != value)
                {
                    statusBlockText3 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Control SubView
        {
            get
            {
                return subView;
            }
            set
            {
                if (subView != value)
                {
                    subView = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<FontFamily> SystemFonts
        {
            get { return _systemFonts; }
        }

        public ICommand TankTrackCommand { get; set; }

        public bool TankTrackEnabled
        {
            get
            {
                return tankTrackEnabled;
            }

            set
            {
                if (tankTrackEnabled != value)
                {
                    tankTrackEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand TextAlignmentCommand { get; set; }

        public ICommand ToolCommand { get; set; }

        public Visibility ToolPaletteVisible
        {
            get
            {
                return toolPaletteVisible;
            }

            set
            {
                if (toolPaletteVisible != value)
                {
                    toolPaletteVisible = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ToolPaletteVisible", toolPaletteVisible);
                }
            }
        }

        public bool TubeEnabled
        {
            get
            {
                return tubeEnabled;
            }
            set
            {
                if (tubeEnabled != value)
                {
                    tubeEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand TwoShapeCommand { get; set; }

        public bool TwoShapeEnabled
        {
            get
            {
                return twoShapeEnabled;
            }

            set
            {
                if (twoShapeEnabled != value)
                {
                    twoShapeEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool UnderlineChecked
        {
            get
            {
                return underLineChecked;
            }
            set
            {
                if (value != underLineChecked)
                {
                    underLineChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Underline", underLineChecked);
                }
            }
        }

        public ICommand UndoCommand { get; set; }

        public ICommand UnrefVertexCommand { get; set; }

        public List<ToolDef> VehicleToolsToShow
        {
            get
            {
                return vehicleToolsToShow;
            }
            set
            {
                if (vehicleToolsToShow != value)
                {
                    vehicleToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }



        public List<ToolDef> BuildingToolsToShow
        {
            get
            {
                return buildingToolsToShow;
            }
            set
            {
                if (buildingToolsToShow != value)
                {
                    buildingToolsToShow = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ICommand ViewCommand { get; set; }

        public bool WingEnabled
        {
            get
            {
                return wingEnabled;
            }
            set
            {
                if (wingEnabled != value)
                {
                    wingEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand Zoom100Command { get; set; }

        public ICommand ZoomInCommand { get; set; }

        public ICommand ZoomOutCommand { get; set; }

        public void LoadSystemFonts()
        {
            _systemFonts.Clear();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());

            foreach (var f in fonts)
                _systemFonts.Add(f);
        }

        internal void SetRibbonMenu(Ribbon mainRibbon)
        {
            MainRibbon = mainRibbon;
        }

        internal void SwitchToView(string v, bool checkForScript = true)
        {
            if (currentViewName == "Script" && checkForScript)
            {
                NotificationManager.Notify("LimpetClosing", "");
            }
            if (v != currentViewName)
            {
                currentViewName = v;
                OnView(v);
            }
        }

        private void CreateAircraftToolMenu()
        {
            aircraftToolsToShow = new List<ToolDef>();
            aircraftToolsToShow.Add(new ToolDef("Bezier Fuselage", true, "BezierFuselage", "Create a fuselage from bezier profiles."));
            aircraftToolsToShow.Add(new ToolDef("Profile Fuselage", true, "ProfileFuselage", "Create a fuselage from  externally prepared top, bottom and spar images."));
            aircraftToolsToShow.Add(new ToolDef("Wing", true, "Wing", "Create a wing from a database of airfoil profiles."));
            aircraftToolsToShow.Add(new ToolDef("Canvas Wing", true, "CanvasWing", "Create a wing that looks like canvas streteched over struts from a database of airfoil profiles."));
            aircraftToolsToShow.Add(new ToolDef("Propeller", true, "Propeller", "Create a propeller."));
            aircraftToolsToShow.Add(new ToolDef("Turbo Fan", true, "TurboFan", "Create a turbofan."));
            SortMenu(aircraftToolsToShow);
            NotifyPropertyChanged("AircraftToolsToShow");
        }

        private void CreateDecorativeToolMenu()
        {
            decorativeToolsToShow = new List<ToolDef>();
            decorativeToolsToShow.Add(new ToolDef("Bicorn", true, "Bicorn", "Create a bicorn shape."));
            decorativeToolsToShow.Add(new ToolDef("Squirkle", true, "Squirkle", "Create a squirkle shape."));
            decorativeToolsToShow.Add(new ToolDef("Trickle", true, "Trickle", "Create a trickle shape."));
            decorativeToolsToShow.Add(new ToolDef("Text", true, "Text", "Create Text."));
            SortMenu(decorativeToolsToShow);
            NotifyPropertyChanged("DecorativeToolsToShow");
        }

        private void CreateLoftingMenu()
        {
            loftedToolsToShow = new List<ToolDef>();
            loftedToolsToShow.Add(new ToolDef("Vertical", true, "LinearLoft", "Create an object by vertical lofting."));
            loftedToolsToShow.Add(new ToolDef("Two Shape", true, "TwoShape", "Loft a solid by connecting a top and bottom shape together."));
            loftedToolsToShow.Add(new ToolDef("Ring", true, "BezierRing", "Create a ring using bezier curves."));
            //  loftedToolsToShow.Add(new ToolDef("Scribble", true, "Scribble", "Draw a polyline which is lofted upwards."));
            SortMenu(loftedToolsToShow);
            NotifyPropertyChanged("LoftedToolsToShow");
        }

        private void CreateBuildingMenu()
        {
            buildingToolsToShow = new List<ToolDef>();
            buildingToolsToShow.Add(new ToolDef("Brick Wall", true, "BrickWall", "Create a brick wall."));
            buildingToolsToShow.Add(new ToolDef("Stone Wall", true, "StoneWall", "Create a stone wall."));
            buildingToolsToShow.Add(new ToolDef("Tiled Roof", true, "TiledRoof", "Create a tiled roof."));
            SortMenu(buildingToolsToShow);
            NotifyPropertyChanged("BuildingToolsToShow");
        }
        private void CreateParametricMenu()
        {
            parametricToolsToShow = new List<ToolDef>();
            parametricToolsToShow.Add(new ToolDef("Bezier Surface", true, "BezierSurface", "Create a surface using control points."));
            //   parametricToolsToShow.Add(new ToolDef("Figure", true, "Figure", "Create a basic figure."));
            parametricToolsToShow.Add(new ToolDef("Reuleaux Polygon", true, "Reuleaux", "Create a Reuleaux polygon."));
            parametricToolsToShow.Add(new ToolDef("Parabolic Dish", true, "ParabolicDish", "Create a parabolic dish."));
            parametricToolsToShow.Add(new ToolDef("Parallelogram", true, "Parallelogram", "Create a parallelogram."));
            parametricToolsToShow.Add(new ToolDef("Platelet", true, "Platelet", "Create an object from a polygon optionaly overlayed on an external image."));
            parametricToolsToShow.Add(new ToolDef("Dual", true, "Dual", "Create an object from two polygons."));
            parametricToolsToShow.Add(new ToolDef("Spur Gear", true, "SpurGear", "Create a spur gear with a variable number of teeth."));
            parametricToolsToShow.Add(new ToolDef("Squared Stadium", true, "SquaredStadium", "Create a stadium or sausage with one end a variable radius and the other square."));

            parametricToolsToShow.Add(new ToolDef("Stadium", true, "Stadium", "Create a stadium or sausage with variable end radii."));
            parametricToolsToShow.Add(new ToolDef("Star", true, "Star", "Create a star."));
            parametricToolsToShow.Add(new ToolDef("Thread", true, "Thread", "Create a thread for a bolt or nut."));
            parametricToolsToShow.Add(new ToolDef("Torus", true, "Torus", "Create a torus."));

            parametricToolsToShow.Add(new ToolDef("Trapezoid", true, "Trapazoid", "Create a trapazoid."));

            parametricToolsToShow.Add(new ToolDef("Tube", true, "Tube", "Create a partial or full tube with bevelled ends."));


            parametricToolsToShow.Add(new ToolDef("DevTest", true, "DevTest", "Test dialog"));
            parametricToolsToShow.Add(new ToolDef("Curved Funnel", true, "CurvedFunnel", "Create a curved funnel"));

            SortMenu(parametricToolsToShow);
            NotifyPropertyChanged("ParametricToolsToShow");
        }

        private void SortMenu(List<ToolDef> tools)
        {
            ToolDef tmp;
            bool swapped = true;
            while (swapped)
            {
                swapped = false;
                for (int i = 0; i < tools.Count - 1; i++)
                {
                    if (String.Compare(tools[i].Name, tools[i + 1].Name) > 0)
                    {
                        tmp = tools[i];
                        tools[i] = tools[i + 1];
                        tools[i + 1] = tmp;
                        swapped = true;
                    }
                }
            }
        }

        private void CreateToolMenus()
        {
            CreateParametricMenu();
            CreateLoftingMenu();
            CreateVehicleToolMenu();
            CreateAircraftToolMenu();
            CreateDecorativeToolMenu();
            CreateBuildingMenu();
        }

        private void CreateVehicleToolMenu()
        {
            vehicleToolsToShow = new List<ToolDef>();
            vehicleToolsToShow.Add(new ToolDef("Tank Track", true, "TankTrack", "Create a tank track by drawing overan external image."));
            vehicleToolsToShow.Add(new ToolDef("Wheel", true, "Wheel", "Create a variety of wheel designs."));
            vehicleToolsToShow.Add(new ToolDef("Rail Wheel", true, "RailWheel", "Create a basic rail wheel."));
            vehicleToolsToShow.Add(new ToolDef("Wagon Wheel", true, "WagonWheel", "Create a spoked wagon wheel."));
            vehicleToolsToShow.Add(new ToolDef("Pulley", true, "Pulley", "Create a basic pulley wheel."));
            SortMenu(vehicleToolsToShow);
            NotifyPropertyChanged("VehicleToolsToShow");
        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void EnableAllTools(bool b)
        {
            LinearEnabled = b;
            DoughnutEnabled = b;
            IrregularEnabled = b;
            FuselageEnabled = b;
            TwoShapeEnabled = b;
            SpurGearEnabled = b;
            TankTrackEnabled = b;
            StadiumEnabled = b;
            BezierRingEnabled = b;
            TubeEnabled = b;
            ProfileFuselageEnabled = b;
            WingEnabled = b;
            EnabledToolIst(b, parametricToolsToShow);
            EnabledToolIst(b, loftedToolsToShow);
            EnabledToolIst(b, vehicleToolsToShow);
            EnabledToolIst(b, aircraftToolsToShow);
            EnabledToolIst(b, decorativeToolsToShow);
            EnabledToolIst(b, buildingToolsToShow);
        }

        private void EnabledToolIst(bool b, List<ToolDef> defs)
        {
            if (defs != null)
            {
                foreach (ToolDef df in defs)
                {
                    df.IsActive = b;
                }
            }
        }

        private void LoadingNewFile(object param)
        {
        }

        private void LoadPartLibrary()
        {
            string pth = GetPartsLibraryPath() + "//BarnaclePartsLibrary.bmf";

            // hide the parts library tab incase we fail to load it
            LibraryVisibility = Visibility.Hidden;
            if (File.Exists(pth))
            {
                PartLibraryProject = new VisualSolutionExplorer.Project();
                PartLibraryProject.Open(pth);
                PartLibraryProject.MarkAsReadOnly();
                LibraryVisibility = Visibility.Visible;
            }
        }

        private void LoadShowSettings()
        {
            Properties.Settings.Default.Reload();
            ShowBuildPlateChecked = Properties.Settings.Default.ShowBuildPlate;
            ShowFloorChecked = Properties.Settings.Default.ShowFloor;
            ShowFloorMarkerChecked = Properties.Settings.Default.ShowMarker;
            ShowAxiesChecked = Properties.Settings.Default.ShowAxis;
            SelectedBuildPlate = Properties.Settings.Default.SelectedBuildPlate;
            CheckIfCanSlice();

            settingsLoaded = true;
            NotificationManager.Notify("ShowBuildPlate", showBuildPlate);
            NotificationManager.Notify("ShowFloor", showFloorChecked);
            NotificationManager.Notify("ShowFloorMarker", showFloorMarkerChecked);
            NotificationManager.Notify("ShowAxies", showAxiesChecked);
            SelectedBuildPlateUpdated();
        }

        private void CheckIfCanSlice()
        {
            CanSlice = false;
            if (Properties.Settings.Default.SlicerPath != null)
            {
                if (File.Exists(System.IO.Path.Combine(Properties.Settings.Default.SlicerPath, "cura.exe")))
                {
                    CanSlice = true;

                }
            }
        }

        private void ObjectNamesChanged(object param)
        {
            NotifyPropertyChanged("ObjectNames");
        }

        private void OnAbout(object obj)
        {
            AboutBoxDlg dlg = new AboutBoxDlg();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void OnAdd(object obj)
        {
            string name = obj.ToString();
            NotificationManager.Notify("AddObject", name);
            NotifyPropertyChanged("ObjectNames");
        }

        private void OnAlignment(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Alignment", s);
        }

        private void OnBend(object obj)
        {
            NotificationManager.Notify("Bend", obj);
        }
        private void OnFold(object obj)
        {
            NotificationManager.Notify("Fold", obj);
        }

        private void OnCircularPaste(object obj)
        {
            NotificationManager.Notify("CircularPaste", null);
        }

        private void OnCopy(object obj)
        {
            NotificationManager.Notify("Copy", null);
        }

        private void OnCut(object obj)
        {
            NotificationManager.Notify("Cut", null);
        }

        private void OnDistribute(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Distribute", s);
        }

        private void OnDoNothing(object obj)
        {
        }

        private void OnDupVertex(object obj)
        {
            NotificationManager.Notify("RemoveDupVertices", null);
        }

        private void OnExit(object obj)
        {
            if (BaseViewModel.Document.Dirty)
            {
                NotificationManager.Notify("CheckExit", null);
            }
            else
            {
                // UndoManager.Clear();
                App.Current.Shutdown();
            }
        }

        private void OnExport(object obj)
        {
            NotificationManager.Notify("Export", obj);
        }

        private void OnExportParts(object obj)
        {
            NotificationManager.Notify("ExportParts", obj);
        }

        private void OnFlip(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Flip", s);
        }

        private void OnFuselage(object obj)
        {
            NotificationManager.Notify("Fuselage", null);
        }

        private async void OnGroup(object obj)
        {
            string s = obj.ToString();
            await NotificationManager.NotifyTask("Group", s);
        }

        private void OnHullEdit(object obj)
        {
            NotificationManager.Notify("MeshHull", null);
        }

        private void OnImport(object obj)
        {
            NotificationManager.Notify("Import", obj);
        }

        private void OnInsert(object obj)
        {
            NotificationManager.Notify("InsertFile", obj);
        }

        private void OnLoopSmooth(object obj)
        {
            NotificationManager.Notify("LoopSmooth", null);
        }

        private void OnManifoldTest(object obj)
        {
            NotificationManager.Notify("ManifoldTest", null);
        }

        private void OnMarker(object obj)
        {
            NotificationManager.Notify("Marker", null);
        }

        private void OnMeshEdit(object obj)
        {
            NotificationManager.Notify("MeshEdit", null);
        }

        private void OnMultiPaste(object obj)
        {
            NotificationManager.Notify("MultiPaste", null);
        }

        private void OnNew(object obj)
        {
            NotificationManager.Notify("NewFile", null);
        }

        private void OnNewProject(object obj)
        {
            NotificationManager.Notify("NewProject", null);
            CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
        }

        private void OnOpen(object obj)
        {
            NotificationManager.Notify("OpenFile", null);
            Caption = BaseViewModel.Document.Caption;
        }

        private void OnOpenProject(object obj)
        {
            NotificationManager.Notify("OpenProject", null);
            CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
        }

        private void OnOpenRecent(object obj)
        {
            string f = obj.ToString();

            NotificationManager.Notify("OpenRecentFile", f);
            Caption = BaseViewModel.Document.Caption;
        }

        private void OnPaste(object obj)
        {
            NotificationManager.Notify("Paste", null);
        }

        private void OnPasteAt(object obj)
        {
            NotificationManager.Notify("PasteAt", null);
        }

        private void OnPrint(object obj)
        {
            NotificationManager.Notify("Print", null);
        }

        private void OnRedo(object obj)
        {
            NotificationManager.Notify("Redo", null);
        }

        private void OnReference(object obj)
        {
            NotificationManager.Notify("Reference", null);
        }

        private void OnReorigin(object obj)
        {
            NotificationManager.Notify("Reorigin", null);
        }

        private void OnSave(object obj)
        {
            NotificationManager.Notify("SaveFile", null);
            Caption = BaseViewModel.Document.Caption;
            //UpdateRecentFiles(BaseViewModel.Document.FilePath);
        }

        private void OnSaveAs(object obj)
        {
            NotificationManager.Notify("SaveAsFile", null);
            Caption = BaseViewModel.Document.Caption;
            //  UpdateRecentFiles(BaseViewModel.Document.FilePath);
        }

        private void OnSelect(object obj)
        {
            NotificationManager.Notify("Select", obj);
        }

        private void OnSettings(object obj)
        {
            NotificationManager.Notify("Settings", null);
            CheckIfCanSlice();
        }

        private void OnSize(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Size", s);
        }

        private void OnSlice(object obj)
        {
            NotificationManager.Notify("Slice", obj);
        }

        private void OnSplit(object obj)
        {
            NotificationManager.Notify("Split", obj);
        }

        private void OnSpurGear(object obj)
        {
            NotificationManager.Notify("SpurGear", obj);
        }

        private void OnTankTrack(object obj)
        {
            NotificationManager.Notify("TankTrack", obj);
        }

        private void OnTextAlignment(object obj)
        {
            NotificationManager.Notify("TextAlignment", obj);
        }

        private void OnTool(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Tool", s);
        }

        private void OnTwoShape(object obj)
        {
            NotificationManager.Notify("TwoShape", null);
        }

        private void OnUndo(object obj)
        {
            NotificationManager.Notify("Undo", null);
        }

        private void OnUnrefVertex(object obj)
        {
            NotificationManager.Notify("UnrefVertices", null);
        }

        private void OnView(object obj)
        {
            String vwName = obj.ToString();
            vwName = vwName.ToLower();
            if (vwName == "editor")
            {
                ToolPaletteVisible = Visibility.Visible;
                NotificationManager.Notify("SolutionPanel", true);
            }
            else
            {
                ToolPaletteVisible = Visibility.Collapsed;
                NotificationManager.Notify("SolutionPanel", false);
            }
            subViewMan.CloseCurrent();
            SubView = subViewMan.GetView(vwName);
        }

        private void OnZoomIn(object obj)
        {
            NotificationManager.Notify("ZoomIn", null);
        }

        private void OnZoomOut(object obj)
        {
            NotificationManager.Notify("ZoomOut", null);
        }

        private void onZoomReset(object obj)
        {
            NotificationManager.Notify("ZoomReset", null);
            NotificationManager.Notify("UpdateDisplay", null);
        }

        private void SaveShowSettings()
        {
            Properties.Settings.Default.ShowBuildPlate = ShowBuildPlateChecked;
            Properties.Settings.Default.ShowFloor = ShowFloorChecked;
            Properties.Settings.Default.ShowMarker = ShowFloorMarkerChecked;
            Properties.Settings.Default.ShowAxis = ShowAxiesChecked;
            Properties.Settings.Default.SelectedBuildPlate = SelectedBuildPlate;
            Properties.Settings.Default.Save();
        }

        private void SelectedBuildPlateUpdated()
        {
            BuildPlate bp = buildPlateManager.FindBuildPlate(selectedBuildPlate);
            if (bp != null)
            {
                NotificationManager.Notify("BuildPlate", bp);
            }
        }

        private void SetActive(string s, List<ToolDef> defs)
        {
            foreach (ToolDef df in defs)
            {
                if (df.CommandParam == s)
                {
                    df.IsActive = true;
                    break;
                }
            }
        }

        private void SetFontBold(object param)
        {
            bool b = (bool)param;
            BoldChecked = b;
        }

        private void SetFontItalic(object param)
        {
            bool b = (bool)param;
            ItalicChecked = b;
        }

        private void SetFontName(object param)
        {
            SelectedFont = param.ToString();
        }

        private void SetFontSize(object param)
        {
            double fs = (double)param;
            FontSize = fs.ToString();
        }

        private void SetFontUnderline(object param)
        {
            bool b = (bool)param;
            UnderlineChecked = b;
        }

        private void SetSingleToolVisible(object param)
        {
            string s = param.ToString();

            SetActive(s, parametricToolsToShow);
            SetActive(s, loftedToolsToShow);
            SetActive(s, vehicleToolsToShow);
            SetActive(s, aircraftToolsToShow);
            SetActive(s, decorativeToolsToShow);
        }

        private void SetStatusText1(object param)
        {
            StatusBlockText1 = param.ToString();
        }

        private void SetStatusText3(object param)
        {
            StatusBlockText3 = param.ToString();
        }

        private void SetTextAlignment(object param)
        {
            TextAlignment ta = (TextAlignment)param;
            switch (ta)
            {
                case TextAlignment.Left:
                    {
                        LeftTextAlignment = true;
                    }
                    break;

                case TextAlignment.Right:
                    {
                        RightTextAlignment = true;
                    }
                    break;

                case TextAlignment.Center:
                    {
                        CenterTextAlignment = true;
                    }
                    break;
            }
        }

        private void SetToolVisibility(object param)
        {
            bool b = Convert.ToBoolean(param);
            EnableAllTools(b);
        }

        private void UpdateRecentFiles(string fileName)
        {
            RecentlyUsedManager.UpdateRecentFiles(fileName);
            CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
        }
    }
}