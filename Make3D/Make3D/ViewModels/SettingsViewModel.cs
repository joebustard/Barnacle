using Barnacle.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace Barnacle.ViewModels
{
    internal class SettingsViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private bool autoSaveScript;
        private AvailableColour objectColour;
        private bool clearPreviousVersionsOnExport;
        private Color defaultObjectColour;
        private string description;
        private bool exportEmptyDocs;
        private string exportScale;
        private bool floorAll;
        private bool importSwapAxis;
        private string rotX;
        private List<AvailableColour> availableColours;
        private string rotY;

        private string rotZ;

        private List<String> scales;

        private string selectedScale;

        private bool swapAxis;

        private bool versionExport;
        private string slicerPath;
        public string SlicerPath
        {
            get { return slicerPath; }
            set
            {
                if (value != slicerPath)
                {
                    slicerPath = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public SettingsViewModel()
        {
            AutoSaveScript = Project.SharedProjectSettings.AutoSaveScript;
            Description = Project.SharedProjectSettings.Description;
            Scales = ModelScales.ScaleNames();
            SelectedScale = Project.SharedProjectSettings.BaseScale;
            ExportScale = Project.SharedProjectSettings.ExportScale;
            RotX = Project.SharedProjectSettings.ExportRotation.X.ToString("F3");
            RotY = Project.SharedProjectSettings.ExportRotation.Y.ToString("F3");
            RotZ = Project.SharedProjectSettings.ExportRotation.Z.ToString("F3");
            SwapAxis = Project.SharedProjectSettings.ExportAxisSwap;
            ImportSwapAxis = Project.SharedProjectSettings.ImportAxisSwap;
            FloorAll = Project.SharedProjectSettings.FloorAll;
            VersionExport = Project.SharedProjectSettings.VersionExport;
            ClearPreviousVersionsOnExport = Project.SharedProjectSettings.ClearPreviousVersionsOnExport;
            IgnoreEmpty = !Project.SharedProjectSettings.ExportEmptyFiles;
            DefaultObjectColour = Project.SharedProjectSettings.DefaultObjectColour;
            SlicerPath = Project.SharedProjectSettings.SlicerPath;
            SetAvailableColours();
            ObjectColour = FindAvailableColour(DefaultObjectColour);
        }

        private AvailableColour FindAvailableColour(Color color)
        {
            AvailableColour res = null;
            foreach (AvailableColour cl in AvailableColours)
            {
                if (cl.Colour.A == color.A &&
                cl.Colour.R == color.R &&
                cl.Colour.G == color.G &&
                cl.Colour.B == color.B)
                {
                    res = cl;
                    break;
                }
            }
            return res;
        }
        private void SetAvailableColours()
        {
            string[] ignore =
            {
        "AliceBlue",
        "Azure",
        "Beige",
        "Cornsilk",
        "Ivory",
        "GhostWhite",
        "LavenderBlush",
        "LightYellow",
        "Linen",
        "MintCream",
        "OldLace",
        "SeaShell",
        "Snow",
        "WhiteSmoke",
        "Transparent"
        };
            List<AvailableColour> cls = new List<AvailableColour>();
            Type colors = typeof(System.Drawing.Color);
            PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public |
                BindingFlags.Static);
            foreach (PropertyInfo info in colorInfo)
            {
                var result = Array.Find(ignore, element => element == info.Name);
                if (result == null || result == String.Empty)
                {
                    cls.Add(new AvailableColour(info.Name));
                }
            }
            AvailableColours = cls;
        }


        public List<AvailableColour> AvailableColours
        {
            get { return availableColours; }
            set
            {
                if (availableColours != value)
                {
                    availableColours = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public AvailableColour ObjectColour
        {
            get
            {
                return objectColour;
            }
            set
            {
                if (objectColour != value)
                {
                    objectColour = value;
                    System.Drawing.Color tmp = System.Drawing.Color.FromName(objectColour.Name);
                    DefaultObjectColour = System.Windows.Media.Color.FromArgb(tmp.A, tmp.R, tmp.G, tmp.B);
                    NotifyPropertyChanged();
                  
                }
            }
        }


        public bool AutoSaveScript
        {
            get
            {
                return autoSaveScript;
            }
            set
            {
                if (autoSaveScript != value)
                {
                    autoSaveScript = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ClearPreviousVersionsOnExport
        {
            get
            {
                return clearPreviousVersionsOnExport;
            }
            set
            {
                if (clearPreviousVersionsOnExport != value)
                {
                    clearPreviousVersionsOnExport = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Color DefaultObjectColour
        {
            get
            {
                return defaultObjectColour;
            }
            set
            {
                if (defaultObjectColour != value)
                {
                    defaultObjectColour = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ExportScale
        {
            get
            {
                return exportScale;
            }
            set
            {
                if (exportScale != value)
                {
                    exportScale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool FloorAll
        {
            get
            {
                return floorAll;
            }
            set
            {
                if (floorAll != value)
                {
                    floorAll = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IgnoreEmpty
        {
            get
            {
                return exportEmptyDocs;
            }
            set
            {
                if (exportEmptyDocs != value)
                {
                    exportEmptyDocs = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ImportSwapAxis
        {
            get
            {
                return importSwapAxis;
            }
            set
            {
                if (importSwapAxis != value)
                {
                    importSwapAxis = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RotX
        {
            get
            {
                return rotX;
            }
            set
            {
                if (rotX != value)
                {
                    rotX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RotY
        {
            get
            {
                return rotY;
            }
            set
            {
                if (rotY != value)
                {
                    rotY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RotZ
        {
            get
            {
                return rotZ;
            }
            set
            {
                if (rotZ != value)
                {
                    rotZ = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<string> Scales
        {
            get
            {
                return scales;
            }

            set
            {
                if (scales != value)
                {
                    scales = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SelectedScale
        {
            get
            {
                return selectedScale;
            }
            set
            {
                if (selectedScale != value)
                {
                    selectedScale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool SwapAxis
        {
            get
            {
                return swapAxis;
            }
            set
            {
                if (swapAxis != value)
                {
                    swapAxis = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool VersionExport
        {
            get
            {
                return versionExport;
            }
            set
            {
                if (versionExport != value)
                {
                    versionExport = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}