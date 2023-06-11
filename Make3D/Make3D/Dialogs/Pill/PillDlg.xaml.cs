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
        private double trayThickness;
        public double TrayThickness
        {
            get { return trayThickness; }
            set
            {
                if (trayThickness != value)
                {
                    trayThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }
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
        private const double minTrayThickness = 0.1;
        public String TrayThicknessToolTip
        {
            get
            {
                return $"Tray thickness must be in the range {minTrayThickness} to half Flat Length";
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

        private int shape;

        private bool plainSelected;

        public bool PlainSelected
        {
            get { return plainSelected; }
            set
            {
                if (plainSelected != value)
                {
                    plainSelected = value;
                    NotifyPropertyChanged();
                    if (plainSelected)
                    {
                        shape = 0;
                        ShowTray = Visibility.Hidden;
                        UpdateDisplay();
                    }
                }
            }
        }

        private bool halfSelected;

        public bool HalfSelected
        {
            get { return halfSelected; }
            set
            {
                if (halfSelected != value)
                {
                    halfSelected = value;
                    NotifyPropertyChanged();
                    if (halfSelected)
                    {
                        shape = 1;
                        ShowTray = Visibility.Hidden;
                        UpdateDisplay();
                    }
                }
            }
        }

        private Visibility showTray;
        public Visibility ShowTray
        {
            get { return showTray; }
            set
            {
                if (showTray != value)
                {
                    showTray = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private bool traySelected;

        public bool TraySelected
        {
            get { return traySelected; }
            set
            {
                if (traySelected != value)
                {
                    traySelected = value;

                    NotifyPropertyChanged();
                    if (traySelected)
                    {
                        shape = 2;
                        ShowTray = Visibility.Visible;
                        UpdateDisplay();
                    }
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            PillMaker maker = new PillMaker(flatLength, flatHeight, edge, pillWidth, trayThickness, shape);
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
            PlainSelected = EditorParameters.GetBoolean("PlainSelected", true);
            HalfSelected = EditorParameters.GetBoolean("HalfSelected", false);
            TraySelected = EditorParameters.GetBoolean("TraySelected", false);
            TrayThickness = EditorParameters.GetDouble("TrayThickness", 1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("FlatLength", FlatLength.ToString());
            EditorParameters.Set("FlatHeight", FlatHeight.ToString());
            EditorParameters.Set("Edge", Edge.ToString());
            EditorParameters.Set("PillWidth", PillWidth.ToString());
            EditorParameters.Set("PlainSelected", PlainSelected.ToString());
            EditorParameters.Set("HalfSelected", HalfSelected.ToString());
            EditorParameters.Set("TraySelected", TraySelected.ToString());
            EditorParameters.Set("TrayThickness", TrayThickness.ToString());
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