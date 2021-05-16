using Make3D.Dialogs.Figure;
using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class FigureDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        // Yes this tool does have its own doucment.
        // Its used to load the models that will be available
        private Document document;

        private Point lastMousePos;
        private List<Object3D> markers;
        private Object3D selectedBone;
        private Object3D selectedMarker;
        private int selectedTabItem;
        private Bone skeleton;
        private List<Object3D> skeletonMeshs;

        public FigureDlg()
        {
            InitializeComponent();
            ToolName = "Figure";
            DataContext = this;
            markers = new List<Object3D>();
            skeletonMeshs = new List<Object3D>();
            document = new Document();
        }

        public int SelectedTabItem
        {
            get
            {
                return selectedTabItem;
            }
            set
            {
                if (selectedTabItem != value)
                {
                    selectedTabItem = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }
            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private static GeometryModel3D GetMesh(Object3D obj)
        {
            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = obj.Mesh;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = obj.Color;
            mt.Brush = new SolidColorBrush(obj.Color);
            gm.Material = mt;
            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = obj.Color;
            mtb.Brush = new SolidColorBrush(Colors.Black);
            gm.BackMaterial = mtb;
            return gm;
        }

        private Object3D CheckHit(GeometryModel3D search, List<Object3D> od3ds)
        {
            Object3D res = null;
            foreach (Object3D ob in od3ds)
            {
                if (ob.Mesh == search.Geometry)
                {
                    res = ob;
                    break;
                }
            }

            return res;
        }

        private void ClearFigure()
        {
        }

        private void ClearSkeleton()
        {
            markers.Clear();
            skeletonMeshs.Clear();
        }

        private void CreateDefaultSkeleton()
        {
            skeleton = new Bone();
            skeleton.Name = "Root";
            Bone neck = skeleton.AddSub("Neck", 4, 5, 5, 0, 0, 90, -1, -1, -1, -1, -1, -1);
            Bone head = neck.AddSub("Head", 10, 9, 9, 0, 0, 0, -5, 5, 85, 95, 0, 0, "headbone");

            // spine
            Bone vert1 = skeleton.AddSub("Vert1", 5, 5, 5, 0, 0, 270, -5, 5, 85, 95, 0, 0);
            Bone vert2 = vert1.AddSub("Vert2", 5, 5, 5, 0, 0, 0, -5, 5, 85, 95, 0, 0);
            Bone vert3 = vert2.AddSub("Vert3", 5, 5, 5, 0, 0, 0, -5, 5, 85, 95, 0, 0);
            Bone vert4 = vert3.AddSub("Vert4", 5, 5, 5, 0, 0, 0, -5, 5, 85, 95, 0, 0);

            // arms
            Bone rs = skeleton.AddSub("RightShoulder", 7, 5, 5, 0, 0, 180, -1, -1, -1, -1, -1, -1);
            Bone rua = rs.AddSub("RightUpperArm", 10, 5, 5, 0, 0, -5, -1, -1, -1, -1, -1, -1);
            Bone rla = rua.AddSub("RightLowerArm", 10, 5, 5, 0, 0, 10, -1, -1, -1, -1, -1, -1);
            Bone rh = rla.AddSub("RightHand", 5, 2.5, 2.5, 0, 0, 10, -1, -1, -1, -1, -1, -1, "handbone");

            Bone ls = skeleton.AddSub("LeftShoulder", 7, 5, 5, 0, 0, 0, -1, -1, -1, -1, -1, -1);
            Bone lua = ls.AddSub("LeftUpperArm", 10, 5, 5, 0, 0, 5, -1, -1, -1, -1, -1, -1);
            Bone lla = lua.AddSub("LeftLowerArm", 10, 5, 5, 0, 0, -10, -1, -1, -1, -1, -1, -1);
            Bone lh = lla.AddSub("LeftHand", 5, 2.50, 2.5, 0, 0, -10, -1, -1, -1, -1, -1, -1, "handbone");

            // pelvis
            Bone pr = vert4.AddSub("RightPelvis", 5, 5, 5, 0, 0, -90, -1, -1, -1, -1, -1, -1);

            Bone pl = vert4.AddSub("LeftPelvis", 5, 5, 5, 0, 0, 90, -1, -1, -1, -1, -1, -1);

            // legs
            Bone rul = pr.AddSub("RightUpperLeg", 17, 5, 5, 0, 0, 85, -1, -1, -1, -1, -1, -1);
            Bone rll = rul.AddSub("RightLowerLeg", 16, 5, 5, 0, 0, -5, -1, -1, -1, -1, -1, -1);

            Bone lul = pl.AddSub("LeftUpperLeg", 17, 5, 5, 0, 0, -85, -1, -1, -1, -1, -1, -1);
            Bone lll = lul.AddSub("LeftLowerLeg", 16, 5, 5, 0, 0, 5, -1, -1, -1, -1, -1, -1);

            Bone rft = rll.AddSub("RightFoot", 1, 9, 9, -90, 0, 10, -1, -1, -1, -1, -1, -1, "footbone");

            Bone lft = lll.AddSub("LeftFoot", 1, 9, 9, -90, 0, -10, -1, -1, -1, -1, -1, -1, "footbone");

            // force recalculation of positions.
            double y = vert1.Length + vert2.Length + vert3.Length + vert4.Length + rul.Length + rll.Length + rft.Length;
            skeleton.StartPosition = new Point3D(0, y, 0);
            skeleton.Update();
            skeleton.Dump(0);
        }

        private void CreateMarker(Point3D position, double size, Color col, string name)
        {
            Object3D marker = new Object3D();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateSphere(ref pnts, ref indices, ref normals);
            marker.Name = name;
            marker.RelativeObjectVertices = pnts;
            marker.TriangleIndices = indices;
            marker.Normals = normals;

            marker.Position = position;
            marker.Scale = new Scale3D(size, size, size);
            marker.ScaleMesh(size, size, size);
            marker.Color = col;
            marker.RelativeToAbsolute();
            marker.SetMesh();
            markers.Add(marker);
        }

        private Object3D FindMarkerForBone(String name)
        {
            Object3D res = null;
            foreach (Object3D ob in markers)
            {
                if (ob.Name == name)
                {
                    res = ob;
                    break;
                }
            }

            return res;
        }

        private void GenerateFigure()
        {
        }

        private void GenerateShape()
        {
            ClearShape();
            ClearFigure();
            ClearSkeleton();

            if (selectedTabItem == 1)
            {
                GenerateSkeleton();
            }
            else
            {
                GenerateFigure();
            }
        }

        private void GenerateSkeleton()
        {
            markers.Clear();
            skeletonMeshs.Clear();
            CreateMarker(skeleton.StartPosition, 4, Colors.Green, "Root");
            List<BoneDisplayRecord> brecs = new List<BoneDisplayRecord>();
            skeleton.GetSubPositions(brecs);
            foreach (BoneDisplayRecord p in brecs)
            {
                CreateMarker(p.MarkerPosition, 4, Colors.Blue, p.Name);
                foreach (Object3D ob in document.Content)
                {
                    if (ob.Name.ToLower() == p.ModelName)
                    {
                        Object3D cl = ob.Clone();
                        // Name the object after the bone it represents
                        cl.Name = p.Name;
                        cl.Position = p.Position;
                        cl.ScaleMesh(p.Scale.X, p.Scale.Y, p.Scale.Z);
                        cl.RotateRad(p.Rotation);
                        cl.Remesh();
                        cl.SetMesh();
                        skeletonMeshs.Add(cl);
                    }
                }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool leftButton = (e.LeftButton == MouseButtonState.Pressed);
            if (leftButton)
            {
                lastMousePos = e.GetPosition(viewport3D1);
            }
            lastHitModel = null;
            lastHitPoint = new Point3D(0, 0, 0);
            selectedBone = null;
            selectedMarker = null;
            HitTest(viewport3D1,sender, e);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (lastHitModel != null)
            {
                selectedMarker = CheckHit(lastHitModel, markers);
                if (selectedMarker == null)
                {
                    selectedBone = CheckHit(lastHitModel, skeletonMeshs);
                    if (selectedBone != null)
                    {
                        selectedMarker = FindMarkerForBone(selectedBone.Name);
                    }
                }

                base.Viewport_MouseDown(viewport3D1, e);
            }
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
        }

        private void LoadFigureShapes()
        {
            // Just in case
            document.Clear();
            // the data folder has a sub folder called figure.
            // Look in there and insert the entire contents of any model files
            // Note that we have to insert, not load.
            // If the files have duplicate part names, things will get tricky
            String pth = AppDomain.CurrentDomain.BaseDirectory + "data\\figure";
            if (Directory.Exists(pth))
            {
                String[] fileNames = Directory.GetFiles(pth, "*.txt");
                foreach (string f in fileNames)
                {
                    try
                    {
                        document.InsertFile(f);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void Redisplay()
        {
            if (MyModelGroup != null)
            {
                MyModelGroup.Children.Clear();

                if (floor != null && ShowFloor)
                {
                    MyModelGroup.Children.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }

                if (axies != null && ShowAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        MyModelGroup.Children.Add(m);
                    }
                }
                GeometryModel3D gm = GetModel();
                MyModelGroup.Children.Add(gm);

                foreach (Object3D ob in markers)
                {
                    ob.Remesh();
                    GeometryModel3D gm2 = GetMesh(ob);

                    MyModelGroup.Children.Add(gm2);
                }

                foreach (Object3D ob in skeletonMeshs)
                {
                    ob.Remesh();
                    GeometryModel3D gm2 = GetMesh(ob);

                    MyModelGroup.Children.Add(gm2);
                }
            }
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void UpdateDisplay()
        {
            GenerateShape();
            Redisplay();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFigureShapes();
            CreateDefaultSkeleton();

            LoadEditorParameters();

            GenerateShape();
            UpdateCameraPos();
            MyModelGroup.Children.Clear();

            Redisplay();
        }
    }
}