using Barnacle.LineLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using PointF = System.Drawing.PointF;
namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for squirkle.xaml
    /// </summary>
    public partial class SquirkleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private double squirkleheight;
        private double depth = 10;
        public double SquirkleHeight
        {
            get { return squirkleheight; }
            set
            {
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
        private void CreateSideFace(List<System.Windows.Point> pnts, int i, bool autoclose = true)
        {
            int v = i + 1;

            if (v == pnts.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, depth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, depth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }
        private void GenerateShape()
        {
            ClearShape();
            double h = squirkleheight / 4.0;
            double l = length / 4.0;
            string hseg = h.ToString();
            string lseg = l.ToString();

            string pathtext = "M 0,0 ";

            pathtext += "RV -" + hseg + " ";
            // bl
            if (BottomLeftCornerShape.Mode == 0)
            {
                pathtext += "RV -" + hseg + " ";
                pathtext += "RH " + lseg + " ";
            }
            if (BottomLeftCornerShape.Mode == 1)
            {
                pathtext += "RQ 0,-" + hseg + " "+lseg+",-"+hseg+" ";               
            }
            pathtext += "RH " + lseg + " ";



            // br
            pathtext += "RH " + lseg + " ";
            if (BottomRightCornerShape.Mode == 0)
            {
                pathtext += "RH " + lseg + " ";
                pathtext += "RV " + hseg + " ";
            }
            if (BottomRightCornerShape.Mode == 1)
            {
                pathtext += "RQ " + lseg + ",0 " + lseg + "," + hseg + " ";
            }
            pathtext += "RV " + lseg + " ";

            // tr
            pathtext += "RV " + lseg + " ";
            if (TopRightCornerShape.Mode == 0)
            {
                pathtext += "RV " + hseg + " ";
                pathtext += "RH -" + lseg + " ";
            }
            if (TopRightCornerShape.Mode == 1)
            {
                pathtext += "RQ 0," +hseg+ " -" + lseg + "," + hseg + " ";
            }
            pathtext += "RH -" + lseg + " ";

            // tl
            pathtext += "RH -" + lseg + " ";
            if (TopLeftCornerShape.Mode == 0)
            {
                pathtext += "RH -" + lseg + " ";
                pathtext += "RV -" + hseg + " ";
            }
            if (TopLeftCornerShape.Mode == 1)
            {
                pathtext += "RQ -" + hseg + ",0 -" + lseg + ",-" + hseg + " ";
            }
            pathtext += "RV -" + lseg + " ";

            GenerateFromPath(pathtext);
            Redisplay();
        }

        private void GenerateFromPath(string pathtext)
        {
            FlexiPath flexiPath = new FlexiPath();
            flexiPath.FromTextPath(pathtext);
            List<System.Windows.Point> points = flexiPath.DisplayPoints();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
           

            // generate side triangles so original points are already in list
            for (int i = 0; i < points.Count; i++)
            {
                CreateSideFace(points, i);
            }
            // triangulate the basic polygon
            TriangulationPolygon ply = new TriangulationPolygon();
            List<PointF> pf = new List<PointF>();
            foreach (System.Windows.Point p in points)
            {
                pf.Add(new PointF((float)p.X, (float)p.Y));
            }
            ply.Points = pf.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(t.Points[0].X, t.Points[0].Y, 0.0);
                int c1 = AddVertice(t.Points[1].X, t.Points[1].Y, 0.0);
                int c2 = AddVertice(t.Points[2].X, t.Points[2].Y, 0.0);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);

                c0 = AddVertice(t.Points[0].X, t.Points[0].Y, depth);
                c1 = AddVertice(t.Points[1].X, t.Points[1].Y, depth);
                c2 = AddVertice(t.Points[2].X, t.Points[2].Y, depth);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);
            }
            CentreVertices();
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
            TopLeftCornerShape.OnModeChanged += TLMode;

            TopRightCornerShape.Location = CornerShape.CornerLocation.TopRight;
            TopRightCornerShape.OnModeChanged += TRMode;

            BottomLeftCornerShape.Location = CornerShape.CornerLocation.BottomLeft;
            BottomLeftCornerShape.OnModeChanged += BLMode;

            BottomRightCornerShape.Location = CornerShape.CornerLocation.BottomRight;
            BottomRightCornerShape.OnModeChanged += BRMode;

            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }

        private void TLMode(int mode)
        {
            UpdateDisplay();
        }
        private void TRMode(int mode)
        {
            UpdateDisplay();
        }
        private void BLMode(int mode)
        {
            UpdateDisplay();
        }
        private void BRMode(int mode)
        {
            UpdateDisplay();
        }

    }
}