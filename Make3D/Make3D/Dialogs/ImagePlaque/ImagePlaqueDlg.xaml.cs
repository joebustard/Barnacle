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

using MakerLib;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        private const double maxplagueThickness = 100;
        private const double minplagueThickness = 0.5;
        private FormatConvertedBitmap grayBitmapSource;
        private bool limitRunLengths;
        private bool loaded;
        private int maxRunLength;
        private double plagueThickness;
        private ImageSource plaqueImage;
        private string plaqueImagePath;
        private double solidLength;
        private string warningText;

        public ImagePlaqueDlg()
        {
            DataContext = this;
            InitializeComponent();
            ToolName = "ImagePlaque";

            loaded = false;
        }

        public bool LimitRunLengths
        {
            get
            {
                return limitRunLengths;
            }

            set
            {
                if (value != limitRunLengths)
                {
                    limitRunLengths = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public int MaxRunLength
        {
            get
            {
                return maxRunLength;
            }

            set
            {
                if (maxRunLength != value)
                {
                    maxRunLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

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

        public ImageSource PlaqueImage
        {
            get
            {
                return plaqueImage;
            }

            set
            {
                if (value != plaqueImage)
                {
                    plaqueImage = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

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

        public double SolidLength
        {
            get
            {
                return solidLength;
            }

            set
            {
                if (value != solidLength)
                {
                    solidLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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

        /// <summary>
        /// Turn on the twirlywoo and allow any controls being changed
        /// </summary>
        public override void Busy()
        {
            Viewer.BusyVisible = Visibility.Visible;
            EditingEnabled = false;
        }

        /// <summary>
        /// Turn off the twirlywoo and allow any controls being changed
        /// </summary>
        public override void NotBusy()
        {
            Viewer.BusyVisible = Visibility.Hidden;
            EditingEnabled = true;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                if (System.IO.File.Exists(dlg.FileName))
                {
                    loaded = false;
                    LoadImageAndConvertToGrayScale(dlg.FileName);
                    loaded = true;
                    PlaqueImagePath = dlg.FileName;
                }
            }
        }

        private AsyncGeneratorResult GenerateAsync()
        {
            AsyncGeneratorResult res = new AsyncGeneratorResult();
            ImagePlaqueMaker maker = new ImagePlaqueMaker(plagueThickness, plaqueImagePath, limitRunLengths, maxRunLength, solidLength);
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            maker.Generate(v1, i1);

            // extract the vertices and indices to thread safe arrays
            // while still in the async function
            res.points = new Point3D[v1.Count];
            for (int i = 0; i < v1.Count; i++)
            {
                res.points[i] = new Point3D(v1[i].X, v1[i].Y, v1[i].Z);
            }
            res.indices = new int[i1.Count];
            for (int i = 0; i < i1.Count; i++)
            {
                res.indices[i] = i1[i];
            }
            v1.Clear();
            i1.Clear();
            return (res);
        }

        private async void GenerateShape()
        {
            DateTime start = DateTime.Now;
            ClearShape();
            if (plagueThickness > 0 && !String.IsNullOrEmpty(plaqueImagePath))
            {
                Busy();
                var result = await Task.Run(() => GenerateAsync());
                GetVerticesFromAsyncResult(result);

                CentreVertices();
                NotBusy();
            }
            TimeSpan duration = DateTime.Now - start;
            Log($"imageplaque duration {duration.TotalSeconds}");
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            PlagueThickness = EditorParameters.GetDouble("PlagueThickness", 5);
            PlaqueImagePath = EditorParameters.Get("PlaqueImagePath");
            LimitRunLengths = EditorParameters.GetBoolean("LimitRunLengths", true);
            MaxRunLength = EditorParameters.GetInt("MaxRunLength", 1);
            SolidLength = EditorParameters.GetDouble("SolidLength", 25);
        }

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

            // Use the BitmapSource object defined above as the source for this new BitmapSource
            // (chain the BitmapSource objects together).
            grayBitmapSource.Source = bitmap;

            // Key of changing the bitmap format is DesitnationFormat property of BitmapSource. It
            // is a type of PixelFormat. FixelFormat has dozens of options to set bitmap formatting.
            grayBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            grayBitmapSource.EndInit();

            PlaqueImage = grayBitmapSource;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("PlagueThickness", PlagueThickness.ToString());
            EditorParameters.Set("PlaqueImagePath", PlaqueImagePath.ToString());
            EditorParameters.Set("LimitRunLengths", LimitRunLengths.ToString());
            EditorParameters.Set("MaxRunLength", MaxRunLength.ToString());
            EditorParameters.Set("SolidLength", SolidLength.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            PlagueThickness = 5;
            PlaqueImagePath = "";
            LimitRunLengths = false;
            MaxRunLength = 1;
            loaded = true;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Viewer.Model = GetModel();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            NotBusy();
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}