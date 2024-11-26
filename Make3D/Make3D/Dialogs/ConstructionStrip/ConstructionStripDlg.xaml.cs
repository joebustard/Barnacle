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
using System;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ConstructionStrip.xaml
    /// </summary>
    public partial class ConstructionStripDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxholeRadius = 98;
        private const double maxnumberOfHoles = 20;
        private const double maxstripHeight = 10;
        private const double maxstripRepeats = 20;
        private const double maxstripWidth = 100;
        private const double minholeRadius = 2;
        private const double minnumberOfHoles = 2;
        private const double minstripHeight = 1;
        private const double minstripRepeats = 1;
        private const double minstripWidth = 5;
        private double holeRadius;
        private bool loaded;
        private int numberOfHoles;
        private double stripHeight;
        private int stripRepeats;
        private double stripWidth;
        private string warningText;

        public ConstructionStripDlg()
        {
            InitializeComponent();
            ToolName = "ConstructionStrip";
            DataContext = this;
            loaded = false;
        }

        public double HoleRadius
        {
            get
            {
                return holeRadius;
            }

            set
            {
                if (holeRadius != value)
                {
                    if (value >= minholeRadius && value <= maxholeRadius)
                    {
                        holeRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HoleRadiusToolTip
        {
            get
            {
                return $"HoleRadius must be in the range {minholeRadius} to {maxholeRadius}";
            }
        }

        public int NumberOfHoles
        {
            get
            {
                return numberOfHoles;
            }

            set
            {
                if (numberOfHoles != value)
                {
                    if (value >= minnumberOfHoles && value <= maxnumberOfHoles)
                    {
                        numberOfHoles = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String NumberOfHolesToolTip
        {
            get
            {
                return $"NumberOfHoles must be in the range {minnumberOfHoles} to {maxnumberOfHoles}";
            }
        }

        public double StripHeight
        {
            get
            {
                return stripHeight;
            }

            set
            {
                if (stripHeight != value)
                {
                    if (value >= minstripHeight && value <= maxstripHeight)
                    {
                        stripHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripHeightToolTip
        {
            get
            {
                return $"StripHeight must be in the range {minstripHeight} to {maxstripHeight}";
            }
        }

        public int StripRepeats
        {
            get
            {
                return stripRepeats;
            }

            set
            {
                if (stripRepeats != value)
                {
                    if (value >= minstripRepeats && value <= maxstripRepeats)
                    {
                        stripRepeats = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripRepeatsToolTip
        {
            get
            {
                return $"StripRepeats must be in the range {minstripRepeats} to {maxstripRepeats}";
            }
        }

        public double StripWidth
        {
            get
            {
                return stripWidth;
            }

            set
            {
                if (stripWidth != value)
                {
                    if (value >= minstripWidth && value <= maxstripWidth)
                    {
                        stripWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String StripWidthToolTip
        {
            get
            {
                return $"StripWidth must be in the range {minstripWidth} to {maxstripWidth}";
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            ConstructionStripMaker maker = new ConstructionStripMaker(stripHeight, stripWidth, stripRepeats, holeRadius, numberOfHoles);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            StripHeight = EditorParameters.GetDouble("StripHeight", 2.5);
            StripWidth = EditorParameters.GetDouble("StripWidth", 17);
            StripRepeats = EditorParameters.GetInt("StripRepeats", 1);
            HoleRadius = EditorParameters.GetDouble("HoleRadius", 4.5);
            NumberOfHoles = EditorParameters.GetInt("NumberOfHoles", 3);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("StripHeight", StripHeight.ToString());
            EditorParameters.Set("StripWidth", StripWidth.ToString());
            EditorParameters.Set("StripRepeats", StripRepeats.ToString());
            EditorParameters.Set("HoleRadius", HoleRadius.ToString());
            EditorParameters.Set("NumberOfHoles", NumberOfHoles.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            StripHeight = 2.5;
            StripWidth = 17;
            StripRepeats = 1;
            HoleRadius = 4.5;
            NumberOfHoles = 3;
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
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}