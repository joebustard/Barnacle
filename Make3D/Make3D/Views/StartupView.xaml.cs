using Barnacle.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for StartupView.xaml
    /// </summary>
    public partial class StartupView : UserControl
    {
        public StartupView()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (this.DataContext as StartupViewModel).SelectionDoubleClick();
                e.Handled = true;
            }
        }
    }
}