﻿using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for SpurGearDialog.xaml
    /// </summary>
    public partial class SpurGearDialog : BaseModellerDialog, INotifyPropertyChanged
    {
        private double thickness;
        private double outterRadius;
        private int numberOfTeeth;
        private double teethBaseWidth;
        private double teethBaseHeight;
        private double teethTopWidth;
        private double teethTopHeight;
        private bool updateDisplayWhenChanged;
        private List<System.Windows.Point> points;

        public SpurGearDialog()
        {
            InitializeComponent();
            DataContext = this;
            ToolName = "SpurGear";
            // stops us trying to make a 3d model whie we are still just initialising
            updateDisplayWhenChanged = false;
            points = new List<System.Windows.Point>();
        }

        public double Thickness
        {
            get => thickness;
            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        private void Redisplay()
        {
            if (updateDisplayWhenChanged)
            {
                GenerateShape();
            }
        }

        public double Radius
        {
            get => outterRadius;
            set
            {
                if (outterRadius != value)
                {
                    outterRadius = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public int NumberOfTeeth
        {
            get => numberOfTeeth;
            set
            {
                if (numberOfTeeth != value)
                {
                    numberOfTeeth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double TeethBaseWidth
        {
            get => teethBaseWidth;
            set
            {
                if (teethBaseWidth != value)
                {
                    teethBaseWidth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double TeethBaseHeight
        {
            get => teethBaseHeight;
            set
            {
                if (teethBaseHeight != value)
                {
                    teethBaseHeight = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double TeethTopWidth
        {
            get => teethTopWidth;
            set
            {
                if (teethTopWidth != value)
                {
                    teethTopWidth = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public double TeethTopHeight
        {
            get => teethTopHeight;
            set
            {
                if (teethTopHeight != value)
                {
                    teethTopHeight = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        private void BaseModellerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (EditorParameters.Get("Radius") == "")
            {
                Thickness = 2;
                Radius = 10;
                NumberOfTeeth = 15;
                TeethBaseHeight = 2;
                TeethBaseWidth = 2;
                TeethTopHeight = 1;
                TeethTopWidth = 1;
            }
            else
            {
                Thickness = Convert.ToDouble(EditorParameters.Get("Thickness"));
                Radius = Convert.ToDouble(EditorParameters.Get("Radius"));
                NumberOfTeeth = Convert.ToInt16(EditorParameters.Get("NumberOfTeeth"));
                TeethBaseHeight = Convert.ToDouble(EditorParameters.Get("TeethBaseHeight"));
                TeethBaseWidth = Convert.ToDouble(EditorParameters.Get("TeethBaseWidth"));
                TeethTopHeight = Convert.ToDouble(EditorParameters.Get("TeethTopHeight"));
                TeethTopWidth = Convert.ToDouble(EditorParameters.Get("TeethTopWidth"));
            }

            GenerateShape();
            updateDisplayWhenChanged = true;
            UpdateCameraPos();
        }

        private void GenerateShape()
        {
            if (numberOfTeeth > 2)
            {
                if (thickness > 0)
                {
                    double actualOutterRadius = outterRadius;

                    double circumference = Math.PI * 2 * outterRadius;
                    double minTeethWidth = circumference / numberOfTeeth;
                    double actualBaseWidth = teethBaseWidth;
                    if (teethBaseWidth > minTeethWidth)
                    {
                        actualBaseWidth = minTeethWidth;
                    }

                    double totalGapLeft = circumference - (actualBaseWidth * numberOfTeeth);

                    double gapPerTooth = totalGapLeft / numberOfTeeth;
                    if (gapPerTooth <= 0)
                    {
                        gapPerTooth = 0.1;
                    }
                    double toothTopRadius = actualOutterRadius + teethBaseHeight + TeethTopHeight;
                    double toothTopCircum = Math.PI * 2 * toothTopRadius;
                    double toothTopDelta = (teethTopWidth / toothTopCircum) * Math.PI * 2;

                    points.Clear();
                    double gapDeltaTheta = gapPerTooth / circumference * Math.PI * 2;
                    double toothDeltaTheta = teethBaseWidth / circumference * Math.PI * 2;

                    double toothTopCenteringTheta = (toothDeltaTheta - toothTopDelta) / 2;

                    double theta = 0;
                    bool tooth = true;
                    while (theta <= Math.PI * 2)
                    {
                        //     2 ---- 3
                        //    /        \
                        //   /          \
                        //   1           4
                        //   !           !
                        //   !           !
                        //   0           5
                        // (Sort of)
                        if (tooth && (Math.PI * 2 - theta > toothDeltaTheta))
                        {
                            // add a tooth
                            // 0
                            double x = outterRadius * Math.Cos(theta);
                            double y = outterRadius * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            //1
                            x = (outterRadius + teethBaseHeight) * Math.Cos(theta);
                            y = (outterRadius + teethBaseHeight) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            //2
                            theta += toothTopCenteringTheta;
                            x = (toothTopRadius) * Math.Cos(theta);
                            y = (toothTopRadius) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            //3
                            theta += toothTopDelta;
                            x = (toothTopRadius) * Math.Cos(theta);
                            y = (toothTopRadius) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            // 4
                            theta += toothTopCenteringTheta;
                            x = (outterRadius + teethBaseHeight) * Math.Cos(theta);
                            y = (outterRadius + teethBaseHeight) * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));

                            // 5
                            x = outterRadius * Math.Cos(theta);
                            y = outterRadius * Math.Sin(theta);
                            points.Add(new System.Windows.Point(x, y));
                        }
                        else
                        {
                            // Add a gap
                            theta += gapDeltaTheta;
                            if (theta < Math.PI * 2)
                            {
                                double x = outterRadius * Math.Cos(theta);
                                double y = outterRadius * Math.Sin(theta);
                                points.Add(new System.Windows.Point(x, y));
                            }
                        }
                        tooth = !tooth;
                    }
                    DisplayFlatView(points);
                    Vertices.Clear();
                    Faces.Clear();

                    // generate side triangles so original points are already in list
                    for (int i = 0; i < points.Count; i++)
                    {
                        CreateSideFace(i);
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

                        c0 = AddVertice(t.Points[0].X, t.Points[0].Y, thickness);
                        c1 = AddVertice(t.Points[1].X, t.Points[1].Y, thickness);
                        c2 = AddVertice(t.Points[2].X, t.Points[2].Y, thickness);
                        Faces.Add(c0);
                        Faces.Add(c1);
                        Faces.Add(c2);
                    }

                    CentreVertices();
                    GeometryModel3D gm = GetModel();
                    MyModelGroup.Children.Clear();
                    if (floor != null)
                    {
                        MyModelGroup.Children.Add(floor.FloorMesh);
                        foreach (GeometryModel3D m in grid.Group.Children)
                        {
                            MyModelGroup.Children.Add(m);
                        }
                    }
                    MyModelGroup.Children.Add(gm);
                }
            }
        }

        private void CreateSideFace(int i)
        {
            int v = i + 1;
            if (v == points.Count)
            {
                v = 0;
            }

            int c0 = AddVertice(points[i].X, points[i].Y, 0.0);
            int c1 = AddVertice(points[i].X, points[i].Y, thickness);
            int c2 = AddVertice(points[v].X, points[v].Y, thickness);
            int c3 = AddVertice(points[v].X, points[v].Y, 0.0);
            Faces.Add(c0);
            Faces.Add(c2);
            Faces.Add(c1);

            Faces.Add(c0);
            Faces.Add(c3);
            Faces.Add(c2);
        }

        private void DisplayFlatView(List<System.Windows.Point> pnts)
        {
            FlatView.Children.Clear();
            double cx = FlatView.ActualWidth / 2;
            double cy = FlatView.ActualHeight / 2;
            Polyline pl = new Polyline();
            pl.Stroke = System.Windows.Media.Brushes.Blue;
            pl.StrokeThickness = 1;
            List<System.Windows.Point> canvasPnts = new List<System.Windows.Point>();
            foreach (System.Windows.Point p in pnts)
            {
                System.Windows.Point np = new System.Windows.Point(cx + p.X, cy + p.Y);
                pl.Points.Add(np);
            }
            FlatView.Children.Add(pl);
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            EditorParameters.Set("Thickness", Thickness.ToString("F4"));
            EditorParameters.Set("Radius", Radius.ToString("F4"));
            EditorParameters.Set("NumberOfTeeth", NumberOfTeeth.ToString());
            EditorParameters.Set("TeethBaseHeight", TeethBaseHeight.ToString("F4"));
            EditorParameters.Set("TeethBaseWidth", TeethBaseWidth.ToString("F4"));
            EditorParameters.Set("TeethTopHeight", TeethTopHeight.ToString("F4"));
            EditorParameters.Set("TeethTopWidth", TeethTopWidth.ToString("F4"));
            DialogResult = true;
            Close();
        }
    }
}