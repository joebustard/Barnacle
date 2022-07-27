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

namespace Barnacle.UserControls.FlexiPathEditorControl
{
    /// <summary>
    /// Interaction logic for FlexiPathEditorControl.xaml
    /// </summary>
    public partial class FlexiPathEditorControl : UserControl
    {
        private FlexiPathEditorControlViewModel vm;
        public FlexiPathEditorControl()
        {
            InitializeComponent();
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                vm = DataContext as FlexiPathEditorControlViewModel;
                if ( vm != null)
                {
                    vm.PropertyChanged += Vm_PropertyChanged; 
                }
            }
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch ( e.PropertyName)
            {
                case "SelectionMode":
                    {
                        SetButtonBorderColours();
                    }
                    break;
            }
        }

        private void DoButtonBorder( Border src, Border trg)
        {
            if ( src == trg)
            {
                trg.BorderBrush = System.Windows.Media.Brushes.CadetBlue;
            }
            else
            {
                trg.BorderBrush = System.Windows.Media.Brushes.AliceBlue;
            }
        }

        private void EnableBorder(Border src)
        {
            DoButtonBorder(src, PickBorder);
            DoButtonBorder(src, AddSegBorder);
            DoButtonBorder(src, AddBezierBorder);
            DoButtonBorder(src, DelSegBorder);
            DoButtonBorder(src, MovePathBorder);
        }
        private void SetButtonBorderColours()
        {
            switch (vm.SelectionMode)
            {

                case FlexiPathEditorControlViewModel.SelectionModeType.AppendPoint:
                case FlexiPathEditorControlViewModel.SelectionModeType.StartPoint:
                case FlexiPathEditorControlViewModel.SelectionModeType.SelectSegmentAtPoint:
                    {
                        EnableBorder(PickBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.SplitLine:
                    {
                        EnableBorder(AddSegBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToCubicBezier:
                    {
                        EnableBorder(AddBezierBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.ConvertToQuadBezier:
                    {
                        EnableBorder(AddQuadBezierBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.DeleteSegment:
                    {
                        EnableBorder(DelSegBorder);
                    }
                    break;

                case FlexiPathEditorControlViewModel.SelectionModeType.MovePath:
                    {
                        EnableBorder(MovePathBorder);
                    }
                    break;

            }
        }
}
