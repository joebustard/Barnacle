using MakerLib;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for WagonWheel.xaml
    /// </summary>
    public partial class WagonWheelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axleBore;
        private double hubRadius;
        private double hubThickness;
        private bool loaded;
        private double numberOfSpokes;
        private double rimDepth;
        private double rimInnerRadius;
        private double rimThickness;
        private double spokeRadius;
        private string warningText;

        public WagonWheelDlg()
        {
            InitializeComponent();
            ToolName = "WagonWheel";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
        }

        public double AxleBore
        {
            get
            {
                return axleBore;
            }
            set
            {
                if (axleBore != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        axleBore = value;
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
                    if (value >= 1 && value <= 20)
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
                    if (value >= 1 && value <= 20)
                    {
                        hubThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double NumberOfSpokes
        {
            get
            {
                return numberOfSpokes;
            }
            set
            {
                if (numberOfSpokes != value)
                {
                    if (value >= 4 && value <= 20)
                    {
                        numberOfSpokes = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimDepth
        {
            get
            {
                return rimDepth;
            }
            set
            {
                if (rimDepth != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimInnerRadius
        {
            get
            {
                return rimInnerRadius;
            }
            set
            {
                if (rimInnerRadius != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimInnerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimThickness
        {
            get
            {
                return rimThickness;
            }
            set
            {
                if (rimThickness != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimThickness = value;
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

        public double SpokeRadius
        {
            get
            {
                return spokeRadius;
            }
            set
            {
                if (spokeRadius != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        spokeRadius = value;
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
            WagonWheelMaker maker = new WagonWheelMaker(hubRadius, hubThickness, rimInnerRadius, rimThickness, rimDepth, numberOfSpokes, spokeRadius, axleBore);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("HubRadius") != "")
            {
                HubRadius = EditorParameters.GetDouble("HubRadius");
            }

            if (EditorParameters.Get("HubThickness") != "")
            {
                HubThickness = EditorParameters.GetDouble("HubThickness");
            }

            if (EditorParameters.Get("RimInnerRadius") != "")
            {
                RimInnerRadius = EditorParameters.GetDouble("RimInnerRadius");
            }

            if (EditorParameters.Get("RimThickness") != "")
            {
                RimThickness = EditorParameters.GetDouble("RimThickness");
            }

            if (EditorParameters.Get("RimDepth") != "")
            {
                RimDepth = EditorParameters.GetDouble("RimDepth");
            }

            if (EditorParameters.Get("NumberOfSpokes") != "")
            {
                NumberOfSpokes = EditorParameters.GetDouble("NumberOfSpokes");
            }

            if (EditorParameters.Get("SpokeRadius") != "")
            {
                SpokeRadius = EditorParameters.GetDouble("SpokeRadius");
            }

            if (EditorParameters.Get("AxleBore") != "")
            {
                AxleBore = EditorParameters.GetDouble("AxleBore");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("HubRadius", HubRadius.ToString());
            EditorParameters.Set("HubThickness", HubThickness.ToString());
            EditorParameters.Set("RimInnerRadius", RimInnerRadius.ToString());
            EditorParameters.Set("RimThickness", RimThickness.ToString());
            EditorParameters.Set("RimDepth", RimDepth.ToString());
            EditorParameters.Set("NumberOfSpokes", NumberOfSpokes.ToString());
            EditorParameters.Set("SpokeRadius", SpokeRadius.ToString());
            EditorParameters.Set("AxleBore", AxleBore.ToString());
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
            HubRadius = 2;
            HubThickness = 3;
            RimInnerRadius = 10;
            RimThickness = 3;
            RimDepth = 2;
            NumberOfSpokes = 6;
            SpokeRadius = 2;
            AxleBore = 2;
            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            loaded = true;
            UpdateDisplay();
        }
    }
}