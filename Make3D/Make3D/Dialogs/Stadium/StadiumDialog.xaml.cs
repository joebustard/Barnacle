﻿// **************************************************************************
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for Stadium.xaml
    /// </summary>
    public partial class StadiumDialog : BaseModellerDialog, INotifyPropertyChanged
    {
        private double gap;
        private double height;
        private bool loaded;
        private double radius1;
        private double radius2;
        private List<string> shapes;
        private string shapeStyle;

        public StadiumDialog()
        {
            InitializeComponent();
            ToolName = "Stadium";
            DataContext = this;
            Radius1 = 5;
            Radius2 = 10;
            Gap = 10;
            height = 10;
            shapes = new List<string>();
            shapes.Add("Flat");
            shapes.Add("Sausage");
            NotifyPropertyChanged("Shapes");
            ShapeStyle = "Flat";
        }

        public double Gap
        {
            get
            {
                return gap;
            }
            set
            {
                if (value != gap)
                {
                    gap = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius1
        {
            get
            {
                return radius1;
            }
            set
            {
                if (value != radius1)
                {
                    radius1 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double Radius2
        {
            get
            {
                return radius2;
            }
            set
            {
                if (value != radius2)
                {
                    radius2 = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeHeight
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    if (value > 0)
                    {
                        height = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public List<string> Shapes
        {
            get
            {
                return shapes;
            }
            set
            {
                if (value != shapes)
                {
                    shapes = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string ShapeStyle
        {
            get
            {
                return shapeStyle;
            }
            set
            {
                if (value != shapeStyle)
                {
                    shapeStyle = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
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
                    UpdateDisplay();
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
                    UpdateDisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParameters();

            DialogResult = true;
            Close();
        }

        private void Generate()
        {
            if (ShapeStyle != null && ShapeStyle != "")
            {
                StadiumMaker stadiumMaker = new StadiumMaker(ShapeStyle, radius1, radius2, gap, height);
                stadiumMaker.Generate(Vertices, Faces);
            }
        }

        private void SaveEditorParameters()
        {
            EditorParameters.Set("Radius1", radius1.ToString());
            EditorParameters.Set("Radius2", radius2.ToString());
            EditorParameters.Set("Gap", gap.ToString());
            EditorParameters.Set("Height", height.ToString());
            EditorParameters.Set("Shape", shapeStyle);
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                Generate();
                Viewer.Model = GetModel();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCameraPos();
            loaded = false;
            string s = EditorParameters.Get("Shape");
            if (s != "")
            {
                ShapeStyle = s;
                Radius1 = EditorParameters.GetDouble("Radius1");
                Radius2 = EditorParameters.GetDouble("Radius2");
                Gap = EditorParameters.GetDouble("Gap");
                ShapeHeight = EditorParameters.GetDouble("Height");
                ShapeStyle = s;
            }
            loaded = true;
            UpdateDisplay();
        }
    }
}