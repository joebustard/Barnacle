using Make3D.Dialogs.Figure;
using Make3D.Models;
using Make3D.Models.Adorners;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class FigureDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private Adorner adorner;

        // Yes this tool does have its own doucment.
        // Its used to load the models that will be available
        private Document document;

        private bool editLimits;
        private Point lastMousePos;
        private Visibility limitsVisible;
        private List<JointMarker> markers;
        private double maxiumXRot;
        private double maxiumYRot;
        private double maxiumZRot;
        private double minimumXRot;
        private double minimumYRot;
        private double minimumZRot;
        private Bone selectedBone;
        private String selectedBoneName;
        private double selectedHeight;
        private double selectedLength;
        private JointMarker selectedMarker;
        private int selectedTabItem;
        private double selectedWidth;
        private double selectedXRot;
        private double selectedYRot;
        private double selectedZRot;
        private Bone skeleton;
        private List<Object3D> skeletonMeshs;

        public FigureDlg()
        {
            InitializeComponent();
            ToolName = "Figure";
            DataContext = this;
            markers = new List<JointMarker>();
            skeletonMeshs = new List<Object3D>();
            document = new Document();
            adorner = null;
            EditLimits = false;
            LimitsVisible = Visibility.Hidden;
        }

        public bool EditLimits
        {
            get
            {
                return editLimits;
            }
            set
            {
                if (editLimits != value)
                {
                    editLimits = value;
                    if (editLimits)
                    {
                        LimitsVisible = Visibility.Visible;
                    }
                    else
                    {
                        LimitsVisible = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility LimitsVisible
        {
            get
            {
                return limitsVisible;
            }
            set
            {
                if (limitsVisible != value)
                {
                    limitsVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double MaximumXRot
        {
            get
            {
                return maxiumXRot;
            }
            set
            {
                if (maxiumXRot != value)
                {
                    maxiumXRot = value;
                    if (selectedBone.MaxXRot != maxiumXRot)
                    {
                        selectedBone.MaxXRot = maxiumXRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double MaximumYRot
        {
            get
            {
                return maxiumYRot;
            }
            set
            {
                if (maxiumYRot != value)
                {
                    maxiumYRot = value;
                    if (selectedBone.MaxYRot != maxiumYRot)
                    {
                        selectedBone.MaxYRot = maxiumYRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double MaximumZRot
        {
            get
            {
                return maxiumZRot;
            }
            set
            {
                if (maxiumZRot != value)
                {
                    maxiumZRot = value;
                    if (selectedBone.MaxZRot != maxiumZRot)
                    {
                        selectedBone.MaxZRot = maxiumZRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double MinimumXRot
        {
            get
            {
                return minimumXRot;
            }
            set
            {
                if (minimumXRot != value)
                {
                    minimumXRot = value;
                    if (selectedBone.MinXRot != minimumXRot)
                    {
                        selectedBone.MinXRot = minimumXRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double MinimumYRot
        {
            get
            {
                return minimumYRot;
            }
            set
            {
                if (minimumYRot != value)
                {
                    minimumYRot = value;
                    if (selectedBone.MinYRot != minimumYRot)
                    {
                        selectedBone.MinYRot = minimumYRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double MinimumZRot
        {
            get
            {
                return minimumZRot;
            }
            set
            {
                if (minimumZRot != value)
                {
                    minimumZRot = value;
                    if (selectedBone.MinZRot != minimumZRot)
                    {
                        selectedBone.MinZRot = minimumZRot;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public String SelectedBoneName
        {
            get
            {
                return selectedBoneName;
            }
            set
            {
                if (selectedBoneName != value)
                {
                    selectedBoneName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double SelectedHeight
        {
            get
            {
                return selectedHeight;
            }
            set
            {
                if (selectedHeight != value)
                {
                    selectedHeight = value;
                    if (selectedBone.Height != selectedHeight)
                    {
                        selectedBone.Height = selectedHeight;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double SelectedLength
        {
            get
            {
                return selectedLength;
            }
            set
            {
                if (selectedLength != value)
                {
                    selectedLength = value;
                    if (selectedBone.Length != selectedLength)
                    {
                        selectedBone.Length = selectedLength;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
                }
            }
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

        public double SelectedWidth
        {
            get
            {
                return selectedWidth;
            }
            set
            {
                if (selectedWidth != value)
                {
                    selectedWidth = value;
                    if (selectedBone.Width != selectedWidth)
                    {
                        selectedBone.Width = selectedWidth;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double SelectedXRot
        {
            get
            {
                return selectedXRot;
            }
            set
            {
                if (selectedXRot != value)
                {
                    selectedXRot = value;
                    if (selectedBone.XRot != selectedXRot)
                    {
                        selectedBone.XRot = selectedXRot;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double SelectedYRot
        {
            get
            {
                return selectedYRot;
            }
            set
            {
                if (selectedYRot != value)
                {
                    selectedYRot = value;
                    if (selectedBone.YRot != selectedYRot)
                    {
                        selectedBone.YRot = selectedYRot;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public double SelectedZRot
        {
            get
            {
                return selectedZRot;
            }
            set
            {
                if (selectedZRot != value)
                {
                    selectedZRot = value;
                    if (selectedBone.ZRot != selectedZRot)
                    {
                        selectedBone.ZRot = selectedZRot;
                        skeleton.Update();
                        RefreshSkeleton(selectedBone);
                    }
                    NotifyPropertyChanged();
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

        protected override void Viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point newPos = e.GetPosition(viewport3D1);
            if (adorner != null && adorner.MouseMove(oldMousePos, newPos, e, false) == true)
            {
                oldMousePos = newPos;
                skeleton.Update();
            }
            else
            {
                base.Viewport_MouseMove(sender, e);
            }
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

        private JointMarker CheckHit(GeometryModel3D search, List<JointMarker> markers)
        {
            JointMarker res = null;
            foreach (JointMarker ob in markers)
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

        private void CreateAdorner()
        {
            adorner = new RotationAdorner(Camera);
            adorner.AdornObject(selectedMarker);
        }

        private void CreateBoneModel(BoneDisplayRecord p)
        {
            foreach (Object3D ob in document.Content)
            {
                if (ob.Name.ToLower() == p.ModelName)
                {
                    Object3D cl = ob.Clone();
                    // Name the object after the bone it represents
                    cl.Name = p.Name;
                    cl.Position = p.Position;
                    cl.ScaleMesh(p.Scale.X, p.Scale.Y, p.Scale.Z);
                    cl.RelativeObjectVertices = p.Bone.RotatedPointCollection(cl.RelativeObjectVertices, p.Rotation.X, p.Rotation.Y, p.Rotation.Z);
                    cl.Remesh();
                    cl.SetMesh();
                    skeletonMeshs.Add(cl);
                }
            }
        }

        private void CreateDefaultSkeleton()
        {
            skeleton = new Bone();
            skeleton.Name = "Root";
            Bone neck = skeleton.AddSub("Neck", 8, 5, 5, 0, 0, 90, -1, -1, -1, -1, -1, -1);
            Bone head = neck.AddSub("Head", 10, 14, 12, 0, -90, -90, -5, 5, 85, 95, 0, 0, "headbone");

            // spine
            Bone vert1 = skeleton.AddSub("Vert1", 8, 5, 5, 0, 0, 270, -5, 5, 85, 95, 0, 0);
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

            Bone rft = rll.AddSub("RightFoot", 10, 5, 2, 0, -90, 90, -1, -1, -1, -1, -1, -1, "footbone");

            Bone lft = lll.AddSub("LeftFoot", 10, 5, 2, 0, -90, 90, -1, -1, -1, -1, -1, -1, "footbone");

            // force recalculation of positions.
            double y = vert1.Length + vert2.Length + vert3.Length + vert4.Length + rul.Length + rll.Length + rft.Height;
            skeleton.StartPosition = new Point3D(0, y, 0);
            skeleton.EndPosition = new Point3D(0, y, 0);

            skeleton.Update();
            skeleton.Dump(0);
        }

        private void CreateMarker(Point3D position, double size, Color col, string name, Bone bn)
        {
            JointMarker marker = new JointMarker();

            Point3DCollection pnts = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PrimitiveGenerator.GenerateSphere(ref pnts, ref indices, ref normals);
            marker.Name = name;
            marker.Bone = bn;
            marker.RelativeObjectVertices = pnts;
            marker.TriangleIndices = indices;
            marker.Normals = normals;

            marker.Position = position;
            marker.Scale = new Scale3D(size, size, size);
            marker.ScaleMesh(size, size, size);
            marker.Color = col;
            marker.RelativeToAbsolute();
            marker.SetMesh();
            marker.Dirty = false;
            markers.Add(marker);
        }

        private JointMarker FindMarkerForBone(String name)
        {
            JointMarker res = null;
            foreach (JointMarker ob in markers)
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
            skeleton.Update();
            CreateMarker(skeleton.StartPosition, 6, Colors.Green, "Root", skeleton);
            List<BoneDisplayRecord> brecs = new List<BoneDisplayRecord>();
            skeleton.GetSubPositions(brecs);
            foreach (BoneDisplayRecord p in brecs)
            {
                CreateMarker(p.MarkerPosition, 4, Colors.Blue, p.Name, p.Bone);
                CreateBoneModel(p);
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

            HitTest(viewport3D1, sender, e);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (lastHitModel != null)
            {
                bool handled = false;
                // was it the adorner
                if (adorner != null)
                {
                    handled = adorner.Select(lastHitModel);
                }

                if (!handled)
                {
                    adorner?.Clear();

                    // not adorner so maybe something in the model
                    Object3D hitBone = null;
                    selectedMarker = null;
                    selectedMarker = CheckHit(lastHitModel, markers);
                    if (selectedMarker == null)
                    {
                        hitBone = CheckHit(lastHitModel, skeletonMeshs);
                        if (hitBone != null)
                        {
                            selectedMarker = FindMarkerForBone(hitBone.Name);
                        }
                    }
                    if (selectedMarker != null)
                    {
                        CreateAdorner();
                        SelectedBoneName = selectedMarker.Name;
                        selectedBone = selectedMarker.Bone;
                        SelectedXRot = selectedMarker.Bone.XRot;
                        SelectedYRot = selectedMarker.Bone.YRot;
                        SelectedZRot = selectedMarker.Bone.ZRot;
                        SelectedWidth = selectedBone.Width;
                        SelectedHeight = selectedBone.Height;
                        SelectedLength = selectedBone.Length;
                        MinimumXRot = selectedBone.MinXRot;
                        MinimumYRot = selectedBone.MinYRot;
                        MinimumZRot = selectedBone.MinZRot;
                        MaximumXRot = selectedBone.MaxXRot;
                        MaximumYRot = selectedBone.MaxYRot;
                        MaximumZRot = selectedBone.MaxZRot;

                        selectedMarker.OnBoneRotated +=   OnBoneRotated;   
                    }
                    base.Viewport_MouseDown(viewport3D1, e);
                    Redisplay();
                }
            }
        }

        private void OnBoneRotated(Bone bn)
        {
            SelectedXRot = bn.XRot;
            SelectedYRot = bn.YRot;
            SelectedZRot = bn.ZRot;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (adorner != null)
            {
                adorner.MouseUp();
                if (adorner.SelectedObjects.Count > 0)
                {
                    JointMarker mk = adorner.SelectedObjects[0] as JointMarker;

                    if (mk != null && mk.Dirty)
                    {
                        RefreshSkeleton(mk.Bone);
                        mk.Dirty = false;
                    }
                }
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
                if (adorner != null)
                {
                    foreach (Model3D md in adorner.Adornments)
                    {
                        MyModelGroup.Children.Add(md);
                    }
                }
            }
        }

        private void RefreshSkeleton(Bone joint)
        {
            List<String> names = new List<string>();
            joint.AllChildNames(names);
            // remove all the markers and meshs for the children of the bone.
            foreach (string child in names)
            {
                for (int i = 0; i < markers.Count; i++)
                {
                    if (markers[i].Name == child)
                    {
                        markers.RemoveAt(i);
                        break;
                    }
                }

                for (int i = 0; i < skeletonMeshs.Count; i++)
                {
                    if (skeletonMeshs[i].Name == child)
                    {
                        skeletonMeshs.RemoveAt(i);
                        i--;
                    }
                }
            }

            for (int i = 0; i < skeletonMeshs.Count; i++)
            {
                if (skeletonMeshs[i].Name == joint.Name)
                {
                    skeletonMeshs.RemoveAt(i);
                    i--;
                }
            }
            List<BoneDisplayRecord> brecs = new List<BoneDisplayRecord>();
            skeleton.GetSubPositions(brecs);

            foreach (string child in names)
            {
                foreach (BoneDisplayRecord p in brecs)
                {
                    if (p.Name == child)
                    {
                        CreateMarker(p.MarkerPosition, 4, Colors.Blue, p.Name, p.Bone);
                        CreateBoneModel(p);
                    }
                }
            }
            foreach (BoneDisplayRecord p in brecs)
            {
                if (p.Name == joint.Name)
                {
                    CreateBoneModel(p);
                }
            }
            Redisplay();
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

        private void ResetPoseClicked(object sender, RoutedEventArgs e)
        {

        }
        private void SavePoseClicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if ( dlg.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement docNode = doc.CreateElement("Pose");
                doc.AppendChild(docNode);
                skeleton.Save(doc, docNode);
                doc.Save(dlg.FileName);
            }
        }
    }
}