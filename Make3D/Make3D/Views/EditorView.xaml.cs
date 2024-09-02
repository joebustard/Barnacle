using Barnacle.Models;
using Barnacle.ViewModels;
using ImageUtils;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        private GeometryModel3D lastHitModel;
        private Point3D lastHitPoint;
        private Point lastMousePos;
        private String screenShotTarget;
        private DispatcherTimer snapShotTimer;
        private EditorViewModel vm;

        public EditorView()
        {
            InitializeComponent();
            NotificationManager.Subscribe("Editor", "UpdateDisplay", UpdateDisplay);
            NotificationManager.Subscribe("Editor", "MultiPaste", OnMultiPaste);
            NotificationManager.Subscribe("Editor", "KeyUp", OnKeyUp);
            NotificationManager.Subscribe("Editor", "KeyDown", OnKeyDown);
            NotificationManager.Subscribe("Editor", "Settings", OnSettings);
            NotificationManager.Subscribe("Editor", "ScreenShot", OnScreenShot);
            UpdateDisplay(null);

            vm = DataContext as EditorViewModel;
            vm.ViewPort = viewport3D1;
            vm.Overlay = overlayCanvas;
        }

        public void HitTest(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            Point mouseposition = args.GetPosition(viewport3D1);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);
            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D
            VisualTreeHelper.HitTest(viewport3D1, null, HTResult, pointparams);
        }

        public HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            HitTestResultBehavior result = HitTestResultBehavior.Continue;
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                RayMeshGeometry3DHitTestResult rayMeshResult = rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                    if (lastHitModel == null)
                    {
                        lastHitModel = hitgeo;
                        lastHitPoint = rayMeshResult.PointHit;
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
        }

        public void UpdateDisplay(object obj)
        {
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool shift = false;
            bool control = false;
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            lastHitModel = null;
            lastHitPoint = new Point3D(0, 0, 0);
            HitTest(sender, e);
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                shift = true;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                control = true;
            }
            lastMousePos = e.GetPosition(this);
            vm.MouseDown(lastMousePos, e);
            if (lastHitModel != null)
            {
                vm.Select(lastHitModel, lastHitPoint, leftButton, shift, control);
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            lastMousePos = e.GetPosition(this);
            vm.MouseMove(lastMousePos, e);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMousePos = e.GetPosition(this);
            vm.MouseUp(lastMousePos, e);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            vm.MouseWheel(e);
        }

        private void OnKeyDown(object param)
        {
            KeyEventArgs e = param as KeyEventArgs;
            e.Handled = vm.KeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                  Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl), Keyboard.IsKeyDown(Key.LeftAlt));
        }

        private void OnKeyUp(object param)
        {
            KeyEventArgs e = param as KeyEventArgs;
            vm.KeyUp(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                  Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }

        private void OnMultiPaste(object param)
        {
            MultiPasteDlg dlg = new MultiPasteDlg();
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    MultiPasteConfig cfg = new MultiPasteConfig();
                    cfg.Repeats = Convert.ToInt16(dlg.RepeatsBox.Text);
                    cfg.Spacing = Convert.ToDouble(dlg.SpaceBox.Text);
                    cfg.AlternatingOffset = Convert.ToDouble(dlg.AltBox.Text);
                    if (dlg.DirectionX.IsChecked == true)
                    {
                        cfg.Direction = "X";
                    }
                    if (dlg.DirectionY.IsChecked == true)
                    {
                        cfg.Direction = "Y";
                    }
                    if (dlg.DirectionZ.IsChecked == true)
                    {
                        cfg.Direction = "Z";
                    }
                    NotificationManager.Notify("DoMultiPaste", cfg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private bool previousShowFloor;
        private bool previousShowAxis;
        private bool previousShowBuildPlate;
        private bool previousShowMarker;

        private void OnScreenShot(object param)
        {
            screenShotTarget = "";
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Portable Network Images (*.png)|*.png";
            if (dlg.ShowDialog() == true)
            {
                if (!String.IsNullOrEmpty(dlg.FileName))
                {
                    screenShotTarget = dlg.FileName;
                    previousShowAxis = vm.ShowAxies;
                    previousShowFloor = vm.ShowFloor;
                    previousShowBuildPlate = vm.ShowBuildPlates;
                    previousShowMarker = vm.ShowFloorMarker;
                    vm.ShowAxies = false;
                    vm.ShowFloor = false;
                    vm.ShowBuildPlates = false;
                    vm.ShowFloorMarker = false;
                    vm.RegenerateDisplayList();
                    // dont take snap shot imediately, wait half a second for screen to update
                    TimeSpan interval = new TimeSpan(0, 0, 0, 0, 500);
                    snapShotTimer = new DispatcherTimer();
                    snapShotTimer.Interval = interval;
                    snapShotTimer.Tick += SnapShotTimer_Tick;
                    snapShotTimer.Start();
                }
            }
        }

        private void OnSettings(object param)
        {
            Settings dlg = new Settings();
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Project.Save();
                BaseViewModel.Document.SaveGlobalSettings();
            }
        }

        private void SnapShotTimer_Tick(object sender, EventArgs e)
        {
            snapShotTimer.Stop();
            if (!String.IsNullOrEmpty(screenShotTarget))
            {
                //ImageCapture.ScreenCaptureElement(viewport3D1, screenShotTarget, false);
                ImageCapture.ScreenCaptureElement(this, screenShotTarget, false);

                vm.ShowAxies = previousShowAxis;
                vm.ShowFloor = previousShowFloor;
                vm.ShowBuildPlates = previousShowBuildPlate;
                vm.ShowFloorMarker = previousShowMarker;
                vm.RegenerateDisplayList();
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            vm.KeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                                 Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl), Keyboard.IsKeyDown(Key.LeftAlt));
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            vm.KeyUp(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                             Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }
    }
}