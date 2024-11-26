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
    /// Interaction logic for ParabolicDish.xaml
    /// </summary>
    public partial class ParabolicDishDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private int pitch;
        private double radius;
        private double wallThickness;
        private string warningText;

        public ParabolicDishDlg()
        {
            InitializeComponent();
            ToolName = "ParabolicDish";
            DataContext = this;
            pitch = 5;
            radius = 10;
            wallThickness = 1;
            loaded = false;
        }

        public int Pitch
        {
            get
            {
                return pitch;
            }

            set
            {
                if (pitch != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        pitch = value;
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
                    if (value >= 0.5 && value <= 99.5)
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
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            ParabolicDishMaker maker = new ParabolicDishMaker(radius, wallThickness, pitch);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Radius = EditorParameters.GetDouble("Radius", 10);
            WallThickness = EditorParameters.GetDouble("WallThickness", 1);
            Pitch = EditorParameters.GetInt("Pitch", 5);
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
            EditorParameters.Set("WallThickness", WallThickness.ToString());
            EditorParameters.Set("Pitch", Pitch.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            Radius = 10;
            WallThickness = 1;
            Pitch = 5;
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
            MeshColour = System.Windows.Media.Colors.Teal;
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}