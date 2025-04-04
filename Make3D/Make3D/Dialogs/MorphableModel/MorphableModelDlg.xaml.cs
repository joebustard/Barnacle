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

using MakerLib.MorphableModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for MorphableModel.xaml
    /// </summary>
    public partial class MorphableModelDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxmodelHeight = 200;
        private const double maxmodelLength = 200;
        private const double maxmodelWidth = 200;
        private const double maxwarpFactor = 1;
        private const double minmodelHeight = 1;
        private const double minmodelLength = 1;
        private const double minmodelWidth = 1;
        private const double minwarpFactor = 0;
        private String factorText;
        private bool loaded;
        private MorphableModelMaker maker;
        private double modelHeight;
        private double modelLength;
        private double modelWidth;
        private double oldWarpFactor;
        private DispatcherTimer regenTimer;
        private string shape1;
        private ObservableCollection<String> shape1Items;
        private string shape2;
        private ObservableCollection<String> shape2Items;
        private string warningText;
        private double warpFactor;

        public MorphableModelDlg()
        {
            InitializeComponent();
            shape1Items = CreateShapeList();
            shape2Items = CreateShapeList();
            maker = new MorphableModelMaker(shape1, shape2);

            ToolName = "Morphable";
            DataContext = this;
            loaded = false;
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, 5);
            regenTimer.Tick += RegenTick;
        }

        public String FactorText
        {
            get
            {
                return factorText;
            }

            set
            {
                if (value != factorText)
                {
                    factorText = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public double ModelHeight
        {
            get
            {
                return modelHeight;
            }

            set
            {
                if (modelHeight != value)
                {
                    if (value >= minmodelHeight && value <= maxmodelHeight)
                    {
                        modelHeight = value;
                        oldWarpFactor = -1;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ModelHeightToolTip
        {
            get
            {
                return $"ModelHeight must be in the range {minmodelHeight} to {maxmodelHeight}";
            }
        }

        public double ModelLength
        {
            get
            {
                return modelLength;
            }

            set
            {
                if (modelLength != value)
                {
                    if (value >= minmodelLength && value <= maxmodelLength)
                    {
                        modelLength = value;
                        oldWarpFactor = -1;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ModelLengthToolTip
        {
            get
            {
                return $"ModelLength must be in the range {minmodelLength} to {maxmodelLength}";
            }
        }

        public double ModelWidth
        {
            get
            {
                return modelWidth;
            }

            set
            {
                if (modelWidth != value)
                {
                    if (value >= minmodelWidth && value <= maxmodelWidth)
                    {
                        modelWidth = value;
                        oldWarpFactor = -1;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ModelWidthToolTip
        {
            get
            {
                return $"ModelWidth must be in the range {minmodelWidth} to {maxmodelWidth}";
            }
        }

        public string Shape1
        {
            get
            {
                return shape1;
            }

            set
            {
                if (shape1 != value)
                {
                    shape1 = value;
                    if (maker != null)
                    {
                        maker.Shape1 = shape1;
                        oldWarpFactor = -1;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> Shape1Items
        {
            get
            {
                return shape1Items;
            }

            set
            {
                if (shape1Items != value)
                {
                    shape1Items = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String shape1ToolTip
        {
            get
            {
                return "Shape1 Text";
            }
        }

        public string Shape2
        {
            get
            {
                return shape2;
            }

            set
            {
                if (shape2 != value)
                {
                    shape2 = value;
                    if (maker != null)
                    {
                        maker.Shape2 = shape2;
                        oldWarpFactor = -1;
                    }
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<String> Shape2Items
        {
            get
            {
                return shape2Items;
            }

            set
            {
                if (shape2Items != value)
                {
                    shape2Items = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String shape2ToolTip
        {
            get
            {
                return "Shape2 Text";
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

        public double WarpFactor
        {
            get
            {
                return warpFactor;
            }

            set
            {
                if (warpFactor != value)
                {
                    if (value >= minwarpFactor && value <= maxwarpFactor)
                    {
                        warpFactor = value;
                        double v = warpFactor * 100;
                        FactorText = v.ToString("F1");
                        if (Math.Abs(oldWarpFactor - warpFactor) >= 0.1)
                        {
                            StartRegen();
                        }
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (regenTimer.IsEnabled)
            {
                regenTimer.Stop();
                UpdateDisplay();
            }
            else
            {
                base.SaveSizeAndLocation();
                SaveEditorParmeters();
                DialogResult = true;
                Close();
            }
        }

        private ObservableCollection<string> CreateShapeList()
        {
            ObservableCollection<string> res = new ObservableCollection<string>();
            res.Add("Cone");
            res.Add("Cylinder");
            res.Add("Cube");
            res.Add("Dice");
            res.Add("HexCone");
            res.Add("Octahedron");
            res.Add("Pyramid");
            res.Add("Pyramid2");
            res.Add("Roof");
            res.Add("RoundRoof");
            res.Add("Sphere");
            res.Add("Star6");
            res.Add("StellateDoDec");
            return res;
        }

        private void GenerateShape()
        {
            if (Math.Abs(warpFactor - oldWarpFactor) > 0.01)
            {
                ClearShape();
                maker.Generate(warpFactor, Vertices, Faces);
                ScaleVertices(modelLength, modelHeight, modelWidth);
                CentreVertices();
                oldWarpFactor = warpFactor;
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            ModelLength = EditorParameters.GetDouble("ModelLength", 40);
            ModelHeight = EditorParameters.GetDouble("ModelHeight", 40);
            ModelWidth = EditorParameters.GetDouble("ModelWidth", 40);
            WarpFactor = EditorParameters.GetDouble("WarpFactor", 0.5);
            Shape1 = EditorParameters.Get("Shape1");
            if (Shape1 == "")
            {
                Shape1 = "Cube";
            }
            Shape2 = EditorParameters.Get("Shape2");
        }

        private void RegenTick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            UpdateDisplay();
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("ModelLength", ModelLength.ToString());
            EditorParameters.Set("ModelHeight", ModelHeight.ToString());
            EditorParameters.Set("ModelWidth", ModelWidth.ToString());
            EditorParameters.Set("WarpFactor", WarpFactor.ToString());
            EditorParameters.Set("Shape1", Shape1.ToString());
            EditorParameters.Set("Shape2", Shape2.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            ModelLength = 40;
            ModelHeight = 40;
            ModelWidth = 40;
            WarpFactor = 0.5;
            Shape1 = "Cube";
            Shape2 = "Sphere";
            loaded = true;
        }

        private void StartRegen()
        {
            regenTimer.Stop();
            regenTimer.Start();
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
            oldWarpFactor = -1;
            LoadEditorParameters();
            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            UpdateDisplay();
        }
    }
}