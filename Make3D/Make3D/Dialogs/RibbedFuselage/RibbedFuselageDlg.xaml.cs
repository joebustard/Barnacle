using Barnacle.Dialogs;
using System;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RibbedFuselageDlg.xaml
    /// </summary>
    public partial class RibbedFuselageDlg : BaseModellerDialog
    {
        public RibbedFuselageDlg()
        {
            InitializeComponent();
        }

        private void RibList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RibList.ScrollIntoView(e.AddedItems[0]);
            }
        }
    }
}