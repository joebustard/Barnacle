using System.Windows.Controls;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for ObjectPropertiesView.xaml
    /// </summary>
    public partial class ObjectPropertiesView : UserControl
    {
        public ObjectPropertiesView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ColourDialog dlg = new ColourDialog();
            if (dlg.ShowDialog() == true)
            {
            }
        }
    }
}