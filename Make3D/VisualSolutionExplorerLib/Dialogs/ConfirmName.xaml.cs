using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualSolutionExplorerLib.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmName
    /// </summary>
    public partial class ConfirmName : Window, INotifyPropertyChanged
    {
        private string fileName;

        private bool nameValid;
        private Visibility okVisible;

        public ConfirmName()
        {
            InitializeComponent();

            NameValid = false;

            DataContext = this;
            Extension = "";
            FolderPath = "";
            FileName = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Extension { get; internal set; }

        public String FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (value != fileName)
                {
                    fileName = value;
                    NotifyPropertyChanged();
                    CheckNameValid();
                }
            }
        }

        public string FolderPath { get; internal set; }

        public bool NameValid
        {
            get
            {
                return nameValid;
            }
            set
            {
                if (value != nameValid)
                {
                    nameValid = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility OKVisible
        {
            get
            {
                return okVisible;
            }
            set
            {
                if (value != okVisible)
                {
                    okVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CheckNameValid()
        {
            bool valid = false;
            if (Extension != "" && FileName.ToLower().EndsWith(Extension.ToLower()))
            {
                valid = true;
            }
            if (valid && FolderPath != "")
            {
                String full = FolderPath + "\\" + FileName;

                if (System.IO.File.Exists(full))
                {
                    valid = false;
                }
            }
            NameValid = valid;
            if (valid)
            {
                OKVisible = Visibility.Visible;
            }
            else
            {
                OKVisible = Visibility.Hidden;
            }
        }
    }
}