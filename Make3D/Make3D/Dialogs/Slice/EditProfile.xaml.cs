using LoggerLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Workflow;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for EditProfile.xaml
    /// </summary>
    public partial class EditProfile : Window, INotifyPropertyChanged
    {
        public bool CreatingNewProfile { get; set; }
       

        public event PropertyChangedEventHandler PropertyChanged;

       

        private string profileName;

        public string ProfileName
        {
            get
            {
                return profileName;
            }

            set
            {
                if (value != profileName)
                {
                    profileName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private CuraDefinitionFile profile;
        public EditProfile()
        {
            InitializeComponent();
            
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            String fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\PrinterProfiles";
            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }
            fileName += System.IO.Path.DirectorySeparatorChar + ProfileName;
            if (!fileName.ToLower().EndsWith(".profile"))
            {
                fileName += ".profile";
            }
            if (CreatingNewProfile && File.Exists(fileName))
            {
                MessageBox.Show($"Profile {ProfileName} already exists. Use a different name.", "Error");
            }
            else
            {
                DialogResult = true;
                SaveFile(fileName);
                Close();
            }
        }

        private void SaveFile(string fName)
        {
            try
            {
                if (File.Exists(fName))
                {
                    File.Delete(fName);
                }
                profile.Save(fName);
                
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void LoadFile(string fileName)
        {
            profile = new CuraDefinitionFile();
            profile.Load(fileName);
        }
    }
}