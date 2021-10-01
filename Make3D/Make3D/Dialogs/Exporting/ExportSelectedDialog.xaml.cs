using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportSelectedDialog.xaml
    /// </summary>
    public partial class ExportSelectedDialog : Window
    {
        public enum ExportChoice
        {
            Selected,
            All
        }

        public ExportChoice Result { get; set; }

        public ExportSelectedDialog()
        {
            InitializeComponent();
            Result = ExportChoice.Selected;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = ExportChoice.Selected;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Result = ExportChoice.All;
            Close();
        }
    }
}