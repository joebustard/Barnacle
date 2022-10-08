using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for EditProfile.xaml
    /// </summary>
    public partial class EditProfile : Window, INotifyPropertyChanged
    {
        public bool CreatingNewProfile { get; set; }
        private ObservableCollection<ProfileEntry> settings;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ProfileEntry> Settings
        {
            get
            {
                return settings;
            }
            set
            {
                if (value != settings)
                {
                    settings = value;
                }
                NotifyPropertyChanged();
            }
        }

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

        public EditProfile()
        {
            InitializeComponent();
            Settings = new ObservableCollection<ProfileEntry>();
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
            fileName += "\\" + ProfileName;
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
                StreamWriter sw = File.AppendText(fName);
                foreach (ProfileEntry pe in Settings)
                {
                    sw.WriteLine($"{pe.SettingName}=\"{pe.SettingValue}\"");
                }
                sw.Close();
            }
            catch
            {
            }
        }

        public void LoadFile(string fileName)
        {
            // We use a dictionary to read in the profile.
            // This means if there are duplicate values, its only the last one thats taken.
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            Settings = new ObservableCollection<ProfileEntry>();
            if (File.Exists(fileName))
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                for (int i = 0; i < lines.GetLength(0); i++)
                {
                    lines[i] = lines[i].Trim();
                    if (lines[i] != "")
                    {
                        string[] words = lines[i].Split('=');
                        if (words.GetLength(0) == 2)
                        {
                            words[1] = words[1].Replace("\"", "");
                        }
                        tmp[words[0]] = words[1];
                    }
                }

                // convert the dictionary back to a list.
                string[] keys = tmp.Keys.ToArray();
                for (int i = 0; i < tmp.Count; i++)
                {
                    Settings.Add(new ProfileEntry(keys[i], tmp[keys[i]]));
                }
            }
        }
    }
}