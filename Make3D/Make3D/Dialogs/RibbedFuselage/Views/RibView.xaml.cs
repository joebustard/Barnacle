using Barnacle.Dialogs.RibbedFuselage.Models;
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

namespace Barnacle.Dialogs.RibbedFuselage.Views
{
    /// <summary>
    /// Interaction logic for RibView.xaml
    /// </summary>
    public partial class RibView : UserControl
    {
        private RibImageDetailsModel vm;

        public RibView()
        {
            InitializeComponent();
            FlexiControl.OnFlexiImageChanged = ImageChanged;
            FlexiControl.OnFlexiPathTextChanged = PathChanged;
        }

        private void PathChanged(string pathText)
        {
            if (vm != null)
            {
                vm.FlexiPathText = pathText;
            }
        }

        private void ImageChanged(string imagePath)
        {
            if (vm != null)
            {
                vm.ImageFilePath = imagePath;
                if (!String.IsNullOrEmpty(imagePath))
                {
                    vm.DisplayFileName = System.IO.Path.GetFileName(imagePath);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm = this.DataContext as RibImageDetailsModel;
        }
    }
}