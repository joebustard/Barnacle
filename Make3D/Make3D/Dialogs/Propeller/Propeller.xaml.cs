using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class Propeller : BaseModellerDialog, INotifyPropertyChanged
    {
        private XmlDocument airFoilDoc;
        private List<String> airfoilGroups;
        private List<String> airfoilNames;
        private string airFoilPath;
        private double bladeAngle;

        private double bladeLength;

        private double bladeMid;
        private double bladeRoot;
        private double bladeTip;
        private int numberOfBlades;
        private string rootGroup;
        private double rootOffset;
        private string selectedAirfoil;

        public Propeller()
        {
            InitializeComponent();
            ToolName = "Propeller";
            DataContext = this;
            NumberOfBlades = 2;
            BladeLength = 60;
            RootOffset = 5;
            BladeRoot = 4;
            BladeMid = 7;
            BladeTip = 5;
            BladeAngle = 45;
            airFoilPath = AppDomain.CurrentDomain.BaseDirectory + "data\\Airfoils.xml";
            airFoilDoc = new XmlDocument();
            airfoilNames = new List<string>();
            airfoilGroups = new List<string>();
        }

        public List<string> AirfoilGroups
        {
            get
            {
                return airfoilGroups;
            }
            set
            {
                if (airfoilGroups != value)
                {
                    airfoilGroups = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> AirfoilNames
        {
            get
            {
                return airfoilNames;
            }
            set
            {
                if (airfoilNames != value)
                {
                    airfoilNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeAngle
        {
            get
            {
                return bladeAngle;
            }
            set
            {
                if (bladeAngle != value)
                {
                    bladeAngle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeLength
        {
            get
            {
                return bladeLength;
            }
            set
            {
                if (bladeLength != value)
                {
                    bladeLength = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeMid
        {
            get
            {
                return bladeMid;
            }
            set
            {
                if (bladeMid != value)
                {
                    bladeMid = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeRoot
        {
            get
            {
                return bladeRoot;
            }
            set
            {
                if (bladeRoot != value)
                {
                    bladeRoot = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double BladeTip
        {
            get
            {
                return bladeTip;
            }
            set
            {
                if (bladeTip != value)
                {
                    bladeTip = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int NumberOfBlades
        {
            get
            {
                return numberOfBlades;
            }
            set
            {
                if (numberOfBlades != value)
                {
                    if (value >= 2)
                    {
                        numberOfBlades = value;
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        public String RootGroup
        {
            get
            {
                return rootGroup;
            }
            set
            {
                if (rootGroup != value)
                {
                    rootGroup = value;
                    List<string> names = new List<String>();
                    SetProfiles(rootGroup, names);

                    NotifyPropertyChanged();
                    AirfoilNames = names;
                }
            }
        }

        public double RootOffset
        {
            get
            {
                return rootOffset;
            }
            set
            {
                if (rootOffset != value)
                {
                    rootOffset = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SelectedAirfoil
        {
            get
            {
                return selectedAirfoil;
            }
            set
            {
                if (selectedAirfoil != value)
                {
                    selectedAirfoil = value;
                    NotifyPropertyChanged();
                    // Update();
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

        private void LoadAirFoils()
        {
            if (File.Exists(airFoilPath))
            {
                airFoilDoc.Load(airFoilPath);
                XmlNode root = airFoilDoc.SelectSingleNode("Airfoils");
                XmlNodeList grps = root.SelectNodes("grp");
                foreach (XmlNode gn in grps)
                {
                    airfoilGroups.Add((gn as XmlElement).GetAttribute("Name"));
                }
                NotifyPropertyChanged("AirfoilGroups");
            }
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

        private void SetProfiles(string grpName, List<string> names)
        {
            XmlNode root = airFoilDoc.SelectSingleNode("Airfoils");
            names.Clear();
            XmlNodeList grps = root.SelectNodes("grp");
            foreach (XmlNode gn in grps)
            {
                if (grpName == (gn as XmlElement).GetAttribute("Name"))
                {
                    XmlNodeList nodeList = gn.SelectNodes("af");
                    foreach (XmlNode nd in nodeList)
                    {
                        XmlElement el = nd as XmlElement;
                        names.Add(el.GetAttribute("Name"));
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAirFoils();

            RootGroup = airfoilGroups[0];

            SelectedAirfoil = airfoilNames[0];

            LoadEditorParameters();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            GenerateShape();
            Redisplay();
        }
    }
}