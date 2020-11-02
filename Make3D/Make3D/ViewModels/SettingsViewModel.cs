using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Make3D.ViewModels
{
    internal class SettingsViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string rotX;

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

        private string rotY;

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

        private string rotZ;

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

        private string description;

        public String Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<String> scales;

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

        private string selectedScale;

        public string SelectedScale
        {
            get { return selectedScale; }
            set
            {
                if (selectedScale != value)
                {
                    selectedScale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool swapAxis;

        public bool SwapAxis
        {
            get { return swapAxis; }
            set
            {
                if (swapAxis != value)
                {
                    swapAxis = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool floorAll;

        public bool FloorAll
        {
            get { return floorAll; }
            set
            {
                if (floorAll != value)
                {
                    floorAll = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SettingsViewModel()
        {
            Description = document.ProjectSettings.Description;
            Scales = ModelScales.ScaleNames();
            SelectedScale = document.ProjectSettings.BaseScale;
            RotX = document.ProjectSettings.ExportRotation.X.ToString("F3");
            RotY = document.ProjectSettings.ExportRotation.Y.ToString("F3");
            RotZ = document.ProjectSettings.ExportRotation.Z.ToString("F3");
            SwapAxis = document.ProjectSettings.ExportAxisSwap;
            FloorAll = document.ProjectSettings.FloorAll;
        }
    }
}