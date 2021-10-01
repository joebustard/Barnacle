using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Barnacle.ViewModels
{
    internal class AboutViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private String appIdentity;

        public AboutViewModel()
        {
            AppIdentity = "Barnacle V" + SoftwareVersion;
            OKCommand = new RelayCommand(OnOk, null);
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

        public ICommand OKCommand { get; set; }

        private void OnOk(object obj)
        {
            NotificationManager.Notify("CloseAbout", null);
        }
    }
}