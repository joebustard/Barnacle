/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using asdflibrary;
using Barnacle.Dialogs.ClaySculpt;
using Barnacle.Dialogs.WireFrame;
using Barnacle.Models;
using Barnacle.Object3DLib;
using HalfEdgeLib;
using MakerLib;

using sdflib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ClaySculpt.xaml
    /// </summary>
    public partial class ClaySculptDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const int cellsPerSector = 16;
        private const double maxtoolsSize = 10;
        private const double maxtoolStrength = 10;
        private const double mintoolsSize = 1;
        private const double mintoolStrength = 1;
        private GeometryModel3D clayModel;
        private bool claySelected;
        private SculptingTool currentTool;
        private double cx;
        private double cy;
        private double cz;
        private bool loaded;
        private List<GeometryModel3D> markers;
        private int maxX;
        private int maxY;
        private int maxZ;
        private int numbersectors;
        private int op;

        //private PlanktonMesh pmesh;
        private Mesh pmesh;

        private Isdf sdf;
        private SectorModel[,,] sectorModels;
        private double strength;
        private bool symetric;
        private Isdf tool;
        private bool toolInverse;
        private ToolSelectionContent toolSelectionContent;
        private string toolShape;
        private ObservableCollection<String> toolShapeItems;
        private double toolsSize;
        private double toolStrength;
        private string warningText;
        private List<GeometryModel3D> wireframes;

        public ClaySculptDlg()
        {
            InitializeComponent();
            ToolName = "ClaySculpt";
            DataContext = this;
            meshColour = Colors.Brown;

            loaded = false;
            // pmesh = new PlanktonMesh();
            pmesh = new Mesh();
            currentTool = new SculptingTool();
            markers = new List<GeometryModel3D>();
            wireframes = new List<GeometryModel3D>();
            /*
            numbersectors = 10;
            sectorModels = new SectorModel[numbersectors, numbersectors, numbersectors];
            for (int i = 0; i < numbersectors; i++)
            {
                for (int j = 0; j < numbersectors; j++)
                {
                    for (int k = 0; k < numbersectors; k++)
                    {
                        sectorModels[i, j, k] = new SectorModel();
                    }
                }
            }
            maxX = numbersectors * cellsPerSector;
            maxY = numbersectors * cellsPerSector;
            maxZ = numbersectors * cellsPerSector;
            cx = maxX / 2;
            cy = maxY / 2;
            cz = maxZ / 2;
            sdf = new Sdf();
            sdf.SetDimension(maxX, maxY, maxZ);
            SetSphere(sdf, maxX / 2, maxY / 2, maxZ / 2, maxX / 8);
            tool = new Sdf();
            tool.SetDimension(11, 11, 11);
            SetSphere(tool, 5, 5, 5, 4);
            op = 0;

            */
            symetric = true;
            ShowFloor = false;
            ToolShapeItems = new ObservableCollection<string>();
            ToolShapeItems.Add("Inflate");
            ToolShapeItems.Add("Smooth");
            ToolShape = ToolShapeItems[0];
        }

        public bool Symetric
        {
            get
            {
                return symetric;
            }

            set
            {
                if (symetric = value)
                {
                    symetric = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SymetricToolTip
        {
            get
            {
                return "When a change is made to one side of the model, apply it to the other side automaticaly";
            }
        }

        public bool ToolInverse
        {
            get
            {
                return toolInverse;
            }

            set
            {
                if (toolInverse != value)
                {
                    toolInverse = value;
                    if (currentTool != null)
                    {
                        currentTool.Inverse = toolInverse;
                    }
                    //SetOpcode();
                    NotifyPropertyChanged();
                    // UpdateDisplay();
                }
            }
        }

        public String ToolInverseToolTip
        {
            get
            {
                return $"Invert the behaviour of the tool.";
            }
        }

        public string ToolShape
        {
            get
            {
                return toolShape;
            }

            set
            {
                if (toolShape != value)
                {
                    toolShape = value;
                    SetOpcode();
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> ToolShapeItems
        {
            get
            {
                return toolShapeItems;
            }

            set
            {
                if (toolShapeItems != value)
                {
                    toolShapeItems = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String toolShapeToolTip
        {
            get
            {
                return "ToolShape Text";
            }
        }

        public double ToolsSize
        {
            get
            {
                return toolsSize;
            }

            set
            {
                if (toolsSize != value)
                {
                    if (value >= mintoolsSize && value <= maxtoolsSize)
                    {
                        toolsSize = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ToolsSizeToolTip
        {
            get
            {
                return $"ToolsSize must be in the range {mintoolsSize} to {maxtoolsSize}";
            }
        }

        public double ToolStrength
        {
            get
            {
                return toolStrength;
            }

            set
            {
                if (toolStrength != value)
                {
                    if (value >= mintoolStrength && value <= maxtoolStrength)
                    {
                        toolStrength = value;
                        strength = toolStrength / 10.0;
                        if (tool != null)
                        {
                            // SetSphere(tool, 5, 5, 5, 4 * strength);
                        }
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        public String ToolStrengthToolTip
        {
            get
            {
                return $"ToolStrength must be in the range {mintoolStrength} to {maxtoolStrength}";
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
                clayModel = GetModel();
                ModelGroup.Children.Add(clayModel);
                foreach (GeometryModel3D gm in markers)
                {
                    ModelGroup.Children.Add(gm);
                }
                foreach (GeometryModel3D gm in wireframes)
                {
                    if (gm != null)
                    {
                        ModelGroup.Children.Add(gm);
                    }
                }
                /*
                for (int scol = 0; scol < numbersectors; scol++)
                {
                    for (int srow = 0; srow < numbersectors; srow++)
                    {
                        for (int splane = 0; splane < numbersectors; splane++)
                        {
                            clayModel = sectorModels[scol, srow, splane].GetModel();
                            ModelGroup.Children.Add(clayModel);
                        }
                    }
                }
                */
            }
        }

        protected override void Viewport_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    lastHitModel = null;
                    ClearToolSelection();
                    oldMousePos = e.GetPosition(vp);
                    HitTest(vp, oldMousePos);

                    claySelected = false;
                    if (lastHitModel == clayModel)
                    {
                        claySelected = true;
                        markers.Clear();
                        wireframes.Clear();
                        SelectFaceVertices(lastHitV0, lastHitV1, lastHitV2, lastHitPoint, currentTool.Radius);
                    }
                    /*
                    for (int i = 0; i < numbersectors && claySelected == false; i++)
                    {
                        for (int j = 0; j < numbersectors && claySelected == false; j++)
                        {
                            for (int k = 0; k < numbersectors && claySelected == false; k++)
                            {
                                if (lastHitModel == sectorModels[i, j, k].HitModel)
                                {
                                    claySelected = true;
                                }
                            }
                        }
                    }
                    */
                }
                else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    oldMousePos = e.GetPosition(vp);
                }
            }
        }

        protected override void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Viewport3D vp = sender as Viewport3D;
            if (vp != null)
            {
                Point pn = e.GetPosition(vp);
                HitTest(vp, pn);
                double dx = pn.X - oldMousePos.X;
                double dy = pn.Y - oldMousePos.Y;
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    if (claySelected)
                    {
                        if (dx < 1 && dy < 1)
                        {
                            if (lastHitPoint != null)
                            {
                                SelectFaceVertices(lastHitV0, lastHitV1, lastHitV2, lastHitPoint, currentTool.Radius);
                                if (symetric)
                                {
                                }
                            }
                        }
                        else
                        {
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist > 0)
                            {
                                double diff = 1 / dist;
                                if (diff < 0.1)
                                {
                                    diff = 0.1;
                                }
                                for (double t = 0; t <= 1; t += diff)

                                {
                                    double px = oldMousePos.X + t * dx;
                                    double py = oldMousePos.Y + t * dy;
                                    HitTest(vp, new Point(px, py));
                                    if (lastHitPoint != null)
                                    {
                                        SelectFaceVertices(lastHitV0, lastHitV1, lastHitV2, lastHitPoint, currentTool.Radius);
                                        if (symetric)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Camera.Move(dx, -dy);
                        UpdateCameraPos();
                    }

                    oldMousePos = pn;
                    e.Handled = true;
                }
                else
                if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed && e.Handled == false)
                {
                    Camera.Move(dx, -dy);
                    UpdateCameraPos();
                    oldMousePos = pn;
                    e.Handled = true;
                }
            }
        }

        private void ClearToolSelection()
        {
            if (toolSelectionContent != null)

            {
                toolSelectionContent.Clear();
            }
            else
            {
                toolSelectionContent = new ToolSelectionContent(pmesh);
            }
        }

        // private void CreateMarker(PlanktonVertex p, Color cl)
        private void CreateMarker(Vertex p, Color cl)
        {
            MeshGeometry3D mesh = MeshUtils.MakeBorder(p.X, p.Y, p.Z, 2);
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = meshColour;
            mt.Brush = new SolidColorBrush(cl);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = cl;
            mtb.Brush = new SolidColorBrush(cl);
            gm.BackMaterial = mtb;

            markers.Add(gm);
        }

        private void CreateWireframeForEdge(int v, Color cl)
        {
            //PlanktonHalfedge he = pmesh.Halfedges[v];
            HalfEdge he = pmesh.HalfEdges[v];
            int s = he.StartVertex;
            he = pmesh.HalfEdges[he.Next];
            int e = he.StartVertex;
            // PlanktonVertex ve = pmesh.Vertices[s];
            Vertex ve = pmesh.Vertices[s];
            Point3D position1 = new Point3D(ve.X, ve.Y, ve.Z);

            ve = pmesh.Vertices[e];
            Point3D position2 = new Point3D(ve.X, ve.Y, ve.Z);
            WireFrameSegment wf = new WireFrameSegment(position1, position2, 1, cl);
            wireframes.Add(wf.Model);
        }

        private void DirtySector(int v1, int v2, int v3, bool neighbours = true)
        {
            if (v1 >= 0 && v1 < maxX && v2 >= 0 && v2 < maxY && v3 >= 0 && v3 < maxZ)
            {
                int c = v1 / cellsPerSector;
                int r = v2 / cellsPerSector;
                int p = v3 / cellsPerSector;
                sectorModels[c, r, p].Dirty = true;
                if (neighbours)
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            for (int k = -2; k <= 2; k++)
                            {
                                if (i != 0 || j != 0 || k != 0) DirtySector(v1 + i, v2 + j, v3 + k, false);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            if (pmesh != null)
            {
                pmesh.ToSoup(Vertices, Faces);
            }

            // CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            ToolShape = EditorParameters.Get("ToolShape");

            ToolsSize = EditorParameters.GetDouble("ToolsSize", 30);

            ToolStrength = EditorParameters.GetDouble("ToolStrength", 5);

            ToolInverse = EditorParameters.GetBoolean("ToolInverse", false);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("ToolShape", ToolShape);
            EditorParameters.Set("ToolsSize", ToolsSize.ToString());
            EditorParameters.Set("ToolStrength", ToolStrength.ToString());
            EditorParameters.Set("ToolInverse", ToolInverse.ToString());
        }

        private void SelectFaceVertices(int v0, int v1, int v2, Point3D lastHitPoint, double radius)
        {
            toolSelectionContent.InitialVertexSelection(v0);
            toolSelectionContent.InitialVertexSelection(v1);
            toolSelectionContent.InitialVertexSelection(v2);
            toolSelectionContent.SelectedInRange(lastHitPoint, radius);
            foreach (int v in toolSelectionContent.SelectedVertices)
            {
                CreateMarker(pmesh.Vertices[v], Colors.Red);
            }
        }

        private void SetDefaults()
        {
            loaded = false;
            ToolShape = "Inflate";
            ToolsSize = 30;
            ToolStrength = 5;
            ToolInverse = false;

            loaded = true;
        }

        private void SetOpcode()
        {
            if (toolShape == "Inflate")
            {
                if (toolInverse)
                {
                    op = 1;
                }
                else
                {
                    op = 0;
                }
            }
            else if (toolShape == "Smooth")
            {
                op = 2;
            }
        }

        private void ToolProcess()
        {
            /*
            List<int> oppositeEdges = new List<int>();
            List<int> allEdges = new List<int>();
            foreach (int i in toolSelectionContent.SelectedVertices)
            {
                List<int> edges = toolSelectionContent.GetListOfEdgesFromPoint(i);

                foreach (int v in edges)
                {
                    allEdges.Add(v);

                    CreateWireframeForEdge(v, Colors.Blue);
                    PlanktonHalfedge he = pmesh.Halfedges[v];
                    int next = he.NextHalfedge;
                    PlanktonHalfedge nexthe = pmesh.Halfedges[next];
                    if (!allEdges.Contains(next) && !oppositeEdges.Contains(next) && !oppositeEdges.Contains(nexthe.Twin) )
                    {
                        oppositeEdges.Add(next);
                    }
                }
            }
            foreach (int v in oppositeEdges)
            {
                CreateWireframeForEdge(v, Colors.Pink);
            }
            */
            currentTool.ApplyTool(toolSelectionContent, lastHitPoint);
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Viewer.Model = GetModel();
            }
        }

        private void Viewport_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (claySelected)
            {
                ToolProcess();
                UpdateDisplay();
                if (lastHitPoint != null && e.LeftButton == System.Windows.Input.MouseButtonState.Released)
                {
                }
                claySelected = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();
            meshColour = Colors.Chocolate;
            UpdateCameraPos();
            Viewer.Clear();
            ToolShape = ToolShapeItems[0];
            Point3DCollection cubeVertices = new Point3DCollection();
            Int32Collection cubeFaces = new Int32Collection();
            // GenerateSphere(cubeVertices, cubeFaces, new Point3D(0, 0, 0), 50, 5, 5);

            IcoSphereCreator ico = new IcoSphereCreator();
            ico.Create(4, ref cubeVertices, ref cubeFaces, 100);

            // pmesh = new PlanktonMesh(cubeVertices, cubeFaces);

            pmesh = new Mesh(cubeVertices, cubeFaces);
            loaded = true;
            UpdateDisplay();
        }
    }
}