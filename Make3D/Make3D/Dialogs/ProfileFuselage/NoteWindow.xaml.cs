using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for NoteWindow.xaml
    /// </summary>
    public partial class NoteWindow : Window, INotifyPropertyChanged
    {
        private string message;

        public NoteWindow()
        {
            InitializeComponent();
            message = "";
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Refresh()
        {
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
        }
    }
}