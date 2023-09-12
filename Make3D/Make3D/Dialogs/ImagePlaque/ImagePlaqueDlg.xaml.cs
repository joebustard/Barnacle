using MakerLib;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ImagePlaque.xaml
    /// </summary>
    public partial class ImagePlaqueDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minplagueThickness = 0.5;
        private const double maxplagueThickness = 100;
        private double plagueThickness;

        public double PlagueThickness
        {
            get
            {
                return plagueThickness;
            }
            set
            {
                if (plagueThickness != value)
                {
                    if (value >= minplagueThickness && value <= maxplagueThickness)
                    {
                        plagueThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PlagueThicknessToolTip
        {
            get
            {
                return $"PlagueThickness must be in the range {minplagueThickness} to {maxplagueThickness}";
            }
        }

        private string plaqueImagePath;

        public string PlaqueImagePath
        {
            get
            {
                return plaqueImagePath;
            }
            set
            {
                if (plaqueImagePath != value)
                {
                    plaqueImagePath = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String PlaqueImagePathToolTip
        {
            get
            {
                return $"PlaqueImagePath must point to a grayscale image file";
            }
        }

        public ImagePlaqueDlg()
        {
            InitializeComponent();
            ToolName = "ImagePlaque";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
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

        private void GenerateShape()
        {
            ClearShape();
            if (plagueThickness > 0 && !String.IsNullOrEmpty(plaqueImagePath))
            {
                ImagePlaqueMaker maker = new ImagePlaqueMaker(plagueThickness, plaqueImagePath);
                maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            PlagueThickness = EditorParameters.GetDouble("PlagueThickness", 5);

            PlaqueImagePath = EditorParameters.Get("PlaqueImagePath");
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("PlagueThickness", PlagueThickness.ToString());
            EditorParameters.Set("PlaqueImagePath", PlaqueImagePath.ToString());
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;

            UpdateDisplay();
        }

        private void SetDefaults()
        {
            loaded = false;
            PlagueThickness = 5;
            PlaqueImagePath = "";

            loaded = true;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                if (System.IO.File.Exists(dlg.FileName))
                {
                    PlaqueImagePath = dlg.FileName;
                    LoadImageAndConvertToGrayScale(dlg.FileName);
                }
            }
        }

        public ImageSource PlaqueImage
        {
            get { return plaqueImage; }
            set
            {
                if (value != plaqueImage)
                {
                    plaqueImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private FormatConvertedBitmap grayBitmapSource;
        private ImageSource plaqueImage;

        private void LoadImageAndConvertToGrayScale(String fileName)
        {
            // Create a BitmapSource
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fileName);
            bitmap.EndInit();

            // Create a FormatConvertedBitmap
            grayBitmapSource = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            grayBitmapSource.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            grayBitmapSource.Source = bitmap;

            // Key of changing the bitmap format is DesitnationFormat property of BitmapSource.
            // It is a type of PixelFormat. FixelFormat has dozens of options to set
            // bitmap formatting.
            grayBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            grayBitmapSource.EndInit();

            PlaqueImage = grayBitmapSource;
        }
    }
}