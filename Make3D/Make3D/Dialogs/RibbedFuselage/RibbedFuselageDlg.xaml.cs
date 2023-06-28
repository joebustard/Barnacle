using Barnacle.Dialogs;
using Barnacle.RibbedFuselage.ViewModels;
using System;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RibbedFuselageDlg.xaml
    /// </summary>
    public partial class RibbedFuselageDlg : BaseModellerDialog
    {
        FuselageViewModel vm;
        public RibbedFuselageDlg()
        {
            InitializeComponent();
            TopPathEditor.OnFlexiImageChanged = TopImageChanged;
            TopPathEditor.OnFlexiPathTextChanged = TopPathChanged;
            SidePathEditor.OnFlexiImageChanged = SideImageChanged;
            SidePathEditor.OnFlexiPathTextChanged = SidePathChanged;
            vm = this.DataContext as FuselageViewModel;
        }

        private void TopPathChanged(string pathText)
        {
          
        }

        private void TopImageChanged(string imagePath)
        {
            
        }

        private void SidePathChanged(string pathText)
        {

        }

        private void SideImageChanged(string imagePath)
        {

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