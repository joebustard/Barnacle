using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Make3D.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {

        private string caption;
        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if ( caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }

        }
        internal MainWindowViewModel()
        {
            Caption = Document.Caption;
            base.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ( e.PropertyName == "Caption")
            {
                Caption = Document.Caption;
            }
        }
    }
}
