using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DualProfile.xaml
    /// </summary>
    public partial class DualProfileDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded ;

        private string frontProfile;
        private double frontXSize;
        private double frontYSize;
        // scaled and centered on 0
        private List<System.Windows.Point> frontpnts;

        private string topProfile;
        private double topXSize;
        private double topYSize;
        // scaled and centered on 0
        private List<System.Windows.Point> toppnts;
        public DualProfileDlg()
        {
            InitializeComponent();
            ToolName = "DualProfile";
            DataContext = this;
            frontProfile = "";
            frontXSize = 0;
            frontYSize = 0;
            frontpnts = new List<Point>();
            
            topProfile = "";
            topXSize = 0;
            topYSize = 0;
            toppnts = new List<Point>();
            FrontPathControl.OnFlexiPathChanged += FrontPointsChanged;
            TopPathControl.OnFlexiPathChanged += TopPointsChanged;
            ModelGroup = MyModelGroup;
            loaded = false;
        }
        private void FrontPointsChanged(List<System.Windows.Point>pnts)
        {
            double tlx=0;
            double tly=0;
            double brx=0;
            double bry=0;
            GetBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
            if ( tlx <double.MaxValue)
            {
                frontXSize = brx - tlx;
                frontYSize = bry - tly;

                double mx = tlx + frontXSize / 2.0;
                double my = tly + frontYSize / 2.0;
                frontpnts.Clear();
                foreach( Point p in pnts)
                {
                    frontpnts.Add(new Point( (p.X-mx)/frontXSize, (p.Y-my)/frontYSize));
                }
            }
        }

        private void GetBounds(List<Point> pnts, ref double tlx, ref double tly, ref double brx, ref double bry)
        {
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            foreach( Point p in pnts)
            {
                if (p.X < tlx) tlx = p.X;
                if (p.X > brx) brx = p.X;
                if (p.Y < tly) tly = p.Y;
                if (p.Y > bry) bry = p.Y;
            }
        }

        private void TopPointsChanged(List<System.Windows.Point> pnts)
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            GetBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
            if (tlx < double.MaxValue)
            {
                topXSize = brx - tlx;
                topYSize = bry - tly;

                double mx = tlx + topXSize / 2.0;
                double my = tly + topYSize / 2.0;
                toppnts.Clear();
                foreach (Point p in pnts)
                {
                    toppnts.Add(new Point((p.X - mx) / topXSize, (p.Y - my) / topYSize));
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
            DualProfileMaker maker = new DualProfileMaker(frontProfile,topProfile );
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
