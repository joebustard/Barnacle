// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using FileUtils;
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
        private CuraDefinitionFile curaDataForPrinter;
        private string originalName;
        private BarnaclePrinter printer;
        private BarnaclePrinterManager printerManager;

        private string profileName;
        private string selectedSection;
        private List<String> settingSections;
        private List<SettingDefinition> settingsToDisplay;
        private string UserProfilePath;

        public EditProfile()
        {
            InitializeComponent();
            UserProfilePath = PathManager.PrinterProfileFolder() + "\\";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CreatingNewProfile
        {
            get; set;
        }

        public string PrinterName
        {
            get; internal set;
        }

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
            get
            {
                return selectedSection;
            }

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
            get
            {
                return settingSections;
            }

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
            get
            {
                return settingsToDisplay;
            }

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
            try
            {
                if (File.Exists(fileName))
                {
                    String[] content = File.ReadAllLines(fileName);
                    for (int i = 0; i < content.GetLength(0); i += 2)
                    {
                        string key = content[i];
                        string val = content[i + 1];
                        foreach (SettingDefinition sd in curaDataForPrinter.Overrides)
                        {
                            if (sd.Name == key)
                            {
                                sd.UserValue = val;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
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
            String fileName = UserProfilePath + $"{ProfileName}.profile";
            if (!Directory.Exists(UserProfilePath))
            {
                Directory.CreateDirectory(UserProfilePath);
            }

            if (CreatingNewProfile && File.Exists(fileName))
            {
                MessageBox.Show($"Profile {ProfileName} already exists. Use a different name.", "Error");
            }
            else
            {
                DialogResult = true;
                SaveFile(fileName);

                // if we have changed the name of the file
                // we will have create a new one so get rid of
                // the original
                string originalFileName = UserProfilePath + $"{originalName}.profile";
                if (originalFileName != fileName)
                {
                    try
                    {
                        File.Delete(originalFileName);
                    }
                    catch (Exception ex)
                    {
                        LoggerLib.Logger.LogException(ex);
                    }
                }
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
                String content = "";
                foreach (SettingDefinition sd in curaDataForPrinter.Overrides)
                {
                    if (sd.UserValue != sd.OverideValue)
                    {
                        content += $"{sd.Name}\n";
                        content += $"{sd.UserValue}\n";
                    }
                }
                File.WriteAllText(fName, content);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void UpdateSettingsToDisplay()
        {
            List<SettingDefinition> tmp = new List<SettingDefinition>();
            foreach (SettingDefinition sd in curaDataForPrinter.Overrides)
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
                curaDataForPrinter = new CuraDefinitionFile();
                String SlicerPath = Properties.Settings.Default.SlicerPath;

                if (SlicerPath != null && SlicerPath != "")
                {
                    string curaPrinterName = SlicerPath + @"\share\cura\Resources\definitions\" + printer.CuraPrinterFile + ".def.json";
                    string curaExtruderName = SlicerPath + @"\share\cura\Resources\definitions\" + printer.CuraExtruderFile + ".def.json";

                    curaDataForPrinter.Load(curaPrinterName);
                    curaDataForPrinter.Load(curaExtruderName);
                    curaDataForPrinter.ProcessSettings();
                    curaDataForPrinter.SetUserValues();
                    SettingSections = curaDataForPrinter.SectionNames();
                    SelectedSection = SettingSections[0];
                }
                if (!CreatingNewProfile)
                {
                    String profile = UserProfilePath + $"{ProfileName}.profile";
                    if (File.Exists(profile))
                    {
                        LoadFile(profile);
                    }
                    originalName = ProfileName;
                }
            }
        }
    }
}