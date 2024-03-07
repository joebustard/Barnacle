using MakerLib;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Bicorn.xaml
    /// </summary>
    public partial class BicornDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool doubleup;
        private double height;
        private double radius1;
        private double radius2;
        private string warningText;

        public BicornDlg()
        {
            InitializeComponent();
            ToolName = "Bicorn";
            DataContext = this;
            ModelGroup = MyModelGroup;
            Radius1 = 5;
            Radius2 = 10;
            DoubleUp = false;
            height = 10;
        }

        public bool DoubleUp
        {
            get
            {
                return doubleup;
            }
            set
            {
                if (value != doubleup)
                {
                    doubleup = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius1
        {
            get
            {
                return radius1;
            }
            set
            {
                if (value != radius1)
                {
                    radius1 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius2
        {
            get
            {
                return radius2;
            }
            set
            {
                if (value != radius2)
                {
                    radius2 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    if (value > 0)
                    {
                        height = value;
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            BicornMaker maker = new BicornMaker(radius1, radius2, height, doubleup);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
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