using LoggerLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Workflow;
using static Workflow.CuraDefinition;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for EditProfile.xaml
    /// </summary>
    public partial class EditProfile : Window, INotifyPropertyChanged
    {
        private CuraDefinitionFile curaPrinter;
        private BarnaclePrinter printer;
        private BarnaclePrinterManager printerManager;
        private SlicerProfile profile;
        private string profileName;
        private string selectedSection;
        private List<String> settingSections;
        private List<SettingDefinition> settingsToDisplay;
        public EditProfile()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CreatingNewProfile { get; set; }
        public string PrinterName { get; internal set; }

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

        public string SelectedSection
        {
            get { return selectedSection; }
            set
            {
                if (selectedSection != value)
                {
                    selectedSection = value;
                    NotifyPropertyChanged();
                    UpdateSettingsToDisplay();
                }
            }
        }

        public List<String> SettingSections
        {
            get { return settingSections; }
            set
            {
                if (value != settingSections)
                {
                    settingSections = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public List<SettingDefinition> SettingsToDisplay
        {
            get { return settingsToDisplay; }
            set
            {
                if (settingsToDisplay != value)
                {
                    settingsToDisplay = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public void LoadFile(string fileName)
        {
            //  profile = new CuraDefinitionFile();
            //  profile.Load(fileName);
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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

        private void UpdateSettingsToDisplay()
        {
            List<SettingDefinition> tmp = new List<SettingDefinition>();
            foreach (SettingDefinition sd in curaPrinter.Overrides)
            {
                if (sd.Section == selectedSection)
                {
                    tmp.Add(sd);
                }
            }
            SettingsToDisplay = tmp;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            printerManager = new BarnaclePrinterManager();
            printer = printerManager.FindPrinter(PrinterName);
            if (printer != null)
            {
                curaPrinter = new CuraDefinitionFile();
                String SlicerPath = Properties.Settings.Default.SlicerPath;

                if (SlicerPath != null && SlicerPath != "")
                {
                    string curaPrinterName = SlicerPath + @"\share\cura\Resources\definitions\" + printer.CuraPrinterFile + ".def.json";
                    string curaExtruderName = SlicerPath + @"\share\cura\Resources\definitions\" + printer.CuraExtruderFile + ".def.json";

                    curaPrinter.Load(curaPrinterName);
                    curaPrinter.Load(curaExtruderName);
                    curaPrinter.ProcessSettings();
                    curaPrinter.SetUserValues();
                    SettingSections = curaPrinter.SectionNames();
                    SelectedSection = SettingSections[0];
                }
            }
        }
    }
}