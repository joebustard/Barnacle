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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Make3D.Adorners;
using Make3D.ViewModels;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        EditorViewModel vm;
        Point lastMousePos;
        GeometryModel3D lastHitModel;
        public EditorView()
        {
            InitializeComponent();
            NotificationManager.Subscribe("UpdateDisplay", UpdateDisplay);
            UpdateDisplay(null);
            vm = DataContext as EditorViewModel;
        }

        public void UpdateDisplay(object obj)
        {

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool shift = false;
            lastHitModel = null;
            HitTest(sender, e);
            if ( Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                shift = true;
            }
                lastMousePos = e.GetPosition(this);
            vm.MouseDown(lastMousePos, e);
            if ( lastHitModel != null)
            {
                vm.Select(lastHitModel,shift);
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
            vm.MouseUp(lastMousePos,e);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            vm.MouseWheel(e);
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
                    if (hitgeo != vm.Floor  && lastHitModel == null)
                    {
                        // UpdateResultInfo(rayMeshResult);
                        lastHitModel = hitgeo;
                        
                        
                    }
                    result = HitTestResultBehavior.Stop;
                }
            }

            return result;
        }


    }
}
