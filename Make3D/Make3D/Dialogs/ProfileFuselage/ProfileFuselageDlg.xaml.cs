﻿/**************************************************************************
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

using Microsoft.Win32;
using PolygonTriangulationLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class ProfileFuselageDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        public List<LetterMarker> markers;
        private bool autoFit;
        private bool backBody;
        private bool dirty;
        private Int32Collection faces;
        private string filePath;
        private bool frontBody;
        private MeshGeometry3D mesh;
        private NoteWindow noteWindow;

        private int numberOfDivisions;

        private string sideViewFilename;
        private string topViewFilename;
        private Point3DCollection vertices;
        private bool wholeBody;
        private double zoomLevel;

        public ProfileFuselageDlg()
        {
            InitializeComponent();
            ToolName = "ProfileFuselage";
            DataContext = this;
            faces = new Int32Collection();
            mesh = new MeshGeometry3D();
            vertices = new Point3DCollection();
            WholeBody = true;
            autoFit = false;
            zoomLevel = 1;
            dirty = false;
            filePath = "";
            noteWindow = new NoteWindow();
            numberOfDivisions = 80;
        }

        public bool AutoFit
        {
            get
            {
                return autoFit;
            }

            set
            {
                if (autoFit != value)
                {
                    autoFit = value;
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public bool BackBody
        {
            get
            {
                return backBody;
            }

            set
            {
                if (backBody != value)
                {
                    backBody = value;
                    NotifyPropertyChanged();
                    if (backBody)
                    {
                        RibManager.ModelType = 1;
                    }

                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public bool FrontBody
        {
            get
            {
                return frontBody;
            }

            set
            {
                if (frontBody != value)
                {
                    frontBody = value;
                    NotifyPropertyChanged();
                    if (frontBody)
                    {
                        RibManager.ModelType = 2;
                    }
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public List<LetterMarker> Markers
        {
            get
            {
                return markers;
            }

            set
            {
                if (markers != value)
                {
                    markers = value;
                }
            }
        }

        public int NumberOfDivisions
        {
            get
            {
                return numberOfDivisions;
            }

            set
            {
                if (numberOfDivisions != value)
                {
                    if ((value >= 10) && (value <= 200))
                    {
                        numberOfDivisions = value;
                        RibManager.NumberOfProfilePoints = numberOfDivisions;
                        GenerateSkin();
                        Redisplay();
                        NotifyPropertyChanged();
                    }
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

        public bool WholeBody
        {
            get
            {
                return wholeBody;
            }

            set
            {
                if (wholeBody != value)
                {
                    wholeBody = value;
                    if (wholeBody)
                    {
                        RibManager.ModelType = 0;
                    }
                    NotifyPropertyChanged();
                    GenerateSkin();
                    Redisplay();
                }
            }
        }

        public void MarkerMoved(string s, System.Windows.Point p, bool finishedMove)
        {
            dirty = true;
            TopView.SetMarker(s, p.X);
            SideView.SetMarker(s, p.X);
            SortRibs();
            UpdateDisplay(finishedMove);
        }

        public async void OnCommand(string com)
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            switch (com)
            {
                case "Load Top":
                    {
                        opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                topViewFilename = opDlg.FileName;
                                TopViewManager.ImageFilePath = topViewFilename;
                                TopView.UpdateDisplay();
                                /*
                                TopView.ImageFilePath = topViewFilename;
                                if (TopView.IsValid)
                                {
                                    TopView.UpdateLayout();
                                    UpdateLimits();
                                    dirty = true;
                                }
                                */
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;

                case "Load Side":
                    {
                        opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                sideViewFilename = opDlg.FileName;
                                SideViewManager.ImageFilePath = sideViewFilename;
                                SideView.UpdateDisplay();
                                /*
                                if (SideView.IsValid)
                                {
                                    SideView.UpdateLayout();
                                    UpdateLimits();
                                }
                                */
                                dirty = true;
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;

                case "SaveProject":
                    {
                        SaveProject();
                    }
                    break;

                case "LoadProject":
                    {
                        opDlg.Filter = "Fusalage spar files (*.spr) | *.spr";
                        if (opDlg.ShowDialog() == true)
                        {
                            try
                            {
                                filePath = opDlg.FileName;
                                noteWindow.Owner = this;
                                noteWindow.Show();

                                RibManager.ControlsEnabled = false;
                                /*  await */
                                Read(filePath);
                                noteWindow.Hide();
                                dirty = false;
                                RibManager.ControlsEnabled = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                RibManager.ControlsEnabled = true;
                                noteWindow.Hide();
                            }
                        }
                    }
                    break;
            }
        }

        public void OnRibAdded(string name, RibAndPlanEditControl rc)
        {
            double nextX = 0;
            double nextY = 10;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position.X >= nextX)
                {
                    nextX = mk.Position.X + 10;
                }
                nextY = 40 - nextY;
            }
            CreateLetter(name, new System.Windows.Point(nextX, nextY), rc);
            TopView.AddRib(name);
            SideView.AddRib(name);
            dirty = true;
        }

        public void OnRibDeleted(RibAndPlanEditControl rc)
        {
            LetterMarker target = null;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Rib == rc)
                {
                    target = mk;
                    break;
                }
            }
            if (target != null)
            {
                markers.Remove(target);
            }
            TopView.DeleteMarker(rc);
            SideView.DeleteMarker(rc);
            dirty = true;
        }

        public void OnRibInserted(string name, RibAndPlanEditControl rc)
        {
            double nextX = 10;
            double nextY = 10;
            foreach (LetterMarker mk in markers)
            {
                if (mk.Position.X >= nextX)
                {
                    nextX = mk.Position.X + 10;
                }
                nextY = 40 - nextY;
            }
            CreateLetter(name, new System.Windows.Point(nextX, nextY), rc);

            TopView.AddRib(name);
            SideView.AddRib(name);
            UpdateDisplay();
            dirty = true;
        }

        public void PinMoved(int x)
        {
            TopView.PinPos = x;
            SideView.PinPos = x;
            dirty = true;
        }

        protected override void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            noteWindow.Close();
            Close();
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (dirty)
            {
                MessageBoxResult res = MessageBox.Show("Profile has changed. Do you want to save it?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    SaveProject();
                }
            }
            SaveEditorParmeters();
            DialogResult = true;
            noteWindow.Close();
            Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeBack();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void Bottom_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeBottom();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void CopyLetter(string name)
        {
            RibManager.CopyARib(name);
        }

        private void CopyPathsToViews()
        {
            CopyPathToSideView();
            CopyPathToFrontView();
        }

        private void CopyPathToFrontView()
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            Bitmap bmp = null;
            //get the flexipath from  the top and render the path onto an image
            // TopViewManager.RenderFlexipath(ref bmp, out tlx, out tly, out brx, out bry);
            TopView.WorkingImage = bmp;
            // TopView.SetupImage( tlx, tly, brx, bry);
            TopView.UpdateDisplay();
        }

        private void CopyPathToSideView()
        {
            double tlx = 0;
            double tly = 0;
            double brx = 0;
            double bry = 0;
            //get the flexipath from  the side and render the path onto an image
            Bitmap bmp = null;
            // SideViewManager.RenderFlexipath(ref bmp, out tlx, out tly, out brx, out bry);
            // SideView.WorkingImage = bmp; SideView.SetupImage(tlx, tly, brx, bry);
            SideView.UpdateDisplay();
        }

        private void CreateLetter(string v1, System.Windows.Point v2, RibAndPlanEditControl rib)
        {
            LetterMarker mk = new LetterMarker(v1, v2);
            mk.Rib = rib;
            markers.Add(mk);
        }

        private void Front_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeFront();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void GenerateSkin()
        {
            try
            {
                // clear out existing 3d model
                ClearShape();
                if (RibManager.Ribs != null && RibManager.Ribs.Count > 1)
                {
                    if (SideView.WorkingImage != null && SideView.WorkingImage.Width > 0 && TopView.WorkingImage != null && TopView.WorkingImage.Width > 0)
                    {
                        bool okToGenerate = true;
                        double prevX = 0;
                        List<RibAndPlanEditControl> theRibs = new List<RibAndPlanEditControl>();
                        List<double> ribXs = new List<double>();
                        List<Dimension> topDims = new List<Dimension>();
                        List<Dimension> sideDims = new List<Dimension>();

                        for (int i = 0; i < RibManager.Ribs.Count; i++)
                        {
                            double x = markers[i].Position.X;

                            RibAndPlanEditControl cp;
                            if (RibManager.Ribs[i].Dirty)
                            {
                                cp = RibManager.Ribs[i].Clone(false);
                                cp.GenerateProfilePoints();
                            }
                            else
                            {
                                cp = RibManager.Ribs[i];
                            }
                            string cpPath = cp.GenPath();
                            if (autoFit && theRibs.Count > 0)
                            {
                                string prevPath = theRibs[theRibs.Count - 1].PathText;

                                if (cpPath == prevPath)
                                {
                                    if (x - prevX > 4)
                                    {
                                        double nx = prevX + 1;
                                        while (nx < x)
                                        {
                                            RibAndPlanEditControl nr = RibManager.Ribs[i].Clone(false);
                                            // nr.GenerateProfilePoints();
                                            theRibs.Add(nr);
                                            ribXs.Add(nx);
                                            Dimension dp1 = TopView.GetUpperAndLowerPoints((int)nx);
                                            topDims.Add(dp1);
                                            dp1 = SideView.GetUpperAndLowerPoints((int)nx);
                                            sideDims.Add(dp1);
                                            nx += 4;
                                        }
                                    }
                                }
                            }
                            theRibs.Add(cp);
                            ribXs.Add(x);
                            Dimension dp = TopView.GetUpperAndLowerPoints((int)x);
                            topDims.Add(dp);
                            dp = SideView.GetUpperAndLowerPoints((int)x);
                            sideDims.Add(dp);
                            prevX = x;
                        }

                        for (int i = 0; i < theRibs.Count; i++)
                        {
                            if (theRibs[i].ProfilePoints.Count == 0)
                            {
                                okToGenerate = false;
                                break;
                            }
                        }
                        // do we have enough data to construct the model
                        if (theRibs.Count > 1 && okToGenerate)
                        {
                            // theRibs[0].GenerateProfilePoints();
                            int facesPerRib = theRibs[0].ProfilePoints.Count;
                            int[,] ribvertices = new int[theRibs.Count, facesPerRib];
                            // there should be a marker and hence a dimension for every rib. If ther
                            // isn't then somethins wrong
                            if (theRibs.Count != topDims.Count)
                            {
                                System.Diagnostics.Debug.WriteLine($"Ribs {theRibs.Count} TopView Dimensions {topDims.Count}");
                            }
                            else
                            {
                                // work out the range of faces we are going to do based upon whether
                                // we are doing the whole model or just fron or back

                                // assume its whole model
                                int start = 0;
                                int end = facesPerRib;
                                if (frontBody)
                                {
                                    end = facesPerRib / 2;
                                }
                                if (backBody)
                                {
                                    start = facesPerRib / 2;
                                }

                                List<PointF> leftEdge = new List<PointF>();
                                List<PointF> rightEdge = new List<PointF>();
                                double x = TopView.GetXmm(ribXs[0]);
                                double leftx = x;
                                double rightx = x;
                                int vert = 0;
                                int vindex = 0;
                                for (int i = 0; i < theRibs.Count; i++)
                                {
                                    x = TopView.GetXmm(ribXs[i]);

                                    if (i == theRibs.Count - 1)
                                    {
                                        rightx = x;
                                    }

                                    vindex = 0;
                                    for (int proind = start; proind < end; proind++)
                                    {
                                        if (proind < theRibs[i].ProfilePoints.Count)
                                        {
                                            PointF pnt = theRibs[i].ProfilePoints[proind];

                                            double v = (double)pnt.X * (double)topDims[i].Height;
                                            double z = TopView.GetYmm(v + (double)topDims[i].P1.Y);

                                            v = (double)pnt.Y * (double)sideDims[i].Height;
                                            double y = -SideView.GetYmm(v + sideDims[i].P1.Y);

                                            vert = AddVertice(x, y, z);
                                            ribvertices[i, vindex] = vert;
                                            vindex++;
                                            if (i == 0)
                                            {
                                                leftEdge.Add(new PointF((float)y, (float)z));
                                            }
                                            if (i == theRibs.Count - 1)
                                            {
                                                rightEdge.Add(new PointF((float)y, (float)z));
                                            }
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"ERROR proind {proind} ProfilePoints Count {RibManager.Ribs[i].ProfilePoints.Count}");
                                        }
                                    }
                                }
                                facesPerRib = leftEdge.Count;
                                for (int ribIndex = 0; ribIndex < theRibs.Count - 1; ribIndex++)
                                {
                                    for (int pIndex = 0; pIndex < facesPerRib; pIndex++)
                                    {
                                        int ind2 = pIndex + 1;
                                        if (ind2 >= facesPerRib)
                                        {
                                            ind2 = 0;
                                        }
                                        int v1 = ribvertices[ribIndex, pIndex];
                                        int v2 = ribvertices[ribIndex, ind2];
                                        int v3 = ribvertices[ribIndex + 1, pIndex];
                                        int v4 = ribvertices[ribIndex + 1, ind2];
                                        Faces.Add(v1);
                                        Faces.Add(v2);
                                        Faces.Add(v3);

                                        Faces.Add(v3);
                                        Faces.Add(v2);
                                        Faces.Add(v4);
                                    }
                                }

                                TriangulatePerimiter(leftEdge, leftx, 0, 0, true);
                                TriangulatePerimiter(rightEdge, rightx, 0, 0, false);
                                CentreVertices();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GenerateSkin");
                throw ex;
            }
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeLeft();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private async void LoadEditorParameters()
        {
            string s = EditorParameters.Get("Path");
            if (s != "")
            {
                filePath = s;
                noteWindow.Owner = this;
                noteWindow.Show();

                RibManager.ControlsEnabled = false;
                /*await*/
                Read(filePath);
                noteWindow.Hide();
                dirty = false;
                RibManager.ControlsEnabled = true;
                wholeBody = true;
                frontBody = false;
                backBody = false;
                s = EditorParameters.Get("Model");
                if (s == "Front")
                {
                    wholeBody = false;
                    frontBody = true;
                    backBody = false;
                }
                else
                if (s == "Back")
                {
                    backBody = true;
                    frontBody = false;
                    wholeBody = false;
                }
                s = EditorParameters.Get("NumberOfDivisions");
                if (s != "")
                {
                    numberOfDivisions = Convert.ToInt16(s);
                }
                autoFit = EditorParameters.GetBoolean("AutoFit");
            }
        }

        private void LoadOneRib(int nextY, XmlElement el, string nme, int pos)
        {
            XmlElement imagepcon = (XmlElement)(el.SelectSingleNode("ImagePath"));
            RibAndPlanEditControl rc = new RibAndPlanEditControl();
            rc.Read(el);
            rc.Header = nme;
            rc.Width = 600;
            rc.Height = 600;
            rc.FetchImage();
            if (rc.IsValid)
            {
                rc.TurnOffGrid();
                rc.SetImageSource();
                rc.OnForceReload = RibManager.OnForceRibReload;
                CreateLetter(nme, new System.Windows.Point(pos, nextY), rc);
                RibManager.Ribs.Add(rc);
                rc.GenerateProfilePoints();
            }
        }

        private bool LoadRib(XmlElement el, string pth, string nme, System.Windows.Point position, double viewScale, string edgePath, double scx, double scy)
        {
            bool res = true;
            try
            {
                RibAndPlanEditControl rc = new RibAndPlanEditControl();
                rc.ImagePath = pth;
                rc.Header = nme;
                rc.Width = 600;
                rc.Height = 600;
                rc.Scale = viewScale;
                rc.EdgePath = edgePath;
                rc.ScrollX = scx;
                rc.ScrollY = scy;
                rc.FetchImage();
                if (rc.IsValid)
                {
                    rc.TurnOffGrid();
                    rc.SetImageSource();
                    rc.OnForceReload = RibManager.OnForceRibReload;
                    CreateLetter(nme, position, rc);
                    RibManager.Ribs.Add(rc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadRib");
                res = false;
            }

            return res;
        }

        private void OnPositionTabSelected(object sender, RoutedEventArgs e)
        {
            CopyPathsToViews();
            UpdateDisplay();
        }

        private void OnRibInserted(string name, RibAndPlanEditControl rc, RibAndPlanEditControl after)
        {
            double nextX = 0;
            double nextY = 10;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i].Rib == after)
                {
                    if (i < markers.Count - 1)
                    {
                        nextX = markers[i].Position.X + (markers[i + 1].Position.X - markers[i].Position.X) / 2;
                    }
                    else
                    {
                        nextX = markers[i].Position.X + 10;
                    }
                }
                nextY = 40 - nextY;
            }
            if (nextX == 0 && markers.Count > 0)
            {
                nextX = markers[markers.Count - 1].Position.X + 10;
            }
            CreateLetter(name, new System.Windows.Point(nextX, nextY), rc);
            SortRibs();
            TopView.AddRib(name);
            SideView.AddRib(name);
            UpdateDisplay();
            dirty = true;
        }

        private void OnRibsRenamed(List<RibManager.NameRec> newNames)
        {
            foreach (LetterMarker mk in markers)
            {
                foreach (RibManager.NameRec rc in newNames)
                {
                    if (mk.Letter == rc.originalName)
                    {
                        mk.Letter = rc.newName;
                        break;
                    }
                }
            }
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            dirty = true;
        }

        private async Task Read(string fileName)
        {
            this.Cursor = Cursors.Wait;
            RibManager.Ribs.Clear();
            Markers.Clear();
            TopViewManager.Clear();
            SideViewManager.Clear();
            TopView.Clear();
            SideView.Clear();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(fileName);
            XmlNode docNode = doc.SelectSingleNode("Spars");
            XmlElement ele = docNode as XmlElement;
            string s = ele.GetAttribute("NextLetter");
            RibManager.NextNameLetter = s[0];
            s = ele.GetAttribute("NextNumber");
            RibManager.NextNameNumber = Convert.ToInt32(s);
            XmlElement topNode = docNode.SelectSingleNode("TopView") as XmlElement;
            TopViewManager.PathControl.Read(topNode);
            TopViewManager.PathControl.FetchImage();
            TopViewManager.PathControl.UpdateDisplay();

            XmlElement sideNode = docNode.SelectSingleNode("SideView") as XmlElement;
            SideViewManager.PathControl.Read(sideNode);
            SideViewManager.PathControl.FetchImage();
            SideViewManager.PathControl.UpdateDisplay();
            CopyPathsToViews();

            XmlNodeList nodes = docNode.SelectNodes("Rib");
            int nextY = 10;
            foreach (XmlNode nd in nodes)
            {
                XmlElement el = nd as XmlElement;
                string nme = el.GetAttribute("Header");
                noteWindow.Message = "Loading Rib " + nme;
                int pos = Convert.ToInt16(el.GetAttribute("Position"));
                // await Task.Run(() => LoadOneRib(nextY, el, nme, pos));
                LoadOneRib(nextY, el, nme, pos);
                nextY = 10 - nextY;
            }

            SortRibs();
            // need to update the top and side views BEFORE generating skin
            TopView.UpdateDisplay();
            SideView.UpdateDisplay();
            UpdateLimits();
            GenerateSkin();
            Redisplay();
            noteWindow.Hide();
            this.Activate();
            this.Cursor = Cursors.Arrow;
        }

        private void ResetMarkers_Click(object sender, RoutedEventArgs e)
        {
            if (markers != null && markers.Count > 1)
            {
                double markerDist = ((TopView.RightLimit - TopView.LeftLimit - 4) / (markers.Count - 1));
                for (int i = 0; i < markers.Count; i++)
                {
                    markers[i].Position = new System.Windows.Point((int)(TopView.LeftLimit + (i * markerDist)) + 2, markers[i].Position.Y);
                }
                UpdateDisplay();
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeRight();
            Camera.Distance = 2 * Bounds.Width;
            UpdateCameraPos();
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Fuselage spar files (*.spr) | *.spr";
            if (saveFileDialog.ShowDialog() == true)
            {
                Write(saveFileDialog.FileName);
                filePath = saveFileDialog.FileName;
                dirty = false;
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            if (filePath != "")
            {
                EditorParameters.Set("Path", filePath);
            }
            string md = "Whole";
            if (FrontBody)
            {
                md = "Front";
            }
            if (BackBody)
            {
                md = "Back";
            }
            EditorParameters.Set("Model", md);
            EditorParameters.Set("NumberOfDivisions", numberOfDivisions.ToString());
            EditorParameters.Set("AutoFit", autoFit.ToString());
        }

        private void SaveProject()
        {
            if (filePath == "")
            {
                SaveAs();
            }
            else
            {
                Write(filePath);
                dirty = false;
            }
        }

        private void SortRibs()
        {
            try
            {
                bool swapped = false;
                do
                {
                    swapped = false;
                    for (int i = 0; i < markers.Count - 1; i++)
                    {
                        if (markers[i].Position.X > markers[i + 1].Position.X)
                        {
                            LetterMarker mk = markers[i];
                            markers[i] = markers[i + 1];
                            markers[i + 1] = mk;
                            swapped = true;
                        }
                    }
                } while (swapped);
                int nextY = 10;
                foreach (LetterMarker mk in markers)
                {
                    mk.Position = new System.Windows.Point(mk.Position.X, nextY);
                    nextY = 40 - nextY;
                }
                SideView.Markers = markers;
                TopView.Markers = markers;
                ObservableCollection<RibAndPlanEditControl> ribs = new ObservableCollection<RibAndPlanEditControl>();
                foreach (LetterMarker mk in markers)
                {
                    ribs.Add(mk.Rib);
                }
                RibManager.Ribs = ribs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SortRibs");
            }
        }

        private void Top_Click(object sender, RoutedEventArgs e)
        {
            Camera.HomeTop();
            Camera.DistanceToFit(Bounds.Width, Bounds.Height);
            UpdateCameraPos();
        }

        private void TopViewManager_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TriangulatePerimiter(List<PointF> points, double xo, double yo, double z, bool invert)
        {
            TriangulationPolygon ply = new TriangulationPolygon();

            ply.Points = points.ToArray();
            List<Triangle> tris = ply.Triangulate();
            foreach (Triangle t in tris)
            {
                int c0 = AddVertice(xo, yo + t.Points[0].X, z + t.Points[0].Y);
                int c1 = AddVertice(xo, yo + t.Points[1].X, z + t.Points[1].Y);
                int c2 = AddVertice(xo, yo + t.Points[2].X, z + t.Points[2].Y);
                if (invert)
                {
                    Faces.Add(c0);
                    Faces.Add(c2);
                    Faces.Add(c1);
                }
                else
                {
                    Faces.Add(c0);
                    Faces.Add(c1);
                    Faces.Add(c2);
                }
            }
        }

        private void UpdateDisplay(bool regenSkin = true)
        {
            SideView.UpdateDisplay();
            TopView.UpdateDisplay();
            if (regenSkin)
            {
                GenerateSkin();
            }
            Viewer.Model = GetModel();
            NotifyPropertyChanged("CameraPos");
        }

        private void UpdateLimits()
        {
            if (TopView.IsValid && SideView.IsValid)
            {
                double l = TopView.LeftLimit;
                if (SideView.LeftLimit > l)
                {
                    l = SideView.LeftLimit;
                }

                double r = TopView.RightLimit;
                if (SideView.RightLimit < r)
                {
                    r = SideView.RightLimit;
                }

                TopView.LeftLimit = l;
                SideView.LeftLimit = l;
                TopView.RightLimit = r;
                SideView.RightLimit = r;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            markers = new List<LetterMarker>();
            TopView.SetHeader("Top View");

            TopView.OnPinMoved = PinMoved;
            TopView.OnMarkerMoved = MarkerMoved;
            TopView.OnCopyLetter = CopyLetter;
            TopView.Markers = markers;
            SideView.SetHeader("Side View");

            SideView.OnPinMoved = PinMoved;
            SideView.OnMarkerMoved = MarkerMoved;
            SideView.Markers = markers;
            SideView.OnCopyLetter = CopyLetter;
            RibManager.OnRibAdded = OnRibAdded;
            RibManager.OnRibInserted = OnRibInserted;
            RibManager.OnCommandHandler = OnCommand;
            RibManager.OnRibsRenamed = OnRibsRenamed;
            RibManager.OnRibDeleted = OnRibDeleted;
            SideViewManager.OnCommandHandler = OnCommand;
            SideViewManager.CommandText = "Load Side";
            TopViewManager.OnCommandHandler = OnCommand;
            TopViewManager.CommandText = "Load Top";
            UpdateCameraPos();
            LoadEditorParameters();
            Viewer.Clear();

            UpdateDisplay();
            NotifyPropertyChanged("WholeBody");
            NotifyPropertyChanged("FrontBody");
            NotifyPropertyChanged("BackBody");
            NotifyPropertyChanged("NumberOfDivisions");
            NotifyPropertyChanged("AutoFit");
        }

        private void Write(string f)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Spars");
            docNode.SetAttribute("NextLetter", RibManager.NextNameLetter.ToString());
            docNode.SetAttribute("NextNumber", RibManager.NextNameNumber.ToString());
            XmlElement topNode = doc.CreateElement("TopView");
            TopViewManager.PathControl.Write(doc, topNode);
            //topNode.SetAttribute("Path", TopViewManager.ImageFilePath);

            docNode.AppendChild(topNode);
            XmlElement sideNode = doc.CreateElement("SideView");
            SideViewManager.PathControl.Write(doc, sideNode);
            sideNode.SetAttribute("Path", SideViewManager.ImageFilePath);
            docNode.AppendChild(sideNode);
            foreach (RibAndPlanEditControl ob in RibManager.Ribs)
            {
                foreach (LetterMarker mk in markers)
                {
                    if (mk.Letter == ob.Header)
                    {
                        // ob.Write(doc, docNode, mk.Position.X, mk.Letter);
                        XmlElement ribNode = doc.CreateElement("Rib");
                        ribNode.SetAttribute("Header", ob.Header);
                        ribNode.SetAttribute("Position", mk.Position.X.ToString());
                        docNode.AppendChild(ribNode);
                        ob.Write(doc, ribNode);
                        break;
                    }
                }
            }
            doc.AppendChild(docNode);
            doc.Save(f);
        }

        private void ZoomFit_Click(object sender, RoutedEventArgs e)
        {
            double imageLength = (TopView.RightLimit - TopView.LeftLimit) + 10;
            double imagescale = TopView.ActualWidth / imageLength;

            zoomLevel = imagescale;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel *= 1.1;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel *= 0.9;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            zoomLevel = 1;
            TopView.SetScale(zoomLevel);
            SideView.SetScale(zoomLevel);
        }
    }
}