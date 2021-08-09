using Make3D.Dialogs.Figure;
using Make3D.Models;
using Make3D.Models.Adorners;
using Make3D.Object3DLib;
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
        private const string poseFilter = "Pose Files (*.pose)|*.pose";
        private Adorner adorner;
        private List<String> allBoneNames;
        private List<string> availableFigureModels = new List<string>();

        // Yes this tool does have its own doucment.
        // Its used to load the models that will be available
        private Document document;

        private bool editLimits;

        private List<Object3D> figureMeshs;

        private List<FigureModel> figureModels;
        private Point lastMousePos;

        private Visibility limitsVisible;

        private List<JointMarker> markers;

        private double maxiumXRot;

        private double maxiumYRot;

        private double maxiumZRot;

        private double minimumXRot;

        private double minimumYRot;

        private double minimumZRot;

        private List<ModelAssignmentControl> modelAssignments;
        private string poseFolder;

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
            figureMeshs = new List<Object3D>();
            document = new Document();
            adorner = null;
            EditLimits = false;
            LimitsVisible = Visibility.Hidden;
            poseFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            poseFolder = System.IO.Path.Combine(poseFolder, "Barnacle\\Poses");
            figureModels = new List<FigureModel>();
            availableFigureModels = new List<string>();
            modelAssignments = new List<ModelAssignmentControl>();
            NotificationManager.Subscribe("SelectedFigure", SelectedFigureModelChanged);
            NotificationManager.Subscribe("FigureScale", SelectedFigureScaleChanged);
            ModelGroup = MyModelGroup;
        }

        public List<String> AllBoneNames
        {
            get
            {
                return allBoneNames;
            }

            set
            {
                if (allBoneNames != value)
                {
                    allBoneNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<ModelAssignmentControl> AllModelAssignments
        {
            get
            {
                return modelAssignments;
            }
            set
            {
                if (value != modelAssignments)
                {
                    modelAssignments = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<String> AvailableFigureModels
        {
            get
            {
                return availableFigureModels;
            }
            set
            {
                if (availableFigureModels != value)
                {
                    availableFigureModels = value;
                    // NotifyPropertyChanged();
                }
            }
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

        public List<FigureModel> FigureModels
        {
            get
            {
                return figureModels;
            }
            set
            {
                if (figureModels != value)
                {
                    figureModels = value;
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
                    if (selectedMarker == null || selectedBone == null || selectedBone.Name != SelectedBoneName)
                    {
                        selectedMarker = FindMarkerForBone(selectedBoneName);
                        UpdateControlsForSelectedMarker();
                        UpdateDisplay();
                    }
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
            ClearFigure();
            ClearSkeleton();
            GenerateFigure();
            TransferFigureToVertices();
            CentreVertices();
            FloorVertices();
            DialogResult = true;
            Close();
        }

        protected override void Redisplay()
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
            figureMeshs.Clear();
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

        private void CreateBoneModel(BoneDisplayRecord p, bool setJointPointsForHull = false)
        {
            foreach (Object3D ob in document.Content)
            {
                if (ob.Name.ToLower() == p.ModelName.ToLower())
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
                    if (setJointPointsForHull)
                    {
                        p.SetJointEndPoints(cl.AbsoluteObjectVertices);
                    }
                    p.DisplayObject = cl;
                }
            }
        }

        private void CreateDefaultSkeleton()
        {
            skeleton = new Bone();
            skeleton.Name = "Root";
            skeleton.Length = 0;
            skeleton.Width = 5;
            skeleton.Height = 5;

            Bone neck = skeleton.AddSub("Neck", 3.0875, 8, 9, 0, 0, 90, -1, -1, -1, -1, -1, -1, "bone", "Neck1");

            Bone head = neck.AddSub("Head", 17, 14, 15, 0, -90, 0, -5, 5, 85, 95, 0, 0, "headbone", "malehead1");

            // spine
            Bone vert1 = skeleton.AddSub("Vert1", 18, 20, 24, 0, 0, 270, -5, 5, 85, 95, 0, 0, "bone", "chest1");

            Bone vert2 = vert1.AddSub("Vert2", 12, 16, 21, 0, 0, 0, -5, 5, 85, 95, 0, 0, "bone", "lowerchest1");

            Bone vert3 = vert2.AddSub("Vert3", 20, 19, 24, 0, 0, 0, -5, 5, 85, 95, 0, 0, "bone", "groin1");

            // Bone vert4 = vert3.AddSub("Vert4", 5, 5, 5, 0, 0, 0, -5, 5, 85, 95, 0, 0);

            // arms
            Bone rs = skeleton.AddSub("RightShoulder", 10, 5, 5, 0, 0, 202, -1, -1, -1, -1, -1, -1, "bone", "rightshoulder1");
            Bone rua = rs.AddSub("RightUpperArm", 21.757, 8, 9.667, -20, 30, -5, -1, -1, -1, -1, -1, -1, "bone", "upperrightarm1");
            Bone rla = rua.AddSub("RightLowerArm", 19.602, 6.55, 6.31, 0, 138.795784112359, -48.587292188853, -1, -1, -1, -1, -1, -1, "bone", "lowerrightarm1");
            Bone rh = rla.AddSub("RightHand", 15, 9, 9, -20, 0, 10, -1, -1, -1, -1, -1, -1, "righthandbone", "righthand1");

            Bone ls = skeleton.AddSub("LeftShoulder", 10, 5, 5, 0, 10, -22, -1, -1, -1, -1, -1, -1, "bone", "leftshoulder1");
            Bone lua = ls.AddSub("LeftUpperArm", 21.7, 8, 9.2, -20, -30, 5, -1, -1, -1, -1, -1, -1, "bone", "upperleftarm1");
            Bone lla = lua.AddSub("LeftLowerArm", 19, 6.5, 6.3, 0, -130, 40, -1, -1, -1, -1, -1, -1, "bone", "lowerleftarm1");
            Bone lh = lla.AddSub("LeftHand", 15, 8, 6, 0, -1.931111, -0.50878, -1, -1, -1, -1, -1, -1, "lefthandbone", "lefthand1");

            // pelvis
            Bone pr = vert3.AddSub("RightPelvis", 14, 5, 5, 0, 0, -135, -1, -1, -1, -1, -1, -1, "bone", "invisible");
            Bone rul = pr.AddSub("RightUpperLeg", 38, 15, 13, 2.68395891907361, -1.51219220499018, 131, -1, -1, -1, -1, -1, -1, "bone", "rightthigh1");
            Bone rll = rul.AddSub("RightLowerLeg", 30, 11, 9, 0, 270, -5, -1, -1, -1, -1, -1, -1, "bone", "lowerrightleg1");
            Bone rft = rll.AddSub("RightFoot", 19, 8, 5, 0, 0, 90, -1, -1, -1, -1, -1, -1, "footbone", "rightfoot1");

            Bone pl = vert3.AddSub("LeftPelvis", 14, 5, 5, 0, 0, 135, -1, -1, -1, -1, -1, -1, "bone", "invisible");
            Bone lul = pl.AddSub("LeftUpperLeg", 38, 15, 14, 2, 180, -130, -1, -1, -1, -1, -1, -1, "bone", "leftthigh1");
            Bone lll = lul.AddSub("LeftLowerLeg", 30, 11, 9, 14.93, 90, 0.5777, -1, -1, -1, -1, -1, -1, "bone", "lowerleftleg1");
            Bone lft = lll.AddSub("LeftFoot", 19, 8, 5, 0, 0, 90, -1, -1, -1, -1, -1, -1, "footbone", "leftfoot1");

            // force recalculation of positions.
            double y = vert1.Length + vert2.Length + vert3.Height + rul.Length + rll.Length + rft.Height - 10;

            // double y = 40; // remove me
            skeleton.StartPosition = new Point3D(0, y, 0);
            skeleton.EndPosition = new Point3D(0, y, 0);

            skeleton.Update();
            GetAllBoneNames(skeleton);
        }

        private void CreateJointModel(List<BoneDisplayRecord> brecs, Bone bn, Point3DCollection parentPnts)
        {
            //    Log($"CreateJoint Model {bn.FigureModelName}");
            if (parentPnts != null)
            {
                //  Log($" link parentPoints {bn.FigureModelName}");
                if (bn.DisplayRecord != null)
                {
                    GenerateJoint(parentPnts, bn.DisplayRecord);
                }
            }
            foreach (Bone sb in bn.SubBones)
            {
                CreateJointModel(brecs, sb, bn.DisplayRecord.EndJointPoints);
            }
        }

        private void CreateJointStartPoint(List<BoneDisplayRecord> brecs, Bone parent, Bone child)
        {
            if (parent != null)
            {
                BoneDisplayRecord parentBrec = FindBrec(brecs, parent.Name);
                BoneDisplayRecord childBrec = FindBrec(brecs, child.Name);
                if (parentBrec != null && childBrec != null)
                {
                    Object3D childOb = childBrec.DisplayObject;
                    if (childOb != null)
                    {
                        childBrec.SetJointStartPoints(parent.EndPosition, childOb.AbsoluteObjectVertices);
                    }
                }
            }
            foreach (Bone sub in child.SubBones)
            {
                CreateJointStartPoint(brecs, child, sub);
            }
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

        private BoneDisplayRecord FindBrec(List<BoneDisplayRecord> brecs, string name)
        {
            BoneDisplayRecord result = null;
            foreach (BoneDisplayRecord br in brecs)
            {
                if (br.Name == name)
                {
                    result = br;
                    break;
                }
            }
            return result;
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
            skeleton.Update();

            List<BoneDisplayRecord> brecs = new List<BoneDisplayRecord>();
            // true for figure
            skeleton.GetSubPositions(brecs, true);
            foreach (BoneDisplayRecord p in brecs)
            {
                CreateBoneModel(p, true);
            }
            // can only do the joints AFTER we have done the bones
            // also exclude the root.

            foreach (Bone bn in skeleton.SubBones)
            {
                CreateJointStartPoint(brecs, null, bn);
            }
            foreach (Bone bn in skeleton.SubBones)
            {
                CreateJointModel(brecs, bn, null);
            }
        }

        private void GenerateJoint(Point3DCollection sPnts, BoneDisplayRecord drec)
        {
            Object3D ob = new Object3D();
            ob.Position = new Point3D(drec.Bone.StartPosition.X, drec.Bone.StartPosition.Y, drec.Bone.StartPosition.Z);
            if (drec.Bone.DisplayRecord.DisplayObject != null)
            {
                ob.Color = drec.Bone.DisplayRecord.DisplayObject.Color;
            }
            else
            {
                ob.Color = Colors.Red;
            }
            foreach (Point3D p in sPnts)
            {
                ob.RelativeObjectVertices.Add(new Point3D(p.X - ob.Position.X, p.Y - ob.Position.Y, p.Z - ob.Position.Z));
            }
            foreach (Point3D p in drec.StartJointPoints)
            {
                ob.RelativeObjectVertices.Add(new Point3D(p.X - ob.Position.X, p.Y - ob.Position.Y, p.Z - ob.Position.Z));
            }

            // must have at least 4 points to do a hull
            if (ob.RelativeObjectVertices.Count > 4)
            {
                try
                {
                    ob.ConvertToHull();
                    skeletonMeshs.Add(ob);
                }
                catch { }
            }
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

        private void GetAllBoneNames(Bone skeleton)
        {
            allBoneNames = new List<string>();
            allBoneNames.Add(skeleton.Name);

            skeleton.AllChildNames(allBoneNames);
            allBoneNames.Sort();
            NotifyPropertyChanged("AllBoneNames");

            figureModels.Clear();
            //FigureModels.Add(new FigureModel(skeleton.Name, skeleton.FigureModelName));
            skeleton.AddFigureModels(figureModels);
            List<ModelAssignmentControl> fc = new List<ModelAssignmentControl>();
            foreach (FigureModel fm in figureModels)
            {
                ModelAssignmentControl mac = new ModelAssignmentControl();
                mac.BoneName = fm.BoneName;
                mac.FigureName = fm.FigureModelName;
                mac.AvailableFigureNames = availableFigureModels;
                mac.LScale = fm.LScale;
                mac.HScale = fm.HScale;
                mac.WScale = fm.WScale;
                fc.Add(mac);
            }
            AllModelAssignments = fc;
            NotifyPropertyChanged("AllModelAssignments");
            foreach (ModelAssignmentControl mac in modelAssignments)
            {
                mac.NotifyPropertyChanged("FigureName");
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
                        UpdateControlsForSelectedMarker();

                        selectedMarker.OnBoneRotated += OnBoneRotated;
                    }
                    base.Viewport_MouseDown(viewport3D1, e);
                    Redisplay();
                }
            }
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

                foreach (Object3D ob in document.Content)
                {
                    ob.CalcScale(false);
                    availableFigureModels.Add(ob.Name);
                    Scale3D p = ob.Scale;
                    if (p.X > 0 && p.Y > 0 && p.Z > 0)
                    {
                        ob.ScaleMesh(1.0 / p.X, 1.0 / p.Y, 1.0 / p.Z);
                    }
                }

                NotifyPropertyChanged("AvailableFigureModels");
            }
        }

        private void LoadPoseClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = poseFolder;
            dlg.Filter = poseFilter;
            if (dlg.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();

                doc.Load(dlg.FileName);
                XmlNode root = doc.FirstChild;
                XmlElement ele = root as XmlElement;
                double X = 0;
                double Y = 0;
                double Z = 0;
                if (ele.HasAttribute("X"))
                {
                    X = Convert.ToDouble(ele.GetAttribute("X"));
                    Y = Convert.ToDouble(ele.GetAttribute("Y"));
                    Z = Convert.ToDouble(ele.GetAttribute("Z"));
                }
                XmlNode nd = root.FirstChild;
                skeleton = new Bone();
                skeleton.StartPosition = new Point3D(X, Y, Z);
                skeleton.EndPosition = new Point3D(X, Y, Z);
                skeleton.Load(nd as XmlElement);
                GetAllBoneNames(skeleton);
                UpdateDisplay();
            }
        }

        private void OnBoneRotated(Bone bn)
        {
            SelectedXRot = bn.XRot;
            SelectedYRot = bn.YRot;
            SelectedZRot = bn.ZRot;
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

        private void ResetPoseClicked(object sender, RoutedEventArgs e)
        {
            CreateDefaultSkeleton();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
        }

        private void SavePoseClicked(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(poseFolder))
            {
                try
                {
                    Directory.CreateDirectory(poseFolder);
                }
                catch
                {
                    MessageBox.Show("Couldn't create Pose folder:" + poseFolder);
                }
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = poseFolder;
            dlg.Filter = poseFilter;
            if (dlg.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement docNode = doc.CreateElement("Pose");
                doc.AppendChild(docNode);
                docNode.SetAttribute("X", skeleton.StartPosition.X.ToString());
                docNode.SetAttribute("Y", skeleton.StartPosition.Y.ToString());
                docNode.SetAttribute("Z", skeleton.StartPosition.Z.ToString());
                skeleton.Save(doc, docNode);
                doc.Save(dlg.FileName);
            }
        }

        private void SelectedFigureModelChanged(object param)
        {
            ModelAssignmentControl mac = param as ModelAssignmentControl;
            if (mac != null)
            {
                if (skeleton.SetModelForBone(mac.BoneName, mac.FigureName))
                {
                    UpdateDisplay();
                }
            }
        }

        private void SelectedFigureScaleChanged(object param)
        {
            ModelAssignmentControl mac = param as ModelAssignmentControl;
            if (mac != null)
            {
                if (skeleton.SetScaleForBone(mac.BoneName, mac.LScale, mac.WScale, mac.HScale))
                {
                    UpdateDisplay();
                }
            }
        }

        private void TransferFigureToVertices()
        {
            int faceOffset = 0;
            foreach (Object3D ob in skeletonMeshs)
            {
                foreach (Point3D p in ob.AbsoluteObjectVertices)
                {
                    Vertices.Add(p);
                }
                foreach (int triV in ob.TriangleIndices)
                {
                    Faces.Add(triV + faceOffset);
                }
                faceOffset += ob.AbsoluteObjectVertices.Count;
            }
        }

        private void UpdateControlsForSelectedMarker()
        {
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

            NotifyPropertyChanged("FigureModels");
        }

        private void XrMinus(object sender, RoutedEventArgs e)
        {
            SelectedXRot = SelectedXRot - 5;
        }

        private void XrPlus(object sender, RoutedEventArgs e)
        {
            SelectedXRot = SelectedXRot + 5;
        }

        private void YrMinus(object sender, RoutedEventArgs e)
        {
            SelectedYRot = SelectedYRot - 5;
        }

        private void YrPlus(object sender, RoutedEventArgs e)
        {
            SelectedYRot = SelectedYRot + 5;
        }

        private void ZrMinus(object sender, RoutedEventArgs e)
        {
            SelectedZRot = SelectedZRot - 5;
        }

        private void ZrPlus(object sender, RoutedEventArgs e)
        {
            SelectedZRot = SelectedZRot + 5;
        }
    }
}