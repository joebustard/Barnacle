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
    /// Interaction logic for TrapezoidDlg.xaml
    /// </summary>
    public partial class TrapezoidDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private bool loaded;
        private double shapeBevel;
        private double shapeBottomLength;
        private double shapeHeight;
        private double shapeTopLength;
        private double shapeWidth;
        private string warningText;

        public TrapezoidDlg()
        {
            InitializeComponent();
            ToolName = "Trapezoid";
            DataContext = this;
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

        public double ShapeBottomLength
        {
            get
            {
                return shapeBottomLength;
            }
            set
            {
                if (shapeBottomLength != value)
                {
                    shapeBottomLength = value;
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
                    shapeHeight = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public double ShapeTopLength
        {
            get
            {
                return shapeTopLength;
            }
            set
            {
                if (shapeTopLength != value)
                {
                    shapeTopLength = value;
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

        public void Reset()
        {
            loaded = false;
            ShapeTopLength = 20;
            ShapeBottomLength = 16;
            ShapeHeight = 10;
            ShapeWidth = 10;
            loaded = true;
            ShapeBevel = 0;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private double BevelLimit()
        {
            double min = shapeTopLength / 2 - 1;
            if (shapeHeight / 2 - 1 < min)
            {
                min = shapeHeight / 2 - 1;
            }
            if (shapeBottomLength / 2 - 1 < min)
            {
                min = shapeBottomLength / 2 - 1;
            }
            return min;
        }

        private void GenerateShape()
        {
            ClearShape();
            TrapezoidMaker pgram = new TrapezoidMaker(shapeTopLength, shapeBottomLength, shapeHeight, shapeWidth, shapeBevel);
            pgram.Generate(Vertices, Faces);
            CentreVertices();
            FloorVertices();
        }

        private void LoadEditorParameters()
        {
            ShapeTopLength = EditorParameters.GetDouble("ShapeTopLength", 20);
            ShapeBottomLength = EditorParameters.GetDouble("ShapeBottomLength", 16);
            ShapeWidth = EditorParameters.GetDouble("ShapeWidth", 10);
            ShapeHeight = EditorParameters.GetDouble("ShapeHeight", 10);
            ShapeBevel = EditorParameters.GetDouble("ShapeBevel", 0);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            Reset();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("ShapeTopLength", shapeTopLength.ToString());
            EditorParameters.Set("ShapeHeight", shapeHeight.ToString());
            EditorParameters.Set("ShapeWidth", shapeWidth.ToString());
            EditorParameters.Set("ShapeBottomLength", shapeBottomLength.ToString());
            EditorParameters.Set("ShapeBevel", shapeBevel.ToString());
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
            loaded = false;
            LoadEditorParameters();

            UpdateCameraPos();
            Viewer.Clear();
            warningText = "";
            loaded = true;
            UpdateDisplay();
        }
    }
}