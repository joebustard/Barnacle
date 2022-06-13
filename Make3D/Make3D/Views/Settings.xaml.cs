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
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
                    BaseViewModel.Project.SharedProjectSettings.SlicerPath = vm.SlicerPath;
                    BaseViewModel.Document.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;
                    //    BaseViewModel.Document.Dirty = true;
                }
                catch (Exception)
                {
                }
            }
            Close();
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
    }
}