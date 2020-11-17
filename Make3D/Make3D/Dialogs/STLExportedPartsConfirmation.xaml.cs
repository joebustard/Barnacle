using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Make3D.Dialogs
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
