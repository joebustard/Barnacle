using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ShapedBrickWall.xaml
    /// </summary>
    public partial class ShapedBrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;
        private List<Point> displayPoints;
        private double brickLength;

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

        private double brickHeight;

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


        private double brickDepth;

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
                    if (value >= 1 && value <= 50)
                    {
                        brickDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double mortarGap;

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
                    if (value >= 1 && value <= 50)
                    {
                        mortarGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public ShapedBrickWallDlg()
        {
            InitializeComponent();
            ToolName = "ShapedBrickWall";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            brickLength = 8;
            brickHeight = 4;
            brickDepth = 1;
            mortarGap = 2;
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
                ShapedBrickWallMaker maker = new ShapedBrickWallMaker(path, brickLength, brickHeight, brickDepth, mortarGap);
                maker.Generate(Vertices, Faces);
            }
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            BrickLength = EditorParameters.GetDouble("BrickLength", 8);
            BrickHeight = EditorParameters.GetDouble("BrickHeight", 4);
            BrickDepth = EditorParameters.GetDouble("BrickDepth", 2);
            MortarGap = EditorParameters.GetDouble("MortarGap", 2);

        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
            EditorParameters.Set("BrickDepth", MortarGap.ToString());
            EditorParameters.Set("MortarGap", MortarGap.ToString());

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