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
    /// Interaction logic for Pill.xaml
    /// </summary>
    public partial class PillDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxedge = 100;
        private const double maxflatHeight = 200;
        private const double maxflatLength = 200;
        private const double maxpillWidth = 200;
        private const double minedge = 1;
        private const double minflatHeight = 1;
        private const double minflatLength = 1;
        private const double minpillWidth = 2;
        private const double minTrayThickness = 0.1;
        private double edge;
        private double flatHeight;
        private double flatLength;
        private bool halfSelected;
        private bool loaded;
        private double pillWidth;
        private bool plainSelected;
        private int shape;
        private Visibility showTray;
        private bool traySelected;
        private double trayThickness;
        private string warningText;

        public PillDlg()
        {
            InitializeComponent();
            ToolName = "Pill";
            DataContext = this;
            loaded = false;
        }

        public double Edge
        {
            get
            {
                return edge;
            }

            set
            {
                if (edge != value)
                {
                    if (value >= minedge && value <= maxedge)
                    {
                        edge = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String EdgeToolTip
        {
            get
            {
                return $"Edge must be in the range {minedge} to {maxedge}";
            }
        }

        public double FlatHeight
        {
            get
            {
                return flatHeight;
            }

            set
            {
                if (flatHeight != value)
                {
                    if (value >= minflatHeight && value <= maxflatHeight)
                    {
                        flatHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FlatHeightToolTip
        {
            get
            {
                return $"Flat Height must be in the range {minflatHeight} to {maxflatHeight}";
            }
        }

        public double FlatLength
        {
            get
            {
                return flatLength;
            }

            set
            {
                if (flatLength != value)
                {
                    if (value >= minflatLength && value <= maxflatLength)
                    {
                        flatLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String FlatLengthToolTip
        {
            get
            {
                return $"Flat Length must be in the range {minflatLength} to {maxflatLength}";
            }
        }

        public bool HalfSelected
        {
            get
            {
                return halfSelected;
            }

            set
            {
                if (halfSelected != value)
                {
                    halfSelected = value;
                    NotifyPropertyChanged();
                    if (halfSelected)
                    {
                        shape = 1;
                        ShowTray = Visibility.Hidden;
                        UpdateDisplay();
                    }
                }
            }
        }

        public double PillWidth
        {
            get
            {
                return pillWidth;
            }

            set
            {
                if (pillWidth != value)
                {
                    if (value >= minpillWidth && value <= maxpillWidth)
                    {
                        pillWidth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String PillWidthToolTip
        {
            get
            {
                return $"Pill Width must be in the range {minpillWidth} to {maxpillWidth}";
            }
        }

        public bool PlainSelected
        {
            get
            {
                return plainSelected;
            }

            set
            {
                if (plainSelected != value)
                {
                    plainSelected = value;
                    NotifyPropertyChanged();
                    if (plainSelected)
                    {
                        shape = 0;
                        ShowTray = Visibility.Hidden;
                        UpdateDisplay();
                    }
                }
            }
        }

        public Visibility ShowTray
        {
            get
            {
                return showTray;
            }

            set
            {
                if (showTray != value)
                {
                    showTray = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool TraySelected
        {
            get
            {
                return traySelected;
            }

            set
            {
                if (traySelected != value)
                {
                    traySelected = value;

                    NotifyPropertyChanged();
                    if (traySelected)
                    {
                        shape = 2;
                        ShowTray = Visibility.Visible;
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TrayThickness
        {
            get
            {
                return trayThickness;
            }

            set
            {
                if (trayThickness != value)
                {
                    trayThickness = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String TrayThicknessToolTip
        {
            get
            {
                return $"Tray thickness must be in the range {minTrayThickness} to half Flat Length";
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
            PillMaker maker = new PillMaker(flatLength, flatHeight, edge, pillWidth, trayThickness, shape);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            FlatLength = EditorParameters.GetDouble("FlatLength", 20);
            FlatHeight = EditorParameters.GetDouble("FlatHeight", 20);
            Edge = EditorParameters.GetDouble("Edge", 5);
            PillWidth = EditorParameters.GetDouble("PillWidth", 10);
            PlainSelected = EditorParameters.GetBoolean("PlainSelected", true);
            HalfSelected = EditorParameters.GetBoolean("HalfSelected", false);
            TraySelected = EditorParameters.GetBoolean("TraySelected", false);
            TrayThickness = EditorParameters.GetDouble("TrayThickness", 1);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("FlatLength", FlatLength.ToString());
            EditorParameters.Set("FlatHeight", FlatHeight.ToString());
            EditorParameters.Set("Edge", Edge.ToString());
            EditorParameters.Set("PillWidth", PillWidth.ToString());
            EditorParameters.Set("PlainSelected", PlainSelected.ToString());
            EditorParameters.Set("HalfSelected", HalfSelected.ToString());
            EditorParameters.Set("TraySelected", TraySelected.ToString());
            EditorParameters.Set("TrayThickness", TrayThickness.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            FlatLength = 20;
            FlatHeight = 20;
            Edge = 5;
            PillWidth = 10;
            PlainSelected = true;
            HalfSelected = false;
            TraySelected = false;
            TrayThickness = 1;
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