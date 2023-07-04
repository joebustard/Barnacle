using Barnacle.Dialogs;
using Barnacle.RibbedFuselage.ViewModels;
using Barnacle.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

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
                            if (vm.TopPath != TopPathEditor.AbsolutePathString)
                            {
                                TopPathEditor.FromString(vm.TopPath, false);
                                CopyPathToTopView();
                            }
                        }
                    }
                    break;

                case "SidePath":
                    {
                        if (!String.IsNullOrEmpty(vm.SidePath))
                        {
                            if (vm.SidePath != SidePathEditor.AbsolutePathString)
                            {
                                SidePathEditor.FromString(vm.SidePath, false);
                                CopyPathToSideView();
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void CopyPathToTopView()
        {
            TopView.OnMarkerMoved = TopMarkerMoved;
            //get the flexipath from  the top and render the path onto an image
            List<PointF> pnts = TopPathEditor.DisplayPointsF();
            TopView.OutlinePoints = pnts;
            TopView.Markers = vm.GetMarkers();
        }

        private void TopMarkerMoved(string s, System.Drawing.Point p, bool finishedMove)
        {
            vm.MoveMarker(s, p.X, finishedMove);
        }

        private void SideMarkerMoved(string s, System.Drawing.Point p, bool finishedMove)
        {
            vm.MoveMarker(s, p.X, finishedMove);
        }

        private void CopyPathToSideView()
        {
            SideView.OnMarkerMoved = SideMarkerMoved;
            //get the flexipath from  the side and render the path onto an image
            List<PointF> pnts = SidePathEditor.DisplayPointsF();
            SideView.OutlinePoints = pnts;
            SideView.Markers = vm.GetMarkers();
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

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void ViewTabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TabControl tc = sender as TabControl;
            if (tc != null)
            {
                if (tc.SelectedIndex == 0)
                {
                    CopyPathToTopView();
                }
                else
                {
                    CopyPathToSideView();
                }
            }
        }
    }
}