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
using MakerLib.TextureUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TexturedTube.xaml
    /// </summary>
    public partial class TexturedTubeDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxinnerRadius = 200;
        private const double maxsweep = 360;
        private const double maxtextureDepth = 10;
        private const double maxtextureResolution = 1;
        private const double maxthickness = 50;
        private const double maxtubeHeight = 200;
        private const double mininnerRadius = 1;
        private const double minsweep = 1;
        private const double mintextureDepth = 0.1;
        private const double mintextureResolution = 0.1;
        private const double minthickness = 1;
        private const double mintubeHeight = 1;
        private bool clippedSingle;
        private bool clippedTile;
        private bool doubleTube;
        private bool fittedSingle;
        private bool fittedTile;
        private double innerRadius;
        private bool innerTube;
        private bool loaded;
        private bool outerTube;
        private Visibility showThickness;
        private bool solid;
        private double sweep;
        private string texture;
        private double textureDepth;
        private TextureManager textureManager;
        private double textureResolution;
        private double thickness;
        private double tubeHeight;
        private string warningText;

        public TexturedTubeDlg()
        {
            InitializeComponent();
            ToolName = "TexturedTube";
            DataContext = this;

            loaded = false;
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureNames();
            textureManager.Mode = TextureManager.MapMode.ClippedTile;
        }

        public bool ClippedSingle
        {
            get
            {
                return clippedSingle;
            }
            set
            {
                if (clippedSingle != value)
                {
                    clippedSingle = value;
                    NotifyPropertyChanged();
                    if (clippedSingle)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.ClippedSingle;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool ClippedTile
        {
            get
            {
                return clippedTile;
            }
            set
            {
                if (clippedTile != value)
                {
                    clippedTile = value;
                    NotifyPropertyChanged();
                    if (clippedTile)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.ClippedTile;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool DoubleTube
        {
            get
            {
                return doubleTube;
            }
            set
            {
                if (doubleTube != value)
                {
                    doubleTube = value;
                    NotifyPropertyChanged();
                    if (doubleTube)
                    {
                        solid = false;
                        innerTube = false;
                        outerTube = false;
                        ShowThickness = Visibility.Visible;
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool FittedSingle
        {
            get
            {
                return fittedSingle;
            }
            set
            {
                if (fittedSingle != value)
                {
                    fittedSingle = value;
                    NotifyPropertyChanged();
                    if (fittedSingle)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.FittedSingle;
                        }
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool FittedTile
        {
            get
            {
                return fittedTile;
            }
            set
            {
                if (fittedTile != value)
                {
                    fittedTile = value;
                    NotifyPropertyChanged();
                    if (fittedTile)
                    {
                        if (textureManager != null)
                        {
                            textureManager.Mode = TextureManager.MapMode.FittedTile;
                        }
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
                    if (value >= mininnerRadius && value <= maxinnerRadius)
                    {
                        innerRadius = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String InnerRadiusToolTip
        {
            get
            {
                return $"InnerRadius must be in the range {mininnerRadius} to {maxinnerRadius}";
            }
        }

        public bool InnerTube
        {
            get
            {
                return innerTube;
            }
            set
            {
                if (innerTube != value)
                {
                    innerTube = value;
                    NotifyPropertyChanged();
                    if (innerTube)
                    {
                        solid = false;
                        outerTube = false;
                        doubleTube = false;
                        ShowThickness = Visibility.Visible;
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool OuterTube
        {
            get
            {
                return outerTube;
            }
            set
            {
                if (outerTube != value)
                {
                    outerTube = value;
                    NotifyPropertyChanged();
                    if (outerTube)
                    {
                        solid = false;
                        innerTube = false;
                        doubleTube = false;
                        ShowThickness = Visibility.Visible;
                        UpdateDisplay();
                    }
                }
            }
        }

        public Visibility ShowThickness
        {
            get
            {
                return showThickness;
            }
            set
            {
                if (value != showThickness)
                {
                    showThickness = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Solid
        {
            get
            {
                return solid;
            }
            set
            {
                if (solid != value)
                {
                    solid = value;
                    NotifyPropertyChanged();
                    if (solid)
                    {
                        outerTube = false;
                        innerTube = false;
                        ShowThickness = Visibility.Hidden;
                        UpdateDisplay();
                    }
                    else
                    {
                        ShowThickness = Visibility.Visible;
                    }
                }
            }
        }

        public String SolidToolTip
        {
            get
            {
                return "If Solid is true a disk is made, if not a tube is made";
            }
        }

        public double Sweep
        {
            get
            {
                return sweep;
            }
            set
            {
                if (sweep != value)
                {
                    if (value >= minsweep && value <= maxsweep)
                    {
                        sweep = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String SweepToolTip
        {
            get
            {
                return $"Sweep must be in the range {minsweep} to {maxsweep}";
            }
        }

        public string Texture
        {
            get
            {
                return texture;
            }
            set
            {
                if (texture != value)
                {
                    texture = value;
                    NotifyPropertyChanged();
                    if (!String.IsNullOrEmpty(texture))
                    {
                        UpdateDisplay();
                    }
                }
            }
        }

        public double TextureDepth
        {
            get
            {
                return textureDepth;
            }
            set
            {
                if (textureDepth != value)
                {
                    if (value >= mintextureDepth && value <= maxtextureDepth)
                    {
                        textureDepth = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TextureDepthToolTip
        {
            get
            {
                return $"TextureDepth must be in the range {mintextureDepth} to {maxtextureDepth}";
            }
        }

        public List<String> TextureItems
        {
            get
            {
                return textureManager.TextureNames;
            }
        }

        public double TextureResolution
        {
            get
            {
                return textureResolution;
            }
            set
            {
                if (textureResolution != value)
                {
                    if (value >= mintextureResolution && value <= maxtextureResolution)
                    {
                        textureResolution = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TextureResolutionToolTip
        {
            get
            {
                return $"Texture Resolution must be in the range {mintextureResolution} to {maxtextureResolution}";
            }
        }

        public String textureToolTip
        {
            get
            {
                return "Texture Text";
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
                    if (value >= minthickness && value <= maxthickness)
                    {
                        thickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String ThicknessToolTip
        {
            get
            {
                return $"Thickness must be in the range {minthickness} to {maxthickness}";
            }
        }

        public double TubeHeight
        {
            get
            {
                return tubeHeight;
            }
            set
            {
                if (tubeHeight != value)
                {
                    if (value >= mintubeHeight && value <= maxtubeHeight)
                    {
                        tubeHeight = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                }
            }
        }

        public String TubeHeightToolTip
        {
            get
            {
                return $"TubeHeight must be in the range {mintubeHeight} to {maxtubeHeight}";
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

        private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            if (solid)
            {
                TexturedDiskMaker diskmaker = new TexturedDiskMaker(tubeHeight, innerRadius, sweep, texture, textureDepth, textureResolution);
                diskmaker.Generate(v1, i1);
            }
            else
            {
                int sideMask = 0;
                if (outerTube)
                {
                    sideMask += 1;
                }
                if (innerTube)
                {
                    sideMask += 2;
                }
                if (doubleTube)
                {
                    sideMask = 3;
                }
                TexturedTubeMaker tubemaker = new TexturedTubeMaker(tubeHeight, innerRadius, thickness, sweep, texture, textureDepth, textureResolution, sideMask);
                tubemaker.Generate(v1, i1);
            }

            AsyncGeneratorResult res = new AsyncGeneratorResult();
            // extract the vertices and indices to thread safe arrays
            // while still in the async function
            res.points = new Point3D[v1.Count];
            for (int i = 0; i < v1.Count; i++)
            {
                res.points[i] = new Point3D(v1[i].X, v1[i].Y, v1[i].Z);
            }
            res.indices = new int[i1.Count];
            for (int i = 0; i < i1.Count; i++)
            {
                res.indices[i] = i1[i];
            }
            v1.Clear();
            i1.Clear();
            return (res);
        }

        private async void GenerateShape()
        {
            ClearShape();
            Viewer.Busy();
            EditingEnabled = false;
            AsyncGeneratorResult result;
            result = await Task.Run(() => GenerateAsync());
            GetVerticesFromAsyncResult(result);
            CentreVertices();
            Viewer.NotBusy();
            EditingEnabled = true;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            TubeHeight = EditorParameters.GetDouble("TubeHeight", 20);
            InnerRadius = EditorParameters.GetDouble("InnerRadius", 10);
            Thickness = EditorParameters.GetDouble("Thickness", 5);
            Solid = EditorParameters.GetBoolean("Solid", true);
            OuterTube = EditorParameters.GetBoolean("OuterTube", false);
            InnerTube = EditorParameters.GetBoolean("InnerTube", false);
            DoubleTube = EditorParameters.GetBoolean("DoubleTube", false); ;
            Sweep = EditorParameters.GetDouble("Sweep", 360);
            Texture = EditorParameters.Get("Texture");
            TextureDepth = EditorParameters.GetDouble("TextureDepth", 0.5);
            TextureResolution = EditorParameters.GetDouble("TextureResolution", 0.5);
            ClippedTile = EditorParameters.GetBoolean("ClippedTile", true);
            FittedTile = EditorParameters.GetBoolean("FittedTile", false);
            ClippedSingle = EditorParameters.GetBoolean("ClippedSingle", false);
            FittedSingle = EditorParameters.GetBoolean("FittedSingle", false);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("TubeHeight", TubeHeight.ToString());
            EditorParameters.Set("InnerRadius", InnerRadius.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("Solid", Solid.ToString());
            EditorParameters.Set("InnerTube", InnerTube.ToString());
            EditorParameters.Set("OuterTube", OuterTube.ToString());
            EditorParameters.Set("DoubleTube", DoubleTube.ToString());
            EditorParameters.Set("Sweep", Sweep.ToString());
            EditorParameters.Set("Texture", Texture.ToString());
            EditorParameters.Set("TextureDepth", TextureDepth.ToString());
            EditorParameters.Set("TextureResolution", TextureResolution.ToString());
            EditorParameters.Set("ClippedTile", ClippedTile.ToString());
            EditorParameters.Set("FittedTile", FittedTile.ToString());
            EditorParameters.Set("ClippedSingle", ClippedSingle.ToString());
            EditorParameters.Set("FittedSingle", FittedSingle.ToString());
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
            NotifyPropertyChanged("Solid");
            UpdateDisplay();
        }
    }
}