using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutBoxDlg.xaml
    /// </summary>
    public partial class AboutBoxDlg : Window
    {
        public AboutBoxDlg()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}