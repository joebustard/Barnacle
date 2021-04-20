using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    internal class StartupViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private String appIdentity;

        public StartupViewModel()
        {
            AppIdentity = "Barnacle V" + SoftwareVersion;
            NewProjectCommand = new RelayCommand(OnNewProjectCommand);
        }

        public String AppIdentity
        {
            get
            {
                return appIdentity;
            }
            set
            {
                if (appIdentity != value)
                {
                    appIdentity = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand NewProjectCommand { get; set; }

        private void OnNewProjectCommand(object obj)
        {
            NotificationManager.Notify("StartWithNewProject", null);
        }
    }
}