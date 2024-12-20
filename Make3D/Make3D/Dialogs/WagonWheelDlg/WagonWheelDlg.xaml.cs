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
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for WagonWheel.xaml
    /// </summary>
    public partial class WagonWheelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double axleBore;
        private double hubRadius;
        private double hubThickness;
        private bool loaded;
        private double numberOfSpokes;
        private double rimDepth;
        private double rimInnerRadius;
        private double rimThickness;
        private double spokeRadius;
        private string warningText;

        public WagonWheelDlg()
        {
            InitializeComponent();
            ToolName = "WagonWheel";
            DataContext = this;
            loaded = false;
        }

        public double AxleBore
        {
            get
            {
                return axleBore;
            }
            set
            {
                if (axleBore != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        axleBore = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubRadius
        {
            get
            {
                return hubRadius;
            }
            set
            {
                if (hubRadius != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        hubRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double HubThickness
        {
            get
            {
                return hubThickness;
            }
            set
            {
                if (hubThickness != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        hubThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double NumberOfSpokes
        {
            get
            {
                return numberOfSpokes;
            }
            set
            {
                if (numberOfSpokes != value)
                {
                    if (value >= 4 && value <= 20)
                    {
                        numberOfSpokes = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimDepth
        {
            get
            {
                return rimDepth;
            }
            set
            {
                if (rimDepth != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimInnerRadius
        {
            get
            {
                return rimInnerRadius;
            }
            set
            {
                if (rimInnerRadius != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimInnerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double RimThickness
        {
            get
            {
                return rimThickness;
            }
            set
            {
                if (rimThickness != value)
                {
                    if (value >= 1 && value <= 20)
                    {
                        rimThickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double SpokeRadius
        {
            get
            {
                return spokeRadius;
            }
            set
            {
                if (spokeRadius != value)
                {
                    if (value >= 1 && value <= 10)
                    {
                        spokeRadius = value;
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
            WagonWheelMaker maker = new WagonWheelMaker(hubRadius, hubThickness, rimInnerRadius, rimThickness, rimDepth, numberOfSpokes, spokeRadius, axleBore);
            maker.Generate(Vertices, Faces);
            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("HubRadius") != "")
            {
                HubRadius = EditorParameters.GetDouble("HubRadius");
            }

            if (EditorParameters.Get("HubThickness") != "")
            {
                HubThickness = EditorParameters.GetDouble("HubThickness");
            }

            if (EditorParameters.Get("RimInnerRadius") != "")
            {
                RimInnerRadius = EditorParameters.GetDouble("RimInnerRadius");
            }

            if (EditorParameters.Get("RimThickness") != "")
            {
                RimThickness = EditorParameters.GetDouble("RimThickness");
            }

            if (EditorParameters.Get("RimDepth") != "")
            {
                RimDepth = EditorParameters.GetDouble("RimDepth");
            }

            if (EditorParameters.Get("NumberOfSpokes") != "")
            {
                NumberOfSpokes = EditorParameters.GetDouble("NumberOfSpokes");
            }

            if (EditorParameters.Get("SpokeRadius") != "")
            {
                SpokeRadius = EditorParameters.GetDouble("SpokeRadius");
            }

            if (EditorParameters.Get("AxleBore") != "")
            {
                AxleBore = EditorParameters.GetDouble("AxleBore");
            }
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
            EditorParameters.Set("HubRadius", HubRadius.ToString());
            EditorParameters.Set("HubThickness", HubThickness.ToString());
            EditorParameters.Set("RimInnerRadius", RimInnerRadius.ToString());
            EditorParameters.Set("RimThickness", RimThickness.ToString());
            EditorParameters.Set("RimDepth", RimDepth.ToString());
            EditorParameters.Set("NumberOfSpokes", NumberOfSpokes.ToString());
            EditorParameters.Set("SpokeRadius", SpokeRadius.ToString());
            EditorParameters.Set("AxleBore", AxleBore.ToString());
        }

        private void SetDefaults()
        {
            HubRadius = 2;
            HubThickness = 3;
            RimInnerRadius = 10;
            RimThickness = 3;
            RimDepth = 2;
            NumberOfSpokes = 6;
            SpokeRadius = 2;
            AxleBore = 2;
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
            SetDefaults();
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            warningText = "";
            loaded = true;
            UpdateDisplay();
        }
    }
}