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
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TopImage":
                    {
                        if (!String.IsNullOrEmpty(vm.TopImage))
                        {
                            TopPathEditor.LoadImage(vm.TopImage);
                        }
                    }
                    break;

                case "SideImage":
                    {
                        if (!String.IsNullOrEmpty(vm.SideImage))
                        {
                            SidePathEditor.LoadImage(vm.SideImage);
                        }
                    }
                    break;

                case "TopPath":
                    {
                        if (!String.IsNullOrEmpty(vm.TopPath))
                        {
                            TopPathEditor.FromString(vm.TopPath);
                        }
                    }
                    break;

                case "SidePath":
                    {
                        if (!String.IsNullOrEmpty(vm.SidePath))
                        {
                            SidePathEditor.FromString(vm.SidePath);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void TopPathChanged(string pathText)
        {
            if (vm != null)
            {
                vm.TopPath = pathText;
            }
        }

        private void TopImageChanged(string imagePath)
        {
            if (vm != null)
            {
                vm.TopImage = imagePath;
            }
        }

        private void SidePathChanged(string pathText)
        {
            if (vm != null)
            {
                vm.SidePath = pathText;
            }
        }

        private void SideImageChanged(string imagePath)
        {
            if (vm != null)
            {
                vm.SideImage = imagePath;
            }
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