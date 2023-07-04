using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageMarker.xaml
    /// </summary>
    public partial class ImageMarker : UserControl, INotifyPropertyChanged
    {
        public List<LetterMarker> markers;
        private bool convertMarkerPositionToScreen;
        public CopyLetter OnCopyLetter;

        public MarkerMoved OnMarkerMoved;

        public PinMoved OnPinMoved;

        public Bounds outlineBounds;
        private const int XOutlineOffset = 10;
        private const int YOutlineOffset = 10;
        private static double scale = 1.0;
        private int brx = int.MinValue;

        private int bry = int.MinValue;

        private int canvasHeight;
        private int canvasWidth;
        private List<Dimension> dimensions;

        private List<UIElement> elements;

        private int gapBelowLetter = 5;

        private string headerText;

        private Image im;

        private string imageFilePath;

        private int imageMargin = 20;
        private bool isValid;

        private double leftLimit;
        private int letterMargin = 20;

        private Point lowerPoint;

        private int markerTop;

        private bool notifyMoved;
        private Point oldPoint;
        private List<PointF> outlinePoints;
        private int pinMargin = 30;

        private int pinPos;

        private bool pinSelected;

        private double rightLimit;
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
            Clear();
        }

        public delegate void CopyLetter(string name);

        public delegate void MarkerMoved(string s, System.Windows.Point p, bool finishedMove);

        public delegate void PinMoved(int x);

        public event PropertyChangedEventHandler PropertyChanged;

        public int CanvasHeight
        {
            get { return canvasHeight; }
            set
            {
                if (canvasHeight != value)
                {
                    canvasHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int CanvasWidth
        {
            get { return canvasWidth; }
            set
            {
                if (canvasWidth != value)
                {
                    canvasWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public bool IsValid
        {
            get { return isValid; }
        }

        public double LeftLimit
        {
            get
            {
                return leftLimit;
            }
            set
            {
                if (leftLimit != value)
                {
                    leftLimit = value;
                    NotifyPropertyChanged();
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
                    convertMarkerPositionToScreen = true;
                }
            }
        }

        public List<PointF> OutlinePoints
        {
            get { return outlinePoints; }
            set
            {
                if (outlinePoints != value)
                {
                    outlinePoints = value;

                    UpdateDisplay();
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

        public double RightLimit
        {
            get
            {
                return rightLimit;
            }
            set
            {
                if (rightLimit != value)
                {
                    rightLimit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public System.Drawing.Bitmap WorkingImage
        {
            get { return workingImage; }
            set
            {
                if (workingImage != null)
                {
                    workingImage = value;
                    NotifyPropertyChanged();
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

        public void Clear()
        {
            elements = new List<UIElement>();

            totalTopMargin = pinMargin + letterMargin + gapBelowLetter;
            markerTop = pinMargin + (letterMargin / 2) + 1;
            selectedMarker = null;
            pinSelected = false;
            dimensions = new List<Dimension>();
            isValid = false;
            notifyMoved = false;
            leftLimit = double.MaxValue;
            rightLimit = double.MinValue;
            workingImage = null;
        }

        public Dimension GetUpperAndLowerPoints(double x)
        {
            Dimension res = null;
            Point up = new Point(0, 0);
            Point down = new Point(0, 0);
            int y = 0;
            bool found = false;
            if (workingImage != null)
            {
                if (x >= 0 && x < workingImage.Width)
                {
                    y = 0;
                    while (y < workingImage.Height && !found)
                    {
                        System.Drawing.Color c = workingImage.GetPixel((int)x, (int)y);
                        if (c.R < 128)
                        {
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
                            System.Drawing.Color c = workingImage.GetPixel((int)x, (int)y);
                            if (c.R < 128)
                            {
                                found = true;
                                down = new Point(x, y);
                            }
                            y--;
                        }
                    }
                }
            }
            res = new Dimension(up, down);
            return res;
        }

        public void SetupImage()
        {
            leftLimit = tlx;
            rightLimit = brx;

            if (workingImage != null)
            {
                im = new System.Windows.Controls.Image();

                im.Source = loadBitmap(workingImage);
                leftLimit = tlx;
                rightLimit = brx;
            }
        }

        public void UpdateDisplay()
        {
            elements.Clear();
            RenderFlexipath();
            SetupImage();
            if (workingImage != null)
            {
                Dimension pinDim = GetUpperAndLowerPoints(pinPos);
                upperPoint = new Point(pinDim.P1.X, pinDim.P1.Y);
                lowerPoint = new Point(pinDim.P2.X, pinDim.P2.Y);
                Point midPoint = new Point(pinDim.Mid.X, pinDim.Mid.Y);
                double size = lowerPoint.Y - upperPoint.Y;
                AddTopPin(pinPos, size.ToString());
                AddCenterMarker();
                if (upperPoint.X != -1)
                {
                    AddCircle((int)upperPoint.X, (int)upperPoint.Y, null);
                }
                if (pinDim.P1.X != -1)
                {
                    AddCircle((int)pinDim.P1.X, (int)midPoint.Y, null);
                }
                if (lowerPoint.X != -1)
                {
                    AddCircle((int)lowerPoint.X, (int)lowerPoint.Y, null);
                }
                Dimensions.Clear();
                // update the markers
                // If there is a selected marker, do it last so it appears in front of the
                // the others
                if (markers != null)
                {
                    if (convertMarkerPositionToScreen)
                    {
                        ConvertMarkerPositionsToScreen();
                    }
                    foreach (LetterMarker mk in markers)
                    {
                        if (mk != selectedMarker)
                        {
                            UpdateMarker(mk);
                        }
                    }
                    if (selectedMarker != null)
                    {
                        UpdateMarker(selectedMarker);
                    }
                }

                // CanvasWidth = (int)ImageBorder.Width;
                // CanvasHeight = (int)workingImage.Height;
            }
            UpdateCanvas();
        }

        private void ConvertMarkerPositionsToScreen()
        {
            convertMarkerPositionToScreen = false;

            foreach (LetterMarker mk in markers)
            {
                
                mk.Position = MarkerPositionToScreen(mk.Position, renderScale);
                
            }
        }

        internal void AddRib(string name)
        {
            UpdateDisplay();
        }

        internal void DeleteMarker(RibAndPlanEditControl rc)
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

        private double renderScale = 10;

        internal void RenderFlexipath()
        {
            ResetBounds();
            //int offset = 4;
            if (outlinePoints != null && mainCanvas.ActualWidth > 0)
            {
                for (int i = 0; i < outlinePoints.Count; i++)
                {
                    AdjustBounds(outlinePoints[i]);
                }
                renderScale = CalcOutlineScale();
                if (outlinePoints.Count > 0)
                {
                    //    outlineBounds.Left *= sc;
                    outlineBounds.Top = outlineBounds.Bottom + renderScale * outlineBounds.Height();
                    //  outlineBounds.Bottom *= sc;
                    outlineBounds.Right = outlineBounds.Left + renderScale * outlineBounds.Width();

                    workingImage = new Bitmap((int)(outlineBounds.Right) + imageMargin + XOutlineOffset, (int)(outlineBounds.Top) + YOutlineOffset + imageMargin);
                    using (var gfx = Graphics.FromImage(workingImage))
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.Black))
                    {
                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        gfx.Clear(System.Drawing.Color.White);
                        for (int i = 0; i < outlinePoints.Count; i++)
                        {
                            int j = i + 1;
                            if (j >= outlinePoints.Count)
                            {
                                j = 0;
                            }
                            var pt1 = ScreenPoint(outlinePoints[i], renderScale);
                            var pt2 = ScreenPoint(outlinePoints[j], renderScale);
                            gfx.DrawLine(pen, pt1, pt2);
                        }
                    }
                    isValid = true;
                }

                tlx = XOutlineOffset;
                tly = YOutlineOffset;
                brx = tlx + (int)outlineBounds.Width();
                bry = tly + (int)outlineBounds.Height();
                LeftLimit = XOutlineOffset;
                RightLimit = brx;

                //System.Drawing.Point np = WorldPoint(ScreenPoint(new System.Drawing.Point(100, 100), renderScale), renderScale);
            }
        }

        internal void SetHeader(string v)
        {
            HeaderText = v;
        }


        internal void SetMarker(string s, double x)
        {
            foreach (LetterMarker mk in markers)
            {
                if (mk.Letter == s)
                {
                    mk.Position = new System.Windows.Point(x, mk.Position.Y);
                    mk.Position = MarkerPositionToScreen(mk.Position, renderScale);
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

        private void AddLetter(double x, double y, string c, object tag, string s)
        {
            int by = bry + (2 * totalTopMargin);
            AddLine((int)x, (int)y, (int)x, (int)(y + by), s);
            Typeface typeface = new Typeface("Arial");
            double txtWidth = new FormattedText(c, CultureInfo.CurrentUICulture,
        FlowDirection.LeftToRight,
        typeface, 14, System.Windows.Media.Brushes.Black).Width;
            double brd = 5;

            Border br = new Border();
            Canvas.SetLeft(br, x - (txtWidth / 2) - brd);
            Canvas.SetTop(br, y - 10);
            br.Width = txtWidth + 2 * brd;
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
            br.ToolTip = s;
            elements.Add(br);
            AddText((int)(x - ((int)txtWidth / 2)), (int)(y - 8), c, System.Windows.Media.Colors.Black, tag, s);

            br = new Border();
            Canvas.SetLeft(br, x - (txtWidth / 2) - brd);
            Canvas.SetTop(br, y - 10 + by);
            br.Width = txtWidth + 2 * brd;
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
            br.ToolTip = s;
            AddText((int)(x - ((int)txtWidth / 2) ), (int)(y - 8 + by), c, System.Windows.Media.Colors.Black, tag, s);
        }

        private void AddLine(int v1, int v2, int v3, int v4, string s = "")
        {
            Line l = new Line();
            l.Stroke = System.Windows.Media.Brushes.Black;
            l.X1 = v1;
            l.Y1 = v2;
            l.X2 = v3;
            l.Y2 = v4;
            if (s != "")
            {
                l.ToolTip = s;
            }

            l.MouseUp += Br_MouseUp;

            elements.Add(l);
        }

        private void AddText(int x, int y, string text, System.Windows.Media.Color color, object tag, string s = "")
        {
            TextBlock textBlock = new TextBlock();
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Arial");
            textBlock.FontSize = 14;
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.Background = new SolidColorBrush(Colors.Yellow);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            textBlock.MouseDown += Br_MouseDown;
            textBlock.MouseUp += Br_MouseUp;
            textBlock.MouseMove += Br_MouseMove;
            textBlock.Tag = tag;
            if (s != "")
            {
                textBlock.ToolTip = s;
            }
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

        private void AdjustBounds(PointF p)
        {
            if (p.X < outlineBounds.Left)
            {
                outlineBounds.Left = (double)p.X;
            }

            if (p.Y < outlineBounds.Bottom)
            {
                outlineBounds.Bottom = (double)p.Y;
            }

            if (p.X > outlineBounds.Right)
            {
                outlineBounds.Right = (double)p.X;
            }

            if (p.Y > outlineBounds.Top)
            {
                outlineBounds.Top = (double)p.Y;
            }
        }

        private void Br_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pinSelected = false;
            oldPoint = e.GetPosition(mainCanvas);
            if (selectedMarker == null)
            {
                if (sender is Border)
                {
                    selectedMarker = (sender as Border).Tag as LetterMarker;
                    UpdateDisplay();
                }
                else
                if (sender is TextBlock)
                {
                    selectedMarker = (sender as TextBlock).Tag as LetterMarker;
                    UpdateDisplay();
                }
                else
                if (sender is Ellipse)
                {
                    selectedMarker = (sender as Ellipse).Tag as LetterMarker;
                    UpdateDisplay();
                }
                else
                {
                    MessageBox.Show($" ImageMarker br_MouseDown sender type {sender.ToString()}");
                }
            }
        }

        private void Br_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedMarker != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(mainCanvas);
                double dx = p.X - oldPoint.X;
                if (Math.Abs(dx) >= 1)
                {
                    if (p.X >= leftLimit && p.X <= rightLimit)
                    {
                        notifyMoved = true;
                        selectedMarker.Position = new System.Windows.Point((int)p.X, selectedMarker.Position.Y);
                        if (OnMarkerMoved != null)
                        {
                            OnMarkerMoved(selectedMarker.Letter, WorldXPoint(selectedMarker.Position, renderScale), false);
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        private void Br_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (OnMarkerMoved != null && notifyMoved && selectedMarker != null)
                {
                    OnMarkerMoved(selectedMarker.Letter, WorldXPoint(selectedMarker.Position, renderScale), true);
                    notifyMoved = false;
                    UpdateDisplay();
                }
            }
            selectedMarker = null;
        }

        private System.Drawing.Point WorldPoint(System.Drawing.Point position, double sc)
        {
            System.Drawing.Point np = new System.Drawing.Point();
            np.X = position.X - XOutlineOffset;
            np.X = (int)(np.X / sc);
            np.X += (int)outlineBounds.Left;

            np.Y = position.Y - YOutlineOffset;
            np.Y = (int)(np.Y / sc);
            np.Y += (int)outlineBounds.Bottom;

            return np;
        }

        private System.Windows.Point WorldXPoint(System.Windows.Point position, double sc)
        {
            System.Windows.Point np = new System.Windows.Point();
            /*
            np.X = position.X - XOutlineOffset;
            np.X = (int)(np.X / sc);
            np.X += (int)outlineBounds.Left;
            */
            np.Y = position.Y;
            np.X = position.X - XOutlineOffset;
            np.X = (np.X / outlineBounds.Width());
            return np;
        }
        internal System.Drawing.Point ScreenPoint(PointF p, double sc)
        {
            System.Drawing.Point np = new System.Drawing.Point();
            np.X = (int)((p.X - (float)outlineBounds.Left) * sc) + XOutlineOffset;
            np.Y = (int)((p.Y - (float)outlineBounds.Bottom) * sc) + YOutlineOffset;
            return np;
        }

        internal System.Windows.Point MarkerPositionToScreen(System.Windows.Point p, double sc)
        {
            System.Windows.Point np = new System.Windows.Point();
            //np.X = (int)((p.X - (float)outlineBounds.Left) * sc) + XOutlineOffset;
            np.Y = (int)p.Y;
            
            np.X = (int)(p.X * outlineBounds.Width());
            np.X = np.X + XOutlineOffset;
            np.Y = (int)p.Y;
            return np;
        }
        private double CalcOutlineScale()
        {
            double winWidth = mainCanvas.ActualWidth;
            winWidth = winWidth - XOutlineOffset - 20;
            double sc = winWidth / outlineBounds.Width();
            sc = 5;
            return sc;
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

        private void Header_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private double InchesToMM(double x)
        {
            return x * 25.4;
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
                    if (p.X >= leftLimit && p.X <= rightLimit)
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
        }

        private void ResetBounds()
        {
            outlineBounds.Left = double.MaxValue;
            outlineBounds.Bottom = double.MaxValue;
            outlineBounds.Right = double.MinValue;
            outlineBounds.Top = double.MinValue;
        }

        private void UpdateCanvas()
        {
            mainCanvas.Children.Clear();
            if (im != null)
            {
                // move the image down a bit so there is room for the markers
                mainCanvas.Children.Add(im);
                Canvas.SetTop(im, totalTopMargin);
            }
            foreach (UIElement el in elements)
            {
                mainCanvas.Children.Add(el);
            }
        }

        private void UpdateMarker(LetterMarker mk)
        {
            Dimension dp = GetUpperAndLowerPoints(mk.Position.X);
            dimensions.Add(dp);
            AddLetter(mk.Position.X, mk.Position.Y, mk.Letter, mk, $"Size at X={mk.Position.X} is {dp.Height}");
            AddCircle((int)dp.P1.X, (int)dp.P1.Y, mk);
            AddCircle((int)dp.P2.X, (int)dp.P2.Y, mk);
            AddCircle((int)dp.P1.X, (int)dp.Mid.Y, mk);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainCanvas.Background = System.Windows.Media.Brushes.Transparent;
            pinSelected = false;

            upperPoint = new Point(-1, -1);
            pinPos = 150;

            MainScale.ScaleX = scale;
            MainScale.ScaleY = scale;
            //RenderFlexipath();
            UpdateDisplay();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDisplay();
        }

        public struct Bounds
        {
            public double Bottom;
            public double Left;
            public double Right;
            public double Top;

            public double Height()
            {
                return Top - Bottom;
            }

            public double Width()
            {
                return Right - Left;
            }
        }
    }
}