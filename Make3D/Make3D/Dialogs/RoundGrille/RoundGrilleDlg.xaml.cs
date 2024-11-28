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
    /// Interaction logic for RoundGrille.xaml
    /// </summary>
    public partial class RoundGrilleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const byte horizontalWontFit = 0x02;
        private const double maxedgeThickness = 200;
        private const double maxgrilleRadius = 200;
        private const double maxgrillWidth = 200;
        private const double maxhorizontalBars = 100;
        private const double maxhorizontalBarThickness = 100;
        private const double maxverticalBars = 100;
        private const double maxverticalBarThickness = 100;
        private const double minedgeThickness = 1;
        private const double mingrilleRadius = 1;
        private const double mingrillWidth = 1;
        private const double minhorizontalBars = 0;
        private const double minhorizontalBarThickness = 1;
        private const double minverticalBars = 0;
        private const double minverticalBarThickness = 1;
        private const byte neitherWillFit = 0x03;
        private const byte verticalWontFit = 0x01;
        private double edgeThickness;
        private double grilleRadius;
        private double grillWidth;
        private double horizontalBars;
        private double horizontalBarThickness;
        private bool loaded;
        private bool makeEdge;
        private double verticalBars;
        private double verticalBarThickness;
        private byte warningMask;
        private string warningText;

        public RoundGrilleDlg()
        {
            InitializeComponent();
            ToolName = "RoundGrille";
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

        public double GrilleRadius
        {
            get
            {
                return grilleRadius;
            }

            set
            {
                if (grilleRadius != value)
                {
                    if (value >= mingrilleRadius && value <= maxgrilleRadius)
                    {
                        grilleRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String grilleRadiusToolTip
        {
            get
            {
                return $"Grille Length must be in the range {mingrilleRadius} to {maxgrilleRadius}";
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
                        if (horizontalBarThickness * value < SpaceAvailable() - value)
                        {
                            horizontalBars = value;
                            NotifyPropertyChanged();
                            UpdateDisplay();
                            warningMask = (byte)(warningMask & 0xFD);
                        }
                        else
                        {
                            warningMask = (byte)(warningMask | horizontalWontFit);
                        }
                        SetWarning();
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
                        if (value * horizontalBars < SpaceAvailable() - value)
                        {
                            horizontalBarThickness = value;
                            NotifyPropertyChanged();
                            UpdateDisplay();
                            warningMask = (byte)(warningMask & 0xFD);
                        }
                        else
                        {
                            warningMask = (byte)(warningMask | horizontalWontFit);
                        }
                        SetWarning();
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
                        if (verticalBarThickness * value < SpaceAvailable() - value)
                        {
                            verticalBars = value;
                            NotifyPropertyChanged();
                            UpdateDisplay();
                            warningMask = (byte)(warningMask & 0xFE);
                        }
                        else
                        {
                            warningMask = (byte)(warningMask | verticalWontFit);
                        }
                        SetWarning();
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
                        if (verticalBars * value < SpaceAvailable() - value)
                        {
                            verticalBarThickness = value;
                            NotifyPropertyChanged();
                            UpdateDisplay();
                            warningMask = (byte)(warningMask & 0xFE);
                        }
                        else
                        {
                            warningMask = (byte)(warningMask | verticalWontFit);
                        }
                        SetWarning();
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
            if (ParametersValid())
            {
                RoundGrilleMaker maker = new RoundGrilleMaker(grilleRadius,
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
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            GrilleRadius = EditorParameters.GetDouble("GrilleRadius", 30);
            GrillWidth = EditorParameters.GetDouble("GrillWidth", 5);
            MakeEdge = EditorParameters.GetBoolean("MakeEdge", true);
            EdgeThickness = EditorParameters.GetDouble("EdgeThickness", 1);
            VerticalBars = EditorParameters.GetDouble("VerticalBars", 3);
            VerticalBarThickness = EditorParameters.GetDouble("VerticalBarThickness", 2);
            HorizontalBars = EditorParameters.GetDouble("HorizontalBars", 3);
            HorizontalBarThickness = EditorParameters.GetDouble("HorizontalBarThickness", 2);
        }

        private bool ParametersValid()
        {
            bool result = true;
            double spaceAvailable = SpaceAvailable(); ;

            if (verticalBars * VerticalBarThickness > spaceAvailable - verticalBarThickness)
            {
                result = false;
            }
            if (horizontalBars * HorizontalBarThickness > spaceAvailable - horizontalBarThickness)
            {
                result = false;
            }
            return result;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("GrilleRadius", GrilleRadius.ToString());
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
            grilleRadius = 30;
            GrillWidth = 5;
            MakeEdge = true;
            EdgeThickness = 1;
            VerticalBars = 3;
            VerticalBarThickness = 2;
            HorizontalBars = 3;
            HorizontalBarThickness = 2;
            loaded = true;
        }

        private void SetWarning()
        {
            switch (warningMask)
            {
                case verticalWontFit:
                    {
                        WarningText = "The vertical bars won't fit in the frame";
                    }
                    break;

                case horizontalWontFit:
                    {
                        WarningText = "The horizontal bars won't fit in the frame";
                    }
                    break;

                case neitherWillFit:
                    {
                        WarningText = "Neither the horizontal or vertical bars will fit in the frame";
                    }
                    break;

                default:
                    {
                        WarningText = "";
                    }
                    break;
            }
        }

        private double SpaceAvailable()
        {
            double spaceAvailable = 0;
            if (makeEdge)
            {
                spaceAvailable = (grilleRadius - edgeThickness) * 2.0;
            }
            else
            {
                spaceAvailable = grilleRadius * 2.0;
            }
            return spaceAvailable;
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