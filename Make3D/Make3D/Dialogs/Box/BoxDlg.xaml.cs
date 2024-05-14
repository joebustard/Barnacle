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
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Box.xaml
    /// </summary>
    public partial class BoxDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string warningText;
        private bool loaded;

        private const double minboxLength = 1;
        private const double maxboxLength = 200;
        private double boxLength;

        public double BoxLength
        {
            get
            {
                return boxLength;
            }

            set
            {
                if (boxLength != value)
                {
                    if (value >= minboxLength && value <= maxboxLength)
                    {
                        boxLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BoxLengthToolTip
        {
            get
            {
                return $"Box Length must be in the range {minboxLength} to {maxboxLength}";
            }
        }

        private const double minboxHeight = 1;
        private const double maxboxHeight = 200;
        private double boxHeight;

        public double BoxHeight
        {
            get
            {
                return boxHeight;
            }

            set
            {
                if (boxHeight != value)
                {
                    if (value >= minboxHeight && value <= maxboxHeight)
                    {
                        boxHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BoxHeightToolTip
        {
            get
            {
                return $"Box Height must be in the range {minboxHeight} to {maxboxHeight}";
            }
        }

        private const double minboxWidth = 1;
        private const double maxboxWidth = 200;
        private double boxWidth;

        public double BoxWidth
        {
            get
            {
                return boxWidth;
            }

            set
            {
                if (boxWidth != value)
                {
                    if (value >= minboxWidth && value <= maxboxWidth)
                    {
                        boxWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BoxWidthToolTip
        {
            get
            {
                return $"Box Width must be in the range {minboxWidth} to {maxboxWidth}";
            }
        }

        private const double minbaseThickness = 1;
        private const double maxbaseThickness = 20;
        private double baseThickness;

        public double BaseThickness
        {
            get
            {
                return baseThickness;
            }

            set
            {
                if (baseThickness != value)
                {
                    if (value >= minbaseThickness && value <= maxbaseThickness)
                    {
                        baseThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BaseThicknessToolTip
        {
            get
            {
                return $"Base Thickness must be in the range {minbaseThickness} to {maxbaseThickness}";
            }
        }

        private const double minleftThickness = 1;
        private const double maxleftThickness = 20;
        private double leftThickness;

        public double LeftThickness
        {
            get
            {
                return leftThickness;
            }

            set
            {
                if (leftThickness != value)
                {
                    if (value >= minleftThickness && value <= maxleftThickness)
                    {
                        leftThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String LeftThicknessToolTip
        {
            get
            {
                return $"Left Thickness must be in the range {minleftThickness} to {maxleftThickness}";
            }
        }

        private const double minrightThickness = 1;
        private const double maxrightThickness = 20;
        private double rightThickness;

        public double RightThickness
        {
            get
            {
                return rightThickness;
            }

            set
            {
                if (rightThickness != value)
                {
                    if (value >= minrightThickness && value <= maxrightThickness)
                    {
                        rightThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String RightThicknessToolTip
        {
            get
            {
                return $"Right Thickness must be in the range {minrightThickness} to {maxrightThickness}";
            }
        }

        private const double minfrontThickness = 1;
        private const double maxfrontThickness = 20;
        private double frontThickness;

        public double FrontThickness
        {
            get
            {
                return frontThickness;
            }

            set
            {
                if (frontThickness != value)
                {
                    if (value >= minfrontThickness && value <= maxfrontThickness)
                    {
                        frontThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FrontThicknessToolTip
        {
            get
            {
                return $"Front Thickness must be in the range {minfrontThickness} to {maxfrontThickness}";
            }
        }

        private const double minbackThickness = 1;
        private const double maxbackThickness = 20;
        private double backThickness;

        public double BackThickness
        {
            get
            {
                return backThickness;
            }

            set
            {
                if (backThickness != value)
                {
                    if (value >= minbackThickness && value <= maxbackThickness)
                    {
                        backThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String BackThicknessToolTip
        {
            get
            {
                return $"Back Thickness must be in the range {minbackThickness} to {maxbackThickness}";
            }
        }

        public BoxDlg()
        {
            InitializeComponent();

            ToolName = "Box";
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();

            if (boxLength < LeftThickness + rightThickness + 1)
            {
                WarningText = "Box length must be greater than left thickness + right thick";
            }
            else
            if (boxWidth < frontThickness + backThickness + 1)

            {
                WarningText = "Box width must be greater than front thickness + back thick";
            }
            else
            {
                BoxMaker maker = new BoxMaker(boxLength, boxHeight, boxWidth, baseThickness, leftThickness, rightThickness, frontThickness, backThickness); maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            BoxLength = EditorParameters.GetDouble("BoxLength", 50);

            BoxHeight = EditorParameters.GetDouble("BoxHeight", 50);

            BoxWidth = EditorParameters.GetDouble("BoxWidth", 50);

            BaseThickness = EditorParameters.GetDouble("BaseThickness", 1);

            LeftThickness = EditorParameters.GetDouble("LeftThickness", 1);

            RightThickness = EditorParameters.GetDouble("RightThickness", 1);

            FrontThickness = EditorParameters.GetDouble("FrontThickness", 1);

            BackThickness = EditorParameters.GetDouble("BackThickness", 1);
        }

        private void SetDefaults()
        {
            loaded = false;
            BoxLength = 50;
            BoxHeight = 50;
            BoxWidth = 50;
            BaseThickness = 1;
            LeftThickness = 1;
            RightThickness = 1;
            FrontThickness = 1;
            BackThickness = 1;
            loaded = true;
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("BoxLength", BoxLength.ToString());
            EditorParameters.Set("BoxHeight", BoxHeight.ToString());
            EditorParameters.Set("BoxWidth", BoxWidth.ToString());
            EditorParameters.Set("BaseThickness", BaseThickness.ToString());
            EditorParameters.Set("LeftThickness", LeftThickness.ToString());
            EditorParameters.Set("RightThickness", RightThickness.ToString());
            EditorParameters.Set("FrontThickness", FrontThickness.ToString());
            EditorParameters.Set("BackThickness", BackThickness.ToString());
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

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }
    }
}