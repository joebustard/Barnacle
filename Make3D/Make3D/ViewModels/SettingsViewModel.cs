using Barnacle.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Barnacle.ViewModels
{
    internal class SettingsViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string description;
        private bool exportEmptyDocs;
        private string exportScale;
        private bool floorAll;
        private string rotX;

        private string rotY;

        private string rotZ;

        private List<String> scales;

        private string selectedScale;

        private bool swapAxis;

        private bool versionExport;

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
            FloorAll = Project.SharedProjectSettings.FloorAll;
            VersionExport = Project.SharedProjectSettings.VersionExport;
            IgnoreEmpty = !Project.SharedProjectSettings.ExportEmptyFiles;
            DefaultObjectColour = Project.SharedProjectSettings.DefaultObjectColour;
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

        private bool autoSaveScript;
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
        private Color defaultObjectColour;

        public Color DefaultObjectColour
        {
            get { return defaultObjectColour; }
            set
            {
                if (defaultObjectColour != value)
                {
                    defaultObjectColour = value;
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