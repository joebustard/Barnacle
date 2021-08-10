using Make3D.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for ScriptView.xaml
    /// </summary>
    public partial class ScriptView : UserControl
    {
        private DispatcherTimer dispatcherTimer;

        private GeometryModel3D lastHitModel;
        private Point3D lastHitPoint;
        private Point lastMousePos;
        private String previousText;

        private ScriptViewModel vm;

        public ScriptView()
        {
            InitializeComponent();
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
                        // UpdateResultInfo(rayMeshResult);
                        lastHitModel = hitgeo;
                        lastHitPoint = rayMeshResult.PointHit;
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
        }

        private void DeferSyntaxCheck()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();

                dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();

            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            string scriptText = tr.Text;
            if (previousText != scriptText)
            {
                if (vm.ScriptText(scriptText))
                {
                    SetDisplayRtf();
                }
                previousText = scriptText;
            }
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
                //     vm.Select(lastHitModel, lastHitPoint, leftButton, shift, control);
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

        private void OnUpdate(object param)
        {
            SetDisplayRtf();
        }

        private void RunClicked(object sender, RoutedEventArgs e)
        {
            ResultsBox.Text = "";
            vm.SetResultsBox(ResultsBox);
            vm.RunScript();
        }

        private void ScriptBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DeferSyntaxCheck();
        }

        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            vm.RawText = tr.Text;
            System.Diagnostics.Debug.WriteLine("******");
            System.Diagnostics.Debug.WriteLine(tr.Text);

            DeferSyntaxCheck();
            vm.Dirty = true;
        }

        private void ScriptView_Loaded(object sender, RoutedEventArgs e)
        {
            vm = DataContext as ScriptViewModel;
            ScriptBox.Width = ScriptGrid.ActualWidth;
            vm.SetResultsBox(ResultsBox);
            SetDisplayRtf();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
            NotificationManager.Subscribe("UpdateScript", OnUpdate);
        }

        private void SetDisplayRtf()
        {
            TextRange tr = new TextRange(ScriptBox.Document.ContentStart, ScriptBox.Document.ContentEnd);
            MemoryStream ms = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(vm.Rtf));
            tr.Load(ms, DataFormats.Rtf);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            vm.KeyDown(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                                 Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            vm.KeyUp(e.Key, Keyboard.IsKeyDown(Key.LeftShift) | Keyboard.IsKeyDown(Key.RightShift),
                             Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl));
        }
    }
}