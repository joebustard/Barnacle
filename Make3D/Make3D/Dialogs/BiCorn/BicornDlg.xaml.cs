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
    /// Interaction logic for Bicorn.xaml
    /// </summary>
    public partial class BicornDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool doubleup;
        private bool loaded;
        private double radius1;
        private double radius2;
        private double shapeHeight;
        private string warningText;

        public BicornDlg()
        {
            InitializeComponent();
            ToolName = "Bicorn";
            DataContext = this;
            ModelGroup = MyModelGroup;
            Radius1 = 5;
            Radius2 = 10;
            DoubleUp = false;
            shapeHeight = 10;
        }

        public bool DoubleUp
        {
            get
            {
                return doubleup;
            }

            set
            {
                if (value != doubleup)
                {
                    doubleup = value;
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
                return shapeHeight;
            }

            set
            {
                if (shapeHeight != value)
                {
                    if (value > 0)
                    {
                        shapeHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
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
            base.SaveSizeAndLocation();
            DialogResult = true;
            Close();
        }

        private void GenerateShape()
        {
            ClearShape();
            BicornMaker maker = new BicornMaker(radius1, radius2, shapeHeight, doubleup);
            maker.Generate(Vertices, Faces);
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            Radius1 = EditorParameters.GetDouble("Radius1", 5);
            Radius2 = EditorParameters.GetDouble("Radius2", 10);
            DoubleUp = EditorParameters.GetBoolean("DoubleUp", false);
            ShapeHeight = EditorParameters.GetDouble("ShapeHeight", 10);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("Radius1", Radius1.ToString());
            EditorParameters.Set("Radius2", Radius2.ToString());
            EditorParameters.Set("DoubleUp", DoubleUp.ToString());
            EditorParameters.Set("ShapeHeight", ShapeHeight.ToString());
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                GenerateShape();
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            LoadEditorParameters();
            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            warningText = "";
            loaded = true;
            UpdateDisplay();
        }
    }
}