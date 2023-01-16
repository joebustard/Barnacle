using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RoofRidge.xaml
    /// </summary>
    public partial class RoofRidgeDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxarmAngle = 180;
        private const double maxarmLength = 100;
        private const double maxarmThickness = 10;
        private const double maxcrownRadius = 100;
        private const double maxflatCrestWidth = 100;
        private const double maxridgeLength = 200;
        private const double minarmAngle = 0;
        private const double minarmLength = 0.1;
        private const double minarmThickness = 1;
        private const double mincrownRadius = 0.1;
        private const double minflatCrestWidth = 0.1;
        private const double minridgeLength = 0.1;
        private const int minshape = 0;
        private double armAngle;
        private double armLength;
        private double crownRadius;
        private ImageSource currentImage;
        private double flatCrestWidth;

        private string[] imageNames =
        {
        "CrownlessRidge","FlatRidge","HollowCrownRidge","CrownRidge"
        };

        private bool loaded;
        private int mode;
        private double ridgeLength;
        private string warningText;
        private double armThickness;

        public RoofRidgeDlg()
        {
            InitializeComponent();
            ToolName = "RoofRidge";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            mode = 0;
        }

        public double ArmAngle
        {
            get
            {
                return armAngle;
            }
            set
            {
                if (armAngle != value)
                {
                    if (value >= minarmAngle && value <= maxarmAngle)
                    {
                        armAngle = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ArmAngleToolTip
        {
            get
            {
                return $"Arm Angle must be in the range {minarmAngle} to {maxarmAngle}";
            }
        }

        public String ArmThicknessToolTip
        {
            get
            {
                return $"Arm Thickness must be in the range {minarmThickness} to {maxarmThickness}";
            }
        }

        public double ArmLength
        {
            get
            {
                return armLength;
            }
            set
            {
                if (armLength != value)
                {
                    if (value >= minarmLength && value <= maxarmLength)
                    {
                        armLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double ArmThickness
        {
            get
            {
                return armThickness;
            }
            set
            {
                if (armThickness != value)
                {
                    if (value >= minarmThickness && value <= maxarmThickness)
                    {
                        armThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ArmLengthToolTip
        {
            get
            {
                return $"Arm Length must be in the range {minarmLength} to {maxarmLength}";
            }
        }

        public double CrownRadius
        {
            get
            {
                return crownRadius;
            }
            set
            {
                if (crownRadius != value)
                {
                    if (value >= mincrownRadius && value <= maxcrownRadius)
                    {
                        crownRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String CrownRadiusToolTip
        {
            get
            {
                return $"Crown Radius must be in the range {mincrownRadius} to {maxcrownRadius}";
            }
        }

        public ImageSource CurrentImage
        {
            get
            {
                return currentImage;
            }
            set
            {
                if (currentImage != value)
                {
                    currentImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double FlatCrestWidth
        {
            get
            {
                return flatCrestWidth;
            }
            set
            {
                if (flatCrestWidth != value)
                {
                    if (value >= minflatCrestWidth && value <= maxflatCrestWidth)
                    {
                        flatCrestWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FlatCrestWidthToolTip
        {
            get
            {
                return $"Flat Crest Width must be in the range {minflatCrestWidth} to {maxflatCrestWidth}";
            }
        }

        public double RidgeLength
        {
            get
            {
                return ridgeLength;
            }
            set
            {
                if (ridgeLength != value)
                {
                    if (value >= minridgeLength && value <= maxridgeLength)
                    {
                        ridgeLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String RidgeLengthToolTip
        {
            get
            {
                return $"Ridge Length must be in the range {minridgeLength} to {maxridgeLength}";
            }
        }

        public String ShapeToolTip
        {
            get
            {
                return $"Click the image to change the basic ridge shape";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mode = mode + 1;
            if (mode > 3)
            {
                mode = 0;
            }
            SetShapeImage(mode);
            UpdateDisplay();
        }

        private void GenerateShape()
        {
            ClearShape();
            RoofRidgeMaker maker = new RoofRidgeMaker(
                armLength, armAngle, armThickness, ridgeLength, crownRadius, flatCrestWidth, mode
                );
            maker.Generate(Vertices, Faces);
            //   CentreVertices();
        }

        private Uri ImageUri(string p)
        {
            return new Uri(@"pack://application:,,,/Images/Buttons/" + p);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            ArmLength = EditorParameters.GetDouble("ArmLength", 5);
            ArmAngle = EditorParameters.GetDouble("ArmAngle", 90);
            ArmThickness = EditorParameters.GetDouble("ArmThickness", 1);
            RidgeLength = EditorParameters.GetDouble("RidgeLength", 100);
            CrownRadius = EditorParameters.GetDouble("CrownRadius", 2);
            FlatCrestWidth = EditorParameters.GetDouble("FlatCrestWidth", 1);

            mode = EditorParameters.GetInt("Mode", 0);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("ArmLength", ArmLength.ToString());
            EditorParameters.Set("ArmAngle", ArmAngle.ToString());
            EditorParameters.Set("ArmThickness", ArmThickness.ToString());
            EditorParameters.Set("RidgeLength", RidgeLength.ToString());
            EditorParameters.Set("CrownRadius", CrownRadius.ToString());
            EditorParameters.Set("FlatCrestWidth", FlatCrestWidth.ToString());
            EditorParameters.Set("Mode", mode.ToString());
        }

        private void SetShapeImage(int mode)
        {
            if (mode >= 0 && mode < imageNames.GetLength(0))
            {
                string im = imageNames[mode] + ".png";
                BitmapImage icon = new BitmapImage(ImageUri(im));
                CurrentImage = icon;
            }
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
            SetShapeImage(mode);
            loaded = true;

            UpdateDisplay();
        }
    }
}