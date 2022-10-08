using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for STLExportedPartsConfirmation.xaml
    /// </summary>
    public partial class STLExportedPartsConfirmation : Window
    {
        public string ExportPath { get; internal set; }

        public STLExportedPartsConfirmation()
        {
            InitializeComponent();
        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ExportPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
                startInfo.Arguments = ExportPath;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(startInfo);
            }
            DialogResult = true;
            Close();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PathLabel.Content = ExportPath;
        }
    }
}