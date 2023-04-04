using MakerLib.TextureUtils;
using MakerLib;

using MakerLib.TextureUtils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

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
        private double innerRadius;
        private bool loaded;
        private bool solid;
        private bool outerTube;
        private bool innerTube;
        private double sweep;
        private string texture;
        private double textureDepth;
        private ObservableCollection<String> textureItems;
        private double textureResolution;
        private double thickness;
        private double tubeHeight;
        private string warningText;
        private TextureManager textureManager;

        public TexturedTubeDlg()
        {
            InitializeComponent();
            ToolName = "TexturedTube";
            DataContext = this;
            ModelGroup = MyModelGroup;
            loaded = false;
            textureManager = TextureManager.Instance();
            textureManager.LoadTextureNames();
            textureManager.Mode = TextureManager.MapMode.ClippedTile;
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

        private bool clippedTile;

        public bool ClippedTile
        {
            get { return clippedTile; }
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

        private bool clippedSingle;

        public bool ClippedSingle
        {
            get { return clippedSingle; }
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

        private bool fittedSingle;

        public bool FittedSingle
        {
            get { return fittedSingle; }
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

        private bool fittedTile;

        public bool FittedTile
        {
            get { return fittedTile; }
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
                        UpdateDisplay();
                    }
                }
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
                        UpdateDisplay();
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
            get { return texture; }
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
            get { return textureManager.TextureNames; }
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
            get { return "Texture Text"; }
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

        private Visibility showThickness;

        public Visibility ShowThickness
        {
            get { return showThickness; }
            set
            {
                if (value != showThickness)
                {
                    showThickness = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void GenerateShape()
        {
            ClearShape();
            if (solid)
            {
                TexturedDiskMaker diskmaker = new TexturedDiskMaker(tubeHeight, innerRadius, sweep, texture, textureDepth, textureResolution);
                diskmaker.Generate(Vertices, Faces);
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
                TexturedTubeMaker tubemaker = new TexturedTubeMaker(tubeHeight, innerRadius, thickness, sweep, texture, textureDepth, textureResolution, sideMask);
                tubemaker.Generate(Vertices, Faces);
            }

            CentreVertices();
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            TubeHeight = EditorParameters.GetDouble("TubeHeight", 20);
            InnerRadius = EditorParameters.GetDouble("InnerRadius", 10);
            Thickness = EditorParameters.GetDouble("Thickness", 5);
            Solid = EditorParameters.GetBoolean("Solid", true);
            OuterTube = EditorParameters.GetBoolean("OuterTube", false);
            InnerTube = EditorParameters.GetBoolean("InnerTube", false); ;
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
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
            loaded = true;
            NotifyPropertyChanged("Solid");
            UpdateDisplay();
        }
    }
}