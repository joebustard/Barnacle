using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    public class ToolDef : INotifyPropertyChanged
    {
        private string _commandParam;
        private bool _isActive;

        public ToolDef(string n, bool a, string cp, string tl)
        {
            Name = n;
            IsActive = a;
            CommandParam = cp;
            ToolTip = tl;
            ToolCommand = new RelayCommand(OnCommand, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string CommandParam { get => _commandParam; set => _commandParam = value; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                NotifyPropertyChanged();
            }
        }

        public string Name { get; set; }

        public ICommand ToolCommand
        { get; set; }

        public string ToolTip { get; set; }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnCommand(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Tool", s);
        }
    }
}