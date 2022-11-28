using MakerLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapedBrickWall.xaml
    /// </summary>
    public partial class ShapedBrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double brickDepth;
        private double brickHeight;
        private double brickLength;
        private List<Point> displayPoints;
        private bool loaded;
        private double mortarGap;
        private double wallWidth;
        private string warningText;

        public ShapedBrickWallDlg()
        {
            InitializeComponent();
            ToolName = "ShapedBrickWall";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            brickLength = 3;
            brickHeight = 1.1;
            brickDepth = 0.25;
            mortarGap = 0.25;
            wallWidth = 2;
        }

        public double BrickDepth
        {
            get
            {
                return brickDepth;
            }
            set
            {
                if (brickDepth != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        brickDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double BrickHeight
        {
            get
            {
                return brickHeight;
            }
            set
            {
                if (brickHeight != value)
                {
                    if (value >= 1 && value <= 50)
                    {
                        brickHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double BrickLength
        {
            get
            {
                return brickLength;
            }
            set
            {
                if (brickLength != value)
                {
                    if (value >= 1 && value <= 50)
                    {
                        brickLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double MortarGap
        {
            get
            {
                return mortarGap;
            }
            set
            {
                if (mortarGap != value)
                {
                    if (value >= 0.1 && value <= 50)
                    {
                        mortarGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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

        public double WallWidth
        {
            get
            {
                return wallWidth;
            }
            set
            {
                if (wallWidth != value)
                {
                    if (value >= 1 && value <= 50)
                    {
                        wallWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public string WarningText
        {
            get
            {
                return warningText;
            }
            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            if (displayPoints != null)
            {
                string path = PathEditor.GetPath();
                ShapedBrickWallMaker maker = new ShapedBrickWallMaker(path, brickLength, brickHeight, brickDepth, wallWidth, mortarGap);
                maker.Generate(Vertices, Faces);
            }
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            BrickLength = EditorParameters.GetDouble("BrickLength", 3);
            BrickHeight = EditorParameters.GetDouble("BrickHeight", 1.1);
            BrickDepth = EditorParameters.GetDouble("BrickDepth", 0.25);
            MortarGap = EditorParameters.GetDouble("MortarGap", 0.25);
            WallWidth = EditorParameters.GetDouble("WallWidth", 2);
            PathEditor.SetPath(EditorParameters.Get("Path"));
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            if (PathEditor.PathClosed)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
            EditorParameters.Set("BrickDepth", MortarGap.ToString());
            EditorParameters.Set("MortarGap", MortarGap.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("Path", PathEditor.GetPath()); ;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }
    }
}