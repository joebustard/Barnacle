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
        private FuselageViewModel vm;

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
            vm?.SetTopPath(pathText);
        }

        private void TopImageChanged(string imagePath)
        {
            vm?.SetTopImage(imagePath);
        }

        private void SidePathChanged(string pathText)
        {
            vm?.SetSidePath(pathText);
        }

        private void SideImageChanged(string imagePath)
        {
            vm?.SetSideImage(imagePath);
        }

        private void RibList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (vm.SelectedRibIndex != -1)
            {
                RibList.ScrollIntoView(vm.SelectedRib);
            }
        }
    }
}