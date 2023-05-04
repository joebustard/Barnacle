using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for PathLoft.xaml
    /// </summary>
    public partial class PathLoftDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minloftHeight = 0.1;
        private const double maxloftHeight = 200;
        private double loftHeight;

        public double LoftHeight
        {
            get
            {
                return loftHeight;
            }
            set
            {
                if (loftHeight != value)
                {
                    if (value >= minloftHeight && value <= maxloftHeight)
                    {
                        loftHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftHeightToolTip
        {
            get
            {
                return $"LoftHeight must be in the range {minloftHeight} to {maxloftHeight}";
            }
        }

        private const double minloftThickness = 0.1;
        private const double maxloftThickness = 200;
        private double loftThickness;

        public double LoftThickness
        {
            get
            {
                return loftThickness;
            }
            set
            {
                if (loftThickness != value)
                {
                    if (value >= minloftThickness && value <= maxloftThickness)
                    {
                        loftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LoftThicknessToolTip
        {
            get
            {
                return $"LoftThickness must be in the range {minloftThickness} to {maxloftThickness}";
            }
        }

        public PathLoftDlg()
        {
            InitializeComponent();
            ToolName = "PathLoft";
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
            PathLoftMaker maker = new PathLoftMaker(loftHeight, loftThickness);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            LoftHeight = EditorParameters.GetDouble("LoftHeight", 1);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("LoftHeight", LoftHeight.ToString());
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