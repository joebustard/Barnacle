using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for squirkle.xaml
    /// </summary>
    public partial class SquirkleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private double squirkleheight;

        public double SquirkleHeight
        {
            get { return squirkleheight; }
            set {
                if (value != squirkleheight)
                {
                    squirkleheight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private double length;
        public double Length
        {
            get { return length; }
            set
            {
                if (value != length)
                {
                    length = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public SquirkleDlg()
        {
            InitializeComponent();
            ToolName = "Squirkle";
            DataContext = this;
            ModelGroup = MyModelGroup;
            squirkleheight = 10;
            length = 10;
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
            string pathtext = "M 0,0 ";

            if (TopLeftCornerShape.Mode == 0)
            {

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
            TopLeftCornerShape.Location = CornerShape.CornerLocation.TopLeft;
            TopRightCornerShape.Location = CornerShape.CornerLocation.TopRight;
            BottomLeftCornerShape.Location = CornerShape.CornerLocation.BottomLeft;
            BottomRightCornerShape.Location = CornerShape.CornerLocation.BottomRight;
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}