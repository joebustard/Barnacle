using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class BlankDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded ;

        //TOOLPROPS

        public BlankDlg()
        {
            InitializeComponent();
            ToolName = "Blank";
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
            BlankMaker maker = new BlankMaker(
                //MAKEPARAMETERS
                );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            //LOADPARMETERS
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            //SAVEPARMETERS
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