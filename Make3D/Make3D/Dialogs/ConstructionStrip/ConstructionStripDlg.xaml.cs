using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ConstructionStrip.xaml
    /// </summary>
    public partial class ConstructionStripDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minstripHeight = 1;
        private const double maxstripHeight = 10;
        private double stripHeight;

        public double StripHeight
        {
            get
            {
                return stripHeight;
            }
            set
            {
                if (stripHeight != value)
                {
                    if (value >= minstripHeight && value <= maxstripHeight)
                    {
                        stripHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripHeightToolTip
        {
            get
            {
                return $"StripHeight must be in the range {minstripHeight} to {maxstripHeight}";
            }
        }

        private const double minstripWidth = 5;
        private const double maxstripWidth = 100;
        private double stripWidth;

        public double StripWidth
        {
            get
            {
                return stripWidth;
            }
            set
            {
                if (stripWidth != value)
                {
                    if (value >= minstripWidth && value <= maxstripWidth)
                    {
                        stripWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripWidthToolTip
        {
            get
            {
                return $"StripWidth must be in the range {minstripWidth} to {maxstripWidth}";
            }
        }

        private const double minstripRepeats = 1;
        private const double maxstripRepeats = 20;
        private int stripRepeats;

        public int StripRepeats
        {
            get
            {
                return stripRepeats;
            }
            set
            {
                if (stripRepeats != value)
                {
                    if (value >= minstripRepeats && value <= maxstripRepeats)
                    {
                        stripRepeats = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripRepeatsToolTip
        {
            get
            {
                return $"StripRepeats must be in the range {minstripRepeats} to {maxstripRepeats}";
            }
        }

        private const double minholeRadius = 2;
        private const double maxholeRadius = 98;
        private double holeRadius;

        public double HoleRadius
        {
            get
            {
                return holeRadius;
            }
            set
            {
                if (holeRadius != value)
                {
                    if (value >= minholeRadius && value <= maxholeRadius)
                    {
                        holeRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HoleRadiusToolTip
        {
            get
            {
                return $"HoleRadius must be in the range {minholeRadius} to {maxholeRadius}";
            }
        }

        private const double minnumberOfHoles = 2;
        private const double maxnumberOfHoles = 20;
        private int numberOfHoles;

        public int NumberOfHoles
        {
            get
            {
                return numberOfHoles;
            }
            set
            {
                if (numberOfHoles != value)
                {
                    if (value >= minnumberOfHoles && value <= maxnumberOfHoles)
                    {
                        numberOfHoles = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String NumberOfHolesToolTip
        {
            get
            {
                return $"NumberOfHoles must be in the range {minnumberOfHoles} to {maxnumberOfHoles}";
            }
        }

        public ConstructionStripDlg()
        {
            InitializeComponent();
            ToolName = "ConstructionStrip";
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
            ConstructionStripMaker maker = new ConstructionStripMaker(stripHeight, stripWidth, stripRepeats, holeRadius, numberOfHoles);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            StripHeight = EditorParameters.GetDouble("StripHeight", 2.5);
            StripWidth = EditorParameters.GetDouble("StripWidth", 17);
            StripRepeats = EditorParameters.GetInt("StripRepeats", 1);
            HoleRadius = EditorParameters.GetDouble("HoleRadius", 4.5);
            NumberOfHoles = EditorParameters.GetInt("NumberOfHoles", 3);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("StripHeight", StripHeight.ToString());
            EditorParameters.Set("StripWidth", StripWidth.ToString());
            EditorParameters.Set("StripRepeats", StripRepeats.ToString());
            EditorParameters.Set("HoleRadius", HoleRadius.ToString());
            EditorParameters.Set("NumberOfHoles", NumberOfHoles.ToString());
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

        private void SetDefaults()
        {
            loaded = false;

            StripHeight = 2.5;
            StripWidth = 17;
            StripRepeats = 1;
            HoleRadius = 4.5;
            NumberOfHoles = 3;

            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}