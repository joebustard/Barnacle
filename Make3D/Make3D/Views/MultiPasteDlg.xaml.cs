using System.Windows;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for MultiPasteDlg.xaml
    /// </summary>
    public partial class MultiPasteDlg : Window
    {
        public MultiPasteDlg()
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