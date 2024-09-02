using MakerLib;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for StoneWall.xaml
    /// </summary>
    public partial class StoneWallDlg : BaseModellerDialog, INotifyPropertyChanged
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

        private double wallThickness;

        public double WallThickness
        {
            get
            {
                return wallThickness;
            }
            set
            {
                if (wallThickness != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        wallThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        private int stoneSize;

        public int StoneSize
        {
            get
            {
                return stoneSize;
            }
            set
            {
                if (stoneSize != value)
                {
                    if (value >= 5 && value <= wallHeight / 2 && value <= wallLength / 2)
                    {
                        stoneSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                    else
                    {
                        WarningText = "Stone size myust be in the range 5 to wallLength / 2 or wallHeight / 2";
                    }
                }
            }
        }

        public StoneWallDlg()
        {
            InitializeComponent();
            ToolName = "StoneWall";
            wallLength = 100;
            wallHeight = 80;
            wallThickness = 4;
            stoneSize = 5;
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
            StoneWallMaker maker = new StoneWallMaker(wallLength, wallHeight, wallThickness, stoneSize);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("WallLength") != "")
            {
                WallLength = EditorParameters.GetDouble("WallLength");
            }

            if (EditorParameters.Get("WallHeight") != "")
            {
                WallHeight = EditorParameters.GetDouble("WallHeight");
            }

            if (EditorParameters.Get("WallThickness") != "")
            {
                WallThickness = EditorParameters.GetDouble("WallThickness");
            }

            if (EditorParameters.Get("StoneSize") != "")
            {
                StoneSize = EditorParameters.GetInt("StoneSize");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("StoneSize", StoneSize.ToString());
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