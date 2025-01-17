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

using Barnacle.LineLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ImagePathControl.xaml
    /// </summary>
    public partial class RibAndPlanEditControl : UserControl, INotifyPropertyChanged
    {
        public ForceReload OnForceReload;
        private double brx = double.MinValue;

        private double bry = double.MinValue;

        private bool canCNVDouble;

        private double divisionLength;

        private FlexiPath flexiPath;

        private string fName;

        private List<Shape> gridMarkers;

        private double gridX = 0;

        private double gridY = 0;

        private string header;

        private ImageEdge imageEdge;

        // raw list of points font around the bitmap
        private string imagePath;

        private bool isValid;

        private bool lineShape;

        private bool loaded;

        private BitmapImage localImage;

        private double middleX;

        private double middleY;

        private bool moving;

        private System.Windows.Controls.Image PathBackgroundImage;

        private double pathHeight = 0;

        private string pathText;

        private double pathWidth = 0;

        private ObservableCollection<FlexiPoint> polyPoints;

        private List<PointF> profilePoints;

        private double scale;

        private double scrollX;

        private double scrollY;

        private int selectedPoint;

        private System.Drawing.Bitmap src;

        private double tlx = double.MaxValue;

        private double tly = double.MaxValue;

        private System.Drawing.Bitmap workingImage;

        public RibAndPlanEditControl()
        {
            InitializeComponent();
            Clear();
            loaded = false;
        }

        public delegate void ForceReload(string pth);

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Dirty { get; set; }

        public double EdgeLength
        {
            get
            {
                if (imageEdge != null)
                {
                    return imageEdge.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string EdgePath
        {
            get
            {
                if (flexiPath != null)
                {
                    return flexiPath.ToPath(true);
                }
                else
                {
                    return "";
                }
            }

            set
            {
                if (flexiPath != null)
                {
                    flexiPath.InterpretTextPath(value);
                }
            }
        }

        public string FName
        {
            get
            {
                return fName;
            }

            set
            {
                if (fName != value)
                {
                    fName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Header
        {
            get
            {
                return header;
            }

            set
            {
                if (header != value)
                {
                    header = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get
            {
                return imagePath;
            }

            set
            {
                if (value != imagePath)
                {
                    imagePath = value;
                    if (imagePath != "")
                    {
                        FName = System.IO.Path.GetFileName(imagePath);
                    }
                }
            }
        }

        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }

        public int NumDivisions { get; set; }

        public string PathText
        {
            get
            {
                return pathText;
            }

            set
            {
                if (pathText != value)
                {
                    pathText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<PointF> ProfilePoints
        {
            get
            {
                return profilePoints;
            }

            set
            {
                if (profilePoints != value)
                {
                    profilePoints = value;
                }
            }
        }

        public double Scale
        {
            get
            {
                return scale;
            }

            set
            {
                if (scale != value)
                {
                    scale = value;
                    // SetFlexiPathScale();
                    NotifyPropertyChanged();
                }
            }
        }

        public double ScrollX
        {
            get
            {
                return scrollX;
            }

            set
            {
                if (scrollX != value)
                {
                    scrollX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ScrollY
        {
            get
            {
                return scrollY;
            }

            set
            {
                if (scrollY != value)
                {
                    scrollY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SelectedPoint
        {
            get
            {
                return selectedPoint;
            }

            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    NotifyPropertyChanged();
                    // ClearPointSelections();
                    if (polyPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < polyPoints.Count)
                        {
                            polyPoints[selectedPoint].Selected = true;
                            polyPoints[selectedPoint].Visible = true;
                        }
                    }
                    UpdateDisplay();
                }
            }
        }

        public System.Drawing.Bitmap WorkingImage
        {
            get
            {
                return workingImage;
            }

            set
            {
                workingImage = value;
            }
        }

        public static BitmapSource LoadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public void Clear()
        {
            Dirty = true;
            Header = "";
            FName = "";
            NumDivisions = 80;
            ProfilePoints = new List<PointF>();
            scale = 1;
            // SetFlexiPathScale();
            selectedPoint = -1;
            //SelectionMode = SelectionModeType.SelectSegmentAtPoint;

            isValid = false;
            DataContext = this;
            imagePath = "";
            workingImage = null;
            src = null;
            PathBackgroundImage = new System.Windows.Controls.Image();
            flexiPath = new FlexiPath();

            InitialisePoints();
            polyPoints = flexiPath.FlexiPoints;
            scrollX = 0;
            scrollY = 0;
        }

        public void FetchImage(bool forceReload = false)
        {
            if (!String.IsNullOrEmpty(imagePath))
            {
                LoadImage(imagePath, forceReload);
            }
        }

        public void GenerateProfilePoints(int modelType = 0)
        {
            profilePoints = new List<PointF>();

            List<PointF> pnts = FlexiControl.DisplayPointsF();
            if (pnts != null)
            {
                pnts.Add(new PointF(pnts[0].X, pnts[0].Y));
                tlx = double.MaxValue;
                tly = double.MaxValue;
                brx = double.MinValue;
                bry = double.MinValue;

                double pathLength = 0;
                for (int i = 0; i < pnts.Count; i++)
                {
                    if (i < pnts.Count - 1)
                    {
                        pathLength += Distance(pnts[i], pnts[i + 1]);
                    }
                    if (pnts[i].X < tlx)
                    {
                        tlx = pnts[i].X;
                    }

                    if (pnts[i].Y < tly)
                    {
                        tly = pnts[i].Y;
                    }

                    if (pnts[i].X > brx)
                    {
                        brx = pnts[i].X;
                    }

                    if (pnts[i].Y > bry)
                    {
                        bry = pnts[i].Y;
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Path bounds tlx {tlx} tly {tly} brx {brx} bry {bry}");
                divisionLength = pathLength / (NumDivisions - 1);
                pathWidth = brx - tlx;
                pathHeight = bry - tly;
                middleX = tlx + pathWidth / 2.0;
                middleY = tly + pathHeight / 2.0;
                double minangle = double.MaxValue;
                int minIndex = int.MaxValue;
                System.Diagnostics.Debug.WriteLine($"Pathwidth {pathWidth} PathHeight {pathHeight} middleX {middleX} middleY {middleY}");
                double deltaT = 1.0 / (NumDivisions + 1);
                List<double> angles = new List<double>();
                for (int div = 0; div < NumDivisions; div++)
                {
                    double t = div * deltaT;
                    double targetDistance = t * pathLength;
                    if (targetDistance < pathLength)
                    {
                        double runningDistance = 0;
                        int cp = 1;
                        bool found = false;

                        while (!found && cp < pnts.Count)
                        {
                            PointF p0 = pnts[cp - 1];
                            PointF p1 = pnts[cp];
                            double d = Distance(p0, p1);
                            if ((runningDistance <= targetDistance) &&
                                 (runningDistance + d >= targetDistance))
                            {
                                found = true;
                                double overhang = targetDistance - runningDistance;
                                if (overhang < 0)
                                {
                                    System.Diagnostics.Debug.WriteLine("Distance error creating profile");
                                }
                                if (overhang != 0.0)
                                {
                                    double delta = overhang / d;
                                    double nx = p0.X + (p1.X - p0.X) * delta;
                                    double ny = p0.Y + (p1.Y - p0.Y) * delta;

                                    double rx = nx - middleX;
                                    double ry = ny - middleY;
                                    double rd = Math.Atan2(ry, rx);
                                    angles.Add(rd);
                                    if (rx > 0 && rd < minangle)
                                    {
                                        minIndex = angles.Count - 1;
                                        minangle = rd;
                                    }

                                    nx = (nx - tlx) / pathWidth;
                                    ny = (ny - tly) / pathHeight;
                                    profilePoints.Add(new PointF((float)nx, (float)ny));
                                    System.Diagnostics.Debug.WriteLine($"nx {nx} ny {ny}");
                                }
                                else
                                {
                                    double nx = p0.X;
                                    double ny = p0.Y;

                                    double rx = nx - middleX;
                                    double ry = ny - middleY;
                                    double rd = Math.Atan2(ry, rx);
                                    angles.Add(rd);
                                    if (rx > 0 && rd < minangle)
                                    {
                                        minIndex = angles.Count - 1;
                                        minangle = rd;
                                    }

                                    nx = (nx - tlx) / pathWidth;
                                    ny = (ny - tly) / pathHeight;
                                    profilePoints.Add(new PointF((float)nx, (float)ny));
                                    System.Diagnostics.Debug.WriteLine($"nx {nx} ny {ny}");
                                }
                            }
                            runningDistance += d;
                            cp++;
                        }
                    }
                }


                List<PointF> tmp = new List<PointF>();

                for (int j = minIndex; j < profilePoints.Count; j++)
                {
                    tmp.Add(profilePoints[j]);
                }
                if (minIndex > 0 && minIndex < profilePoints.Count)
                {
                    for (int j = 0; j < minIndex; j++)
                    {
                        tmp.Add(profilePoints[j]);
                    }
                }
                profilePoints = tmp;
                Dirty = false;
            }
        }

        public string GenPath()
        {
            // PathText = flexiPath.ToPath(); return PathText;
            return "";
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetImageSource()
        {
            if (workingImage != null)
            {
                PathBackgroundImage.Source = LoadBitmap(workingImage);
                // FlexiPathCanvas.Width = workingImage.Width; FlexiPathCanvas.Height = workingImage.Height;
            }
        }

        public void UpdateDisplay()
        {
            /*
              FlexiPathCanvas.Children.Clear();
              FlexiPathCanvas.Children.Add(PathBackgroundImage);
              // this is the grid on the path edit
              if (showGrid)
              {
                  if (gridMarkers == null)
                  {
                      gridMarkers = new List<Shape>();
                      BaseModellerDialog.CreateCanvasGrid(FlexiPathCanvas, out gridX, out gridY, 10.0, gridMarkers);
                  }
                  foreach (Shape sh in gridMarkers)
                  {
                      FlexiPathCanvas.Children.Add(sh);
                  }
              }
              DisplayLines();
              DisplayPoints();
              if (showProfilePoints)
              {
                  ShowProfile();
              }
              PathText = flexiPath.ToPath();
              */
        }

        public void UpdateHeaderLabel()
        {
            Dirty = true;
            /*
            HeaderLabel.Content = Header;
            FNameLabel.Content = FName;
            */
        }

        public void Write(XmlDocument doc, XmlElement parentNode)
        {
            XmlElement ele = doc.CreateElement("ImagePath");

            ele.SetAttribute("Path", imagePath);
            ele.SetAttribute("Scale", scale.ToString());
            ele.SetAttribute("ScrollX", ScrollX.ToString());
            ele.SetAttribute("ScrollY", ScrollY.ToString());
            parentNode.AppendChild(ele);
            XmlElement pnts = doc.CreateElement("Edge");
            // make sure we get the points with absolute coordinates
            pnts.InnerText = FlexiControl.ToPath(true);
            ele.AppendChild(pnts);
        }

        internal RibAndPlanEditControl Clone(bool copyImage = true)
        {
            RibAndPlanEditControl cl = new RibAndPlanEditControl();

            cl.Header = Header;
            cl.NumDivisions = NumDivisions;
            cl.TurnOffGrid();
            cl.ImagePath = ImagePath;
            cl.FlexiControl.FromTextPath(FlexiControl.ToPath(true));
            cl.ProfilePoints = new List<PointF>();
            cl.Width = Width;
            cl.Height = Height;
            cl.LoadImage(ImagePath, true);
            foreach (PointF fp in profilePoints)
            {
                PointF fn = new PointF(fp.X, fp.Y);
                cl.ProfilePoints.Add(fn);
            }

            cl.src = src;
            cl.OnForceReload = OnForceReload;

            return cl;
        }

        internal void Read(XmlElement parentNode)
        {
            XmlElement ele = (XmlElement)parentNode.SelectSingleNode("ImagePath");
            ImagePath = ele.GetAttribute("Path");
            if (!String.IsNullOrEmpty(ImagePath))
            {
                LoadImage(imagePath, false);
            }
            scale = Convert.ToDouble(ele.GetAttribute("Scale"));
            scrollX = Convert.ToDouble(ele.GetAttribute("ScrollX"));
            scrollY = Convert.ToDouble(ele.GetAttribute("ScrollY"));
            XmlNode edgeNode = (XmlElement)ele.SelectSingleNode("Edge");
            string p = edgeNode.InnerText;
            FlexiControl.FromString(p);
            //flexiPath.FromTextPath(p);
            loaded = false;
            //ScrollView.ScrollToHorizontalOffset(scrollX);
            //ScrollView.ScrollToVerticalOffset(scrollY);
            UpdateDisplay();
            loaded = true;
            NotifyPropertyChanged("Scale");

            Dirty = true;
        }

        internal void RenderFlexipath(ref Bitmap bmp, out double tlx, out double tly, out double brx, out double bry)
        {
            int sc = 2;
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            int offset = 4;
            List<PointF> pnts = FlexiControl.DisplayPointsF();
            if (bmp == null && pnts.Count > 0 && workingImage != null)
            {
                bmp = new Bitmap(sc * workingImage.Width, sc * workingImage.Height);
            }
            if (bmp != null)
            {
                for (int i = 0; i < pnts.Count; i++)
                {
                    if (pnts[i].X < tlx)
                    {
                        tlx = (double)pnts[i].X;
                    }

                    if (pnts[i].Y < tly)
                    {
                        tly = (double)pnts[i].Y;
                    }

                    if (pnts[i].X > brx)
                    {
                        brx = (double)pnts[i].X;
                    }

                    if (pnts[i].Y > bry)
                    {
                        bry = (double)pnts[i].Y;
                    }
                }
                tlx *= sc;
                tly *= sc;
                brx *= sc;
                bry *= sc;
                using (var gfx = Graphics.FromImage(bmp))
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
                {
                    // draw one thousand random white lines on a dark blue background
                    gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    gfx.Clear(System.Drawing.Color.White);
                    for (int i = 0; i < pnts.Count; i++)
                    {
                        int j = i + 1;
                        if (j >= pnts.Count)
                        {
                            j = 0;
                        }
                        var pt1 = new System.Drawing.Point((int)(pnts[i].X * sc) - (int)tlx + offset, (int)(pnts[i].Y * sc) - (int)(tly) + offset);
                        var pt2 = new System.Drawing.Point((int)(pnts[j].X * sc) - (int)tlx + offset, (int)(pnts[j].Y * sc) - (int)(tly) + offset);
                        gfx.DrawLine(pen, pt1, pt2);
                    }
                }
            }
            brx = brx - tlx + offset;
            bry = bry - tly + offset;
            tlx = offset;
            tly = offset;
        }

        internal void TurnOffGrid()
        {
            FlexiControl.TurnOffGrid();
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private void CopySrcToWorking()
        {
            if (src != null)
            {
                System.Drawing.Color c;
                workingImage = new System.Drawing.Bitmap(src);
                for (int px = 0; px < src.Width; px++)
                {
                    for (int py = 0; py < src.Height; py++)
                    {
                        workingImage.SetPixel(px, py, System.Drawing.Color.White);
                        c = src.GetPixel(px, py);
                        if (c.R < 200 || c.G < 200 || c.B < 200)
                        {
                            if (px < tlx)
                            {
                                tlx = px;
                            }
                            if (py < tly)
                            {
                                tly = py;
                            }
                        }
                    }
                }

                for (int px = (int)tlx; px < src.Width; px++)
                {
                    for (int py = (int)tly; py < src.Height; py++)
                    {
                        c = src.GetPixel(px, py);
                        // if (c.R < 200 || c.G < 200 || c.B < 200)
                        {
                            int tx = px - (int)tlx + 1;
                            int ty = py - (int)tly + 1;
                            if ((tx < workingImage.Width - 1) &&
                                 (ty < workingImage.Height - 1))
                            {
                                //workingImage.SetPixel(tx, ty, System.Drawing.Color.Black);
                                workingImage.SetPixel(tx, ty, c);
                            }
                        }
                    }
                }
            }
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private void InitialisePoints()
        {
            flexiPath.Clear();
            // selectionMode = SelectionModeType.StartPoint;
        }

        // Load a bitmap without locking it.
        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);
            }
        }

        private void LoadImage(string imagePath, bool force)
        {
            if (File.Exists(imagePath))
            {
                if (src == null || force)
                {
                    src = new System.Drawing.Bitmap(imagePath);
                    src = LoadBitmapUnlocked(imagePath);
                    FlexiControl.LoadImage(imagePath);
                    isValid = true;
                }
                ShowWorkingImage();
            }
            else
            {
                MessageBox.Show($"Cant find image {imagePath}");
            }
        }

        private void LoadImage(string f)
        {
            Uri fileUri = new Uri(f);
            localImage = new BitmapImage();
            localImage.BeginInit();
            localImage.UriSource = fileUri;
            // localImage.DecodePixelWidth = 800;
            localImage.EndInit();
            //EditorParameters.Set("ImagePath", f);
            // FlexiPathCanvas.Width = localImage.Width;
            //   FlexiPathCanvas.Height = localImage.Height;
            UpdateDisplay();
        }

        private void ShowWorkingImage()
        {
            CopySrcToWorking();

            SetImageSource();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;

            FlexiControl.AbsolutePaths = true;
            UpdateDisplay();
            loaded = true;
        }

        /*
        public delegate void ForceReload(string pth);

        public event PropertyChangedEventHandler PropertyChanged;

        public enum SelectionModeType
        {
            StartPoint,
            AppendPoint,
            SelectSegmentAtPoint,
            SplitLine,
            ConvertToCubicBezier,
            DeleteSegment,
            ConvertToQuadBezier,
            MovePath
        };

        public bool ShowPolyGrid
        {
            get { return showGrid; }
            set
            {
                if (value != showGrid)
                {
                    showGrid = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool CanCNVDouble
        {
            get { return canCNVDouble; }
            set
            {
                if (canCNVDouble != value)
                {
                    canCNVDouble = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CNVDoubleVisible");
                }
            }
        }

        public Visibility CNVDoubleVisible
        {
            get
            {
                if (canCNVDouble)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }

        public string EdgePath
        {
            get
            {
                if (flexiPath != null)
                {
                    return flexiPath.ToPath(true);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (flexiPath != null)
                {
                    flexiPath.FromTextPath(value);
                }
            }
        }

        public string FName
        {
            get
            {
                return fName;
            }
            set
            {
                if (fName != value)
                {
                    fName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HasPoints
        {
            get
            {
                bool res = true;
                if (selectionMode == SelectionModeType.StartPoint || selectionMode == SelectionModeType.SelectSegmentAtPoint)
                {
                    res = false;
                }
                if (polyPoints.Count < 3)
                {
                    res = false;
                }

                return res;
            }
        }

        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                if (header != value)
                {
                    header = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;
                    if (imagePath != "")
                    {
                        FName = System.IO.Path.GetFileName(imagePath);
                    }
                }
            }
        }

        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }

        public int NumDivisions { get; set; }

        public string PathText
        {
            get
            {
                return pathText;
            }
            set
            {
                if (pathText != value)
                {
                    pathText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<FlexiPoint> Points
        {
            get
            {
                return polyPoints;
            }
            set
            {
                if (value != polyPoints)
                {
                    polyPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<PointF> ProfilePoints
        {
            get
            {
                return profilePoints;
            }
            set
            {
                if (profilePoints != value)
                {
                    profilePoints = value;
                }
            }
        }

        public double Scale
        {
            get
            {
                return scale;
            }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    SetFlexiPathScale();
                    NotifyPropertyChanged();
                }
            }
        }

        public double ScrollX
        {
            get
            {
                return scrollX;
            }
            set
            {
                if (scrollX != value)
                {
                    scrollX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ScrollY
        {
            get
            {
                return scrollY;
            }
            set
            {
                if (scrollY != value)
                {
                    scrollY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SelectedPoint
        {
            get
            {
                return selectedPoint;
            }
            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    NotifyPropertyChanged();
                    ClearPointSelections();
                    if (polyPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < polyPoints.Count)
                        {
                            polyPoints[selectedPoint].Selected = true;
                            polyPoints[selectedPoint].Visible = true;
                        }
                    }
                    UpdateDisplay();
                }
            }
        }

        public SelectionModeType SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                if (value != selectionMode)
                {
                    selectionMode = value;
                    SetButtonBorderColours();
                }
            }
        }

        public bool ShowOrtho
        {
            get
            {
                return showOrtho;
            }
            set
            {
                if (showOrtho != value)
                {
                    showOrtho = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool ShowProfilePoints
        {
            get
            {
                return showProfilePoints;
            }
            set
            {
                if (value != showProfilePoints)
                {
                    showProfilePoints = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool Snap
        {
            get { return snap; }
            set
            {
                if (snap != value)
                {
                    snap = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public System.Drawing.Bitmap WorkingImage
        {
            get
            {
                return workingImage;
            }
            set
            {
                workingImage = value;
            }
        }

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public void Clear()
        {
            Dirty = true;
            Header = "";
            FName = "";
            NumDivisions = 80;
            ProfilePoints = new List<PointF>();
            scale = 1;
            SetFlexiPathScale();
            selectedPoint = -1;
            SelectionMode = SelectionModeType.SelectSegmentAtPoint;

            isValid = false;
            DataContext = this;
            imagePath = "";
            workingImage = null;
            src = null;
            PathBackgroundImage = new System.Windows.Controls.Image();
            flexiPath = new FlexiPath();

            InitialisePoints();
            polyPoints = flexiPath.FlexiPoints;
            scrollX = 0;
            scrollY = 0;
        }

        public void FetchImage(bool forceReload = false)
        {
            LoadImage(imagePath, forceReload);
        }

        public void GenerateProfilePoints(int modelType = 0)
        {
            profilePoints = new List<PointF>();
            List<PointF> pnts = flexiPath.DisplayPointsF();
            pnts.Add(new PointF(pnts[0].X, pnts[0].Y));
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;

            double pathLength = 0;
            for (int i = 0; i < pnts.Count; i++)
            {
                if (i < pnts.Count - 1)
                {
                    pathLength += Distance(pnts[i], pnts[i + 1]);
                }
                if (pnts[i].X < tlx)
                {
                    tlx = pnts[i].X;
                }

                if (pnts[i].Y < tly)
                {
                    tly = pnts[i].Y;
                }

                if (pnts[i].X > brx)
                {
                    brx = pnts[i].X;
                }

                if (pnts[i].Y > bry)
                {
                    bry = pnts[i].Y;
                }
            }
            System.Diagnostics.Debug.WriteLine($"Path bounds tlx {tlx} tly {tly} brx {brx} bry {bry}");
            divisionLength = pathLength / (NumDivisions - 1);
            pathWidth = brx - tlx;
            pathHeight = bry - tly;
            middleX = tlx + pathWidth / 2.0;
            middleY = tly + pathHeight / 2.0;
            double minangle = double.MaxValue;
            int minIndex = int.MaxValue;
            System.Diagnostics.Debug.WriteLine($"Pathwidth {pathWidth} PathHeight {pathHeight} middleX {middleX} middleY {middleY}");
            double deltaT = 1.0 / (NumDivisions + 1);
            List<double> angles = new List<double>();
            for (int div = 0; div < NumDivisions; div++)
            {
                double t = div * deltaT;
                double targetDistance = t * pathLength;
                if (targetDistance < pathLength)
                {
                    double runningDistance = 0;
                    int cp = 1;
                    bool found = false;

                    while (!found && cp < pnts.Count)
                    {
                        PointF p0 = pnts[cp - 1];
                        PointF p1 = pnts[cp];
                        double d = Distance(p0, p1);
                        if ((runningDistance <= targetDistance) &&
                             (runningDistance + d >= targetDistance))
                        {
                            found = true;
                            double overhang = targetDistance - runningDistance;
                            if (overhang < 0)
                            {
                                System.Diagnostics.Debug.WriteLine("Distance error creating profile");
                            }
                            if (overhang != 0.0)
                            {
                                double delta = overhang / d;
                                double nx = p0.X + (p1.X - p0.X) * delta;
                                double ny = p0.Y + (p1.Y - p0.Y) * delta;

                                double rx = nx - middleX;
                                double ry = ny - middleY;
                                double rd = Math.Atan2(ry, rx);
                                angles.Add(rd);
                                if (rx > 0 && rd < minangle)
                                {
                                    minIndex = angles.Count - 1;
                                    minangle = rd;
                                }

                                nx = (nx - tlx) / pathWidth;
                                ny = (ny - tly) / pathHeight;
                                profilePoints.Add(new PointF((float)nx, (float)ny));
                                System.Diagnostics.Debug.WriteLine($"nx {nx} ny {ny}");
                            }
                            else
                            {
                                double nx = p0.X;
                                double ny = p0.Y;

                                double rx = nx - middleX;
                                double ry = ny - middleY;
                                double rd = Math.Atan2(ry, rx);
                                angles.Add(rd);
                                if (rx > 0 && rd < minangle)
                                {
                                    minIndex = angles.Count - 1;
                                    minangle = rd;
                                }

                                nx = (nx - tlx) / pathWidth;
                                ny = (ny - tly) / pathHeight;
                                profilePoints.Add(new PointF((float)nx, (float)ny));
                                System.Diagnostics.Debug.WriteLine($"nx {nx} ny {ny}");
                            }
                        }
                        runningDistance += d;
                        cp++;
                    }
                }
            }


            List<PointF> tmp = new List<PointF>();

            for (int j = minIndex; j < profilePoints.Count; j++)
            {
                tmp.Add(profilePoints[j]);
            }
            if (minIndex > 0 && minIndex<profilePoints.Count)
            {
                for (int j = 0; j < minIndex; j++)
                {
                    tmp.Add(profilePoints[j]);
                }
            }
            profilePoints = tmp;
            Dirty = false;
        }

        public string GenPath()
        {
            PathText = flexiPath.ToPath();
            return PathText;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetImageSource()
        {
            if (workingImage != null)
            {
                PathBackgroundImage.Source = loadBitmap(workingImage);
                FlexiPathCanvas.Width = workingImage.Width;
                FlexiPathCanvas.Height = workingImage.Height;
            }
        }

        public void UpdateDisplay()
        {
            FlexiPathCanvas.Children.Clear();
            FlexiPathCanvas.Children.Add(PathBackgroundImage);
            // this is the grid on the path edit
            if (showGrid)
            {
                if (gridMarkers == null)
                {
                    gridMarkers = new List<Shape>();
                    BaseModellerDialog.CreateCanvasGrid(FlexiPathCanvas, out gridX, out gridY, 10.0, gridMarkers);
                }
                foreach (Shape sh in gridMarkers)
                {
                    FlexiPathCanvas.Children.Add(sh);
                }
            }
            DisplayLines();
            DisplayPoints();
            if (showProfilePoints)
            {
                ShowProfile();
            }
            PathText = flexiPath.ToPath();
        }

        public void UpdateHeaderLabel()
        {
            Dirty = true;
            HeaderLabel.Content = Header;
            FNameLabel.Content = FName;
        }

        public void Write(XmlDocument doc, XmlElement parentNode)
        {
            XmlElement ele = doc.CreateElement("ImagePath");

            ele.SetAttribute("Path", imagePath);
            ele.SetAttribute("Scale", scale.ToString());
            ele.SetAttribute("ScrollX", ScrollX.ToString());
            ele.SetAttribute("ScrollY", ScrollY.ToString());
            parentNode.AppendChild(ele);
            XmlElement pnts = doc.CreateElement("Edge");
            // make sure we get the points with absolute coordinates
            pnts.InnerText = flexiPath.ToPath(true);
            ele.AppendChild(pnts);
        }

        internal ImagePathControl Clone(bool copyImage = true)
        {
            ImagePathControl cl = new ImagePathControl();
            cl.Header = Header;
            cl.NumDivisions = NumDivisions;

            cl.ImagePath = ImagePath;
            cl.flexiPath.FromTextPath(flexiPath.ToPath(true));
            cl.ProfilePoints = new List<PointF>();
            foreach (PointF fp in profilePoints)
            {
                PointF fn = new PointF(fp.X, fp.Y);
                cl.ProfilePoints.Add(fn);
            }
            if (copyImage)
            {
                cl.WorkingImage = new System.Drawing.Bitmap(workingImage);
                for (int px = 0; px < workingImage.Width; px++)
                {
                    for (int py = 0; py < workingImage.Height; py++)
                    {
                        cl.WorkingImage.SetPixel(px, py, workingImage.GetPixel(px, py));
                    }
                }
            }
            cl.src = src;
            cl.OnForceReload = OnForceReload;
            return cl;
        }

        internal void Read(XmlElement parentNode)
        {
            XmlElement ele = (XmlElement)parentNode.SelectSingleNode("ImagePath");
            ImagePath = ele.GetAttribute("Path");
            // LoadImage(imagePath,false);
            scale = Convert.ToDouble(ele.GetAttribute("Scale"));
            scrollX = Convert.ToDouble(ele.GetAttribute("ScrollX"));
            scrollY = Convert.ToDouble(ele.GetAttribute("ScrollY"));
            XmlNode edgeNode = (XmlElement)ele.SelectSingleNode("Edge");
            string p = edgeNode.InnerText;
            flexiPath.FromTextPath(p);
            loaded = false;
            ScrollView.ScrollToHorizontalOffset(scrollX);
            ScrollView.ScrollToVerticalOffset(scrollY);
            UpdateDisplay();
            loaded = true;
            NotifyPropertyChanged("Scale");
            Dirty = true;
        }

        internal void RenderFlexipath(ref Bitmap bmp, out double tlx, out double tly, out double brx, out double bry)
        {
            int sc = 2;
            tlx = double.MaxValue;
            tly = double.MaxValue;
            brx = double.MinValue;
            bry = double.MinValue;
            int offset = 4;
            List<PointF> pnts = flexiPath.DisplayPointsF();
            if (bmp == null && pnts.Count > 0 && workingImage != null)
            {
                bmp = new Bitmap(sc * workingImage.Width, sc * workingImage.Height);
            }
            if (bmp != null)
            {
                for (int i = 0; i < pnts.Count; i++)
                {
                    if (pnts[i].X < tlx)
                    {
                        tlx = (double)pnts[i].X;
                    }

                    if (pnts[i].Y < tly)
                    {
                        tly = (double)pnts[i].Y;
                    }

                    if (pnts[i].X > brx)
                    {
                        brx = (double)pnts[i].X;
                    }

                    if (pnts[i].Y > bry)
                    {
                        bry = (double)pnts[i].Y;
                    }
                }
                tlx *= sc;
                tly *= sc;
                brx *= sc;
                bry *= sc;
                using (var gfx = Graphics.FromImage(bmp))
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
                {
                    // draw one thousand random white lines on a dark blue background
                    gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    gfx.Clear(System.Drawing.Color.White);
                    for (int i = 0; i < pnts.Count; i++)
                    {
                        int j = i + 1;
                        if (j >= pnts.Count)
                        {
                            j = 0;
                        }
                        var pt1 = new System.Drawing.Point((int)(pnts[i].X * sc) - (int)tlx + offset, (int)(pnts[i].Y * sc) - (int)(tly) + offset);
                        var pt2 = new System.Drawing.Point((int)(pnts[j].X * sc) - (int)tlx + offset, (int)(pnts[j].Y * sc) - (int)(tly) + offset);
                        gfx.DrawLine(pen, pt1, pt2);
                    }
                }
            }
            brx = brx - tlx + offset;
            bry = bry - tly + offset;
            tlx = offset;
            tly = offset;
        }

                public double EdgeLength
                {
                    get
                    {
                        if (imageEdge != null)
                        {
                            return imageEdge.Length;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }

        public void FindEdge()
        {
            imageEdge.Clear();

            // left side
            double px = 0;
            double py = 0;
            while (py < workingImage.Height)
            {
                px = 0;
                bool found = false;
                while (px < workingImage.Width && !found)
                {
                    System.Drawing.Color c = workingImage.GetPixel((int)px, (int)py);
                    if (c.R == 0 && c.G == 0 && c.B == 0 && c.A == 255)
                    {
                        found = true;
                        imageEdge.Add(px, py); ;
                    }
                    else
                    {
                        px++;
                    }
                }
                py++;
            }

            // right side
            py = workingImage.Height - 1;
            while (py >= 0)
            {
                px = workingImage.Width - 1;
                bool found = false;
                while (px >= 0 && !found)
                {
                    System.Drawing.Color c = workingImage.GetPixel((int)px, (int)py);
                    if (c.R == 0 && c.G == 0 && c.B == 0 && c.A == 255)
                    {
                        found = true;
                        imageEdge.Add(px, py);
                    }
                    else
                    {
                        px--;
                    }
                }
                py--;
            }

            imageEdge.Analyse();

            ShowEdge();
        }

        public void GenerateProfilePoints(int modelType)
        {
            System.Diagnostics.Debug.WriteLine("=================================== GenerateProfilePoints ================================");
            System.Diagnostics.Debug.WriteLine($"{imagePath}");

            divisionLength = EdgeLength / (NumDivisions - 1);

            middleX = imageEdge.MiddleX;
            middleY = imageEdge.MiddleY;
            System.Diagnostics.Debug.WriteLine($"MiddleX {middleX} middleY {middleY} ");
            profilePoints = new List<PointF>();

            List<PointF> tmp = new List<PointF>();
            int startP = -1;
            int endP = -1;
            if (modelType == 0)
            {
                startP = imageEdge.WholeStart;
                endP = imageEdge.WholeEnd;
                divisionLength = imageEdge.Length / (NumDivisions - 1);
                System.Diagnostics.Debug.WriteLine($"shape {modelType} start {startP} end {endP} DivisionLength {divisionLength}");
            }
            if (modelType == 1)
            {
                startP = imageEdge.BackStart;
                endP = imageEdge.BackEnd;
                divisionLength = imageEdge.CalcLength(startP, endP) / (NumDivisions - 1);
                System.Diagnostics.Debug.WriteLine($"shape {modelType} start {startP} end {endP} DivisionLength {divisionLength}");
            }
            if (modelType == 2)
            {
                startP = imageEdge.FrontStart;
                endP = imageEdge.FrontEnd;
                divisionLength = imageEdge.CalcLength(startP, endP) / (NumDivisions - 1);
                System.Diagnostics.Debug.WriteLine($"shape {modelType} start {startP} end {endP} DivisionLength {divisionLength}");
            }

            for (int i = startP; i < endP; i++)
            {
                tmp.Add(new PointF((float)(imageEdge.EdgePoints[i].X - middleX), (float)(imageEdge.EdgePoints[i].Y - middleY)));
            }
            // for convenience add the first point onto the end again
            tmp.Add(new PointF(tmp[0].X, tmp[0].Y));

            for (int i = 0; i < NumDivisions; i++)
            {
                double targetDistance = i * divisionLength;
                double runningDistance = 0;
                int cp = 1;
                bool found = false;
                while (!found && cp < tmp.Count)
                {
                    PointF p0 = tmp[cp - 1];
                    PointF p1 = tmp[cp];
                    double d = Distance(p0, p1);
                    if ((runningDistance <= targetDistance) &&
                         (runningDistance + d >= targetDistance))
                    {
                        found = true;
                        double overhang = targetDistance - runningDistance;
                        if (overhang < 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Distance error creating profile");
                        }
                        if (overhang != 0.0)
                        {
                            double delta = overhang / d;
                            double nx = p0.X + (p1.X - p0.X) * delta;
                            double ny = p0.Y + (p1.Y - p0.Y) * delta;

                            nx = nx / middleX;
                            ny = ny / middleY;
                            profilePoints.Add(new PointF((float)nx, (float)ny));
                        }
                        else
                        {
                            double nx = p0.X;
                            double ny = p0.Y;

                            nx = nx / middleX;
                            ny = ny / middleY;
                            profilePoints.Add(new PointF((float)nx, (float)ny));
                        }
                    }
                    runningDistance += d;
                    cp++;
                }
            }
            if (profilePoints.Count != NumDivisions)
            {
                System.Diagnostics.Debug.WriteLine("ERROR incorrect number of profile divisions");
            }
        }

        private static void AdjustBounds(ref double tlx, ref double tly, ref double brx, ref double bry, double px, double py)
        {
            if (px < tlx)
            {
                tlx = px;
            }
            if (px > brx)
            {
                brx = px;
            }

            if (py < tly)
            {
                tly = py;
            }
            if (py > bry)
            {
                bry = py;
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private void AddAnotherPointToPoly(MouseButtonEventArgs e)
        {
            Dirty = true;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = SnapPositionToMM(position);

            if (Math.Abs(position.X - flexiPath.Start.X) < 2 && Math.Abs(position.Y - flexiPath.Start.Y) < 2)
            {
                selectedPoint = -1;
                selectionMode = SelectionModeType.SelectSegmentAtPoint;
                flexiPath.ClosePath();
            }
            else
            {
                flexiPath.AddLine(new System.Windows.Point(position.X, position.Y));

                selectionMode = SelectionModeType.AppendPoint;
                selectedPoint = polyPoints.Count - 1;
                moving = true;
            }
            UpdateDisplay();
        }

        private void AddBezierButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.ConvertToCubicBezier; ;
        }

        private bool AddLineFromPoint(System.Windows.Input.MouseButtonEventArgs e, Line ln)
        {
            Dirty = true;
            int found = -1;
            bool added = false;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            for (int i = 0; i < polyPoints.Count; i++)
            {
                if (Math.Abs(ToPixelX(polyPoints[i].X) - ln.X1) < 0.0001 && Math.Abs(ToPixelY(polyPoints[i].Y) - ln.Y1) < 0.0001)
                {
                    found = i;
                    break;
                }
            }
            if (found != -1)
            {
                if (found < polyPoints.Count - 1)
                {
                    InsertLineSegment(found, position);
                }
                else
                {
                    flexiPath.AddLine(position);
                }
                added = true;
                PathText = flexiPath.ToPath();
            }

            return added;
        }

        private void AddPointClicked(object sender, RoutedEventArgs e)
        {
        }

        private void AddQuadBezierButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.ConvertToQuadBezier;
        }

        private void AddSegButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.SplitLine;
        }

        private void AddStartPointToPoly(MouseButtonEventArgs e)
        {
            Dirty = true;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = SnapPositionToMM(position);
            flexiPath.Start = new FlexiPoint(new System.Windows.Point(position.X, position.Y), 0);
            selectionMode = SelectionModeType.AppendPoint;
            UpdateDisplay();
        }

        private void ClearPointSelections()
        {
            if (polyPoints != null)
            {
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    polyPoints[i].Selected = false;
                    polyPoints[i].Visible = false;
                }
            }
        }

        private void CNVSegButton_Click(object sender, RoutedEventArgs e)
        {
            Dirty = true;
            if (canCNVDouble)
            {
                flexiPath.ConvertTwoLineSegmentsToQuadraticBezier();

                UpdateDisplay();
                PathText = flexiPath.ToPath();
                CanCNVDouble = false;
            }
        }

        private bool ConvertLineAtPointToBezier(MouseButtonEventArgs e, Line ln, bool cubic)
        {
            Dirty = true;
            int found = -1;
            bool added = false;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position, true))
            {
                if (cubic)
                {
                    added = flexiPath.ConvertToCubic(position);
                }
                else
                {
                    added = flexiPath.ConvertToQuadQuadAtSelected(position);
                }
                if (added)
                {
                    UpdateDisplay();

                    PathText = flexiPath.ToPath();
                }
            }

            return added;
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            PathText = flexiPath.ToPath();
            Clipboard.SetText(PathText);
        }

        private void CopySrcToWorking()
        {
            System.Drawing.Color c;
            workingImage = new System.Drawing.Bitmap(src);
            for (int px = 0; px < src.Width; px++)
            {
                for (int py = 0; py < src.Height; py++)
                {
                    workingImage.SetPixel(px, py, System.Drawing.Color.White);
                    c = src.GetPixel(px, py);
                    if (c.R < 200 || c.G < 200 || c.B < 200)
                    {
                        if (px < tlx)
                        {
                            tlx = px;
                        }
                        if (py < tly)
                        {
                            tly = py;
                        }
                    }
                }
            }

            for (int px = (int)tlx; px < src.Width; px++)
            {
                for (int py = (int)tly; py < src.Height; py++)
                {
                    c = src.GetPixel(px, py);
                    // if (c.R < 200 || c.G < 200 || c.B < 200)
                    {
                        int tx = px - (int)tlx + 1;
                        int ty = py - (int)tly + 1;
                        if ((tx < workingImage.Width - 1) &&
                             (ty < workingImage.Height - 1))
                        {
                            //workingImage.SetPixel(tx, ty, System.Drawing.Color.Black);
                            workingImage.SetPixel(tx, ty, c);
                        }
                    }
                }
            }
        }

        private void DashLine(double x1, double y1, double x2, double y2)
        {
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 2;
            ln.StrokeDashArray = new DoubleCollection();
            ln.StrokeDashArray.Add(0.5);
            ln.StrokeDashArray.Add(0.5);
            ln.Fill = br;
            ln.X1 = x1;
            ln.Y1 = y1;
            ln.X2 = x2;
            ln.Y2 = y2;

            FlexiPathCanvas.Children.Add(ln);
        }

        private void DeletePointClicked(object sender, RoutedEventArgs e)
        {
        }

        private bool DeleteSegment(MouseButtonEventArgs e, Line ln)
        {
            Dirty = true;
            int found = -1;
            bool deleted = false;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);

            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position))
            {
                deleted = flexiPath.DeleteSelectedSegment();
                if (deleted)
                {
                    UpdateDisplay();
                    PathText = flexiPath.ToPath();
                    e.Handled = true;
                }
            }

            return deleted;
        }

        private void DelSegButton_Click(object sender, RoutedEventArgs e)
        {
            Dirty = true;
            if (flexiPath.DeleteSelectedSegment())
            {
                UpdateDisplay();
            }
            else
            {
                SelectionMode = SelectionModeType.DeleteSegment;
            }
        }

        private void DisplayLines()
        {
            if (flexiPath != null)
            {
                List<System.Windows.Point> points = flexiPath.DisplayPoints();
                if (points != null && points.Count > 1)
                {
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        DrawLine(i, i + 1, points);
                    }
                }
            }
        }

        private void DisplayPoints()
        {
            if (polyPoints != null)
            {
                double ox = double.NaN;
                double oy = double.NaN;
                if (selectedPoint != -1)
                {
                    ox = polyPoints[selectedPoint].X;
                    oy = polyPoints[selectedPoint].Y;
                }

                double rad = 3;
                System.Windows.Media.Brush br = null;
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    bool ortho = false;

                    System.Windows.Point p = polyPoints[i].ToPoint();
                    if (selectedPoint == i)
                    {
                        rad = 6;
                        br = System.Windows.Media.Brushes.LightGreen;
                    }
                    else
                    {
                        rad = 3;
                        br = System.Windows.Media.Brushes.Red;
                        if (ox != double.NaN)
                        {
                            if (Math.Abs(p.X - ox) < 0.1 || Math.Abs(p.Y - oy) < 0.1)
                            {
                                br = System.Windows.Media.Brushes.Blue;
                                ortho = true;
                            }
                        }
                    }

                    // only show the points if they are marked as visible OR they are orthogonal to
                    // the selected one
                    if (polyPoints[i].Visible || (ortho && showOrtho))
                    {
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Data)
                        {
                            p = MakeEllipse(rad, br, p);
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control1 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            p = MakeRect(rad, br, p);
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control2)
                        {
                            p = MakeTri(rad, br, p);
                        }

                        if (selectedPoint == i && showOrtho)
                        {
                            DashLine(ToPixelX(p.X), 0, ToPixelX(p.X), FlexiPathCanvas.ActualHeight - 1);
                            DashLine(0, ToPixelY(p.Y), FlexiPathCanvas.ActualWidth - 1, ToPixelY(p.Y));
                        }
                    }
                }
                // If we are appending points to the polygon then always draw the start point
                if ((selectionMode == SelectionModeType.StartPoint || selectionMode == SelectionModeType.AppendPoint) && polyPoints.Count > 0)
                {
                    br = System.Windows.Media.Brushes.Red;
                    var p = polyPoints[0].ToPoint();
                    MakeEllipse(6, br, p);
                }
                // now draw any control connectors
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    if (polyPoints[i].Visible)
                    {
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control1 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i - 1;
                            if (j < 0)
                            {
                                j = polyPoints.Count - 1;
                            }
                            DrawControlLine(polyPoints[i].ToPoint(), polyPoints[j].ToPoint());
                        }
                        if (polyPoints[i].Mode == FlexiPoint.PointMode.Control2 ||
                            polyPoints[i].Mode == FlexiPoint.PointMode.ControlQ)
                        {
                            int j = i + 1;
                            if (j == polyPoints.Count)
                            {
                                j = 0;
                            }
                            DrawControlLine(polyPoints[i].ToPoint(), polyPoints[j].ToPoint());
                        }
                    }
                }
            }
        }

        private double Distance(PointF point1, PointF point2)
        {
            double diff = ((point2.X - point1.X) * (point2.X - point1.X)) +
            ((point2.Y - point1.Y) * (point2.Y - point1.Y));

            return Math.Sqrt(diff);
        }

        private void DrawControlLine(System.Windows.Point p1, System.Windows.Point p2)
        {
            SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));
            Line ln = new Line();
            ln.Stroke = br;
            ln.StrokeThickness = 1;
            ln.StrokeDashArray = new DoubleCollection();
            ln.StrokeDashArray.Add(0.5);
            ln.StrokeDashArray.Add(0.5);
            ln.Fill = br;
            ln.X1 = ToPixelX(p1.X);
            ln.Y1 = ToPixelY(p1.Y);
            ln.X2 = ToPixelX(p2.X);
            ln.Y2 = ToPixelY(p2.Y);

            FlexiPathCanvas.Children.Add(ln);
        }

        private void DrawLine(int i, int v, List<System.Windows.Point> points)
        {
            if (v < points.Count)
            {
                SolidColorBrush br = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 255, 255, 5));
                Line ln = new Line();
                ln.Stroke = br;
                ln.StrokeThickness = 6;
                ln.Fill = br;
                ln.X1 = ToPixelX(points[i].X);
                ln.Y1 = ToPixelY(points[i].Y);
                ln.X2 = ToPixelX(points[v].X);
                ln.Y2 = ToPixelY(points[v].Y);
                ln.MouseLeftButtonDown += Ln_MouseLeftButtonDown;
                ln.MouseRightButtonDown += Ln_MouseRightButtonDown;
                FlexiPathCanvas.Children.Add(ln);
            }
        }

        private void FlexiPathCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectionMode == SelectionModeType.StartPoint)
            {
                AddStartPointToPoly(e);
                e.Handled = true;
            }
            else if (selectionMode == SelectionModeType.AppendPoint)
            {
                AddAnotherPointToPoly(e);
                e.Handled = true;
            }
            else
            {
                try
                {
                    if (selectedPoint >= 0)
                    {
                        Points[selectedPoint].Selected = false;
                    }
                    SelectedPoint = -1;

                    System.Windows.Point position = e.GetPosition(FlexiPathCanvas);

                    // do this test here because the othe modes only trigger ifn you click a line
                    if (selectionMode == SelectionModeType.MovePath)
                    {
                        Dirty = true;
                        position = SnapPositionToMM(position);
                        MoveWholePath(position);
                        SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        UpdateDisplay();
                        e.Handled = true;
                    }
                    else
                    {
                        // dont snap position when selecting points as they may have been positioned
                        // between two grid points . just convert position to mm
                        position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));

                        // find the closest point thats within search radius
                        double rad = 3;
                        double minDist = double.MaxValue;
                        for (int i = 0; i < Points.Count; i++)
                        {
                            System.Windows.Point p = Points[i].ToPoint();
                            double dist = Math.Sqrt(((position.X - p.X) * (position.X - p.X)) +
                                                    ((position.Y - p.Y) * (position.Y - p.Y)));
                            if (dist < minDist && dist < rad)
                            {
                                SelectedPoint = i;
                                minDist = dist;
                            }
                        }
                        if (SelectedPoint != -1)
                        {
                            Points[SelectedPoint].Selected = true;
                            moving = true;
                        }
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            e.Handled = true;
                            UpdateDisplay();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void FlexiPathCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);

            System.Windows.Point snappedPos = SnapPositionToMM(position);
            if (selectedPoint != -1)
            {
                if (e.LeftButton == MouseButtonState.Pressed && moving)
                {
                    Dirty = true;
                    polyPoints[selectedPoint].X = position.X;
                    polyPoints[selectedPoint].Y = position.Y;
                    flexiPath.SetPointPos(selectedPoint, snappedPos);
                    PathText = flexiPath.ToPath();
                    if (selectionMode != SelectionModeType.StartPoint && selectionMode != SelectionModeType.SelectSegmentAtPoint)
                    {
                        GenerateProfilePoints();
                    }
                    UpdateDisplay();
                }
                else
                {
                    moving = false;
                }
            }
        }

        private void FlexiPathCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moving = false;
            if (selectedPoint != -1 && moving)
            {
                Dirty = true;
                System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
                System.Windows.Point positionSnappedToMM = SnapPositionToMM(position);
                polyPoints[selectedPoint].X = position.X;
                polyPoints[selectedPoint].Y = position.Y;

                flexiPath.SetPointPos(selectedPoint, positionSnappedToMM);
                PathText = flexiPath.ToPath();
                GenerateProfilePoints();
                UpdateDisplay();
                selectedPoint = -1;
            }
            moving = false;
        }

        private void FlexiPathCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedPoint != -1)
            {
                bool shift = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
                double d = 1;
                if (shift)
                {
                    d = 0.1;
                }
                switch (e.Key)
                {
                    case Key.Left:
                    case Key.L:
                        {
                            polyPoints[selectedPoint].X -= d;
                        }
                        break;

                    case Key.Right:
                    case Key.R:
                        {
                            polyPoints[selectedPoint].X += d;
                        }
                        break;

                    case Key.U:
                    case Key.Up:
                        {
                            polyPoints[selectedPoint].Y -= d;
                        }
                        break;

                    case Key.D:
                    case Key.Down:
                        {
                            polyPoints[selectedPoint].Y += d;
                        }
                        break;
                }
                Dirty = true;
                UpdateDisplay();
            }
        }

        private void GeneratePointParams()
        {
            String s = flexiPath.ToString();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            opDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png) | *.jpg; *.jpeg; *.jpe; *.png";
            if (opDlg.ShowDialog() == true)
            {
                try
                {
                    LoadImage(opDlg.FileName);
                }
                catch
                {
                }
            }

            UpdateDisplay();
        }

        private void InButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 1.1;
            PathCanvasScale.ScaleX = scale;
            PathCanvasScale.ScaleY = scale;
        }

        private void InitialisePoints()
        {
            flexiPath.Clear();
            selectionMode = SelectionModeType.StartPoint;
        }

        private void InsertCurveSegment(int startIndex, System.Windows.Point position)
        {
            Dirty = true;
            flexiPath.ConvertLineCurveSegment(startIndex, position);
            PathText = flexiPath.ToPath();
        }

        private void InsertLineSegment(int startIndex, System.Windows.Point position)
        {
            Dirty = true;
            flexiPath.InsertLineSegment(startIndex, position);
            PathText = flexiPath.ToPath();
        }

        private void InsertQuadCurveSegment(int startIndex, System.Windows.Point position)
        {
            Dirty = true;
            flexiPath.ConvertLineQuadCurveSegment(startIndex, position);
            PathText = flexiPath.ToPath();
        }

        private void Ln_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line ln = sender as Line;
            bool found = false;
            if (ln != null)
            {
                switch (selectionMode)
                {
                    case SelectionModeType.SelectSegmentAtPoint:
                        {
                            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                            found = SelectLineFromPoint(e, shiftDown);
                        }
                        break;

                    case SelectionModeType.SplitLine:
                        {
                            found = SplitLineAtPoint(e, ln);
                            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case SelectionModeType.ConvertToCubicBezier:
                        {
                            found = ConvertLineAtPointToBezier(e, ln, true);
                            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case SelectionModeType.ConvertToQuadBezier:
                        {
                            found = ConvertLineAtPointToBezier(e, ln, false);
                            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;

                    case SelectionModeType.DeleteSegment:
                        {
                            found = DeleteSegment(e, ln);
                            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
                        }
                        break;
                }
                if (found)
                {
                    e.Handled = true;
                    UpdateDisplay();
                }
                canCNVDouble = flexiPath.HasTwoConsecutiveLineSegmentsSelected();
            }
        }

        private void Ln_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        // Load a bitmap without locking it.
        private Bitmap LoadBitmapUnlocked(string file_name)
        {
            using (Bitmap bm = new Bitmap(file_name))
            {
                return new Bitmap(bm);
            }
        }

        private void LoadImage(string f)
        {
            Uri fileUri = new Uri(f);
            localImage = new BitmapImage();
            localImage.BeginInit();
            localImage.UriSource = fileUri;
            // localImage.DecodePixelWidth = 800;
            localImage.EndInit();
            //EditorParameters.Set("ImagePath", f);
            FlexiPathCanvas.Width = localImage.Width;
            FlexiPathCanvas.Height = localImage.Height;
            UpdateDisplay();
        }

        private void LoadImage(string imagePath, bool force)
        {
            if (File.Exists(imagePath))
            {
                if (src == null || force)
                {
                    //src = new System.Drawing.Bitmap(imagePath);
                    src = LoadBitmapUnlocked(imagePath);
                    isValid = true;
                }
                ShowWorkingImage();
            }
            else
            {
                MessageBox.Show($"Cant find image {imagePath}");
            }
        }

        private System.Windows.Point MakeEllipse(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            Ellipse el = new Ellipse();

            Canvas.SetLeft(el, ToPixelX(p.X) - rad);
            Canvas.SetTop(el, ToPixelY(p.Y) - rad);
            el.Width = 2 * rad;
            el.Height = 2 * rad;
            el.Stroke = br;
            el.Fill = br;
            el.MouseDown += FlexiPathCanvas_MouseDown;
            el.MouseMove += FlexiPathCanvas_MouseMove;
            el.MouseUp += FlexiPathCanvas_MouseUp;
            el.ContextMenu = PointMenu(el);
            FlexiPathCanvas.Children.Add(el);
            return p;
        }

        private System.Windows.Point MakeRect(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            System.Windows.Shapes.Rectangle el = new System.Windows.Shapes.Rectangle();
            if (p.X >= 0 && p.X < double.MaxValue)
            {
                Canvas.SetLeft(el, ToPixelX(p.X) - rad);
                Canvas.SetTop(el, ToPixelY(p.Y) - rad);
                el.Width = 2 * rad;
                el.Height = 2 * rad;
                el.Stroke = br;
                el.Fill = br;
                el.MouseDown += FlexiPathCanvas_MouseDown;
                el.MouseMove += FlexiPathCanvas_MouseMove;
                el.MouseUp += FlexiPathCanvas_MouseUp;
                el.ContextMenu = PointMenu(el);
                FlexiPathCanvas.Children.Add(el);
            }
            return p;
        }

        private System.Windows.Point MakeTri(double rad, System.Windows.Media.Brush br, System.Windows.Point p)
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new System.Windows.Point(0.5, 0));
            myPointCollection.Add(new System.Windows.Point(0, 1));
            myPointCollection.Add(new System.Windows.Point(1, 1));

            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = br;

            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            myPolygon.StrokeThickness = 2;
            Canvas.SetLeft(myPolygon, ToPixelX(p.X) - rad);
            Canvas.SetTop(myPolygon, ToPixelY(p.Y) - rad);
            myPolygon.Width = 2 * rad;
            myPolygon.Height = 2 * rad;
            myPolygon.Stroke = br;
            myPolygon.Fill = br;
            myPolygon.MouseDown += FlexiPathCanvas_MouseDown;
            myPolygon.MouseMove += FlexiPathCanvas_MouseMove;
            myPolygon.MouseUp += FlexiPathCanvas_MouseUp;
            myPolygon.ContextMenu = PointMenu(myPolygon);
            FlexiPathCanvas.Children.Add(myPolygon);
            return p;
        }

        private void Mark(int x, int y, System.Drawing.Color c)
        {
            int size = 1;
            for (int i = -size; i <= size; i++)
            {
                for (int j = -size; j <= size; j++)
                {
                    if (x + i > 0 && x + i < workingImage.Width && y + j > 0 && y + j < workingImage.Height)
                    {
                        workingImage.SetPixel(x + i, y + j, c);
                    }
                }
            }
        }

        private void MovePathButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.MovePath;
        }

        private void MoveWholePath(System.Windows.Point position)
        {
            Dirty = true;
            flexiPath.MoveTo(position);
            GenerateProfilePoints();
        }

        private void OnAddQuadBezier(object obj)

        {
            SelectionMode = SelectionModeType.ConvertToQuadBezier;
        }

        private void OnCNVDoublePath(object obj)
        {
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.9;
            PathCanvasScale.ScaleX = scale;
            PathCanvasScale.ScaleY = scale;
        }

        private void PastePath_Click(object sender, RoutedEventArgs e)
        {
            string pth = Clipboard.GetText();
            if (pth != null && pth != "" && pth.StartsWith("M"))
            {
                Dirty = true;
                PathText = pth;
                flexiPath.FromTextPath(pth);
                UpdateDisplay();
                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
        }

        private System.Windows.Point Perpendicular(System.Windows.Point p1, System.Windows.Point p2, double t, double distanceFromLine)
        {
            double x;
            double y;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            if (Math.Abs(p1.X - p2.X) < 0.00001)
            {
                if (dy > 0)
                {
                    x = p1.X - distanceFromLine;
                }
                else
                {
                    x = p1.X + distanceFromLine;
                }
                y = p1.Y + t * (p2.Y - p1.Y);
            }
            else if (Math.Abs(p1.Y - p2.Y) < 0.00001)
            {
                x = p1.X + t * (p2.X - p1.X);
                if (dx > 0)
                {
                    y = p1.Y + distanceFromLine;
                }
                else
                {
                    y = p1.Y - distanceFromLine;
                }
            }
            else
            {
                double grad = dy / dx;
                double perp = -1.0 / grad;

                double sgn = Math.Sign(distanceFromLine);
                distanceFromLine = Math.Abs(distanceFromLine);
                System.Windows.Point tp = new System.Windows.Point(p1.X + t * dx, p1.Y + t * dy);
                x = tp.X + sgn * Math.Sqrt((distanceFromLine * distanceFromLine) / (1.0 + (1.0 / (grad * grad))));
                y = tp.Y + perp * (x - tp.X);
            }
            System.Windows.Point res = new System.Windows.Point(x, y);
            return res;
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionMode = SelectionModeType.SelectSegmentAtPoint;
        }

        private ContextMenu PointMenu(object tag)
        {
            ContextMenu mn = new ContextMenu();
            MenuItem mni = new MenuItem();
            mni.Header = "Delete Point";
            mni.Click += DeletePointClicked;
            mni.Tag = tag;
            mn.Items.Add(mni);
            return mn;
        }

        private void ResetPathButton_Click(object sender, RoutedEventArgs e)
        {
            InitialisePoints();

            selectedPoint = -1;
            UpdateDisplay();
            PathText = flexiPath.ToPath();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (loaded)
            {
                scrollX = e.HorizontalOffset;
                scrollY = e.VerticalOffset;
            }
        }

        private bool SelectLineFromPoint(MouseButtonEventArgs e, bool shift)
        {
            bool found;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            found = flexiPath.SelectAtPoint(position, !shift);

            if (found)
            {
                UpdateDisplay();
            }
            CanCNVDouble = flexiPath.HasTwoConsecutiveLineSegmentsSelected();
            return found;
        }

        private void SetButtonBorderColours()
        {
            switch (selectionMode)
            {
                case SelectionModeType.SelectSegmentAtPoint:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.SplitLine:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.ConvertToCubicBezier:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.ConvertToQuadBezier:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.DeleteSegment:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.MovePath:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                    }
                    break;
            }
        }

        private void SetFlexiPathScale()
        {
            PathCanvasScale.ScaleX = scale;
            PathCanvasScale.ScaleY = scale;
        }

        private void SetPointIds()
        {
            for (int i = 0; i < polyPoints.Count; i++)
            {
                polyPoints[i].Id = i + 1;
            }
        }

        private void ShowAllPointsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (FlexiPoint p in polyPoints)
            {
                p.Visible = true;
            }
            UpdateDisplay();
        }

        private void ShowCenters()
        {
            int topi = imageEdge.BackStart;
            PointF pt = imageEdge.EdgePoints[topi];

            int bottomi = imageEdge.BackEnd;
            PointF pb = imageEdge.EdgePoints[bottomi];
            double y;
            double my = pt.Y + (pb.Y - pt.Y) / 2;
            for (y = pt.Y; y <= my; y++)
            {
                Mark((int)pt.X, (int)y, System.Drawing.Color.DarkMagenta);
            }
            for (y = my; y <= pb.Y; y++)
            {
                Mark((int)pb.X, (int)y, System.Drawing.Color.Teal);
            }
        }

        private void ShowCenters_Click(object sender, RoutedEventArgs e)
        {
            if (src != null)
            {
                ShowWorkingImage();
            }
        }

        private void ShowEdge()
        {
            if (imageEdge.EdgePoints != null)
            {
                foreach (PointF pnt in imageEdge.EdgePoints)
                {
                    Mark((int)pnt.X, (int)pnt.Y, System.Drawing.Color.Blue);
                }
            }
        }

        private void ShowProfile()
        {
            if (profilePoints == null || profilePoints.Count == 0)
            {
                GenerateProfilePoints();
            }

            System.Windows.Media.Brush br = System.Windows.Media.Brushes.Blue;
            foreach (PointF pnt in profilePoints)
            {
                System.Windows.Point p = new System.Windows.Point((int)(pnt.X * pathWidth) + tlx, (int)(pnt.Y * pathHeight) + tly);
                MakeRect(3, br, p);
                br = System.Windows.Media.Brushes.Red;
            }
        }

        private void ShowProfile_Click(object sender, RoutedEventArgs e)
        {
            if (src != null)
            {
                ShowWorkingImage();
            }
        }

        private void ShowWorkingImage()
        {
            CopySrcToWorking();

            SetImageSource();
        }

        private System.Windows.Point SnapPositionToMM(System.Windows.Point pos)
        {
            double gx = pos.X;
            double gy = pos.Y;
            if (snap)
            {
                gx = pos.X / gridX;
                gx = Math.Round(gx) * gridX;
                gy = pos.Y / gridY;
                gy = Math.Round(gy) * gridY;
            }
            return new System.Windows.Point(ToMMX(gx), ToMMY(gy));
        }

        private bool SplitLineAtPoint(MouseButtonEventArgs e, Line ln)
        {
            bool added = false;
            System.Windows.Point position = e.GetPosition(FlexiPathCanvas);
            position = new System.Windows.Point(ToMMX(position.X), ToMMY(position.Y));
            if (flexiPath.SelectAtPoint(position, true))
            {
                if (flexiPath.SplitSelectedLineSegment(position))
                {
                    UpdateDisplay();
                    added = true;
                    PathText = flexiPath.ToPath();
                }
            }
            return added;
        }

        private double ToMM(double x)
        {
            double res = x;
            if (localImage != null)
            {
                res = 25.4 * x / localImage.DpiX;
            }
            return res;
        }

        private double ToMMX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(FlexiPathCanvas);
            double res = 25.4 * x / sc.PixelsPerInchX;
            return res;
        }

        private double ToMMY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(FlexiPathCanvas);
            double res = 25.4 * y / sc.PixelsPerInchY;
            return res;
        }

        private double ToPixelX(double x)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(FlexiPathCanvas);
            double res = sc.PixelsPerInchX * x / 25.4;
            return res;
        }

        private double ToPixelY(double y)
        {
            DpiScale sc = VisualTreeHelper.GetDpi(FlexiPathCanvas);
            double res = sc.PixelsPerInchY * y / 25.4;
            return res;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            showOrtho = true;
            ScrollView.ScrollToHorizontalOffset(scrollX);
            ScrollView.ScrollToVerticalOffset(scrollY);
            selectionMode = SelectionModeType.SelectSegmentAtPoint;
            UpdateDisplay();
            loaded = true;
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            scale = 1.1 * scale;
            SetFlexiPathScale();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            scale = 0.9 * scale;
            SetFlexiPathScale();
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            scale = 1.0;
            SetFlexiPathScale();
        }
        */
    }
}