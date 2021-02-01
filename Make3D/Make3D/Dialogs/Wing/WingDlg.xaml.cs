using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for WingDlg.xaml
    /// </summary>
    public partial class WingDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool wholeModelChecked;
        private bool topModelChecked;
        private bool bottomModelChecked;
        public bool WholeModelChecked
        {
            get { return wholeModelChecked; }
            set
            {
                if ( wholeModelChecked != value)
                {
                    wholeModelChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool TopModelChecked
        {
            get { return topModelChecked; }
            set
            {
                if (topModelChecked != value)
                {
                    topModelChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool BottomModelChecked
        {
            get { return bottomModelChecked; }
            set
            {
                if (bottomModelChecked != value)
                {
                    bottomModelChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private List<String> shapeNames;
        public List<String>ShapeNames
        {
            get { return shapeNames; }
            set
            {
                if ( shapeNames != value)
                {
                    shapeNames = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private List<String> tipNames;
        public List<String> TipNames
        {
            get { return tipNames; }
            set
            {
                if (tipNames != value)
                {
                    tipNames = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string selectedShape;
        public String SelectedShape
        {
            get { return selectedShape; }
            set
            {
                if (selectedShape != value) 
                {
                    selectedShape = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string selectedTip;
        public String SelectedTip
        {
            get { return selectedTip; }
            set
            {
                if (selectedTip != value)
                {
                    selectedTip = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public WingDlg()
        {
            InitializeComponent();
            ToolName = "Wing";
            DataContext = this;
            shapeNames = new List<string>();
            tipNames = new List<string>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetShapeNames();
            SelectedShape = "Straight";
            SelectedShape = "None";
            WholeModelChecked = true;
            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }

        private void SetShapeNames()
        {
            shapeNames.Add("Straight");
            shapeNames.Add("Tapered");
            shapeNames.Add("Delta");
            shapeNames.Add("Swept Back");
            shapeNames.Add("Swept Forward");
            NotifyPropertyChanged("ShapeNames");
            tipNames.Add("None");
            tipNames.Add("Straight");
            tipNames.Add("Curved");
            NotifyPropertyChanged("TipNames");
        }

        private void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);
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

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }
        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }
    }
}