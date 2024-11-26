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
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for CurvedFunnel.xaml
    /// </summary>
    public partial class CurvedFunnelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double factorA;
        private bool loaded;
        private double radius;
        private double shapeHeight;
        private double wallThickness;
        private string warningText;

        public CurvedFunnelDlg()
        {
            InitializeComponent();
            ToolName = "CurvedFunnel";
            DataContext = this;
            loaded = false;
        }

        public double FactorA
        {
            get
            {
                return factorA;
            }

            set
            {
                if (factorA != value)
                {
                    if (value >= 0.1 && value <= 10)
                    {
                        factorA = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }

            set
            {
                if (radius != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        radius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double ShapeHeight
        {
            get
            {
                return shapeHeight;
            }

            set
            {
                if (shapeHeight != value)
                {
                    if (value >= 5 && value <= 100)
                    {
                        shapeHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double WallThickness
        {
            get
            {
                return wallThickness;
            }

            set
            {
                if (wallThickness != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        wallThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
            if (Radius - WallThickness <= 0)
            {
                WarningText = "Radius must be bigger than Wall Thickness";
            }
            else
            {
                WarningText = "";
                CurvedFunnelMaker maker = new CurvedFunnelMaker(radius, factorA, wallThickness, shapeHeight);
                maker.Generate(Vertices, Faces);
                CentreVertices();
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Radius = EditorParameters.GetDouble("Radius", 10);
            FactorA = EditorParameters.GetDouble("FactorA", 0.75);
            WallThickness = EditorParameters.GetDouble("WallThickness", 1);
            ShapeHeight = EditorParameters.GetDouble("Height", 20);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("FactorA", FactorA.ToString());
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("Height", ShapeHeight.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            Radius = 10;
            FactorA = 0.75;
            WallThickness = 1;
            ShapeHeight = 20;
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