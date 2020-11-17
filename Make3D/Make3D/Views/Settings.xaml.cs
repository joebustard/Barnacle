using Make3D.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Make3D.Views
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

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SettingsViewModel vm = DataContext as SettingsViewModel;
            if (vm != null)
            {
                BaseViewModel.Document.ProjectSettings.Description = vm.Description;
                BaseViewModel.Document.ProjectSettings.BaseScale = vm.SelectedScale;
                BaseViewModel.Document.ProjectSettings.ExportScale = vm.ExportScale;
                try
                {
                    double x = Convert.ToDouble(vm.RotX);
                    double y = Convert.ToDouble(vm.RotY);
                    double z = Convert.ToDouble(vm.RotZ);
                    BaseViewModel.Document.ProjectSettings.ExportRotation = new Point3D(x, y, z);
                    BaseViewModel.Document.ProjectSettings.ExportAxisSwap = vm.SwapAxis;
                    BaseViewModel.Document.ProjectSettings.FloorAll = vm.FloorAll;
                    BaseViewModel.Document.Dirty = true;
                }
                catch (Exception)
                {
                }
            }
            Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}