using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageMarker.xaml
    /// </summary>
    public partial class ImageMarker : UserControl, INotifyPropertyChanged
    {
        public List<LetterMarker> markers;

        public CopyLetter OnCopyLetter;

        public MarkerMoved OnMarkerMoved;

        public PinMoved OnPinMoved;

        private int brx = int.MinValue;

        private int bry = int.MinValue;

        private List<Dimension> dimensions;

        // private WriteableBitmap wbx;
        private List<UIElement> elements;

        private int gapBelowLetter = 5;

        private string headerText;

        private Image im;

        private string imageFilePath;

        private bool isValid;

        private int letterMargin = 20;

        private Point lowerPoint;

        private int markerTop;

        private bool notifyMoved;
        private int pinMargin = 20;

        private int pinPos;

        private bool pinSelected;

        private double scale;

        private LetterMarker selectedMarker = null;

        private int tlx = int.MaxValue;

        private int tly = int.MaxValue;

        // room to display a pin above and below
        private int totalTopMargin;

        private Point upperPoint;

        private System.Drawing.Bitmap workingImage;

        public ImageMarker()
        {
            InitializeComponent();
            DataContext = this;
            elements = new List<UIElement>();

            totalTopMargin = pinMargin + letterMargin + gapBelowLetter;
            markerTop = pinMargin + (letterMargin / 2) + 1;
            selectedMarker = null;
            pinSelected = false;
            dimensions = new List<Dimension>();
            isValid = false;
            notifyMoved = false;
        }

        public delegate void CopyLetter(string name);

        public delegate void MarkerMoved(string s, int x);

        public delegate void PinMoved(int x);

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Dimension> Dimensions
        {
            get { return dimensions; }
        }

        public string HeaderText
        {
            get
            {
                return headerText;
            }
            set
            {
                if (headerText != value)
                {
                    headerText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ImageFilePath
        {
            get
            {
                return imageFilePath;
            }
            set
            {
                if (imageFilePath != value)
                {
                    imageFilePath = value;
                    if (imageFilePath != "")
                    {
                        LoadImage();
                    }
                }
            }
        }

        public bool IsValid
        {
            get { return isValid; }
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

        public int PinPos
        {
            get
            {
                return pinPos;
            }
            set
            {
                if (pinPos != value)
                {
                    pinPos = value;
                    UpdateDisplay();
                }
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

        public void UpdateDisplay()
        {
            elements.Clear();
            if (imageFilePath != null && imageFilePath != "")
            {
                Dimension pinDim = GetUpperAndLowerPoints(pinPos);
                upperPoint = new Point(pinDim.P1.X, pinDim.P1.Y);
                lowerPoint = new Point(pinDim.P2.X, pinDim.P2.Y);
                double size = lowerPoint.Y - upperPoint.Y;
                AddTopPin(pinPos, size.ToString());
                AddCenterMarker();
                if (upperPoint.X != -1)
                {
                    AddCircle((int)upperPoint.X, (int)upperPoint.Y, null);
                }
                if (lowerPoint.X != -1)
                {
                    AddCircle((int)lowerPoint.X, (int)lowerPoint.Y, null);
                }
                Dimensions.Clear();
                foreach (LetterMarker mk in markers)
                {
                    AddLetter(mk.Position, markerTop, mk.Letter, mk);
                    Dimension dp = GetUpperAndLowerPoints(mk.Position);
                    dimensions.Add(dp);
                    AddCircle((int)dp.P1.X, (int)dp.P1.Y, mk);
                    AddCircle((int)dp.P2.X, (int)dp.P2.Y, mk);
                }
            }
            UpdateCanvas();
        }

        internal void AddRib(string name)
        {
            UpdateDisplay();
        }

        internal void DeleteMarker(RibControl rc)
        {
            UpdateDisplay();
        }

        internal double GetXmm(double position)
        {
            double dpi = workingImage.HorizontalResolution;
            double posInches = position / dpi;

            return InchesToMM(posInches);
        }

        internal double GetYmm(double position)
        {
            double dpi = workingImage.VerticalResolution;
            double posInches = position / dpi;

            return InchesToMM(posInches);
        }

        internal void SetHeader(string v)
        {
            HeaderText = v;
        }

        internal void SetMarker(string s, int x)
        {
            foreach (LetterMarker mk in markers)
            {
                if (mk.Letter == s)
                {
                    mk.Position = x;
                    UpdateDisplay();
                }
            }
        }

        internal void SetScale(double zoomLevel)
        {
            scale = zoomLevel;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private void AddCenterMarker()
        {
            if (brx > tlx)
            {
                int cx = (tlx + (brx - tlx) / 2);
                int cy = (tly + (bry - tly) / 2) + totalTopMargin;
                AddLine(cx, cy - 5, cx, cy + 5);
                AddLine(cx - 5, cy, cx + 5, cy);
            }
        }

        private void AddCircle(int x, int y, LetterMarker mk)
        {
            Ellipse el = new Ellipse();
            Canvas.SetLeft(el, x - 4);
            Canvas.SetTop(el, y - 4 + totalTopMargin);
            el.Width = 8;
            el.Height = 8;
            el.Stroke = System.Windows.Media.Brushes.Black;
            el.Fill = System.Windows.Media.Brushes.Red;
            el.Tag = mk;
            elements.Add(el);
            el.MouseDown += Br_MouseDown;
            el.MouseUp += Br_MouseUp;
            el.MouseMove += Br_MouseMove;
        }

        private void AddLetter(int x, int y, string c, object tag)
        {
            int by = bry + (2 * totalTopMargin);
            AddLine(x, y, x, y + by);
            int dx = 10;
            if (c.Length > 1)
            {
                dx *= c.Length;
            }
            Border br = new Border();
            Canvas.SetLeft(br, x - dx);
            Canvas.SetTop(br, y - 10);
            br.Width = 2 * dx;
            br.Height = 20;
            br.BorderBrush = System.Windows.Media.Brushes.Black;
            br.Background = System.Windows.Media.Brushes.Yellow;
            br.BorderThickness = new Thickness(2.0);
            br.CornerRadius = new CornerRadius(2);
            br.MouseDown += Br_MouseDown;
            br.MouseUp += Br_MouseUp;
            br.MouseMove += Br_MouseMove;
            br.Tag = tag;
            br.ContextMenu = this.FindResource("cmLetter") as ContextMenu;
            elements.Add(br);
            AddText(x - (dx / 2), y - 8, c, System.Windows.Media.Colors.Black, tag);

            br = new Border();
            Canvas.SetLeft(br, x - dx);
            Canvas.SetTop(br, y - 10 + by);
            br.Width = 2 * dx;
            br.Height = 20;
            br.BorderBrush = System.Windows.Media.Brushes.Black;
            br.Background = System.Windows.Media.Brushes.Yellow;
            br.BorderThickness = new Thickness(2.0);
            br.CornerRadius = new CornerRadius(2);
            br.MouseDown += Br_MouseDown;
            br.MouseUp += Br_MouseUp;
            br.MouseMove += Br_MouseMove;
            br.Tag = tag;
            br.ContextMenu = this.FindResource("cmLetter") as ContextMenu;

            elements.Add(br);
            AddText(x - (dx / 2), y - 8 + by, c, System.Windows.Media.Colors.Black, tag);
        }

        private void AddLine(int v1, int v2, int v3, int v4)
        {
            Line l = new Line();
            l.Stroke = System.Windows.Media.Brushes.Black;
            l.X1 = v1;
            l.Y1 = v2;
            l.X2 = v3;
            l.Y2 = v4;
            elements.Add(l);
        }

        private void AddText(int x, int y, string text, System.Windows.Media.Color color, object tag)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Background = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            textBlock.MouseDown += Br_MouseDown;
            textBlock.MouseUp += Br_MouseUp;
            textBlock.MouseMove += Br_MouseMove;
            textBlock.Tag = tag;
            elements.Add(textBlock);
            textBlock.ContextMenu = this.FindResource("cmLetter") as ContextMenu;
        }

        private void AddTopPin(int x, string s)
        {
            Polygon ply = new Polygon();
            ply.Stroke = System.Windows.Media.Brushes.Black;
            ply.Fill = System.Windows.Media.Brushes.Green;
            ply.Points.Add(new System.Windows.Point(x - 5, 0));
            ply.Points.Add(new System.Windows.Point(x + 5, 0));
            ply.Points.Add(new System.Windows.Point(x, pinMargin / 2));
            ply.MouseDown += Ply_MouseDown;
            ply.MouseUp += Ply_MouseUp;
            ply.MouseMove += Ply_MouseMove;
            ply.ToolTip = s;
            elements.Add(ply);
        }

        private void Br_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pinSelected = false;
            if (selectedMarker == null)
            {
                if (sender is Border)
                {
                    selectedMarker = (sender as Border).Tag as LetterMarker;
                }
                else
                if (sender is TextBlock)
                {
                    selectedMarker = (sender as TextBlock).Tag as LetterMarker;
                }
                else
                if (sender is Ellipse)
                {
                    selectedMarker = (sender as Ellipse).Tag as LetterMarker;
                }
            }
        }

        private void Br_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedMarker != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(mainCanvas);
                if (p.X >= tlx && p.X < brx)
                {
                    notifyMoved = true;
                    selectedMarker.Position = (int)p.X;
                    UpdateDisplay();
                }
            }
        }

        private void Br_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (OnMarkerMoved != null && notifyMoved)
                {
                    OnMarkerMoved(selectedMarker.Letter, selectedMarker.Position);
                    notifyMoved = false;
                }
                selectedMarker = null;
            }
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mn = sender as MenuItem;
            if (selectedMarker != null)
            {
                if (OnCopyLetter != null)
                {
                    OnCopyLetter(selectedMarker.Letter);
                    selectedMarker = null;
                }
            }
        }

        private Dimension GetUpperAndLowerPoints(int x)
        {
            Dimension res = null;
            Point up = new Point(0, 0);
            Point down = new Point(0, 0);
            int y = 0;
            bool found = false;

            y = 0;
            while (y < workingImage.Height && !found)
            {
                System.Drawing.Color c = workingImage.GetPixel(x, y);
                if (c.R != 255)
                {
                    //}
                    // if (c == System.Drawing.Color.Black)
                    // {
                    found = true;
                    up = new Point(x, y);
                    break;
                }
                y++;
            }

            if (found)
            {
                found = false;
                y = workingImage.Height - 1;
                while (y > 0 && !found)
                {
                    System.Drawing.Color c = workingImage.GetPixel(x, y);
                    if (c.R != 255)
                    {
                        found = true;
                        down = new Point(x, y);
                    }
                    y--;
                }
            }
            res = new Dimension(up, down);
            return res;
        }

        private void Header_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private double InchesToMM(double x)
        {
            return x * 25.4;
        }

        private void LoadImage()
        {
            if (File.Exists(imageFilePath))
            {
                System.Drawing.Color c;
                using (System.Drawing.Bitmap src = new System.Drawing.Bitmap(imageFilePath))
                {
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
                                if (px > brx)
                                {
                                    brx = px;
                                }
                                if (py > bry)
                                {
                                    bry = py;
                                }
                            }
                        }
                    }

                    // allow a little bit of extra space
                    if (tlx > 0)
                    {
                        tlx--;
                    }
                    if (tly > 0)
                    {
                        tly++;
                    }

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
                    brx -= tlx;
                    tlx = 0;
                    bry -= tly;
                    tly = 0;
                    im = new Image();
                    im.Source = loadBitmap(workingImage);
                    isValid = true;
                }
                UpdateDisplay();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectedMarker != null)
                {
                    Br_MouseMove(sender, e);
                }
                else
                if (pinSelected)
                {
                    Ply_MouseMove(sender, e);
                }
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Br_MouseUp(sender, e);

            Ply_MouseUp(sender, e);
        }

        private void Ply_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pinSelected = true;
            selectedMarker = null;
        }

        private void Ply_MouseMove(object sender, MouseEventArgs e)
        {
            if (pinSelected)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p = e.GetPosition(mainCanvas);
                    if (p.X >= tlx && p.X < brx)
                    {
                        pinPos = (int)p.X;
                        UpdateDisplay();
                        if (OnPinMoved != null)
                        {
                            OnPinMoved(pinPos);
                        }
                    }
                }
            }
        }

        private void Ply_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pinSelected = false;
            //            selectedMarker = null;
        }

        private void UpdateCanvas()
        {
            mainCanvas.Children.Clear();
            if (im != null)
            {
                mainCanvas.Children.Add(im);
                Canvas.SetTop(im, totalTopMargin);
            }
            foreach (UIElement el in elements)
            {
                mainCanvas.Children.Add(el);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Background = System.Windows.Media.Brushes.Transparent;
            pinSelected = false;
            upperPoint = new Point(-1, -1);
            pinPos = 150;
            scale = 1;
            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            UpdateDisplay();
        }
    }
}