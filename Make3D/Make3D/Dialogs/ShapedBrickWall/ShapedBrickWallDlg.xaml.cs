using MakerLib;
using System;
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
            string path = "";
            ShapedBrickWallMaker maker = new ShapedBrickWallMaker(path, brickLength, brickHeight, mortarGap);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("BrickLength") != "")
            {
                BrickLength = EditorParameters.GetDouble("BrickLength");
            }

            if (EditorParameters.Get("BrickHeight") != "")
            {
                BrickHeight = EditorParameters.GetDouble("BrickHeight");
            }

            if (EditorParameters.Get("MortarGap") != "")
            {
                MortarGap = EditorParameters.GetDouble("MortarGap");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BrickLength", BrickLength.ToString());
            EditorParameters.Set("BrickHeight", BrickHeight.ToString());
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