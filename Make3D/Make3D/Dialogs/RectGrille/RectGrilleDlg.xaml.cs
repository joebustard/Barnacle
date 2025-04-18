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
    /// Interaction logic for RectGrille.xaml
    /// </summary>
    public partial class RectGrilleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxedgeThickness = 200;
        private const double maxgrillHeight = 200;
        private const double maxgrillLength = 200;
        private const double maxgrillWidth = 200;
        private const double maxhorizontalBars = 100;
        private const double maxhorizontalBarThickness = 100;
        private const double maxverticalBars = 100;
        private const double maxverticalBarThickness = 100;
        private const double minedgeThickness = 0.1;
        private const double mingrillHeight = 5;
        private const double mingrillLength = 5;
        private const double mingrillWidth = 0.1;
        private const double minhorizontalBars = 0;
        private const double minhorizontalBarThickness = 0.1;
        private const double minverticalBars = 0;
        private const double minverticalBarThickness = 0.1;
        private double edgeThickness;
        private double grillHeight;
        private double grillLength;
        private double grillWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private bool loaded;
        private bool makeEdge;
        private double verticalBars;
        private double verticalBarThickness;
        private string warningText;

        public RectGrilleDlg()
        {
            InitializeComponent();
            ToolName = "RectGrille";
            DataContext = this;
            loaded = false;
        }

        public double EdgeThickness
        {
            get
            {
                return edgeThickness;
            }

            set
            {
                if (edgeThickness != value)
                {
                    if (value >= minedgeThickness && value <= maxedgeThickness)
                    {
                        edgeThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeThicknessToolTip
        {
            get
            {
                return $"Edge Thickness must be in the range {minedgeThickness} to {maxedgeThickness}";
            }
        }

        public String GrilleHeightToolTip
        {
            get
            {
                return $"Grille Height must be in the range {mingrillHeight} to {maxgrillHeight}";
            }
        }

        public double GrillHeight
        {
            get
            {
                return grillHeight;
            }

            set
            {
                if (grillHeight != value)
                {
                    if (value >= mingrillHeight && value <= maxgrillHeight)
                    {
                        grillHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double GrillLength
        {
            get
            {
                return grillLength;
            }

            set
            {
                if (grillLength != value)
                {
                    if (value >= mingrillLength && value <= maxgrillLength)
                    {
                        grillLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillLengthToolTip
        {
            get
            {
                return $"Grille Length must be in the range {mingrillLength} to {maxgrillLength}";
            }
        }

        public double GrillWidth
        {
            get
            {
                return grillWidth;
            }

            set
            {
                if (grillWidth != value)
                {
                    if (value >= mingrillWidth && value <= maxgrillWidth)
                    {
                        grillWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String GrillWidthToolTip
        {
            get
            {
                return $"Grille Width must be in the range {mingrillWidth} to {maxgrillWidth}";
            }
        }

        public double HorizontalBars
        {
            get
            {
                return horizontalBars;
            }

            set
            {
                if (horizontalBars != value)
                {
                    if (value >= minhorizontalBars && value <= maxhorizontalBars)
                    {
                        horizontalBars = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HorizontalBarsToolTip
        {
            get
            {
                return $"Horizontal Bars must be in the range {minhorizontalBars} to {maxhorizontalBars}";
            }
        }

        public double HorizontalBarThickness
        {
            get
            {
                return horizontalBarThickness;
            }

            set
            {
                if (horizontalBarThickness != value)
                {
                    if (value >= minhorizontalBarThickness && value <= maxhorizontalBarThickness)
                    {
                        horizontalBarThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String HorizontalBarThicknessToolTip
        {
            get
            {
                return $"Horizontal Bar Thickness must be in the range {minhorizontalBarThickness} to {maxhorizontalBarThickness}";
            }
        }

        public bool MakeEdge
        {
            get
            {
                return makeEdge;
            }

            set
            {
                if (makeEdge != value)
                {
                    makeEdge = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String MakeEdgeToolTip
        {
            get
            {
                return $"If selected the outer frame is created.";
            }
        }

        public double VerticalBars
        {
            get
            {
                return verticalBars;
            }

            set
            {
                if (verticalBars != value)
                {
                    if (value >= minverticalBars && value <= maxverticalBars)
                    {
                        verticalBars = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String VerticalBarsToolTip
        {
            get
            {
                return $"Vertical Bars must be in the range {minverticalBars} to {maxverticalBars}";
            }
        }

        public double VerticalBarThickness
        {
            get
            {
                return verticalBarThickness;
            }

            set
            {
                if (verticalBarThickness != value)
                {
                    if (value >= minverticalBarThickness && value <= maxverticalBarThickness)
                    {
                        verticalBarThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String VerticalBarThicknessToolTip
        {
            get
            {
                return $"Vertical Bar Thickness must be in the range {minverticalBarThickness} to {maxverticalBarThickness}";
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
            RectGrilleMaker maker = new RectGrilleMaker(grillLength,
            grillHeight,
            grillWidth,
            makeEdge,
            edgeThickness,
            verticalBars,
            verticalBarThickness,
            horizontalBars,
            horizontalBarThickness);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            GrillLength = EditorParameters.GetDouble("GrillLength", 30);
            GrillHeight = EditorParameters.GetDouble("GrillHeight", 40);
            GrillWidth = EditorParameters.GetDouble("GrillWidth", 5);
            MakeEdge = EditorParameters.GetBoolean("MakeEdge", true);
            EdgeThickness = EditorParameters.GetDouble("EdgeThickness", 1);
            VerticalBars = EditorParameters.GetDouble("VerticalBars", 3);
            VerticalBarThickness = EditorParameters.GetDouble("VerticalBarThickness", 2);
            HorizontalBars = EditorParameters.GetDouble("HorizontalBars", 3);
            HorizontalBarThickness = EditorParameters.GetDouble("HorizontalBarThickness", 2);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("GrillLength", GrillLength.ToString());
            EditorParameters.Set("GrillHeight", GrillHeight.ToString());
            EditorParameters.Set("GrillWidth", GrillWidth.ToString());
            EditorParameters.Set("MakeEdge", MakeEdge.ToString());
            EditorParameters.Set("EdgeThickness", EdgeThickness.ToString());
            EditorParameters.Set("VerticalBars", VerticalBars.ToString());
            EditorParameters.Set("VerticalBarThickness", VerticalBarThickness.ToString());
            EditorParameters.Set("HorizontalBars", HorizontalBars.ToString());
            EditorParameters.Set("HorizontalBarThickness", HorizontalBarThickness.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            GrillLength = 30;
            GrillHeight = 40;
            GrillWidth = 5;
            MakeEdge = true;
            EdgeThickness = 1;
            VerticalBars = 3;
            VerticalBarThickness = 2;
            HorizontalBars = 3;
            HorizontalBarThickness = 2;
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