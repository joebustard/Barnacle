using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Barnacle.ViewModels
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

        private bool editingActive;

        public bool EditingActive
        {
            get
            {
                return editingActive;
            }
            set
            {
                if (editingActive != value)
                {
                    editingActive = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void SuspendEditing(object param)
        {
            bool b = Convert.ToBoolean(param);
            EditingActive = !b;
        }

        public ObjectPaletteViewModel()
        {
            AddCommand = new RelayCommand(OnAdd);
            NotificationManager.Subscribe("ToolPaletteVisible", OnToolPaletteVisibleChanged);
            NotificationManager.Subscribe("ObjectPalette", "SuspendEditing", SuspendEditing);
            editingActive = true;
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