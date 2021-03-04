using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for RibControl.xaml
    /// </summary>
    public partial class RibControl : UserControl
    {
        private int brx = int.MinValue;

        private int bry = int.MinValue;

        private double divisionLength;

        private List<PointF> edgePoints;

        private string imagePath;

        private double mx;
        private double my;
        private List<PointF> profilePoints;

        private double scale;

        private System.Drawing.Bitmap src;

        private int tlx = int.MaxValue;

        private int tly = int.MaxValue;

        private System.Drawing.Bitmap workingImage;

        public RibControl()
        {
            InitializeComponent();
            Width = 400;
            Height = 400;
            Header = "";
            edgePoints = null;
            NumDivisions = 120;
            ProfilePoints = new List<PointF>();
            scale = 1;
            SetRibScale();
        }

        public double EdgeLength { get; set; }

        public string Header { get; set; }

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
                }
            }
        }

        public int NumDivisions { get; set; }

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

        public void ClearSinglePixels()
        {
            for (int px = 1; px < workingImage.Width - 1; px++)
            {
                for (int py = 1; py < workingImage.Height - 1; py++)
                {
                    // find a black pixel
                    System.Drawing.Color c = workingImage.GetPixel(px, py);
                    if (c.R == 0)
                    {
                        // are any of its neighbours black too
                        bool doit = true;
                        for (int i = -1; i < 2 && doit; i++)
                        {
                            for (int j = -1; j < 2 && doit; j++)
                            {
                                if (i != 0 || j != 0)
                                {
                                    int nx = px + i;
                                    int ny = py + j;
                                    if (nx >= 0 && nx < workingImage.Width && ny >= 0 && ny < workingImage.Height)
                                    {
                                        System.Drawing.Color f = workingImage.GetPixel(nx, ny);
                                        if (f.R == 0)
                                        {
                                            doit = false;
                                        }
                                    }
                                }
                            }
                        }
                        // if no neighbours were black then just remove this one
                        if (doit)
                        {
                            workingImage.SetPixel(px, py, System.Drawing.Color.White);
                        }
                    }
                }
            }
        }

        public void FetchImage()
        {
            LoadImage(imagePath);
        }

        public void FindEdge()
        {
            double tlx = double.MaxValue;
            double tly = double.MaxValue;
            double brx = double.MinValue;
            double bry = double.MinValue;
            edgePoints = new List<PointF>();
            EdgeLength = 0;

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
                        HandlePoint(ref tlx, ref tly, ref brx, ref bry, px, py, System.Drawing.Color.Blue);
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
                        AdjustBounds(ref tlx, ref tly, ref brx, ref bry, px, py);
                        HandlePoint(ref tlx, ref tly, ref brx, ref bry, px, py, System.Drawing.Color.Red);
                    }
                    else
                    {
                        px--;
                    }
                }
                py--;
            }

            divisionLength = EdgeLength / NumDivisions;

            mx = tlx + (brx - tlx) / 2;
            my = tly + (bry - tly) / 2;
            profilePoints = new List<PointF>();
            List<PointF> tmp = new List<PointF>();
            for (int i = 0; i < edgePoints.Count; i++)
            {
                tmp.Add(new PointF((float)(edgePoints[i].X - mx), (float)(edgePoints[i].Y - my)));
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
                        }
                        if (overhang != 0.0)
                        {
                            double delta = overhang / d;
                            double nx = p0.X + (p1.X - p0.X) * delta;
                            double ny = p0.Y + (p1.Y - p0.Y) * delta;
                            //   Mark((int)(nx + mx), (int)(ny + my), System.Windows.Media.Colors.LightGreen);
                            nx = nx / mx;
                            ny = ny / my;
                            profilePoints.Add(new PointF((float)nx, (float)ny));
                        }
                        else
                        {
                            double nx = p0.X;
                            double ny = p0.Y;
                            //   Mark((int)(nx + mx), (int)(ny + my), System.Windows.Media.Colors.LightGreen);
                            nx = nx / mx;
                            ny = ny / my;
                            profilePoints.Add(new PointF((float)nx, (float)ny));
                        }
                    }
                    runningDistance += d;
                    cp++;
                }
            }

            ShowEdge();
        }

        public void SetImageSource()
        {
            if (workingImage != null)
            {
                RibImage.Source = loadBitmap(workingImage);
            }
        }

        public void UpdateHeaderLabel()
        {
            HeaderLabel.Content = Header;
        }

        internal RibControl Clone()
        {
            RibControl cl = new RibControl();
            cl.Header = Header;
            cl.NumDivisions = NumDivisions;
            cl.EdgeLength = EdgeLength;
            cl.ImagePath = ImagePath;
            cl.ProfilePoints = new List<PointF>();
            foreach (PointF fp in profilePoints)
            {
                PointF fn = new PointF(fp.X, fp.Y);
                cl.ProfilePoints.Add(fn);
            }

            cl.WorkingImage = new System.Drawing.Bitmap(workingImage);
            for (int px = 0; px < workingImage.Width; px++)
            {
                for (int py = 0; py < workingImage.Height; py++)
                {
                    cl.WorkingImage.SetPixel(px, py, workingImage.GetPixel(px, py));
                }
            }

            return cl;
        }

        internal void Write(XmlDocument doc, XmlElement docNode, int pos, string name)
        {
            XmlElement ele = doc.CreateElement("Rib");
            ele.SetAttribute("Header", Header);
            ele.SetAttribute("Path", imagePath);
            ele.SetAttribute("Position", pos.ToString());
            docNode.AppendChild(ele);
            XmlElement pnts = doc.CreateElement("Edge");
            pnts.SetAttribute("EdgeLength", EdgeLength.ToString());

            foreach (PointF p in profilePoints)
            {
                XmlElement v = doc.CreateElement("V");
                v.SetAttribute("X", p.X.ToString());
                v.SetAttribute("Y", p.Y.ToString());
                pnts.AppendChild(v);
            }
            ele.AppendChild(pnts);
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

        private void CopySrcToWorking()
        {
            Color c;
            workingImage = new System.Drawing.Bitmap(src);
            for (int px = 0; px < src.Width; px++)
            {
                for (int py = 0; py < src.Height; py++)
                {
                    workingImage.SetPixel(px, py, System.Drawing.Color.White);
                    c = src.GetPixel(px, py);
                    if (c.R < 128 || c.G < 128 || c.B < 128)
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
            /*
            // allow a little bit of extra space
            if (tlx > 0)
            {
                tlx--;
            }
            if (tly > 0)
            {
                tly++;
            }
            */
            for (int px = tlx; px < src.Width; px++)
            {
                for (int py = tly; py < src.Height; py++)
                {
                    c = src.GetPixel(px, py);
                    if (c.R < 128 || c.G < 128 || c.B < 128)
                    {
                        workingImage.SetPixel(px - tlx, py - tly, System.Drawing.Color.Black);
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

        private bool DuplicateEdgePoint(double px, double py)
        {
            bool result = false;
            foreach (PointF p in edgePoints)
            {
                if (p.X == px && p.Y == py)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private void HandlePoint(ref double tlx, ref double tly, ref double brx, ref double bry, double px, double py, System.Drawing.Color c)
        {
            AdjustBounds(ref tlx, ref tly, ref brx, ref bry, px, py);

            if (edgePoints.Count > 0)
            {
                if (!DuplicateEdgePoint(px, py))
                {
                    PointF p = new PointF((float)px, (float)py);
                    edgePoints.Add(p);
                    //                    Mark((int)px, (int)py, c);
                    EdgeLength += Distance(edgePoints[edgePoints.Count - 2],
                    edgePoints[edgePoints.Count - 1]);
                }
            }
            else
            {
                PointF p = new PointF((float)px, (float)py);
                edgePoints.Add(p);
                //                Mark((int)px, (int)py, c);
            }
        }

        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                System.Drawing.Color c;
                if (src == null)
                {
                    src = new System.Drawing.Bitmap(imagePath);
                }
                ShowWorkingImage();
            }
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

        private void RibCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
        }

        private void RibCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void SetRibScale()
        {
            RibScale.ScaleX = scale;
            RibScale.ScaleY = scale;
        }

        private void ShowEdge()
        {
            if (edgePoints != null)
            {
                foreach (PointF pnt in edgePoints)
                {
                    Mark((int)pnt.X, (int)pnt.Y, Color.Blue);
                }
            }
        }

        private void ShowProfile()
        {
            if (profilePoints != null)
            {
                foreach (PointF pnt in profilePoints)
                {
                    Mark((int)((1 + pnt.X) * mx), (int)((1 + pnt.Y) * my), Color.Red);
                }
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
            if (ShowProfileBut.IsChecked == false)
            {
                ShowEdge();
            }
            else
            {
                ShowProfile();
            }

            SetImageSource();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HeaderLabel.Content = Header;
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            scale = 1.1 * scale;
            SetRibScale();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            scale = 0.9 * scale;
            SetRibScale();
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            scale = 1.0;
            SetRibScale();
        }
    }
}