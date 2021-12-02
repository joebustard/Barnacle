using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Pulley.xaml
    /// </summary>
    public partial class PulleyDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axleBoreRadius;
        private double extraRimRadius;
        private double extraRimThickness;
        private double grooveDepth;
        private double mainRadius;
        private double mainThickness;
        private string warningText;

        public PulleyDlg()
        {
            InitializeComponent();
            ToolName = "Pulley";
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

        public double ExtraRimRadius
        {
            get
            {
                return extraRimRadius;
            }
            set
            {
                if (extraRimRadius != value)
                {
                    if (value >= 0 && value <= 20)
                    {
                        extraRimRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double ExtraRimThickness
        {
            get
            {
                return extraRimThickness;
            }
            set
            {
                if (extraRimThickness != value)
                {
                    if (value >= 0 && value <= 20)
                    {
                        extraRimThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double GrooveDepth
        {
            get
            {
                return grooveDepth;
            }
            set
            {
                if (grooveDepth != value)
                {
                    if (value >= 1 && value <= 50)
                    {
                        grooveDepth = value;
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
                    if (value >= 1 && value <= 200)
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
            PulleyMaker maker = new PulleyMaker(mainRadius, mainThickness, extraRimRadius, extraRimThickness, grooveDepth, axleBoreRadius);
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

            if (EditorParameters.Get("MainThickness") != "")
            {
                MainThickness = EditorParameters.GetDouble("MainThickness");
            }

            if (EditorParameters.Get("ExtraRimRadius") != "")
            {
                MainThickness = EditorParameters.GetDouble("ExtraRimRadius");
            }

            if (EditorParameters.Get("ExtraRimThickness") != "")
            {
                MainThickness = EditorParameters.GetDouble("ExtraRimThickness");
            }

            if (EditorParameters.Get("AxleBoreRadius") != "")
            {
                MainThickness = EditorParameters.GetDouble("AxleBoreRadius");
            }

            if (EditorParameters.Get("GrooveDepth") != "")
            {
                MainThickness = EditorParameters.GetDouble("GrooveDepth");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("MainRadius", MainRadius.ToString());
            EditorParameters.Set("MainThickness", MainThickness.ToString());
            EditorParameters.Set("ExtraRimRadius", ExtraRimRadius.ToString());
            EditorParameters.Set("ExtraRimThickness", ExtraRimThickness.ToString());
            EditorParameters.Set("GrooveDepth", GrooveDepth.ToString());
            EditorParameters.Set("AxleBoreRadius", AxleBoreRadius.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainRadius = 10;
            MainThickness = 4;
            ExtraRimRadius = 2;
            ExtraRimThickness = 1;
            AxleBoreRadius = 2;
            GrooveDepth = 2;

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}