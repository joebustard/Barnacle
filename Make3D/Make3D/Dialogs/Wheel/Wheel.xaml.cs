using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class Wheel : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axelBore;
        private List<String> hubStyles;
        private List<String> rimStyles;
        private double tyreDepth;

        private double wheelWidth;

        public Wheel()
        {
            InitializeComponent();
            ToolName = "Wheel";
            DataContext = this;
            tyreDepth = 50;
            wheelWidth = 50;
            axelBore = 10;
            hubStyles = new List<string>();
            rimStyles = new List<string>();
        }

        public double AxelBore
        {
            get
            {
                return axelBore;
            }
            set
            {
                if (axelBore != value)
                {
                    axelBore = value; NotifyPropertyChanged();
                }
            }
        }

        public List<String> HubStyles
        {
            get
            {
                return hubStyles;
            }
            set
            {
                if (hubStyles != value)
                {
                    hubStyles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<String> RimStyles
        {
            get
            {
                return rimStyles;
            }
            set
            {
                rimStyles = value;
                NotifyPropertyChanged();
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

        public double TyreDepth
        {
            get
            {
                return tyreDepth;
            }
            set
            {
                if (tyreDepth != value)
                {
                    tyreDepth = value; NotifyPropertyChanged();
                }
            }
        }

        public double WheelWidth
        {
            get
            {
                return wheelWidth;
            }
            set
            {
                if (wheelWidth != value)
                {
                    wheelWidth = value; NotifyPropertyChanged();
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

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hubStyles.Add("1");
            hubStyles.Add("2");
            hubStyles.Add("3");

            rimStyles.Add("1");
            rimStyles.Add("2");
            rimStyles.Add("3");

            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}