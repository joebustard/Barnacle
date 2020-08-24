using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3D.ViewModels
{
    internal class SettingsViewModel : BaseViewModel, INotifyPropertyChanged
    {
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
                if ( scales != value)
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
                if ( selectedScale != value)
                {
                    selectedScale = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public SettingsViewModel()
        {
            Description = document.ProjectSettings.Description;
            Scales = ModelScales.ScaleNames();
            SelectedScale = document.ProjectSettings.BaseScale;
        }
    }
}
