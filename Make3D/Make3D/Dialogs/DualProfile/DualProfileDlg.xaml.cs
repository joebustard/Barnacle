using asdflibrary;
using Barnacle.Object3DLib;
using MakerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
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
            GenerateShape();
            Redisplay();
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
            GenerateShape();
            Redisplay();
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
            if (frontpnts.Count > 3 && toppnts.Count > 3)
            {
                DualProfileMaker maker = new DualProfileMaker(frontProfile, topProfile);
                maker.Generate(Vertices, Faces);

                AdaptiveSignedDistanceField adfTest = new AdaptiveSignedDistanceField();
                AdaptiveSignedDistanceField.OnCalculateDistance += CalculateDistance;

                adfTest.SetDimensions(-1F, -1F, -1F, 1F, 1F, 1F);

                adfTest.SplitRoot();
                System.Diagnostics.Debug.WriteLine("After split");
                adfTest.Dump();
                Object3D cb = new Object3D();
                Point3DCollection pnts = new Point3DCollection();
                Int32Collection tris = new Int32Collection();
                // adfTest.GetCube(7, pnts, tris);
                cb.TriangleIndices = tris;
                foreach (Point3D p in pnts)
                {
                    cb.RelativeObjectVertices.Add(new P3D(p.X, p.Y, p.Z));
                }
                cb.Remesh();
            }
        }

        private float CalculateDistance(float x, float y, float z)
        {
            float res = float.MaxValue;
            bool inFront = IsPointInPolygon( x, y, frontpnts);
            float frontDist = DistToPoly(frontpnts, x, y);
            return res;
        }

        private float DistToPoly(List<Point> pnts, float x, float y)
        {
            float v = float.MaxValue;
            float d = 0;
            Point closest;
            for ( int i = 0; i < pnts.Count -1; i ++)
            {
                d = FindClosestToLine(x, y, pnts[i], pnts[i + 1], out closest);
                if ( d< v)
                {
                    v = d;
                }
            }
            return v;
        }
        public  float FindClosestToLine(
          float x, float y, Point p1, Point p2, out Point closest)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = x - p1.X;
                dy = y - p1.Y;
                return (float) Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((x - p1.X) * dx + (y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(p1.X, p1.Y);
                dx = x - p1.X;
                dy = y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Point(p2.X, p2.Y);
                dx = x - p2.X;
                dy = y - p2.Y;
            }
            else
            {
                closest = new Point(p1.X + t * dx, p1.Y + t * dy);
                dx = x - closest.X;
                dy = y - closest.Y;
            }

            return (float) Math.Sqrt(dx * dx + dy * dy);
        }
        public static bool IsPointInPolygon(float x, float y, List<Point> polygon)
        {
            int polyCorners = polygon.Count;
            int i = 0;
            int j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if (polygon[i].Y < y && polygon[j].Y >= y
                || polygon[j].Y < y && polygon[i].Y >= y)
                {
                    if (polygon[i].X + (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
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
