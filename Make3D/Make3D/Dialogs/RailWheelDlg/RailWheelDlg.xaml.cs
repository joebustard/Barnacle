using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RailWheel.xaml
    /// </summary>
    public partial class RailWheelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axleBoreRadius;
        private double flangeRadius;
        private double flangeThickness;
        private double hubRadius;
        private double hubThickness;
        private double mainRadius;
        private double mainThickness;
        private string warningText;

        public RailWheelDlg()
        {
            InitializeComponent();
            ToolName = "RailWheel";
            DataContext = this;
            ModelGroup = MyModelGroup;
        }

        public double AxleBoreRadius
        {
            get
            {
                return axleBoreRadius;
            }
            set
            {
                if (axleBoreRadius != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        axleBoreRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double FlangeRadius
        {
            get
            {
                return flangeRadius;
            }
            set
            {
                if (flangeRadius != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        flangeRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double FlangeThickness
        {
            get
            {
                return flangeThickness;
            }
            set
            {
                if (flangeThickness != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        flangeThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubRadius
        {
            get
            {
                return hubRadius;
            }
            set
            {
                if (hubRadius != value)
                {
                    if (value >= 0 && value <= 10)
                    {
                        hubRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubThickness
        {
            get
            {
                return hubThickness;
            }
            set
            {
                if (hubThickness != value)
                {
                    if (value >= 0 && value <= 10)
                    {
                        hubThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double MainRadius
        {
            get
            {
                return mainRadius;
            }
            set
            {
                if (mainRadius != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        mainRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double MainThickness
        {
            get
            {
                return mainThickness;
            }
            set
            {
                if (mainThickness != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        mainThickness = value;
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
            RailWheelMaker maker = new RailWheelMaker(mainRadius, mainThickness, flangeRadius, flangeThickness, hubRadius, hubThickness, axleBoreRadius);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("MainRadius") != "")
            {
                MainRadius = EditorParameters.GetDouble("MainRadius");
            }

            if (EditorParameters.Get("AxleBoreRadius") != "")
            {
                MainRadius = EditorParameters.GetDouble("AxleBoreRadius");
            }
            if (EditorParameters.Get("MainThickness") != "")
            {
                MainThickness = EditorParameters.GetDouble("MainThickness");
            }

            if (EditorParameters.Get("FlangeRadius") != "")
            {
                FlangeRadius = EditorParameters.GetDouble("FlangeRadius");
            }

            if (EditorParameters.Get("FlangeThickness") != "")
            {
                FlangeThickness = EditorParameters.GetDouble("FlangeThickness");
            }

            if (EditorParameters.Get("HubRadius") != "")
            {
                HubRadius = EditorParameters.GetDouble("HubRadius");
            }

            if (EditorParameters.Get("HubThickness") != "")
            {
                HubThickness = EditorParameters.GetDouble("HubThickness");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("MainRadius", MainRadius.ToString());
            EditorParameters.Set("MainThickness", MainThickness.ToString());
            EditorParameters.Set("FlangeRadius", FlangeRadius.ToString());
            EditorParameters.Set("FlangeThickness", FlangeThickness.ToString());
            EditorParameters.Set("HubRadius", HubRadius.ToString());
            EditorParameters.Set("HubThickness", HubThickness.ToString());
            EditorParameters.Set("AxleBoreRadius", AxleBoreRadius.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainRadius = 2;
            MainThickness = 4;
            HubRadius = 2;
            HubThickness = 2;
            FlangeRadius = 1;
            FlangeThickness = 1;
            AxleBoreRadius = 1;

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}