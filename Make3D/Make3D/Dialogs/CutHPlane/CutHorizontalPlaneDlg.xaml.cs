using Barnacle.Models;
using Barnacle.Object3DLib;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CutHorizontalPlane.xaml
    /// </summary>
    public partial class CutHorizontalPlaneDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxplaneLevel = 300;
        private const double minplaneLevel = -300;
        private bool loaded;
        private HorizontalPlane plane;
        private double planeLevel;
        private string warningText;

        public CutHorizontalPlaneDlg()
        {
            InitializeComponent();
            ToolName = "CutHorizontalPlane";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            planeLevel = 5;
        }

        public double PlaneLevel
        {
            get
            {
                return planeLevel;
            }
            set
            {
                if (planeLevel != value)
                {
                    if (value >= minplaneLevel && value <= maxplaneLevel)
                    {
                        planeLevel = value;
                        if (plane != null)

                        {
                            plane.MoveTo(PlaneLevel);
                        }
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PlaneLevelToolTip
        {
            get
            {
                return $"Plane Level must be in the range {minplaneLevel} to {maxplaneLevel}";
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

        public Point3DCollection OriginalVertices
        {
            get;
            set;
        }

        public Int32Collection OriginalFaces { get; internal set; }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            //  SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        protected override void Redisplay()
        {
            if (ModelGroup != null)
            {
                ModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    ModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        ModelGroup.Children.Add(m);
                    }
                }

                GeometryModel3D gm = GetModel();
                ModelGroup.Children.Add(gm);

                if (plane != null && plane.PlaneMesh != null)
                {
                    ModelGroup.Children.Add(plane.PlaneMesh);
                }
            }
        }

        private void GenerateShape()
        {
            //    ClearShape();
            //       CutHorizontalPlaneMaker maker = new CutHorizontalPlaneMaker(
            //           planeLevel
            //           );
            //       maker.Generate(Vertices, Faces);
            // CentreVertices();
        }

        /*
                private void LoadEditorParameters()
                {
                    // load back the tool specific parameters

                    PlaneLevel = EditorParameters.GetDouble("PlaneLevel", 0);
                }
                */

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        /*
        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("PlaneLevel", PlaneLevel.ToString());
        }
        */

        private void SetDefaults()
        {
            loaded = false;
            PlaneLevel = 0;

            loaded = true;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private Bounds3D bounds;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            //    LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            RestoreOriginal();
            plane = new HorizontalPlane(planeLevel, bounds.Width + 20, bounds.Depth + 20);
            plane.MoveTo(PlaneLevel);
            UpdateDisplay();
        }

        private void RestoreOriginal()
        {
            bounds = new Bounds3D();
            bounds.Zero();
            ClearShape();
            if (OriginalVertices != null)
            {
                foreach (Point3D p in OriginalVertices)
                {
                    AddVertice(new Point3D(p.X, p.Y, p.Z));
                    bounds.Adjust(p);
                }
            }
            Faces.Clear();
            if (OriginalFaces != null)
            {
                foreach (int i in OriginalFaces)
                {
                    Faces.Add(i);
                }
            }
            CentreVertices();
        }


        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            EdgeProcessor edgeProc = new EdgeProcessor();

            Int32Collection newFaces = new Int32Collection();
            for (int i = 0; i < Faces.Count; i += 3)
            {
                int a = Faces[i];
                int b = Faces[i + 1];
                int c = Faces[i + 2];

                int upCount = 0;
                bool aUp = false;
                bool bUp = false;
                bool cUp = false;
                if (Vertices[a].Y > planeLevel)
                {
                    upCount++;
                    aUp = true;
                }
                if (Vertices[b].Y > planeLevel)
                {
                    upCount++;
                    bUp = true;
                }
                if (Vertices[c].Y > planeLevel)
                {
                    upCount++;
                    cUp = true;
                }

                switch (upCount)
                {
                    case 0:
                        {
                            //all three points of trinagle are on or below the cut plane
                        }
                        break;

                    case 1:
                        {
                            //one point of triangle is above the cut plane
                            // clip it against the plane
                            if (aUp)
                            {
                                MakeTri1(a, ref b, ref c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                edgeProc.Add(b, c);
                            }
                            else if (bUp)
                            {
                                MakeTri1(b, ref c, ref a);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                edgeProc.Add(c, a);
                            }
                            else if (cUp)
                            {
                                MakeTri1(c, ref a, ref b);
                                newFaces.Add(c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                edgeProc.Add(a, b);
                            }
                        }
                        break;

                    case 2:
                        {
                            // two points are above the cut plane
                            if (aUp && bUp)
                            {
                                int dp = CrossingPoint(b, c);
                                int ep = CrossingPoint(a, c);
                                newFaces.Add(a);
                                newFaces.Add(b);
                                newFaces.Add(dp);

                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (bUp && cUp)
                            {
                                int dp = CrossingPoint(c, a);
                                int ep = CrossingPoint(a, b);
                                newFaces.Add(b);
                                newFaces.Add(c);
                                newFaces.Add(dp);

                                newFaces.Add(b);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                            else if (cUp && aUp)
                            {
                                int dp = CrossingPoint(a, b);
                                int ep = CrossingPoint(b, c);
                                newFaces.Add(a);
                                newFaces.Add(dp);
                                newFaces.Add(c);

                                newFaces.Add(c);
                                newFaces.Add(dp);
                                newFaces.Add(ep);
                                edgeProc.Add(dp, ep);
                            }
                        }
                        break;

                    case 3:
                        {
                            //all three points of triangle are above the cut plane
                            // entire triangle should be taken as is
                            newFaces.Add(a);
                            newFaces.Add(b);
                            newFaces.Add(c);
                        }
                        break;
                }
            }
            bool moreLoops = true;
            while (moreLoops)
            {
                moreLoops = false;
                List<EdgeRecord> loop = edgeProc.MakeLoop();
                if (loop.Count > 3)
                {
                    TriangulationPolygon ply = new TriangulationPolygon();
                    List<System.Drawing.PointF> pf = new List<System.Drawing.PointF>();
                    foreach (EdgeRecord er in loop)
                    {
                        Point3D p = Vertices[er.Start];
                        pf.Add(new System.Drawing.PointF((float)p.X, (float)p.Z));
                    }
                    ply.Points = pf.ToArray();
                    List<Triangle> tris = ply.Triangulate();
                    foreach (Triangle t in tris)
                    {
                        int c0 = AddVertice(t.Points[0].X, planeLevel, t.Points[0].Y);
                        int c1 = AddVertice(t.Points[1].X, planeLevel, t.Points[1].Y);
                        int c2 = AddVertice(t.Points[2].X, planeLevel, t.Points[2].Y);
                        newFaces.Add(c0);
                        newFaces.Add(c1);
                        newFaces.Add(c2);
                    }
                }
                if ( loop.Count != 0 && edgeProc.EdgeRecords.Count > 0)
                {
                    moreLoops = true;
                }

            }
            Faces.Clear();
            foreach (int j in newFaces)
            {
                Faces.Add(j);
            }

            UpdateDisplay();
        }

        private int CrossingPoint(int a, int b)
        {
            double t;
            double x0 = Vertices[a].X;
            double y0 = Vertices[a].Y;
            double z0 = Vertices[a].Z;
            double x1 = Vertices[b].X;
            double y1 = Vertices[b].Y;
            double z1 = Vertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);
            int res = AddVertice(x, planeLevel, z);
            return res;
        }

        private void MakeTri1(int a, ref int b, ref int c)
        {
            double t;
            double x0 = Vertices[a].X;
            double y0 = Vertices[a].Y;
            double z0 = Vertices[a].Z;
            double x1 = Vertices[b].X;
            double y1 = Vertices[b].Y;
            double z1 = Vertices[b].Z;
            t = (planeLevel - y0) / (y1 - y0);

            double x = x0 + t * (x1 - x0);
            double z = z0 + t * (z1 - z0);

            b = AddVertice(x, planeLevel, z);

            x1 = Vertices[c].X;
            y1 = Vertices[c].Y;
            z1 = Vertices[c].Z;
            t = (planeLevel - y0) / (y1 - y0);

            x = x0 + t * (x1 - x0);
            z = z0 + t * (z1 - z0);

            c = AddVertice(x, planeLevel, z);
        }

        private void UncutButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginal();
            UpdateDisplay();
        }
    }
}