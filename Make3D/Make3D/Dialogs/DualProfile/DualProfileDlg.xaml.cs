using asdflibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for DualProfile.xaml
    /// </summary>
    public partial class DualProfileDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

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
            ToolName = "Dual";
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

        private void FrontPointsChanged(List<System.Windows.Point> pnts)
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            Get2DBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
            if (tlx < double.MaxValue)
            {
                frontXSize = brx - tlx;
                frontYSize = bry - tly;

                double mx = tlx + frontXSize / 2.0;
                double my = tly + frontYSize / 2.0;
                frontpnts.Clear();
                foreach (Point p in pnts)
                {
                    frontpnts.Add(new Point((p.X - mx) / frontXSize, (p.Y - my) / frontYSize));
                }
            }
            GenerateShape();
            Redisplay();
        }

        private void TopPointsChanged(List<System.Windows.Point> pnts)
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            Get2DBounds(pnts, ref tlx, ref tly, ref brx, ref bry);
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
            if (TopPathControl.PathClosed && FrontPathControl.PathClosed)
            {
                if (frontpnts.Count > 3 && toppnts.Count > 3)
                {
                    CubeMarcher cm = new CubeMarcher();
                    GridCell gc = new GridCell();
                    List<Triangle> triangles = new List<Triangle>();

                    float dd = 0.025F;
                    Functions.SphereRadius = 0.5F;
                    for (float x = -0.6F; x <= 0.6; x += dd)
                    {
                        for (float y = -0.6F; y <= 0.6F; y += dd)
                        {
                            for (float z = -0.6F; z <= 0.6; z += dd)
                            {
                                gc.p[0] = new XYZ(x, y, z);

                                gc.p[1] = new XYZ(x + dd, y, z);
                                gc.p[2] = new XYZ(x + dd, y, z + dd);
                                gc.p[3] = new XYZ(x, y, z + dd);
                                gc.p[4] = new XYZ(x, y + dd, z);
                                gc.p[5] = new XYZ(x + dd, y + dd, z);
                                gc.p[6] = new XYZ(x + dd, y + dd, z + dd);
                                gc.p[7] = new XYZ(x, y + dd, z + dd);

                                for (int i = 0; i < 8; i++)
                                {
                                    //gc.val[i] = Functions.Sphere(gc.p[i].x, gc.p[i].y, gc.p[i].z); ;
                                    bool fin = InFront((float)gc.p[i].x, (float)gc.p[i].y);
                                    float fd = FrontDist((float)gc.p[i].x, (float)gc.p[i].y, fin);
                                    if (Math.Abs(fd) < 0.001) fin = true;

                                    bool tin = InTop((float)gc.p[i].x, (float)gc.p[i].z);
                                    float td = TopDist((float)gc.p[i].x, (float)gc.p[i].z, tin);
                                    if (Math.Abs(td) < 0.001) tin = true;

                                    gc.val[i] = GetDist(fin, fd, tin, td);
                                }
                                triangles.Clear();

                                cm.Polygonise(gc, 0, triangles);

                                foreach (Triangle t in triangles)
                                {
                                    int p0 = AddVertice(t.p[0].x * frontXSize, -(t.p[0].y * frontYSize), t.p[0].z * topYSize);
                                    int p1 = AddVertice(t.p[1].x * frontXSize, -(t.p[1].y * frontYSize), t.p[1].z * topYSize);
                                    int p2 = AddVertice(t.p[2].x * frontXSize, -(t.p[2].y * frontYSize), t.p[2].z * topYSize);

                                    Faces.Add(p0);
                                    Faces.Add(p2);
                                    Faces.Add(p1);
                                }
                            }
                        }
                    }
                }
            }
            CentreVertices();
        }

        private double GetDist(bool fin, float fid, bool tin, float ftop)
        {
            double res = Math.Abs(fid);

            if (Math.Abs(ftop) > res)
            {
                res = Math.Abs(ftop);
            }

            if (fin && tin)
            {
                res = -res;
            }
            return res;
        }

        private bool InFront(float x, float y)
        {
            bool inFront = IsPointInPolygon(x, y, frontpnts);
            return inFront;
        }

        private float FrontDist(float x, float y, bool inside)
        {
            Point closest = new Point(0, 0);
            float frontDist = DistToPoly(frontpnts, x, y, ref closest);
            if (inside)
            {
                frontDist = -frontDist;
            }
            return frontDist;
        }

        private bool InTop(float x, float y)
        {
            bool inTop = IsPointInPolygon(x, y, toppnts);
            return inTop;
        }

        private float TopDist(float x, float y, bool inside)
        {
            Point closest = new Point(0, 0);
            float topDist = DistToPoly(toppnts, x, y, ref closest);
            if (inside)
            {
                topDist = -topDist;
            }
            return topDist;
        }

        private float CalculateFrontDistance(float x, float y, float z)
        {
            Point closest = new Point(0, 0);

            float res = float.MaxValue;
            bool inFront = IsPointInPolygon(x, y, frontpnts);
            float frontDist = DistToPoly(frontpnts, x, y, ref closest);
            res = (float)((closest.X - x) * (closest.X - x) +
                          (closest.Y - y) * (closest.Y - y));
            bool inTop = IsPointInPolygon(x, z, toppnts);
            float topDist = DistToPoly(toppnts, x, z, ref closest);
            res += (float)((closest.Y - y) * (closest.Y - y));

            res = (float)Math.Sqrt(res);
            if (inFront && inTop)
            {
                res = -res;
            }
            return res;
        }

        private float CalculateDistance(float x, float y, float z)
        {
            Point closest = new Point(0, 0);

            float res = float.MaxValue;
            bool inFront = IsPointInPolygon(x, y, frontpnts);
            float frontDist = DistToPoly(frontpnts, x, y, ref closest);
            res = (float)((closest.X - x) * (closest.X - x) +
                          (closest.Y - y) * (closest.Y - y));
            bool inTop = IsPointInPolygon(x, z, toppnts);
            float topDist = DistToPoly(toppnts, x, z, ref closest);
            res += (float)((closest.Y - y) * (closest.Y - y));

            res = (float)Math.Sqrt(res);
            if (inFront && inTop)
            {
                res = -res;
            }
            return res;
        }

        private float DistToPoly(List<Point> pnts, float x, float y, ref Point closest)
        {
            float v = float.MaxValue;
            float d = 0;

            Point cl2 = new Point(0, 0);
            for (int i = 0; i < pnts.Count - 1; i++)
            {
                d = FindClosestToLine(x, y, pnts[i], pnts[i + 1], out cl2);
                if (d < v)
                {
                    v = d;
                    closest.X = cl2.X;
                    closest.Y = cl2.Y;
                }
            }

            return v;
        }

        public float FindClosestToLine(
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
                return (float)Math.Sqrt(dx * dx + dy * dy);
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

            return (float)Math.Sqrt(dx * dx + dy * dy);
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
            string s = EditorParameters.Get("TopShape");
            TopPathControl.SetPath(s);
            s = EditorParameters.Get("FrontShape");
            FrontPathControl.SetPath(s);
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("TopShape", TopPathControl.GetPath());
            EditorParameters.Set("FrontShape", FrontPathControl.GetPath());
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