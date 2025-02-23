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

        public bool Dirty
        {
            get; set;
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
            get
            {
                return isValid;
            }
            set
            {
                isValid = value;
            }
        }

        public int NumDivisions
        {
            get; set;
        }

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

                    if (polyPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < polyPoints.Count)
                        {
                            polyPoints[selectedPoint].Selected = true;
                            polyPoints[selectedPoint].Visible = true;
                        }
                    }
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

            selectedPoint = -1;

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
            }
        }

        public void UpdateHeaderLabel()
        {
            Dirty = true;
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

            localImage.EndInit();
        }

        private void ShowWorkingImage()
        {
            CopySrcToWorking();

            SetImageSource();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FlexiControl.AbsolutePaths = true;
            loaded = true;
        }
    }
}