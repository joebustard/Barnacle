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

using Barnacle.ViewModels;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            ColourOfNewObject.PropertyChanged += ColourOfNewObject_PropertyChanged;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = DataContext as SettingsViewModel;
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (vm.SlicerPath != "")
            {
                string pth = System.IO.Path.GetDirectoryName(vm.SlicerPath);
                if (pth != "" && System.IO.Directory.Exists(pth))
                {
                    dlg.SelectedPath = pth;
                }
            }

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.SlicerPath = dlg.SelectedPath;
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ColourOfNewObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SettingsViewModel vm = DataContext as SettingsViewModel;
            if (vm != null)
            {
                vm.ObjectColour = ColourOfNewObject.SelectedColour;
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SettingsViewModel vm = DataContext as SettingsViewModel;
            if (vm != null)
            {
                BaseViewModel.Project.SharedProjectSettings.Description = vm.Description;
                BaseViewModel.Project.SharedProjectSettings.BaseScale = vm.SelectedScale;
                BaseViewModel.Project.SharedProjectSettings.ExportScale = vm.ExportScale;
                BaseViewModel.Project.SharedProjectSettings.AutoSaveScript = vm.AutoSaveScript;
                BaseViewModel.Project.SharedProjectSettings.PlaceNewAtMarker = vm.PlaceNewAtMarker;

                try
                {
                    double x = Convert.ToDouble(vm.RotX);
                    double y = Convert.ToDouble(vm.RotY);
                    double z = Convert.ToDouble(vm.RotZ);
                    BaseViewModel.Project.SharedProjectSettings.ExportRotation = new Point3D(x, y, z);
                    BaseViewModel.Project.SharedProjectSettings.ExportAxisSwap = vm.SwapAxis;
                    BaseViewModel.Project.SharedProjectSettings.ImportAxisSwap = vm.ImportSwapAxis;
                    BaseViewModel.Project.SharedProjectSettings.FloorAll = vm.FloorAll;
                    BaseViewModel.Project.SharedProjectSettings.VersionExport = vm.VersionExport;

                    BaseViewModel.Project.SharedProjectSettings.ClearPreviousVersionsOnExport = vm.ClearPreviousVersionsOnExport;
                    BaseViewModel.Project.SharedProjectSettings.DefaultObjectColour = vm.DefaultObjectColour;
                    BaseViewModel.Project.SharedProjectSettings.ExportEmptyFiles = !vm.IgnoreEmpty;

                    BaseViewModel.Document.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;
                    Properties.Settings.Default.SlicerPath = vm.SlicerPath;
                    Properties.Settings.Default.SDCardLabel = vm.SDCardName;
                    Properties.Settings.Default.ConfirmNameAfterCSG = vm.ConfirmNameAfterCSG;
                    Properties.Settings.Default.RepeatHoleFixes = vm.RepeatHoleFixes;
                    Properties.Settings.Default.AutoSaveOn = vm.AutoSaveChanges;
                    Properties.Settings.Default.MinPrimVertices = vm.MinVerticesForPrimitives;
                    NotificationManager.IdleMode = Properties.Settings.Default.AutoSaveOn;
                    Properties.Settings.Default.Save();
                }
                catch (Exception)
                {
                }
            }
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsViewModel vm = DataContext as SettingsViewModel;
            ColourOfNewObject.SelectedColour = vm.ObjectColour;
        }
    }
}