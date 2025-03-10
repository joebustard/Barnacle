// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using MakerLib;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Spring.xaml
    /// </summary>
    public partial class SpringDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double coilGap;
        private double facesPerTurn;
        private double innerRadius;
        private bool loaded;
        private double turns;
        private string warningText;
        private double wireFacets;

        private double wireRadius;

        public SpringDlg()
        {
            InitializeComponent();
            ToolName = "Spring";
            DataContext = this;
            loaded = false;
        }

        public double CoilGap
        {
            get
            {
                return coilGap;
            }
            set
            {
                if (coilGap != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        coilGap = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double FacesPerTurn
        {
            get
            {
                return facesPerTurn;
            }
            set
            {
                if (facesPerTurn != value)
                {
                    if (value >= 10 && value <= 360)
                    {
                        facesPerTurn = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double InnerRadius
        {
            get
            {
                return innerRadius;
            }
            set
            {
                if (innerRadius != value)
                {
                    if (value >= 0 && value <= 100)
                    {
                        innerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double Turns
        {
            get
            {
                return turns;
            }
            set
            {
                if (turns != value)
                {
                    if (value >= 1 && value <= 100)
                    {
                        turns = value;
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

        public double WireFacets
        {
            get
            {
                return wireFacets;
            }
            set
            {
                if (wireFacets != value)
                {
                    if (value >= 10 && value <= 360)
                    {
                        wireFacets = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double WireRadius
        {
            get
            {
                return wireRadius;
            }
            set
            {
                if (wireRadius != value)
                {
                    if (value >= 0.1 && value <= 100)
                    {
                        wireRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
            SpringMaker maker = new SpringMaker(innerRadius, wireRadius, coilGap, turns, facesPerTurn, wireFacets);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            InnerRadius = EditorParameters.GetDouble("InnerRadius", 5);
            WireRadius = EditorParameters.GetDouble("WireRadius", 2);
            CoilGap = EditorParameters.GetDouble("CoilGap", 4);
            Turns = EditorParameters.GetDouble("Turns", 5);
            FacesPerTurn = EditorParameters.GetDouble("FacesPerTurn", 20);
            WireFacets = EditorParameters.GetDouble("WireFacets", 20);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            loaded = false;
            SetDefaults();
            loaded = true;
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("InnerRadius", InnerRadius.ToString());
            EditorParameters.Set("WireRadius", WireRadius.ToString());
            EditorParameters.Set("CoilGap", CoilGap.ToString());
            EditorParameters.Set("Turns", Turns.ToString());
            EditorParameters.Set("FacesPerTurn", FacesPerTurn.ToString());
            EditorParameters.Set("WireFacets", WireFacets.ToString());
        }

        private void SetDefaults()
        {
            InnerRadius = 5;
            WireRadius = 2;
            CoilGap = 4;
            Turns = 5;
            FacesPerTurn = 20;
            WireFacets = 20;
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