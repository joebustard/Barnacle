using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TrapezoidDlg.xaml
    /// </summary>
    public partial class TrapezoidDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double shapeBevel;
        private double shapeBottomLength;
        private double shapeHeight;
        private double shapeTopLength;
        private double shapeWidth;
        private string warningText;

        public TrapezoidDlg()
        {
            InitializeComponent();
            ToolName = "Trapezoid";
            DataContext = this;
            ModelGroup = MyModelGroup;
            shapeTopLength = 20;
            shapeBottomLength = 16;
            shapeHeight = 10;
            shapeWidth = 10;
            shapeBevel = 0;
        }

        public double ShapeBevel
        {
            get
            {
                return shapeBevel;
            }
            set
            {
                if (shapeBevel != value)
                {
                    if (shapeBevel < 0 || shapeBevel > BevelLimit())
                    {
                        WarningText = "Bevel must be in range 0 to " + BevelLimit().ToString();
                    }
                    else
                    {
                        shapeBevel = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double ShapeHeight
        {
            get
            {
                return shapeHeight;
            }
            set
            {
                if (shapeHeight != value)
                {
                    shapeHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeTopLength
        {
            get
            {
                return shapeTopLength;
            }
            set
            {
                if (shapeTopLength != value)
                {
                    shapeTopLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeBottomLength
        {
            get
            {
                return shapeBottomLength;
            }
            set
            {
                if (shapeBottomLength != value)
                {
                    shapeBottomLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeWidth
        {
            get
            {
                return shapeWidth;
            }
            set
            {
                if (shapeWidth != value)
                {
                    shapeWidth = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        private double BevelLimit()
        {
            double min = shapeTopLength / 2 - 1;
            if (shapeHeight / 2 - 1 < min)
            {
                min = shapeHeight / 2 - 1;
            }
            if (shapeBottomLength / 2 - 1 < min)
            {
                min = shapeBottomLength / 2 - 1;
            }
            return min;
        }

        private void GenerateShape()
        {
            ClearShape();
            TrapezoidMaker pgram = new TrapezoidMaker(shapeTopLength, shapeBottomLength, shapeHeight, shapeWidth, shapeBevel);
            pgram.Generate(Vertices, Faces);
            CentreVertices();
            FloorVertices();
        }

        private void LoadEditorParameters()
        {
            string s = EditorParameters.Get("ShapeTopLength");
            if (s != "")
            {
                shapeTopLength = EditorParameters.GetDouble("ShapeTopLength");
                shapeBottomLength = EditorParameters.GetDouble("ShapeBottomLength");
                shapeWidth = EditorParameters.GetDouble("ShapeWidth");
                shapeHeight = EditorParameters.GetDouble("ShapeHeight");
                
                shapeBevel = EditorParameters.GetDouble("ShapeBevel");
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ShapeTopLength", shapeTopLength.ToString());
            EditorParameters.Set("ShapeHeight", shapeHeight.ToString());
            EditorParameters.Set("ShapeWidth", shapeWidth.ToString());
            EditorParameters.Set("ShapeBottomLength", shapeBottomLength.ToString());
            EditorParameters.Set("ShapeBevel", shapeBevel.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}