using System.Windows;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for ColourDialog.xaml
    /// </summary>
    public partial class ColourDialog : Window
    {
        public ColourDialog()
        {
            InitializeComponent();
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}