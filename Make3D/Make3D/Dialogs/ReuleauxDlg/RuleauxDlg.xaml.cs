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
    /// Interaction logic for Ruleaux.xaml
    /// </summary>
    public partial class ReuleauxDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private int numberOfSides;
        private double radius;
        private double thickness;
        private string warningText;

        public ReuleauxDlg()
        {
            InitializeComponent();
            ToolName = "Reuleaux";
            DataContext = this;
            loaded = false;
            numberOfSides = 3;
            radius = 10;
            thickness = 5;
        }

        public int NumberOfSides
        {
            get
            {
                return numberOfSides;
            }

            set
            {
                if (numberOfSides != value)
                {
                    if (value == 3 || value == 5 || value == 7)
                    {
                        WarningText = "";
                        numberOfSides = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                    else
                    {
                        WarningText = "Number Of Sides must be 3, 5 or 7";
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

        public double Thickness
        {
            get
            {
                return thickness;
            }

            set
            {
                if (thickness != value)
                {
                    if (value >= 0.1 && value <= 100)
                    {
                        thickness = value;
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
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            ReuleauxMaker maker = new ReuleauxMaker(numberOfSides, radius, thickness);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            if (EditorParameters.Get("NumberOfSides") != "")
            {
                NumberOfSides = EditorParameters.GetInt("NumberOfSides");
            }

            if (EditorParameters.Get("Radius") != "")
            {
                Radius = EditorParameters.GetDouble("Radius");
            }

            if (EditorParameters.Get("Thickness") != "")
            {
                Thickness = EditorParameters.GetDouble("Thickness");
            }
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("NumberOfSides", NumberOfSides.ToString());
            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            NumberOfSides = 3;
            Radius = 10;
            Thickness = 5;
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