using Barnacle.ViewModels;
using System;
using System.Windows;
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
                    BaseViewModel.Project.SharedProjectSettings.FloorAll = vm.FloorAll;
                    BaseViewModel.Project.SharedProjectSettings.VersionExport = vm.VersionExport;
                    BaseViewModel.Project.SharedProjectSettings.DefaultObjectColour = vm.DefaultObjectColour;
                    BaseViewModel.Project.SharedProjectSettings.ExportEmptyFiles = !vm.IgnoreEmpty;
                    //    BaseViewModel.Document.Dirty = true;
                }
                catch (Exception)
                {
                }
            }
            Close();
        }
    }
}