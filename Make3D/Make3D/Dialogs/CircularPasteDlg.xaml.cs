using System.Windows;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for CircularPasteDlg.xaml
    /// </summary>
    public partial class CircularPasteDlg : Window
    {
        public CircularPasteDlg()
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
            Close();
        }
    }
}