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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic forSquaredStadiumDlg.xaml
    /// </summary>
    public partial class SquaredStadiumDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private List<String> overRuns;
        private double shapeHeight;
        private double shapeLength;
        private String shapeOverRun;
        private double shapeRadius;

        private double squaredEndSize;
        private string warningText;

        public SquaredStadiumDlg()
        {
            InitializeComponent();
            ToolName = "SquaredStadium";
            DataContext = this;
            ModelGroup = MyModelGroup;
            shapeLength = 5;
            shapeHeight = 10;
            shapeRadius = 5;
            squaredEndSize = 10;
            overRuns = new List<string>();
            overRuns.Add("0");
            overRuns.Add("5");
            overRuns.Add("10");
            overRuns.Add("15");
            overRuns.Add("20");
            overRuns.Add("25");
            overRuns.Add("30");
            shapeOverRun = "0";
        }

        public List<String> OverRuns
        {
            get { return overRuns; }
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

        public String ShapeOverRun
        {
            get
            {
                return shapeOverRun;
            }
            set
            {
                if (shapeOverRun != value)
                {
                    shapeOverRun = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeRadius
        {
            get
            {
                return shapeRadius;
            }
            set
            {
                if (shapeRadius != value)
                {
                    shapeRadius = value;
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

        public double SquaredEndSize
        {
            get
            {
                return squaredEndSize;
            }
            set
            {
                if (squaredEndSize != value)
                {
                    squaredEndSize = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
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
            double overRun = Convert.ToDouble(shapeOverRun);
            SquaredStadiumMaker stadiumMaker = new SquaredStadiumMaker(shapeRadius, shapeLength, squaredEndSize, shapeHeight, overRun);
            stadiumMaker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            if (EditorParameters.Get("ShapeHeight") != "")
            {
                ShapeHeight = EditorParameters.GetDouble("ShapeHeight");
                ShapeLength = EditorParameters.GetDouble("ShapeLength");
                ShapeOverRun = EditorParameters.Get("ShapeOverRun");
                ShapeRadius = EditorParameters.GetDouble("ShapeRadius");
                SquaredEndSize = EditorParameters.GetDouble("SquaredEndSize");
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ShapeHeight", ShapeHeight.ToString());
            EditorParameters.Set("ShapeLength", ShapeLength.ToString());
            EditorParameters.Set("ShapeOverRun", ShapeOverRun);
            EditorParameters.Set("ShapeRadius", ShapeRadius.ToString());
            EditorParameters.Set("SquaredEndSize", SquaredEndSize.ToString());
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