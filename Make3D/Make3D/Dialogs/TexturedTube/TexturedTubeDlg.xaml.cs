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
        private bool tube;
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
                        UpdateDisplay();
                    }
                }
            }
        }

        public bool Tube
        {
            get
            {
                return tube;
            }
            set
            {
                if (tube != value)
                {
                    tube = value;
                    NotifyPropertyChanged();
                    if (tube)
                    {
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
                TexturedTubeMaker tubemaker = new TexturedTubeMaker(tubeHeight, innerRadius, thickness, sweep, texture, textureDepth, textureResolution);
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

            Solid = EditorParameters.GetBoolean("Solid", false);
            Tube = !Solid;
            Sweep = EditorParameters.GetDouble("Sweep", 360);

            Texture = EditorParameters.Get("Texture");

            TextureDepth = EditorParameters.GetDouble("TextureDepth", 0.5);

            TextureResolution = EditorParameters.GetDouble("TextureResolution", 0.5);
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool

            EditorParameters.Set("TubeHeight", TubeHeight.ToString());
            EditorParameters.Set("InnerRadius", InnerRadius.ToString());
            EditorParameters.Set("Thickness", Thickness.ToString());
            EditorParameters.Set("Solid", Solid.ToString());
            EditorParameters.Set("Sweep", Sweep.ToString());
            EditorParameters.Set("Texture", Texture.ToString());
            EditorParameters.Set("TextureDepth", TextureDepth.ToString());
            EditorParameters.Set("TextureResolution", TextureResolution.ToString());
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