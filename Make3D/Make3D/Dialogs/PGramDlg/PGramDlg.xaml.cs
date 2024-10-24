﻿/**************************************************************************
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
    /// Interaction logic for PGramDlg.xaml
    /// </summary>
    public partial class PGramDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private double shapeAngle;
        private double shapeBevel;
        private double shapeHeight;
        private double shapeLength;
        private double shapeWidth;
        private string warningText;

        public PGramDlg()
        {
            InitializeComponent();
            ToolName = "PGram";
            DataContext = this;
            ModelGroup = MyModelGroup;
            shapeAngle = 45;
            shapeLength = 20;
            shapeHeight = 10;
            shapeWidth = 10;
            shapeBevel = 0;
        }

        public double ShapeAngle
        {
            get
            {
                return shapeAngle;
            }

            set
            {
                if (shapeAngle != value)
                {
                    if (value < 1 || value > 89)
                    {
                        WarningText = "Angle must be in range 1 to 89";
                    }
                    else
                    {
                        WarningText = "";
                        shapeAngle = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public double ShapeBevel
        {
            get
            {
                return shapeBevel;
            }

            set
            {
                if (shapeBevel != value)
                {
                    if (shapeBevel < 0 || shapeBevel > BevelLimit())
                    {
                        WarningText = "Bevel must be in range 0 to " + BevelLimit().ToString();
                    }
                    else
                    {
                        shapeBevel = value;
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
                    shapeHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeLength
        {
            get
            {
                return shapeLength;
            }

            set
            {
                if (shapeLength != value)
                {
                    shapeLength = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeWidth
        {
            get
            {
                return shapeWidth;
            }

            set
            {
                if (shapeWidth != value)
                {
                    shapeWidth = value;
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
                    Redisplay();
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
                    Redisplay();
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

        private double BevelLimit()
        {
            double min = shapeLength / 2 - 1;
            if (shapeHeight / 2 - 1 < min)
            {
                min = shapeHeight / 2 - 1;
            }
            return min;
        }

        private void GenerateShape()
        {
            ClearShape();
            PGramMaker pgram = new PGramMaker(shapeLength, shapeHeight, shapeWidth, shapeAngle, shapeBevel);
            pgram.Generate(Vertices, Faces);
            CentreVertices();
            FloorVertices();
        }

        private void LoadEditorParameters()
        {
            string s = EditorParameters.Get("ShapeLength");
            if (s != "")
            {
                shapeLength = EditorParameters.GetDouble("ShapeLength");
                shapeWidth = EditorParameters.GetDouble("ShapeWidth");
                shapeHeight = EditorParameters.GetDouble("ShapeHeight");
                shapeAngle = EditorParameters.GetDouble("ShapeAngle");
                shapeBevel = EditorParameters.GetDouble("ShapeBevel");
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ShapeLength", shapeLength.ToString());
            EditorParameters.Set("ShapeHeight", shapeHeight.ToString());
            EditorParameters.Set("ShapeWidth", shapeWidth.ToString());
            EditorParameters.Set("ShapeAngle", shapeAngle.ToString());
            EditorParameters.Set("ShapeBevel", shapeBevel.ToString());
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            Redisplay();
        }
    }
}