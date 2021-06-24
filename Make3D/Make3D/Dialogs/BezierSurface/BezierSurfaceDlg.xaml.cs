using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for BezierSurface.xaml
    /// </summary>
    public partial class BezierSurfaceDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        public BezierSurfaceDlg()
        {
            InitializeComponent();
            ToolName = "BezierSurface";
            DataContext = this;
            ModelGroup = MyModelGroup;
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
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