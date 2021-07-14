using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ThreadDlg.xaml
    /// </summary>
    public partial class ThreadDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double angle;
        private double crest;
        private double majorRadius;

        private double minorRadius;

        private double pitch;
        private double root;
        private string warningText;

        public ThreadDlg()
        {
            InitializeComponent();
            ToolName = "ThreadDlg";
            DataContext = this;
            ModelGroup = MyModelGroup;
            minorRadius = 12.5;
            majorRadius = 15;
            pitch = 3.5;
            root = 0.1;
            crest = 0.1;
            warningText = "";
        }

        public double Angle
        {
            get
            {
                return angle;
            }
            set
            {
                if (angle != value)
                {
                    angle = value;
                    NotifyPropertyChanged();

                    UpdateDisplay();
                }
            }
        }

        public double Crest
        {
            get
            {
                return crest;
            }
            set
            {
                if (crest != value)
                {
                    crest = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double MajorRadius
        {
            get
            {
                return majorRadius;
            }
            set
            {
                if (majorRadius != value)
                {
                    majorRadius = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double MinorRadius
        {
            get
            {
                return minorRadius;
            }
            set
            {
                if (minorRadius != value)
                {
                    minorRadius = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                if (pitch != value)
                {
                    pitch = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Root
        {
            get
            {
                return root;
            }
            set
            {
                if (root != value)
                {
                    root = value;
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

        private void GenerateShape()
        {
            ClearShape();
            WarningText = "";
            if (minorRadius >= majorRadius)
            {
                WarningText = "Minor Radius must be less than Major Radius";
            }
            else
            {
                if (crest + root >= pitch)
                {
                    WarningText = "Pitch must be more than Root + Crest";
                }
                else
                {
                }
            }
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

            Redisplay();
        }
    }
}