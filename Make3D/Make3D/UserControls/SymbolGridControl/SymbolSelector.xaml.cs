using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Barnacle.UserControls
{
    /// <summary>
    /// Interaction logic for SymbolSelector.xaml
    /// </summary>
    public partial class SymbolSelector : UserControl
    {
        private SymbolGridViewModel vm;

        public SymbolSelector()
        {
            InitializeComponent();
            vm = this.DataContext as SymbolGridViewModel;
        }

        public SymbolGridViewModel.SymbolChanged OnSymbolChanged
        {
            get
            {
                return
                    vm.OnSymbolChanged;
            }
            set
            {
                vm.OnSymbolChanged = value;
            }
        }
    }
}