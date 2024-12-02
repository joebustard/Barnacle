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
    /// Interaction logic for Trickle.xaml
    /// </summary>
    public partial class TrickleDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private double radius;
        private double side;
        private double thickness;
        private string warningText;

        public TrickleDlg()
        {
            InitializeComponent();
            ToolName = "Trickle";
            DataContext = this;

            loaded = false;
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

        public double Side
        {
            get
            {
                return side;
            }
            set
            {
                if (side != value)
                {
                    if (value >= 1 && value <= 200)
                    {
                        side = value;
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
            TrickleMaker maker = new TrickleMaker(
                radius, side, thickness
                );
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters

            if (EditorParameters.Get("Radius") != "")
            {
                Radius = EditorParameters.GetDouble("Radius");
            }

            if (EditorParameters.Get("Side") != "")
            {
                Side = EditorParameters.GetDouble("Side");
            }

            if (EditorParameters.Get("Thickness") != "")
            {
                Thickness = EditorParameters.GetDouble("Thickness");
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("Radius", Radius.ToString());
            EditorParameters.Set("Side", Side.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
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
            Side = 20;
            Thickness = 5;
            Radius = 5;
            LoadEditorParameters();

            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}