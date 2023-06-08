using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Pill.xaml
    /// </summary>
    public partial class PillDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minflatLength = 1;
        private const double maxflatLength = 100;
        private double flatLength;

        public double FlatLength
        {
            get
            {
                return flatLength;
            }
            set
            {
                if (flatLength != value)
                {
                    if (value >= minflatLength && value <= maxflatLength)
                    {
                        flatLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FlatLengthToolTip
        {
            get
            {
                return $"Flat Length must be in the range {minflatLength} to {maxflatLength}";
            }
        }

        private const double minflatHeight = 1;
        private const double maxflatHeight = 100;
        private double flatHeight;

        public double FlatHeight
        {
            get
            {
                return flatHeight;
            }
            set
            {
                if (flatHeight != value)
                {
                    if (value >= minflatHeight && value <= maxflatHeight)
                    {
                        flatHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FlatHeightToolTip
        {
            get
            {
                return $"Flat Height must be in the range {minflatHeight} to {maxflatHeight}";
            }
        }

        private const double minedge = 1;
        private const double maxedge = 100;
        private double edge;

        public double Edge
        {
            get
            {
                return edge;
            }
            set
            {
                if (edge != value)
                {
                    if (value >= minedge && value <= maxedge)
                    {
                        edge = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeToolTip
        {
            get
            {
                return $"Edge must be in the range {minedge} to {maxedge}";
            }
        }

        private const double minpillWidth = 2;
        private const double maxpillWidth = 200;
        private double pillWidth;

        public double PillWidth
        {
            get
            {
                return pillWidth;
            }
            set
            {
                if (pillWidth != value)
                {
                    if (value >= minpillWidth && value <= maxpillWidth)
                    {
                        pillWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PillWidthToolTip
        {
            get
            {
                return $"Pill Width must be in the range {minpillWidth} to {maxpillWidth}";
            }
        }

        public PillDlg()
        {
            InitializeComponent();
            ToolName = "Pill";
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

        private bool frontOnly;

        public bool FrontOnly
        {
            get { return frontOnly; }
            set
            {
                if (frontOnly != value)
                {
                    frontOnly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            PillMaker maker = new PillMaker(flatLength, flatHeight, edge, pillWidth, frontOnly);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            FlatLength = EditorParameters.GetDouble("FlatLength", 20);

            FlatHeight = EditorParameters.GetDouble("FlatHeight", 20);

            Edge = EditorParameters.GetDouble("Edge", 5);

            PillWidth = EditorParameters.GetDouble("PillWidth", 10);
            FrontOnly = EditorParameters.GetBoolean("FrontOnly", false);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("FlatLength", FlatLength.ToString());
            EditorParameters.Set("FlatHeight", FlatHeight.ToString());
            EditorParameters.Set("Edge", Edge.ToString());
            EditorParameters.Set("PillWidth", PillWidth.ToString());
            EditorParameters.Set("FrontOnly", FrontOnly.ToString());
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