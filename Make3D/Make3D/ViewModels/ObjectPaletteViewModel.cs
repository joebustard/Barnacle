using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    internal class ObjectPaletteViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ICommand AddCommand { get; set; }
        private Visibility toolPaletteVisible;

        public Visibility ToolPaletteVisible
        {
            get
            {
                return toolPaletteVisible;
            }

            set
            {
                if (toolPaletteVisible != value)
                {
                    toolPaletteVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObjectPaletteViewModel()
        {
            AddCommand = new RelayCommand(OnAdd);
            NotificationManager.Subscribe("ToolPaletteVisible", OnToolPaletteVisibleChanged);
        }

        private void OnToolPaletteVisibleChanged(object param)
        {
            Visibility v = (Visibility)param;
            ToolPaletteVisible = v;
        }

        private void OnAdd(object obj)
        {
            string name = obj.ToString();
            NotificationManager.Notify("AddObject", name);
        }
    }
}
