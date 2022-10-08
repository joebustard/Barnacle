using MakerLib;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for BrickWall.xaml
    /// </summary>
    public partial class BrickWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private double wallLength;

        public double WallLength
        {
            get
            {
                return wallLength;
            }
            set
            {
                if (wallLength != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double wallHeight;

        public double WallHeight
        {
            get
            {
                return wallHeight;
            }
            set
            {
                if (wallHeight != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private double wallWidth;

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
                    if (value >= 1 && value <= 600)
                    {
                        wallWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

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
                    if (value >= 1 && value <= 100)
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
                    if (value >= 1 && value <= 100)
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
                    if (value >= 0 && value <= 100)
                    {
                        mortarGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public BrickWallDlg()
        {
            InitializeComponent();
            ToolName = "BrickWall";
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
            BrickWallMaker maker = new BrickWallMaker(wallLength, wallHeight, wallWidth, brickLength, brickLength / 2, brickHeight, mortarGap);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("WallLength") != "")
            {
                wallLength = EditorParameters.GetDouble("WallLength");
            }

            if (EditorParameters.Get("WallHeight") != "")
            {
                wallHeight = EditorParameters.GetDouble("WallHeight");
            }

            if (EditorParameters.Get("WallWidth") != "")
            {
                wallWidth = EditorParameters.GetDouble("WallWidth");
            }

            if (EditorParameters.Get("BrickLength") != "")
            {
                brickLength = EditorParameters.GetDouble("BrickLength");
            }

            if (EditorParameters.Get("BrickHeight") != "")
            {
                brickHeight = EditorParameters.GetDouble("BrickHeight");
            }

            if (EditorParameters.Get("MortarGap") != "")
            {
                mortarGap = EditorParameters.GetDouble("MortarGap");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
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
            wallLength = 200;
            wallHeight = 100;
            wallWidth = 8;
            brickLength = 12;

            brickHeight = 6;
            mortarGap = 2;
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            NotifyPropertyChanged("WallLength");
            NotifyPropertyChanged("WallHeight");

            NotifyPropertyChanged("WallWidth");
            NotifyPropertyChanged("BrickLength");
            NotifyPropertyChanged("BrickHeight");
            NotifyPropertyChanged("MortarGap");

            UpdateDisplay();
        }
    }
}