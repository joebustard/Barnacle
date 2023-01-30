using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Pie.xaml
    /// </summary>
    public partial class PieDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxlowerBevel = 10;
        private const double maxradius = 200;
        private const double maxsweep = 359;
        private const double maxthickness = 100;
        private const double maxupperBevel = 10;
        private const double minlowerBevel = 0;
        private const double minradius = 5;
        private const double minsweep = 1;
        private const double minthickness = 1;
        private const double minupperBevel = 0;
        private bool loaded;
        private double lowerBevel;
        private double radius;
        private double sweep;
        private double thickness;
        private double upperBevel;
        private string warningText;

        public PieDlg()
        {
            InitializeComponent();
            ToolName = "Pie";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
        }

        public double LowerBevel
        {
            get
            {
                return lowerBevel;
            }
            set
            {
                if (lowerBevel != value)
                {
                    if (value >= minlowerBevel && value <= maxlowerBevel)
                    {
                        lowerBevel = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LowerBevelToolTip
        {
            get
            {
                return $"LowerBevel must be in the range {minlowerBevel} to {maxlowerBevel}. Lower Bevel + Upper Bevel must be less han thickness.";
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (radius != value)
                {
                    if (value >= minradius && value <= maxradius)
                    {
                        radius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String RadiusToolTip
        {
            get
            {
                return $"Radius must be in the range {minradius} to {maxradius}";
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

        public double Sweep
        {
            get
            {
                return sweep;
            }
            set
            {
                if (sweep != value)
                {
                    if (value >= minsweep && value <= maxsweep)
                    {
                        sweep = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SweepToolTip
        {
            get
            {
                return $"Sweep must be in the range {minsweep} to {maxsweep}";
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    if (value >= minthickness && value <= maxthickness)
                    {
                        thickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ThicknessToolTip
        {
            get
            {
                return $"Thickness must be in the range {minthickness} to {maxthickness}";
            }
        }

        public double UpperBevel
        {
            get
            {
                return upperBevel;
            }
            set
            {
                if (upperBevel != value)
                {
                    if (value >= minupperBevel && value <= maxupperBevel)
                    {
                        upperBevel = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String UpperBevelToolTip
        {
            get
            {
                return $"UpperBevel must be in the range {minupperBevel} to {maxupperBevel}. Lower Bevel + Upper Bevel must be less han thickness.";
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
            PieMaker maker = new PieMaker(radius, thickness, sweep, upperBevel, lowerBevel);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Radius = EditorParameters.GetDouble("Radius", 20);
            Thickness = EditorParameters.GetDouble("Thickness", 5);
            Sweep = EditorParameters.GetDouble("Sweep", 90);
            UpperBevel = EditorParameters.GetDouble("UpperBevel", 0);
            LowerBevel = EditorParameters.GetDouble("LowerBevel", 0);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("Sweep", Sweep.ToString());
            EditorParameters.Set("UpperBevel", UpperBevel.ToString());
            EditorParameters.Set("LowerBevel", LowerBevel.ToString());
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