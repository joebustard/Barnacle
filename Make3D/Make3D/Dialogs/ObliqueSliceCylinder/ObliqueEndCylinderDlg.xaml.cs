using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ObliqueEndCylinder.xaml
    /// </summary>
    public partial class ObliqueEndCylinderDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;
        private Visibility showPoints;

        public Visibility ShowPoints
        {
            get { return showPoints; }
            set
            {
                if (showPoints != value)
                {
                    showPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private const double minradius = 0.5;
        private const double maxradius = 200;
        private double radius;

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

        private const double minmainHeight = 0.1;
        private const double maxmainHeight = 200;
        private double mainHeight;

        public double MainHeight
        {
            get
            {
                return mainHeight;
            }
            set
            {
                if (mainHeight != value)
                {
                    if (value >= minmainHeight && value <= maxmainHeight)
                    {
                        mainHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String MainHeightToolTip
        {
            get
            {
                return $"MainHeight must be in the range {minmainHeight} to {maxmainHeight}";
            }
        }

        private const double mincutHeight = 0.1;
        private const double maxcutHeight = 200;
        private double cutHeight;

        public double CutHeight
        {
            get
            {
                return cutHeight;
            }
            set
            {
                if (cutHeight != value)
                {
                    if (value >= mincutHeight && value <= maxcutHeight)
                    {
                        cutHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String CutHeightToolTip
        {
            get
            {
                return $"CutHeight must be in the range {mincutHeight} to {maxcutHeight}";
            }
        }

        private const double mincutStyle = 1;
        private const double maxcutStyle = 3;
        private double cutStyle;

        public double CutStyle
        {
            get
            {
                return cutStyle;
            }
            set
            {
                if (cutStyle != value)
                {
                    if (value >= mincutStyle && value <= maxcutStyle)
                    {
                        cutStyle = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                        if (cutStyle == 1)
                        {
                            ShowPoints = Visibility.Hidden;
                        }
                        else
                        {
                            ShowPoints = Visibility.Visible;
                        }
                    }
                }
            }
        }

        public String CutStyleToolTip
        {
            get
            {
                return $"CutStyle must be in the range {mincutStyle} to {maxcutStyle}";
            }
        }

        private const double mincutPoints = 1;
        private const double maxcutPoints = 10;
        private double cutPoints;

        public double CutPoints
        {
            get
            {
                return cutPoints;
            }
            set
            {
                if (cutPoints != value)
                {
                    if (value >= mincutPoints && value <= maxcutPoints)
                    {
                        cutPoints = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String CutPointsToolTip
        {
            get
            {
                return $"CutPoints must be in the range {mincutPoints} to {maxcutPoints}";
            }
        }

        public ObliqueEndCylinderDlg()
        {
            InitializeComponent();
            ToolName = "ObliqueEndCylinder";
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
            ObliqueEndCylinderMaker maker = new ObliqueEndCylinderMaker(radius,
                                                                        mainHeight,
                                                                        cutHeight,
                                                                        cutStyle,
                                                                        cutPoints);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            Radius = EditorParameters.GetDouble("Radius", 5);

            MainHeight = EditorParameters.GetDouble("MainHeight", 5);

            CutHeight = EditorParameters.GetDouble("CutHeight", 5);

            CutStyle = EditorParameters.GetDouble("CutStyle", 1);

            CutPoints = EditorParameters.GetDouble("CutPoints", 1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("MainHeight", MainHeight.ToString());
            EditorParameters.Set("CutHeight", CutHeight.ToString());
            EditorParameters.Set("CutStyle", CutStyle.ToString());
            EditorParameters.Set("CutPoints", CutPoints.ToString());
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
            Radius = 5;
            MainHeight = 5;
            CutHeight = 5;
            CutStyle = 1;
            CutPoints = 1;
            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}