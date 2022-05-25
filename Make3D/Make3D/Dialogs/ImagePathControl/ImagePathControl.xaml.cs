using Barnacle.LineLib;
using Microsoft.Win32;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Barnacle.Dialogs.ProfileFuselage.ViewModels;
namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ImagePathControl.xaml
    /// </summary>
    public partial class ImagePathControl : UserControl, INotifyPropertyChanged
    {
        public ForceReload OnForceReload;
        private double brx = double.MinValue;
        private double bry = double.MinValue;
        private double divisionLength;
        private FlexiPath flexiPath;
        private string fName;
        private string header;

        private double height = 10;

        private bool hollowShape;

        private ImageEdge imageEdge;

        // raw list of points font around the bitmap
        private string imagePath;

        private bool isValid;

        private bool lineShape;

        public string ImagePath
        {
            get
            {
                if (vm != null)
                {
                    return vm.ImagePath;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (vm != null)
                {
                    vm.ImagePath = value;
                }
            }

        }

        private bool loaded;




        private double scrollX;
        private double scrollY;


        internal void RenderFlexipath(ref Bitmap bmp, out double tlx, out double tly, out double brx, out double bry)
        {
            tlx = 0;
            tly = 0;
            brx = 0;
            bry = 0;
            if (vm != null)
            {
                vm.RenderFlexipath(ref bmp, out tlx, out tly, out brx, out bry);
            }
        }

        private SelectionModeType selectionMode;






        private System.Drawing.Bitmap workingImage;
        private ImagePathViewModel vm;

        public ImagePathControl()
        {
            InitializeComponent();

            Clear();
            loaded = false;

        }

        public delegate void ForceReload(string pth);

        public event PropertyChangedEventHandler PropertyChanged;

        public enum SelectionModeType
        {
            StartPoint,
            AddPoint,
            SelectPoint,
            AddLine,
            AddBezier,
            DeleteSegment,
            AddQuadBezier,
            MovePath
        };

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



        public void Clear()
        {
            if (vm != null)
            {
                vm.Clear();
            }
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

        }



        public void UpdateHeaderLabel()
        {
            //          HeaderLabel.Content = ImagePathHeader;
            //        FNameLabel.Content = FName;
        }





        private void DeletePointClicked(object sender, RoutedEventArgs e)
        {
        }




        private void FlexiPathCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }


        private void FlexiPathCanvas_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void FlexiPathCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {


        }

        private void FlexiPathCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }



        private void Ln_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

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


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (loaded)
            {
                scrollX = e.HorizontalOffset;
                scrollY = e.VerticalOffset;
            }
        }



        private void SetButtonBorderColours()
        {
            /*
            switch (selectionMode)
            {
                case SelectionModeType.SelectPoint:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddLine:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddBezier:
                    {
                        PickBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        AddBezierBorder.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
                        AddQuadBezierBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        DelSegBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                        MovePathBorder.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
                    }
                    break;

                case SelectionModeType.AddQuadBezier:
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
            */
        }


        private void ShowCenters_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ShowProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowWorkingImage()
        {
            //   CopySrcToWorking();

            //   SetImageSource();
        }

      

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;

            vm = (ImagePathViewModel)this.DataContext;
            vm.DisplayCanvas = FlexiPathCanvas;
            vm.ScreenScale = VisualTreeHelper.GetDpi(FlexiPathCanvas);

            loaded = true;
        }

        internal void Read(XmlElement nd)
        {
            if (vm != null)
            {
                vm.Read(nd);
            }
        }

        internal void FetchImage()
        {
            if (vm != null)
            {
                vm.FetchImage();
            }
        }

        internal void UpdateDisplay()
        {
            if (vm != null)
            {
                vm.UpdateDisplay();
            }
        }

        internal void Write(XmlDocument doc, XmlElement sideNode)
        {
            if (vm != null)
            {
                vm.Write(doc, sideNode);
            }
        }
    }
}