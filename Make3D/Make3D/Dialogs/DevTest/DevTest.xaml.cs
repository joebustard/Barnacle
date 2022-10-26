using Barnacle.Models;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class DevTest : BaseModellerDialog, INotifyPropertyChanged
    {
        private List<System.Windows.Point> displayPoints;
        private bool hollowShape;
        private bool loaded;
        private Visibility showWidth;
        private bool solidShape;
        private double wallWidth;
        private string warningText;
        public DevTest()
        {
            InitializeComponent();
            ToolName = "DevTest";
            DataContext = this;
            // ModelGroup = MyModelGroup;
            PathEditor.OnFlexiPathChanged += PathPointsChanged;
            PathEditor.AbsolutePaths = true;
            wallWidth = 2;
            solidShape = true;
            ModelGroup = MyModelGroup;
            loaded = false;

        }

        public bool HollowShape
        {
            get
            {
                return hollowShape;
            }
            set
            {
                if (hollowShape != value)
                {
                    hollowShape = value;
                    if (hollowShape == true)
                    {
                        ShowWidth = Visibility.Visible;
                    }
                    else
                    {
                        ShowWidth = Visibility.Hidden;
                    }

                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        public Visibility ShowWidth
        {
            get
            {
                return showWidth;
            }
            set
            {
                if (showWidth != value)
                {
                    showWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool SolidShape
        {
            get
            {
                return solidShape;
            }
            set
            {
                if (solidShape != value)
                {
                    solidShape = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }
        public double WallWidth
        {
            get { return wallWidth; }
            set
            {
                if (wallWidth != value)
                {
                    wallWidth = value;
                    NotifyPropertyChanged();
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
                if (autoclose)
                {
                    v = 0;
                }
                else
                {
                    // dont process the final point if caller doesn't want it
                    return;
                }
            }

            int c0 = AddVertice(pnts[i].X, pnts[i].Y, 0.0);
            int c1 = AddVertice(pnts[i].X, pnts[i].Y, wallWidth);
            int c2 = AddVertice(pnts[v].X, pnts[v].Y, wallWidth);
            int c3 = AddVertice(pnts[v].X, pnts[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void PathPointsChanged(List<System.Windows.Point> pnts)
        {
            displayPoints = pnts;
            GenerateShape();
            Redisplay();
        }

        private void GenerateHollow()
        {
            List<System.Windows.Point> points = displayPoints;
            List<PointF> outerPolygon = new List<PointF>();
            List<PointF> innerPolygon = new List<PointF>();
            ClearShape();
            List<System.Windows.Point> tmp = new List<System.Windows.Point>();
            double top = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Y > top)
                {
                    top = points[i].Y;
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (PathEditor.LocalImage == null)
                {
                    // flipping coordinates so have to reverse polygon too
                    //tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                    tmp.Add( new System.Windows.Point(points[i].X, top - points[i].Y));
                }
                else
                {
                    double x = PathEditor.ToMM(points[i].X);
                    double y = PathEditor.ToMM(top - points[i].Y);
                    tmp.Insert(0, new System.Windows.Point(x, y));
                }
            }

            for (int i = 0; i < tmp.Count-1; i++)
            {
                outerPolygon.Add(new PointF((float)tmp[i].X, (float)tmp[i].Y));
                innerPolygon.Add(new PointF((float)tmp[i].X, (float)tmp[i].Y));
            }
            
            outerPolygon = LineUtils.RemoveCoplanarSegments(outerPolygon);
            innerPolygon = LineUtils.RemoveCoplanarSegments(innerPolygon);
            // outerPolygon = LineUtils.GetEnlargedPolygon(outerPolygon, (float)wallWidth / 2.0F);
            innerPolygon = LineUtils.GetEnlargedPolygon(innerPolygon, -(float)wallWidth);
            
            //innerPolygon = ShrinkPolygon(outerPolygon, wallWidth); 

            tmp.Clear();
            for (int i = outerPolygon.Count - 1; i >= 0; i--)
            {
                tmp.Add(new System.Windows.Point(outerPolygon[i].X, outerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }

            tmp.Clear();
            for (int i = 0; i < innerPolygon.Count; i++)
            {
                tmp.Add(new System.Windows.Point(innerPolygon[i].X, innerPolygon[i].Y));
            }
            // generate side triangles so original points are already in list
            for (int i = 0; i < tmp.Count; i++)
            {
                CreateSideFace(tmp, i);
            }

            for (int i = 0; i < outerPolygon.Count; i++)
            {
                int j = i + 1;
                if (j == outerPolygon.Count)
                {
                    j = 0;
                }
                int c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, 0.0);
                int c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, 0.0);
                int c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, 0.0);
                int c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, 0.0);
                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c1);



                Faces.Add(c0);
                Faces.Add(c3);
                Faces.Add(c2);



                c0 = AddVertice(outerPolygon[i].X, outerPolygon[i].Y, wallWidth);
                c1 = AddVertice(innerPolygon[i].X, innerPolygon[i].Y, wallWidth);
                c2 = AddVertice(innerPolygon[j].X, innerPolygon[j].Y, wallWidth);
                c3 = AddVertice(outerPolygon[j].X, outerPolygon[j].Y, wallWidth);
                Faces.Add(c0);
                Faces.Add(c1);
                Faces.Add(c2);



                Faces.Add(c0);
                Faces.Add(c2);
                Faces.Add(c3);


            }

            CentreVertices();
        }

        private List<PointF> ShrinkPolygon(List<PointF> ply, double offset)
        {
            List<PointF> res = new List<PointF>();
            float  cX=0.0F;
            float cY=0.0F;
            foreach ( PointF pf in ply)
            {
                cX += pf.X;
                cY += pf.Y;
            }
            cX /= ply.Count;
            cY /= ply.Count;

            float dx;
            float dy;
            foreach (PointF pf in ply)
            {
                dx = pf.X - cX;
                dy = pf.Y - cY;
                double dist = Math.Sqrt((dx * dx) + (dy * dy));
                double theta = Math.Atan2(dy, dx);
                if ( dist > offset)
                {
                    dist -= offset;
                }
                PointF np = new PointF((float)(dist * Math.Cos(theta)) + cX, (float)(dist*Math.Sin(theta)) + cY);
                res.Add(np);
            }
            return res;
        }

        private void GenerateShape()
        {
            if (loaded)
            {
                if (SolidShape)
                {
                    GenerateSolid();
                }
                else
                {
                    if (HollowShape)
                    {
                        GenerateHollow();
                    }
                    else
                    {
                        //GenerateLine();
                    }
                }
            }
        }

        private void GenerateSolid()
        {
            ClearShape();
            if (displayPoints?.Count > 3)
            {
                List<System.Windows.Point> points = displayPoints;
                List<System.Windows.Point> tmp = new List<System.Windows.Point>();
                double top = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Y > top)
                    {
                        top = points[i].Y;
                    }
                }
                for (int i = 0; i < points.Count; i++)
                {
                        if (PathEditor.LocalImage == null)
                        {
                            // flipping coordinates so have to reverse polygon too
                            tmp.Insert(0, new System.Windows.Point(points[i].X, top - points[i].Y));
                        }
                        else
                        
                    {
                        double x = PathEditor.ToMM(points[i].X);
                        double y = PathEditor.ToMM(top - points[i].Y);
                        tmp.Insert(0, new System.Windows.Point(x, y));
                    }
                }

                // generate side triangles so original points are already in list
                for (int i = 0; i < tmp.Count; i++)
                {
                    CreateSideFace(tmp, i);
                }
                // triangulate the basic polygon
                TriangulationPolygon ply = new TriangulationPolygon();
                List<PointF> pf = new List<PointF>();
                foreach (System.Windows.Point p in tmp)
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

                    c0 = AddVertice(t.Points[0].X, t.Points[0].Y, wallWidth);
                    c1 = AddVertice(t.Points[1].X, t.Points[1].Y, wallWidth);
                    c2 = AddVertice(t.Points[2].X, t.Points[2].Y, wallWidth);
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
                CentreVertices();
            }
        }
        private void LoadEditorParameters()
        {
            string imageName = EditorParameters.Get("ImagePath");
            if (imageName != "")
            {
                PathEditor.LoadImage(imageName);
            }

            String s = EditorParameters.Get("Path");
            if (s != "")
            {
                PathEditor.FromString(s);
            }
            WallWidth = EditorParameters.GetDouble("WallWidth", 10);
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ImagePath", PathEditor.ImagePath);
            EditorParameters.Set("Path", PathEditor.PathString);
            EditorParameters.Set("WallWidth", WallWidth.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            LoadEditorParameters();
            loaded = true;
            GenerateShape();
            UpdateCameraPos();
            // MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}