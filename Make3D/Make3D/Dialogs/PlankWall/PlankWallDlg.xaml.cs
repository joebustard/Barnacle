using MakerLib;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PlankWall.xaml
    /// </summary>
    public partial class PlankWallDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double gap;
        private double gapDepth;
        private bool loaded;
        private double plankWidth;
        private double wallHeight;
        private double wallLength;
        private double wallWidth;
        private string warningText;
        public PlankWallDlg()
        {
            InitializeComponent();
            ToolName = "PlankWall";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            WallHeight = 25;
            WallLength = 50;
            WallWidth = 10;
            PlankWidth = 5;
            Gap = 1;
            GapDepth = 2;
        }

        public double Gap
        {
            get
            {
                return gap;
            }
            set
            {
                if (gap != value)
                {
                    if (value >= 1 && value <= 300)
                    {
                        gap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double GapDepth
        {
            get
            {
                return gapDepth;
            }
            set
            {
                if (gapDepth != value)
                {
                    if (value >= 1 && value <= 300)
                    {
                        gapDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double PlankWidth
        {
            get
            {
                return plankWidth;
            }
            set
            {
                if (plankWidth != value)
                {
                    if (value >= 1 && value <= 300)
                    {
                        plankWidth = value;
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
                    if (value >= 1 && value <= 300)
                    {
                        wallHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

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
                    if (value >= 1 && value <= 300)
                    {
                        wallLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
                    if (value >= 1 && value <= 300)
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
            PlankWallMaker maker = new PlankWallMaker(
                wallLength, wallHeight, wallWidth, plankWidth, gap, gapDepth
                );
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

            if (EditorParameters.Get("WallWidth") != "")
            {
                WallWidth = EditorParameters.GetDouble("WallWidth");
            }

            if (EditorParameters.Get("PlankWidth") != "")
            {
                PlankWidth = EditorParameters.GetDouble("PlankWidth");
            }

            if (EditorParameters.Get("Gap") != "")
            {
                Gap = EditorParameters.GetDouble("Gap");
            }

            if (EditorParameters.Get("GapDepth") != "")
            {
                GapDepth = EditorParameters.GetDouble("GapDepth");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("WallLength", WallLength.ToString());
            EditorParameters.Set("WallHeight", WallHeight.ToString());
            EditorParameters.Set("WallWidth", WallWidth.ToString());
            EditorParameters.Set("PlankWidth", PlankWidth.ToString());
            EditorParameters.Set("Gap", Gap.ToString());
            EditorParameters.Set("GapDepth", GapDepth.ToString());
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