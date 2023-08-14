using Barnacle.Dialogs;
using Barnacle.Dialogs.Slice;
using Barnacle.EditorParameterLib;
using Barnacle.Models;
using Barnacle.Models.Adorners;
using Barnacle.Models.LoopSmoothing;
using Barnacle.Object3DLib;
using Barnacle.ViewModel.BuildPlates;
using CSGLib;
using FixLib;
using HoleLibrary;
using ManifoldLib;
using MeshDecimator;
using Microsoft.Win32;
using PlacementLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private static CancellationTokenSource csgCancelation;

        // some of the primitives need to be rotated when they are first created so they match the
        // orientaion shown on the icons
        private static string[] rotatedPrimitives =
        {
            "roof","cone","pyramid","roundroof","cap","polygon","rightangle","pointy"
        };

        private Bounds3D allBounds;

        private Axies axies;

        private PolarCamera camera;

        private Point3D CameraLookObject = new Point3D(0, 0, 0);

        private CameraModes cameraMode;

        private Point3D CameraScrollDelta = new Point3D(1, 1, 0);

        private Floor floor;

        private FloorMarker floorMarker;

        private Grid3D grid;

        private bool isEditingEnabled;

        private double keyrotationx = 90;

        private double keyrotationy = 90;

        private double keyrotationz = 90;

        private Point lastMouse;

        private Vector3D lookDirection;

        private Model3DCollection modelItems;

        private double onePercentZoom;

        private PrinterPlate printerPlate;

        private List<Object3D> selectedItems;

        private Adorner selectedObjectAdorner;

        private bool showAdorners;

        private bool showAxies;

        private bool showBuildPlate;

        private bool showFloor;

        private bool showFloorMarker;

        private int totalFaces;

        private double zoomPercent = 100;

        public EditorViewModel()
        {
            floor = new Floor();
            axies = new Axies();
            printerPlate = new PrinterPlate();
            floorMarker = null;
            grid = new Grid3D();

            camera = new PolarCamera();

            onePercentZoom = camera.Distance / 100.0;
            LookToCenter();

            cameraMode = CameraModes.CameraMoveLookCenter;

            modelItems = new Model3DCollection();

            modelItems.Add(floor.FloorMesh);
            NotificationManager.Subscribe("Editor", "AutoFix", AutoFix);
            NotificationManager.Subscribe("Editor", "ZoomIn", ZoomIn);
            NotificationManager.Subscribe("Editor", "FixHoles", FixHoles);
            NotificationManager.Subscribe("Editor", "ZoomOut", ZoomOut);
            NotificationManager.Subscribe("Editor", "ZoomReset", ZoomReset);
            NotificationManager.Subscribe("Editor", "CameraCommand", OnCameraCommand);
            NotificationManager.Subscribe("Editor", "AddObject", OnAddObject);
            NotificationManager.Subscribe("Editor", "Refresh", OnRefresh);
            NotificationManager.Subscribe("Editor", "RefreshAdorners", OnRefreshAdorners);
            NotificationManager.Subscribe("Editor", "NewDocument", OnNewDocument);
            NotificationManager.Subscribe("Editor", "Remesh", OnRemesh);
            NotificationManager.Subscribe("Editor", "Select", Select);
            NotificationManager.SubscribeTask("Editor", "Group", OnGroup);
            NotificationManager.Subscribe("Editor", "Cut", OnCut);
            NotificationManager.Subscribe("Editor", "Copy", OnCopy);
            NotificationManager.Subscribe("Editor", "CloneInPlace", OnCloneInPlace);
            NotificationManager.Subscribe("Editor", "Paste", OnPaste);
            NotificationManager.Subscribe("Editor", "PasteAt", OnPasteAt);
            NotificationManager.Subscribe("Editor", "DoMultiPaste", OnMultiPaste);
            NotificationManager.Subscribe("Editor", "CircularPaste", OnCircularPaste);

            NotificationManager.Subscribe("Editor", "Export", OnExport);
            NotificationManager.Subscribe("Editor", "ExportParts", OnExportParts);
            NotificationManager.Subscribe("Editor", "Slice", OnSlice);
            NotificationManager.Subscribe("Editor", "Split", OnSplit);
            NotificationManager.Subscribe("Editor", "Bend", OnBend);
            NotificationManager.Subscribe("Editor", "Fold", OnFold);
            NotificationManager.Subscribe("Editor", "Import", OnImport);
            NotificationManager.Subscribe("Editor", "MoveObjectToFloor", OnMoveObjectToFloor);
            NotificationManager.Subscribe("Editor", "MoveObjectToCentre", OnMoveObjectToCentre);
            NotificationManager.Subscribe("Editor", "Marker", MoveToMarker);
            NotificationManager.Subscribe("Editor", "Alignment", OnAlignment);
            NotificationManager.Subscribe("Editor", "Distribute", OnDistribute);
            NotificationManager.Subscribe("Editor", "Flip", OnFlip);
            NotificationManager.Subscribe("Editor", "LoopSmooth", OnLoopSmooth);
            NotificationManager.Subscribe("Editor", "Reorigin", OnReorigin);

            NotificationManager.Subscribe("Editor", "Size", OnSize);
            NotificationManager.Subscribe("Editor", "Undo", OnUndo);

            NotificationManager.Subscribe("Editor", "BezierFuselage", OnFuselage);

            NotificationManager.Subscribe("Editor", "MeshEdit", OnMeshEdit);
            NotificationManager.Subscribe("Editor", "MeshHull", OnMeshHull);

            NotificationManager.Subscribe("Editor", "ShowFloor", OnShowFloor);
            NotificationManager.Subscribe("Editor", "ShowBuildPlate", OnShowBuildPlate);
            NotificationManager.Subscribe("Editor", "ShowFloorMarker", OnShowFloorMarker);
            NotificationManager.Subscribe("Editor", "ShowAxies", OnShowAxies);
            NotificationManager.Subscribe("Editor", "SelectObjectName", SelectObjectByName);

            NotificationManager.Subscribe("Editor", "Tool", OnTool);

            NotificationManager.Subscribe("Editor", "ManifoldTest", OnManifoldTest);
            NotificationManager.Subscribe("Editor", "RemoveDupVertices", OnRemoveDupVertices);
            NotificationManager.Subscribe("Editor", "UnrefVertices", OnRemoveUnrefVertices);
            NotificationManager.Subscribe("Editor", "Loading", LoadingNewFile);

            NotificationManager.Subscribe("Editor", "BuildPlate", BuildPlateChanged);

            NotificationManager.Subscribe("Editor", "ObjectXRotationChange", XRotationChanged);
            NotificationManager.Subscribe("Editor", "ObjectYRotationChange", YRotationChanged);
            NotificationManager.Subscribe("Editor", "ObjectZRotationChange", ZRotationChanged);

            NotificationManager.Subscribe("Editor", "AddObjectToLibrary", OnAddObjectToLibrary);
            NotificationManager.Subscribe("Editor", "Mirror", OnMirror);

            ReportCameraPosition();
            selectedItems = new List<Object3D>();
            allBounds = new Bounds3D();
            allBounds.Adjust(new Point3D(0, 0, 0));
            totalFaces = 0;
            showAxies = true;
            showFloor = true;
            showAdorners = true;
            showFloorMarker = true;
            showBuildPlate = true;
            isEditingEnabled = true;
            CheckForScriptResults();
            RegenerateDisplayList();
        }

        private void OnMirror(object param)
        {
            string s = param.ToString();
            if (selectedItems == null || selectedItems.Count != 1)
            {
                MessageBox.Show("Mirror operation requires a single selected object");
            }
            else
            {
                MirrorObject(s);
            }
        }

        private void MirrorObject(string s)
        {
            bool confirmed = true;
            Object3D ob = selectedObjectAdorner.SelectedObjects[0];
            if (ob is Group3D)
            {
                MessageBoxResult res = MessageBox.Show("The object will have to be converted to a mesh first. Convert now.", "Warning", MessageBoxButton.OKCancel);
                confirmed = res == MessageBoxResult.OK;
                if (confirmed)
                {
                    Document.Content.Remove(ob);

                    Object3D it = ob.ConvertToMesh();
                    it.Remesh();
                    Document.Content.Add(it);
                    Document.Dirty = true;
                    ob = it;
                }
            }
            if (confirmed)
            {
                CheckPoint();
                Bounds3D bnds = new Bounds3D();

                foreach (P3D p in ob.RelativeObjectVertices)
                {
                    bnds.Adjust(new Point3D((double)p.X, (double)p.Y, (double)p.Z));
                }
                int numPoints = ob.RelativeObjectVertices.Count;
                int numFaces = ob.TriangleIndices.Count;
                switch (s.ToLower())
                {
                    case "left":
                        {
                            double ox = 2 * bnds.Lower.X;
                            for (int i = 0; i < numPoints; i++)
                            {
                                P3D op = ob.RelativeObjectVertices[i];
                                P3D np = new P3D(ox - op.X, op.Y, op.Z);
                                ob.RelativeObjectVertices.Add(np);
                            }

                            for (int f = 0; f < numFaces; f += 3)
                            {
                                int v0 = ob.TriangleIndices[f] + numPoints;
                                int v1 = ob.TriangleIndices[f + 1] + numPoints;
                                int v2 = ob.TriangleIndices[f + 2] + numPoints;
                                ob.TriangleIndices.Add(v0);
                                ob.TriangleIndices.Add(v2);
                                ob.TriangleIndices.Add(v1);
                            }
                        }
                        break;

                    case "right":
                        {
                            double ox = bnds.Upper.X + bnds.Width + bnds.Lower.X;
                            for (int i = 0; i < numPoints; i++)
                            {
                                P3D op = ob.RelativeObjectVertices[i];
                                P3D np = new P3D(ox - op.X, op.Y, op.Z);
                                ob.RelativeObjectVertices.Add(np);
                            }

                            for (int f = 0; f < numFaces; f += 3)
                            {
                                int v0 = ob.TriangleIndices[f] + numPoints;
                                int v1 = ob.TriangleIndices[f + 1] + numPoints;
                                int v2 = ob.TriangleIndices[f + 2] + numPoints;
                                ob.TriangleIndices.Add(v0);
                                ob.TriangleIndices.Add(v2);
                                ob.TriangleIndices.Add(v1);
                            }
                        }
                        break;
                }
                RemoveDuplicateVertices(ob);
                ob.Remesh();

                ob.CalcScale(false);
                allBounds += ob.AbsoluteBounds;
                GeometryModel3D gm = GetMesh(ob);
                RegenerateDisplayList();
            }
        }

        private void AutoFix(object param)
        {
            AutoFixDlg dlg = new AutoFixDlg();
            dlg.ShowDialog();
        }

        private void FixHoles(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                if (ob != null)
                {
                    CheckPoint();
                    HoleFinder hf = new HoleFinder(ob.RelativeObjectVertices, ob.TriangleIndices);
                    Tuple<int, int> res = hf.FindHoles();

                    ob.Remesh();

                    ob.CalcScale(false);
                    allBounds += ob.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(ob);
                    Document.Dirty = true;

                    selectedObjectAdorner.Clear();
                    selectedObjectAdorner.AdornObject(ob);
                    RegenerateDisplayList();
                    MessageBox.Show($"Found {res.Item1.ToString()} holes, Filled {res.Item2.ToString()}", "Information");
                }
            }
            else
            {
                MessageBox.Show("Must have a single object selected", "Error");
            }
        }

        private void OnCloneInPlace(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                if (ob != null)
                {
                    CheckPoint();
                    Object3D o = ob.Clone();
                    o.Name = Document.DuplicateName(o.Name);

                    o.Remesh();
                    o.MoveToFloor();
                    o.CalcScale(false);
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;

                    selectedObjectAdorner.Clear();
                    selectedObjectAdorner.AdornObject(o);
                    RegenerateDisplayList();
                    NotificationManager.Notify("ObjectNamesChanged", null);
                    NotificationManager.Notify("ObjectSelected", o);
                    PassOnGroupStatus(o);
                }
            }
            else
            {
                MessageBox.Show("Must have a single object selected", "Error");
            }
        }

        private void PassOnGroupStatus(Object3D o)
        {
            if (o is Group3D)
            {
                NotificationManager.Notify("GroupSelected", true);
            }
            else
            {
                NotificationManager.Notify("GroupSelected", false);
            }
        }

        private void OnAddObjectToLibrary(object param)
        {
            string libPath = (param as string);
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                if (ob != null)
                {
                    Object3D ob2 = ob.Clone();
                    if (ob2.PrimType != "Mesh")
                    {
                        ob2 = ob2.ConvertToMesh();
                    }
                    ob2.Color = Colors.SkyBlue;
                    ob2.MoveToCentre();
                    ob2.MoveToFloor();
                    LibrarySnapShotDlg dlg = new LibrarySnapShotDlg();
                    dlg.Part = ob2;
                    string pth = System.IO.Path.GetDirectoryName(GetPartsLibraryPath()) + libPath;
                    dlg.PartPath = pth;
                    dlg.PartProjectSection = libPath;
                    if (dlg.ShowDialog() == true)
                    {
                        // reminder the contents of the partslibrary that they are in a library
                        BaseViewModel.PartLibraryProject.LibraryAdd = true;
                    }
                }
                else
                {
                    MessageBox.Show("Must have a single object selected", "Error");
                }
            }
        }

        private enum CameraModes
        {
            None,
            CameraMove,
            CameraMoveLookCenter,
            CameraMoveLookObject
        }

        // used when user trys to rotate an object using keyboard shortcuts
        private enum KeyboardRotation
        {
            None,
            x1,
            x2,
            y1,
            y2,
            z1,
            z2
        }

        /// <summary>
        /// Current position of camera
        /// </summary>
        public Point3D CameraPos
        {
            get { return camera.CameraPos; }
            set { NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Direction that camera is looking
        /// </summary>
        public Vector3D LookDirection
        {
            get
            {
                return lookDirection;
            }
            set
            {
                if (lookDirection != value)
                {
                    lookDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Items to be displayed in main editor area
        /// </summary>
        public Model3DCollection ModelItems
        {
            get
            {
                return modelItems;
            }
            set
            {
                if (modelItems != value)
                {
                    modelItems = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Canvas to draw things like text labels on
        /// </summary>
        public Canvas Overlay { get; internal set; }

        /// <summary>
        /// Calculate total number of faces of all objects
        /// </summary>
        public void CountFaces()
        {
            totalFaces = 0;
            foreach (Object3D ob in Document.Content)
            {
                totalFaces += ob.TotalFaces;
            }
        }

        /// <summary>
        /// Regenerate display in response to something outside this view model
        /// requesting it
        /// </summary>
        /// <param name="param"></param>
        public void OnRefreshAdorners(object param)
        {
            allBounds = new Bounds3D();
            allBounds.Zero();
            modelItems.Clear();
            if (showFloor)
            {
                modelItems.Add(floor.FloorMesh);
                modelItems.Add(grid.Group);
                if (floorMarker != null)
                {
                    foreach (GeometryModel3D m in floorMarker.MarkerMesh.Children)
                    {
                        modelItems.Add(m);
                    }
                }
            }
            if (showBuildPlate)
            {
                foreach (GeometryModel3D m in printerPlate.Group.Children)
                {
                    modelItems.Add(m);
                }
            }
            if (showAxies)
            {
                foreach (GeometryModel3D m in axies.Group.Children)
                {
                    modelItems.Add(m);
                }
            }
            foreach (Object3D ob in Document.Content)
            {
                GeometryModel3D gm = GetMesh(ob);
                modelItems.Add(gm);
                allBounds += ob.AbsoluteBounds;
            }
            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }
            NotifyPropertyChanged("ModelItems");
        }

        /// <summary>
        /// Regenerate the main display list. That is all the
        /// things that are to be shown in the main area. This will
        /// most likely be trigged by something happening in this viewmodel rather
        /// than elsewhere
        /// </summary>
        public void RegenerateDisplayList()
        {/*
            AdaptiveSignedDistanceField adfTest = new AdaptiveSignedDistanceField();
            AdaptiveSignedDistanceField.OnCalculateDistance += asdflibrary.Functions.Sphere;
            asdflibrary.Functions.SphereRadius = 5;
            adfTest.SetDimensions(-10F, -10F, -10F, 10F, 10F, 10F);
            adfTest.Dump();
            adfTest.SplitRoot();
            System.Diagnostics.Debug.WriteLine("After split");
            adfTest.Dump();
            Object3D cb = new Object3D();
            Point3DCollection pnts = new Point3DCollection();
            Int32Collection tris = new Int32Collection();
            adfTest.GetCube(7, pnts, tris);
            cb.TriangleIndices = tris;
            foreach( Point3D p in pnts)
            {
                cb.RelativeObjectVertices.Add(new P3D(p.X, p.Y, p.Z));
            }
            cb.Remesh();
            */
            try
            {
                // CountFaces();
                allBounds = new Bounds3D();
                allBounds.Zero();
                modelItems.Clear();

                // GeometryModel3D gmcb = GetMesh(cb);
                //   modelItems.Add(gmcb);
                if (showFloor)
                {
                    modelItems.Add(floor.FloorMesh);
                    foreach (GeometryModel3D m in grid.Group.Children)
                    {
                        modelItems.Add(m);
                    }
                    if (floorMarker != null && showFloorMarker)
                    {
                        foreach (GeometryModel3D m in floorMarker.MarkerMesh.Children)
                        {
                            modelItems.Add(m);
                        }
                    }
                }
                if (showBuildPlate)
                {
                    foreach (GeometryModel3D m in printerPlate.Group.Children)
                    {
                        modelItems.Add(m);
                    }
                }
                if (showAxies)
                {
                    foreach (GeometryModel3D m in axies.Group.Children)
                    {
                        modelItems.Add(m);
                    }
                }
                totalFaces = 0;
                foreach (Object3D ob in Document.Content)
                {
                    totalFaces += ob.TotalFaces;

                    GeometryModel3D gm = GetMesh(ob);
                    modelItems.Add(gm);
                    allBounds += ob.AbsoluteBounds;
                }
                if (showAdorners)
                {
                    if (selectedObjectAdorner != null)
                    {
                        foreach (Model3D md in selectedObjectAdorner.Adornments)
                        {
                            modelItems.Add(md);
                        }
                    }
                }
                NotifyPropertyChanged("ModelItems");
                ReportStatistics();
            }
            catch
            {
            }
        }

        internal void DeselectAll()
        {
            NotificationManager.Notify("SetToolsVisibility", true);
            if (selectedObjectAdorner != null)
            {
                selectedObjectAdorner.Clear();
            }

            selectedItems.Clear();
            Overlay.Children.Clear();
            RegenerateDisplayList();
            NotificationManager.Notify("ObjectSelected", null);
            NotificationManager.Notify("GroupSelected", false);
        }

        internal void KeyDown(Key key, bool shift, bool ctrl)
        {
            if (isEditingEnabled)
            {
                HandleKeyWhenEditingIsEnabled(key, shift, ctrl);
            }
            else
            {
                HandleKeyWhenEditingDisabled(key);
            }
        }

        private void HandleKeyWhenEditingIsEnabled(Key key, bool shift, bool ctrl)
        {
            switch (key)
            {
                case Key.Up:
                    {
                        if (selectedObjectAdorner != null)
                        {
                            CheckPointForNudge();
                            if (ctrl)
                            {
                                if (shift)
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Back, 0.1);
                                }
                                else
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Back, 1.0);
                                }
                            }
                            else
                            {
                                if (shift)
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Up, 0.1);
                                }
                                else
                                {
                                    // If R is down treat as rotate
                                    if (Keyboard.IsKeyDown(Key.R))
                                    {
                                        KeyboardRotation rd = GetRotationDirection(Key.Up);
                                        OnKeyRotate(rd);
                                    }
                                    else
                                    {
                                        selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Up, 1.0);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case Key.Down:
                    {
                        if (selectedObjectAdorner != null)
                        {
                            CheckPointForNudge();
                            if (ctrl)
                            {
                                if (shift)
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Forward, 0.1);
                                }
                                else
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Forward, 1.0);
                                }
                            }
                            else
                            {
                                if (shift)
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Down, 0.1);
                                }
                                else
                                {
                                    // If R is down treat as rotate
                                    if (Keyboard.IsKeyDown(Key.R))
                                    {
                                        KeyboardRotation rd = GetRotationDirection(Key.Down);
                                        OnKeyRotate(rd);
                                    }
                                    else
                                    {
                                        selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Down, 1.0);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case Key.Left:
                    {
                        if (selectedObjectAdorner != null)
                        {
                            CheckPointForNudge();
                            if (shift)
                            {
                                selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Left, 0.1);
                            }
                            else
                            {
                                // If R is down treat as rotate
                                if (Keyboard.IsKeyDown(Key.R))
                                {
                                    bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                                    KeyboardRotation rd = GetRotationDirection(Key.Left, ctrlDown);
                                    OnKeyRotate(rd);
                                }
                                else
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Left, 1.0);
                                }
                            }
                        }
                    }
                    break;

                case Key.Right:
                    {
                        if (selectedObjectAdorner != null)
                        {
                            CheckPointForNudge();
                            if (shift)
                            {
                                selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Right, 0.1);
                            }
                            else
                            {
                                if (Keyboard.IsKeyDown(Key.R))
                                {
                                    bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                                    KeyboardRotation rd = GetRotationDirection(Key.Right, ctrlDown);
                                    OnKeyRotate(rd);
                                }
                                else
                                {
                                    selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Right, 1.0);
                                }
                            }
                        }
                    }
                    break;

                case Key.A:
                    {
                        if (ctrl)
                        {
                            SelectAll();
                        }
                    }
                    break;

                case Key.C:
                    {
                        if (ctrl)
                        {
                            OnCopy(null);
                        }
                        else
                        {
                            CheckPoint();
                            MoveSelectionToCentre();
                        }
                    }
                    break;

                case Key.E:
                    {
                        CheckEditSelection();
                    }
                    break;

                case Key.V:
                    {
                        if (ctrl)
                        {
                            OnPaste(null);
                        }
                    }
                    break;

                case Key.F:
                    {
                        if (ctrl)
                        {
                            CheckPoint();
                            FloorAllObjects();
                            RegenerateDisplayList();
                        }
                        else
                        {
                            CheckPoint();
                            AlignSelectedObjects("Floor");
                            RegenerateDisplayList();
                        }
                    }
                    break;

                case Key.K:
                    {
                        OnCloneInPlace(null);
                    }
                    break;

                case Key.M:
                    {
                        CheckPoint();
                        MoveToMarker(null);
                        RegenerateDisplayList();
                    }
                    break;

                case Key.Delete:
                    {
                        OnCut(null);
                    }
                    break;

                case Key.Z:
                    {
                        if (ctrl)
                        {
                            Undo();
                        }
                        else
                        {
                            CheckPoint();
                            MoveSelectionToZero();
                        }
                    }
                    break;

                case Key.H:
                    {
                        showAdorners = false;
                        RegenerateDisplayList();
                    }
                    break;

                case Key.Home:
                    {
                        HomeCamera();
                    }
                    break;

                case Key.Escape:
                    {
                        if (csgCancelation != null && !csgCancelation.IsCancellationRequested)
                        {
                            csgCancelation.Cancel();
                        }
                    }
                    break;

                case Key.F5:
                    {
                        RotateCamera(-0.5, 0.0);
                    }
                    break;

                case Key.F6:
                    {
                        RotateCamera(0.5, 0.0);
                    }
                    break;

                case Key.F7:
                    {
                        RotateCamera(0.0, -0.5);
                    }
                    break;

                case Key.F8:
                    {
                        RotateCamera(0.0, 0.5);
                    }
                    break;
            }
        }

        private void HandleKeyWhenEditingDisabled(Key key)
        {
            switch (key)
            {
                // if editing is disable, ignore all keys except escape ( which may enale it again)
                case Key.Escape:
                    {
                        if (csgCancelation != null && !csgCancelation.IsCancellationRequested)
                        {
                            csgCancelation.Cancel();
                        }
                    }
                    break;

                case Key.Home:
                    {
                        HomeCamera();
                    }
                    break;

                case Key.F5:
                    {
                        RotateCamera(-0.5, 0.0);
                    }
                    break;

                case Key.F6:
                    {
                        RotateCamera(0.5, 0.0);
                    }
                    break;

                case Key.F7:
                    {
                        RotateCamera(0.0, -0.5);
                    }
                    break;

                case Key.F8:
                    {
                        RotateCamera(0.0, 0.5);
                    }
                    break;
            }
        }

        internal void KeyUp(Key key, bool shift, bool ctrl)
        {
            switch (key)
            {
                case Key.H:
                    {
                        showAdorners = true;
                        RegenerateDisplayList();
                    }
                    break;
            }
        }

        internal void MouseDown(System.Windows.Point lastMousePos, MouseButtonEventArgs e)
        {
            lastMouse = lastMousePos;
        }

        internal void MouseMove(System.Windows.Point newPos, MouseEventArgs e)
        {
            bool ctrlDown = false;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ctrlDown = true;
            }
            if (isEditingEnabled && selectedObjectAdorner != null && selectedObjectAdorner.MouseMove(lastMouse, newPos, e, ctrlDown) == true)
            {
                lastMouse = newPos;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (cameraMode == CameraModes.CameraMove)
                    {
                        MoveCameraDelta(lastMouse, newPos);
                        lastMouse = newPos;
                    }
                    else if (cameraMode == CameraModes.CameraMoveLookCenter)
                    {
                        MoveCameraDelta(lastMouse, newPos);
                        LookToCenter();
                        lastMouse = newPos;
                    }
                    else if (cameraMode == CameraModes.CameraMoveLookObject)
                    {
                        MoveCameraDelta(lastMouse, newPos);
                        LookToObject();
                        lastMouse = newPos;
                    }
                }
            }
        }

        internal void MouseUp(System.Windows.Point lastMousePos, MouseButtonEventArgs e)
        {
            if (isEditingEnabled && selectedObjectAdorner != null)
            {
                selectedObjectAdorner.MouseUp();
            }
            ReportCameraPosition();
        }

        internal void MouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomIn(null);
            }
            else
            {
                ZoomOut(null);
            }
        }

        internal void Select(GeometryModel3D geo, Point3D hitPos, bool size, bool append, bool control)
        {
            if (isEditingEnabled)
            {
                bool handled = false;
                NotificationManager.Notify("SetToolsVisibility", false);

                if (selectedObjectAdorner != null)
                {
                    handled = selectedObjectAdorner.Select(geo);
                }
                if (!handled)
                {
                    handled = CheckIfContentSelected(geo, append, size, control);
                    if (handled)
                    {
                        SetSelectionColours();
                    }
                }
                if (!handled)
                {
                    if (floor.Matches(geo) || grid.Matches(geo))
                    {
                        if (selectedObjectAdorner != null)
                        {
                            selectedObjectAdorner.Clear();
                            Overlay.Children.Clear();
                            NotificationManager.Notify("ObjectSelected", null);
                            NotificationManager.Notify("GroupSelected", false);
                        }
                        floorMarker = new FloorMarker();
                        floorMarker.Position = hitPos;
                        RegenerateDisplayList();
                    }
                }

                ShowToolForCurrentSelection();
            }
        }

        private static void CleanFolder(string pth, string subName, string search)
        {
            string targetFolder = pth + System.IO.Path.DirectorySeparatorChar + subName;
            if (Directory.Exists(targetFolder))
            {
                string[] filepaths = Directory.GetFiles(pth + System.IO.Path.DirectorySeparatorChar + subName, search);
                foreach (string f in filepaths)
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch
                    {
                    }
                }
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

        private static Mesh ObjectMeshToDecimatorMesh(Object3D ob)
        {
            Mesh mesh;
            MeshDecimator.Math.Vector3d[] vex = new MeshDecimator.Math.Vector3d[ob.RelativeObjectVertices.Count];
            int i = 0;
            //foreach (Point3D pnt in ob.RelativeObjectVertices)
            foreach (P3D pnt in ob.RelativeObjectVertices)
            {
                MeshDecimator.Math.Vector3d v = new MeshDecimator.Math.Vector3d(pnt.X, pnt.Y, pnt.Z);
                vex[i] = v;
                i++;
            }

            int[] tris = new int[ob.TriangleIndices.Count];
            for (int j = 0; j < ob.TriangleIndices.Count; j++)
            {
                tris[j] = ob.TriangleIndices[j];
            }
            mesh = new MeshDecimator.Mesh(vex, tris);
            return mesh;
        }

        private int RemoveDuplicateVertices(Object3D ob)
        {
            CheckPoint();
            int numberRemoved = ob.RelativeObjectVertices.Count;
            Fixer checker = new Fixer();
            Point3DCollection points = new Point3DCollection();
            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, points);

            checker.RemoveDuplicateVertices(points, ob.TriangleIndices);
            PointUtils.PointCollectionToP3D(checker.Vertices, ob.RelativeObjectVertices);
            ob.TriangleIndices = checker.Faces;
            ob.Remesh();
            numberRemoved -= ob.RelativeObjectVertices.Count;
            return numberRemoved;
        }

        private static void RemoveUnrefVertices(Object3D ob)
        {
            ManifoldChecker checker = new ManifoldChecker();

            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, checker.Points);
            checker.Indices = ob.TriangleIndices;
            checker.RemoveUnreferencedVertices();

            PointUtils.PointCollectionToP3D(checker.Points, ob.RelativeObjectVertices);
            ob.TriangleIndices = checker.Indices;
            ob.Remesh();
        }

        private void AlignSelectedObjects(string s)
        {
            if (s == "Floor")
            {
                FloorSelectedObjects();
            }
            else
            if (selectedObjectAdorner.SelectedObjects.Count > 0)
            {
                Bounds3D bns = new Bounds3D(selectedObjectAdorner.SelectedObjects[0].AbsoluteBounds);
                double midX = bns.MidPoint().X;
                double midY = bns.MidPoint().Y;
                double midZ = bns.MidPoint().Z;
                // adorner  should already have the bounds of the selected objects

                for (int i = 1; i < selectedObjectAdorner.SelectedObjects.Count; i++)
                {
                    Object3D ob = selectedObjectAdorner.SelectedObjects[i];
                    double dAbsX = 0;
                    double dAbsY = 0;
                    double dAbsZ = 0;
                    switch (s)
                    {
                        case "Left":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.Lower.X - bns.Lower.X);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                ob.Remesh();
                            }
                            break;

                        case "Right":
                            {
                                dAbsX = ob.Position.X + (bns.Upper.X - ob.AbsoluteBounds.Upper.X);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                ob.Remesh();
                            }
                            break;

                        case "Top":
                            {
                                dAbsY = ob.Position.Y + (bns.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                ob.Remesh();
                            }
                            break;

                        case "Bottom":
                            {
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Lower.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                ob.Remesh();
                            }
                            break;

                        case "Back":
                            {
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Lower.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                ob.Remesh();
                            }
                            break;

                        case "Front":
                            {
                                dAbsZ = ob.Position.Z + (bns.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                ob.Remesh();
                            }
                            break;

                        case "Centre":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - bns.MidPoint().Z);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, dAbsZ);
                                ob.Remesh();
                            }
                            break;

                        case "StackY":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Upper.Y) - 0.001;
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackBottom":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = bns.Lower.Y - (ob.AbsoluteBounds.Height / 2) + 0.001;
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackRight":
                            {
                                dAbsX = bns.Upper.X + ob.AbsoluteBounds.Width / 2 - 0.001;
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackLeft":
                            {
                                dAbsX = bns.Lower.X - ob.AbsoluteBounds.Width / 2 + 0.001;
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackZ":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Upper.Z) - 0.001;
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackBehind":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);

                                dAbsZ = bns.Lower.Z - (ob.AbsoluteBounds.Depth / 2) + 0.001;
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.Remesh();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "Marker":
                            {
                                if (floorMarker != null)
                                {
                                    Point3D mp = floorMarker.Position;
                                    dAbsX = mp.X + (ob.AbsoluteBounds.MidPoint().X - midX);
                                    dAbsY = mp.Y + (ob.AbsoluteBounds.MidPoint().Y - midY);
                                    dAbsZ = mp.Z + (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                    ob.Remesh();
                                }
                            }
                            break;
                    }
                }
            }
            selectedObjectAdorner.GenerateAdornments();
        }

        private void BackCamera()
        {
            ResetSelection();
            camera.HomeBack();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void BendObjectInHalf(Object3D ob, string ori, bool fold = false)
        {
            double smallObjectsLimit = 1000;
            double bendAngle = 10 * Math.PI / 180.0;
            if (ob != null)
            {
                // objects with a small number of vertices dont bend well
                if (ob.RelativeObjectVertices.Count < smallObjectsLimit)
                {
                    LoopSmoother cms = new LoopSmoother();

                    Point3DCollection p3col = new Point3DCollection();
                    PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, p3col);
                    Int32Collection icol = ob.TriangleIndices;
                    while (p3col.Count < smallObjectsLimit)
                    {
                        cms.Subdivide(ref p3col, ref icol);
                    }
                    PointUtils.PointCollectionToP3D(p3col, ob.RelativeObjectVertices);
                    ob.TriangleIndices = icol;
                    ob.Remesh();
                }
                // calculate the bend forces that will applied to the points
                int numForces = 100;
                double limit = numForces * numForces;
                double[] forces = new double[numForces];

                for (int i = 0; i < numForces; i++)
                {
                    // forces[i] = Math.Cos(theta)* Math.Cos(theta);
                    if (fold)
                    {
                        forces[i] = 1.0;
                    }
                    else
                    {
                        forces[i] = (double)(i * i) / limit;
                    }
                }
                //
                switch (ori)
                {
                    case "XDown":
                    case "XUp":
                        {
                            double halfWidth = ob.Scale.Z / 2.0;
                            for (int i = 0; i < ob.RelativeObjectVertices.Count; i++)
                            {
                                double relDist = Math.Abs(ob.RelativeObjectVertices[i].Z / halfWidth);
                                int forcesIndex = (int)(relDist * (numForces - 1));
                                if (forcesIndex >= 0 && forcesIndex < numForces)
                                {
                                    double sn = -Math.Sign(ob.RelativeObjectVertices[i].Z);
                                    double y = ob.RelativeObjectVertices[i].Y;
                                    double z = ob.RelativeObjectVertices[i].Z;
                                    double dtheta = forces[forcesIndex] * bendAngle;
                                    double theta = Math.Atan2(ob.RelativeObjectVertices[i].Y, ob.RelativeObjectVertices[i].Z);
                                    double r = Math.Sqrt(y * y + z * z);
                                    double nz;
                                    double ny;
                                    if (ori == "XDown")
                                    {
                                        nz = Math.Cos(theta + (sn * dtheta)) * r;
                                        ny = Math.Sin(theta + (sn * dtheta)) * r;
                                    }
                                    else
                                    {
                                        nz = Math.Cos(theta - (sn * dtheta)) * r;
                                        ny = Math.Sin(theta - (sn * dtheta)) * r;
                                    }
                                    ob.RelativeObjectVertices[i] = new P3D(ob.RelativeObjectVertices[i].X, ny, nz);
                                }
                            }
                            ob.Remesh();
                            document.Dirty = true;
                        }
                        break;

                    case "YDown":
                    case "YUp":
                        {
                            double halfWidth = ob.Scale.X / 2.0;
                            for (int i = 0; i < ob.RelativeObjectVertices.Count; i++)
                            {
                                double relDist = Math.Abs(ob.RelativeObjectVertices[i].X / halfWidth);
                                int forcesIndex = (int)(relDist * (numForces - 1));
                                if (forcesIndex >= 0 && forcesIndex < numForces)
                                {
                                    double sn = -Math.Sign(ob.RelativeObjectVertices[i].X);
                                    if (ori == "YUp")
                                    {
                                        sn = -sn;
                                    }
                                    double z = ob.RelativeObjectVertices[i].Z;
                                    double x = ob.RelativeObjectVertices[i].X;
                                    double dtheta = forces[forcesIndex] * bendAngle;
                                    double theta = Math.Atan2(ob.RelativeObjectVertices[i].Z, ob.RelativeObjectVertices[i].X);
                                    double r = Math.Sqrt(z * z + x * x);
                                    double nx = Math.Cos(theta + (sn * dtheta)) * r;
                                    double nz = Math.Sin(theta + (sn * dtheta)) * r;
                                    ob.RelativeObjectVertices[i] = new P3D(nx, ob.RelativeObjectVertices[i].Y, nz);
                                }
                            }
                            ob.Remesh();
                            document.Dirty = true;
                        }
                        break;

                    case "ZUp":
                    case "ZDown":
                        {
                            double halfWidth = ob.Scale.X / 2.0;
                            for (int i = 0; i < ob.RelativeObjectVertices.Count; i++)
                            {
                                double relDist = Math.Abs(ob.RelativeObjectVertices[i].X / halfWidth);
                                int forcesIndex = (int)(relDist * (numForces - 1));
                                if (forcesIndex >= 0 && forcesIndex < numForces)
                                {
                                    double sn = -Math.Sign(ob.RelativeObjectVertices[i].X);
                                    if (ori == "ZUp")
                                    {
                                        sn = -sn;
                                    }
                                    double y = ob.RelativeObjectVertices[i].Y;
                                    double x = ob.RelativeObjectVertices[i].X;
                                    double dtheta = forces[forcesIndex] * bendAngle;
                                    double theta = Math.Atan2(ob.RelativeObjectVertices[i].Y, ob.RelativeObjectVertices[i].X);
                                    double r = Math.Sqrt(y * y + x * x);
                                    double nx = Math.Cos(theta + (sn * dtheta)) * r;
                                    double ny = Math.Sin(theta + (sn * dtheta)) * r;
                                    ob.RelativeObjectVertices[i] = new P3D(nx, ny, ob.RelativeObjectVertices[i].Z);
                                }
                            }
                            ob.Remesh();
                            document.Dirty = true;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void BendOver(object param, bool fold)
        {
            bool warning = true;

            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() == 1)
                {
                    bool confirmed = true;
                    Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                    if (ob is Group3D)
                    {
                        MessageBoxResult res = MessageBox.Show("Some objects will have to be converted to meshes first. Convert now.", "Warning", MessageBoxButton.OKCancel);
                        confirmed = res == MessageBoxResult.OK;
                        if (confirmed)
                        {
                            Document.Content.Remove(ob);

                            Object3D it = ob.ConvertToMesh();
                            it.Remesh();
                            Document.Content.Add(it);
                            Document.Dirty = true;
                            ob = it;
                        }
                    }
                    if (confirmed)
                    {
                        CheckPoint();
                        string ori = param.ToString();
                        BendObjectInHalf(ob, ori, fold);
                    }
                    warning = false;
                }
            }

            if (warning)
            {
                MessageBox.Show("Requires a single object to be selected", "Warning");
            }
        }

        private void BottomCamera()
        {
            ResetSelection();
            camera.HomeBottom();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 0, -1);
            LookToCenter();
            zoomPercent = 100;
        }

        private bool BreakGroup()
        {
            bool res = false;
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() == 1 && selectedObjectAdorner.SelectedObjects[0] is Group3D)
                {
                    CheckPoint();
                    Group3D grp = selectedObjectAdorner.SelectedObjects[0] as Group3D;
                    Document.SplitGroup(grp);

                    res = true;
                }
            }
            return res;
        }

        private void BuildPlateChanged(object param)
        {
            BuildPlate bp = param as BuildPlate;
            printerPlate.BorderColour = bp.BorderColour;
            printerPlate.Width = bp.Width;
            printerPlate.Height = bp.Height;
            printerPlate.BorderThickness = bp.BorderThickness;
            RegenerateDisplayList();
        }

        private void CheckEditSelection()
        {
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.SelectedObjects.Count == 1)
                {
                    Object3D obj = selectedObjectAdorner.SelectedObjects[0];
                    string toolName = obj.EditorParameters.ToolName;
                    if (toolName != "")
                    {
                        NotificationManager.Notify("Tool", toolName);
                    }
                }
            }
        }

        private void CheckForScriptResults()
        {
            if (ScriptResults != null)
            {
                List<Object3D> obs = ScriptResults as List<Object3D>;
                if (obs != null)
                {
                    if (obs.Count > 0 && ScriptClearBed)
                    {
                        document.Content.Clear();
                    }
                    foreach (Object3D ob in obs)
                    {
                        if (ob != null)
                        {
                            if (Document.ContainsName(ob.Name))
                            {
                                ob.Name = Document.DuplicateName(ob.Name);
                            }
                            document.Content.Add(ob);
                        }
                    }
                    document.Dirty = true;
                }
            }
            ScriptResults = null;
        }

        private bool CheckIfContentSelected(GeometryModel3D geo, bool append, bool sizer, bool control)
        {
            bool handled = false;
            if (!append)
            {
                RestoreUnselectedColours();
                selectedItems.Clear();

                foreach (Object3D ob in Document.Content)
                {
                    if (ob.Mesh == geo.Geometry)
                    {
                        geo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                        selectedItems.Add(ob);
                        EnableTool(ob);
                        handled = true;
                        break;
                    }
                }
                if (selectedItems.Count > 0)
                {
                    CameraLookObject = selectedItems[0].Position;
                    RemoveObjectAdorner();
                    GenerateSelectionBox(selectedItems[0], sizer, control);
                    NotificationManager.Notify("ObjectSelected", selectedItems[0]);
                    if (selectedItems.Count == 1)
                    {
                        PassOnGroupStatus(selectedItems[0]);
                    }
                    else
                    {
                        NotificationManager.Notify("GroupSelected", false);
                    }
                    handled = true;
                }
                NotifyPropertyChanged("ModelItems");
            }
            else
            {
                NotificationManager.Notify("ObjectSelected", null);
                foreach (Object3D ob in Document.Content)
                {
                    if (ob.Mesh == geo.Geometry)
                    {
                        geo.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                        selectedItems.Add(ob);
                        if (selectedObjectAdorner != null)
                        {
                            // remove the currnt visible elements of the adorner
                            RemoveObjectAdorner();
                            // append the the object to the existing list of
                            selectedObjectAdorner.AdornObject(ob);
                            PassOnGroupStatus(ob);
                            handled = true;
                            // update the display
                            foreach (Model3D md in selectedObjectAdorner.Adornments)
                            {
                                modelItems.Add(md);
                            }
                            NotifyPropertyChanged("ModelItems");
                        }
                    }
                }
            }
            return handled;
        }

        private void CleanExports()
        {
            if (MessageBox.Show("Delete all exported stl and printer files", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                String pth = BaseViewModel.Project.BaseFolder;
                string[] files = Directory.GetFiles(pth + "\\export", "*.stl");
                foreach (string f in files)
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch
                    {
                    }
                }

                CleanFolder(pth, "gcode", "*.gcode");
                CleanFolder(pth, "printer", "*.gcode");
                CleanFolder(pth, "gcode", "*.ctb");
                CleanFolder(pth, "printer", "*.ctb");
                NotificationManager.Notify("ExportRefresh", null);
            }
        }

        private void Decimate()
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                CheckPoint();

                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                ob.AbsoluteObjectVertices.Clear();
                GC.Collect();
                MeshDecimator.Mesh mesh = ObjectMeshToDecimatorMesh(ob);
                DecimatorSettings dlg = new DecimatorSettings();
                dlg.OriginalFaceCount = mesh.TriangleCount;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dlg.ShowDialog() == true)
                {
                    int target = dlg.TargetFaceCount;
                    if (target > 0 && target < dlg.OriginalFaceCount)
                    {
                        MeshDecimation.CreateAlgorithm(MeshDecimator.Algorithm.Default);

                        MeshDecimator.Mesh smallerMesh = MeshDecimation.DecimateMesh(mesh, target);
                        ob.RelativeObjectVertices.Clear();
                        for (int i = 0; i < smallerMesh.Vertices.GetLength(0); i++)
                        {
                            MeshDecimator.Math.Vector3d v = smallerMesh.Vertices[i];
                            //Point3D pnt = new Point3D(v.x, v.y, v.z);
                            P3D pnt = new P3D(v.x, v.y, v.z);
                            ob.RelativeObjectVertices.Add(pnt);
                        }
                        ob.TriangleIndices.Clear();
                        for (int i = 0; i < smallerMesh.Indices.GetLength(0); i++)
                        {
                            ob.TriangleIndices.Add(smallerMesh.Indices[i]);
                        }

                        Document.Dirty = true;
                    }
                }
                ob.Remesh();
                RegenerateDisplayList();
                GC.Collect();
            }
            else
            {
                MessageBox.Show("Decimate requires a single object to be selected");
            }
        }

        private void DisplayModeller(BaseModellerDialog dlg)
        {
            CheckPoint();
            dlg.DefaultImagePath = BaseViewModel.Project.ProjectPathToAbsPath("Images");
            dlg.MeshColour = Project.SharedProjectSettings.DefaultObjectColour;
            EditorParameters pm = new EditorParameters();
            Object3D editingObj = null;
            Point3D placement = new Point3D(0, 0, 0);
            bool needToAdd = false;
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                editingObj = selectedObjectAdorner.SelectedObjects[0];
                if (editingObj.EditorParameters != null)
                {
                    if (editingObj.EditorParameters.ToolName == dlg.EditorParameters.ToolName)
                    {
                        dlg.EditorParameters = editingObj.EditorParameters;
                        dlg.MeshColour = editingObj.Color;
                        editingObj.CalcScale(false);
                        placement = editingObj.Position;
                    }
                }
            }
            if (dlg.ShowDialog() == true)
            {
                bool positionAtRight = false;
                if (editingObj == null)
                {
                    editingObj = new Object3D();
                    editingObj.Name = Document.NextName;
                    editingObj.Description = "";
                    needToAdd = true;

                    editingObj.Color = dlg.MeshColour;
                    positionAtRight = true;
                }
                DeselectAll();

                editingObj.EditorParameters = dlg.EditorParameters;
                PointUtils.PointCollectionToP3D(dlg.Vertices, editingObj.RelativeObjectVertices);
                //editingObj.RelativeObjectVertices = dlg.Vertices;
                editingObj.TriangleIndices = dlg.Faces;

                RecalculateAllBounds();

                editingObj.Position = new Point3D(0, 0, 0);

                editingObj.PrimType = "Mesh";

                editingObj.Remesh();

                editingObj.CalcScale(false);

                if (positionAtRight)
                {
                    if (allBounds.Upper.X > double.MinValue)
                    {
                        placement = new Point3D(allBounds.Upper.X + editingObj.Scale.X / 2, editingObj.Scale.Y / 2, editingObj.Scale.Z / 2);
                        if (Project.SharedProjectSettings.PlaceNewAtMarker && floorMarker != null)
                        {
                            placement = floorMarker.Position;
                        }
                    }
                    else
                    {
                        placement = new Point3D(editingObj.Scale.X / 2, editingObj.Scale.Y / 2, editingObj.Scale.Z / 2);
                    }
                }
                editingObj.Position = placement;

                allBounds += editingObj.AbsoluteBounds;

                GeometryModel3D gm = GetMesh(editingObj);

                if (needToAdd)
                {
                    editingObj.MoveToFloor();
                    Document.Content.Add(editingObj);
                }
                Document.Dirty = true;
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void EnableTool(Object3D ob)
        {
            if (ob != null)
            {
                string toolName = ob.EditorParameters.ToolName;
                if (toolName != "")
                {
                    NotificationManager.Notify("SetSingleToolsVisible", toolName);
                }
            }
        }

        private void ExportAllModels()
        {
            if (Document.Dirty)
            {
                MessageBox.Show("Document must be saved first");
            }
            else
            {
                string[] filenames = BaseViewModel.Project.GetExportFiles(".txt");
                String pth = BaseViewModel.Project.BaseFolder;

                ProjectExporter pe = new ProjectExporter();
                pe.ExportAsync(BaseViewModel.Project, filenames, pth + "\\export", Project.SharedProjectSettings.VersionExport, BaseViewModel.Project.SharedProjectSettings.ExportEmptyFiles, BaseViewModel.Project.SharedProjectSettings.ClearPreviousVersionsOnExport);
                NotificationManager.Notify("ExportRefresh", null);
            }
        }

        private void FlipSelectedObjects(string s)
        {
            bool needsConversion = false;
            bool confirmed = true;
            foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
            {
                if (ob.PrimType != "Mesh")
                {
                    needsConversion = true;
                }
            }
            if (needsConversion)
            {
                MessageBoxResult res = MessageBox.Show("Some objects will have to be converted to meshes first. Convert now.", "Warning", MessageBoxButton.OKCancel);
                confirmed = res == MessageBoxResult.OK;
            }
            List<Object3D> tmp = new List<Object3D>();
            foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
            {
                Document.Content.Remove(ob);
                if (confirmed)
                {
                    Object3D it = ob.ConvertToMesh();
                    it.Remesh();
                    tmp.Add(it);
                }
                else
                {
                    tmp.Add(ob);
                }
            }
            selectedObjectAdorner.Clear();
            foreach (Object3D ob in tmp)
            {
                if (ob.IsSizable())
                {
                    switch (s)
                    {
                        case "Horizontal":
                            {
                                ob.FlipX();
                                ob.FlipInside();
                                ob.Remesh();
                            }
                            break;

                        case "Vertical":
                            {
                                ob.FlipY();
                                ob.FlipInside();
                                ob.Remesh();
                            }
                            break;

                        case "Distal":
                            {
                                ob.FlipZ();
                                ob.FlipInside();
                                ob.Remesh();
                            }
                            break;

                        case "Inside":
                            {
                                ob.FlipInside();
                                ob.Remesh();
                            }
                            break;
                    }
                }
                Document.Content.Add(ob);
                Document.Dirty = true;
            }
        }

        private void FloorAllObjects()
        {
            document.Dirty = true;
            foreach (Object3D ob in Document.Content)
            {
                ob.MoveToFloor();
            }
        }

        private void FloorSelectedObjects()
        {
            document.Dirty = true;
            for (int i = 0; i < selectedObjectAdorner.SelectedObjects.Count; i++)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[i];

                ob.MoveToFloor();
                ob.Remesh();
            }
        }

        private void GenerateSelectionBox(Object3D object3D, bool sizer, bool control)
        {
            if (sizer)
            {
                if (control)
                {
                    selectedObjectAdorner = new SkewAdorner(camera);
                    selectedObjectAdorner.ViewPort = ViewPort;
                    selectedObjectAdorner.Overlay = Overlay;
                }
                else
                {
                    MakeSizeAdorner();
                }
            }
            else
            {
                selectedObjectAdorner = new RotationAdorner(camera);
            }
            selectedObjectAdorner.AdornObject(object3D);

            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }
            NotifyPropertyChanged("ModelItems");
        }

        private KeyboardRotation GetRotationDirection(Key k, bool ctrlDown = false)
        {
            KeyboardRotation result = KeyboardRotation.z1;
            result = GetRotationDirectionFromKey(k, ctrlDown);

            return result;
        }

        private KeyboardRotation GetRotationDirectionFromKey(Key k, bool ctrlDown)
        {
            KeyboardRotation res = KeyboardRotation.None;
            switch (k)
            {
                case Key.Left:
                    {
                        if (ctrlDown)
                        {
                            res = KeyboardRotation.y1;
                        }
                        else
                        {
                            res = KeyboardRotation.z1;
                        }
                    }
                    break;

                case Key.Right:
                    {
                        if (ctrlDown)
                        {
                            res = KeyboardRotation.y2;
                        }
                        else
                        {
                            res = KeyboardRotation.z2;
                        }
                    }
                    break;

                case Key.Up:
                    {
                        res = KeyboardRotation.x1;
                    }
                    break;

                case Key.Down:
                    {
                        res = KeyboardRotation.x2;
                    }
                    break;
            }
            return res;
        }

        private bool GroupToMesh()
        {
            document.Dirty = true;
            bool res = false;
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() == 1 && selectedObjectAdorner.SelectedObjects[0] is Group3D)
                {
                    CheckPoint();
                    Group3D grp = selectedObjectAdorner.SelectedObjects[0] as Group3D;
                    Document.GroupToMesh(grp);
                    res = true;
                }
            }
            return res;
        }

        private async Task<Object3D> GroupTwo(Object3D leftie, int i, string s, bool cut)
        {
            bool res;
            var progress = new Progress<CSGGroupProgress>(ShowCSGProgress);
            Group3D grp = new Group3D();
            grp.Name = leftie.Name;
            grp.Description = leftie.Description;
            grp.LeftObject = leftie;
            if (cut)
            {
                Object3D cln = selectedObjectAdorner.SelectedObjects[i].Clone();
                grp.RightObject = cln;
            }
            else
            {
                grp.RightObject = selectedObjectAdorner.SelectedObjects[i];
            }
            grp.PrimType = s;

            res = await grp.InitAsync(csgCancelation, progress);
            if (res)
            {
                Document.ReplaceObjectsByGroup(grp);
            }
            else
            {
                grp = null;
            }
            return grp; ;
        }

        private void HomeCamera()
        {
            ResetSelection();
            camera.HomeFront();
            SetCameraDistance();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void ImportOneOfMany(string fpath)
        {
            string rootName = System.IO.Path.GetFileNameWithoutExtension(fpath);

            string targetPath = BaseViewModel.Project.ProjectPathToAbsPath(rootName + ".txt");
            if (!File.Exists(targetPath))
            {
                try
                {
                    Document localDoc = new Document();
                    localDoc.ImportStl(fpath, BaseViewModel.Project.SharedProjectSettings.ImportAxisSwap);
                    int numObs = 0;
                    foreach (Object3D ob in localDoc.Content)
                    {
                        ob.Name = rootName;
                        if (numObs > 0)
                        {
                            ob.Name += "_" + numObs.ToString();
                        }
                        ob.FlipInside();
                        ob.MoveOriginToCentroid();
                        ob.MoveToFloor();
                        ob.MoveToCentre();
                        numObs++;
                    }
                    localDoc.Save(targetPath);

                    string fldr = System.IO.Path.GetDirectoryName(targetPath);
                    BaseViewModel.Project.AddFileToFolder(fldr, rootName + ".txt");

                    localDoc.Clear();
                    localDoc = null;
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("File already exists: " + targetPath, "Error");
            }
        }

        private void LeftCamera()
        {
            ResetSelection();
            camera.HomeLeft();
            // true because we are looking from the side
            SetCameraDistance(true);
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, 1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void LoadingNewFile(object param)
        {
            // about to switch files
            // dont leave the adorners hanging around

            if (selectedObjectAdorner != null)
            {
                selectedObjectAdorner.Clear();
            }

            selectedItems.Clear();
            Overlay.Children.Clear();
            // dont leave obect palette show the details of an obect whih doesn't exist in the new file
            NotificationManager.Notify("ObjectSelected", null);
            NotificationManager.Notify("GroupSelected", false);
        }

        private bool Loft(Object3D obj, string obType)
        {
            bool res = false;
            if (obType == "shapeloft")
            {
                ShapeLoftDialog dlg = new ShapeLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    PointUtils.PointCollectionToP3D(dlg.Vertices, obj.RelativeObjectVertices);
                    //obj.RelativeObjectVertices = dlg.Vertices;
                    obj.TriangleIndices = dlg.Faces;
                    obj.CalcScale(false);
                    res = true;
                }
            }
            else if (obType == "fuselageloft")
            {
                FuselageLoftDialog dlg = new FuselageLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    // obj.RelativeObjectVertices = dlg.Vertices;
                    PointUtils.PointCollectionToP3D(dlg.Vertices, obj.RelativeObjectVertices);
                    obj.TriangleIndices = dlg.Faces;
                    obj.CalcScale(false);
                    res = true;
                }
            }

            return res;
        }

        private void LookAtObject()
        {
            if (selectedItems.Count == 1)
            {
                Object3D sel = selectedItems[0];
                CameraLookObject = sel.Position;
                camera.LookAt(CameraLookObject);
                LookToObject();
                NotifyPropertyChanged("CameraPos");
                NotifyPropertyChanged("LookDirection");
            }
        }

        private void LookToCenter()
        {
            lookDirection.X = -camera.CameraPos.X;
            lookDirection.Y = -camera.CameraPos.Y;
            lookDirection.Z = -camera.CameraPos.Z;
            lookDirection.Normalize();
            NotifyPropertyChanged("LookDirection");
        }

        private void LookToObject()
        {
            Vector3D v = new Vector3D(CameraLookObject.X - camera.CameraPos.X,
                  CameraLookObject.Y - camera.CameraPos.Y,
                  CameraLookObject.Z - camera.CameraPos.Z);
            v.Normalize();
            LookDirection = v;
            NotifyPropertyChanged("LookDirection");
        }

        private void LoopSmooth(Object3D ob)
        {
            CheckPoint();
            LoopSmoother cms = new LoopSmoother();
            // Point3DCollection p3col = ob.RelativeObjectVertices;
            Point3DCollection p3col = new Point3DCollection();
            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, p3col);
            Int32Collection icol = ob.TriangleIndices;

            cms.Smooth(ref p3col, ref icol);
            PointUtils.PointCollectionToP3D(p3col, ob.RelativeObjectVertices);
            ob.TriangleIndices = icol;
            ob.Remesh();
        }

        private async Task<bool> MakeGroup3D(string s)
        {
            NotificationManager.Notify("SuspendEditing", true);
            isEditingEnabled = false;
            bool res = false;
            bool cut = false;
            if (s == "groupcut")
            {
                cut = true;
                s = "groupdifference";
            }

            if (selectedObjectAdorner.NumberOfSelectedObjects() >= 2)
            {
                CheckPoint();

                Object3D leftie = selectedObjectAdorner.SelectedObjects[0];
                int i = 1;
                while (i < selectedObjectAdorner.NumberOfSelectedObjects() && leftie != null)
                {
                    leftie = await GroupTwo(leftie, i, s, cut);
                    i++;
                }
                if (i < selectedObjectAdorner.NumberOfSelectedObjects() || csgCancelation.IsCancellationRequested || leftie == null)
                {
                    Undo();
                }
                csgCancelation = null;
                InfoWindow.Instance().Hide();
            }
            NotificationManager.Notify("SuspendEditing", false);
            isEditingEnabled = true;
            return res;
        }

        private void MakeSizeAdorner()
        {
            selectedObjectAdorner = new SizeAdorner(camera);
            selectedObjectAdorner.Overlay = Overlay;
            selectedObjectAdorner.ViewPort = ViewPort;
        }

        private void MoveAllToCentre()
        {
            Point3D target = new Point3D(0, 0, 0);
            Bounds3D tmpBounds = new Bounds3D();

            foreach (Object3D ob in Document.Content)
            {
                tmpBounds.Add(ob.AbsoluteBounds);
            }

            foreach (Object3D ob in Document.Content)
            {
                double dAbsX = target.X + (ob.Position.X - tmpBounds.MidPoint().X);

                double dAbsY = ob.Position.Y;

                double dAbsZ = target.Z + (ob.Position.Z - tmpBounds.MidPoint().Z);
                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                ob.Remesh();
            }
            Document.Dirty = true;
        }

        private void MoveCameraDelta(Point lastMouse, Point newPos)
        {
            double dx = newPos.X - lastMouse.X;
            double dy = newPos.Y - lastMouse.Y;
            double dz = newPos.X - lastMouse.X;

            camera.Move(dx, dy);
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void RotateCamera(double dt, double dp)
        {
            camera.RotateDegrees(dt, dp);
            LookToCenter();
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void MoveSelectionToCentre()
        {
            Point3D centre = new Point3D(0, 0, 0);
            MoveToPoint(centre);
        }

        private void MoveSelectionToZero()
        {
            Point3D centre = new Point3D(0, 0, 0);
            MoveToPoint(centre, false);
        }

        private void MoveToMarker(object param)
        {
            if (showFloorMarker == true && floorMarker != null)
            {
                Point3D target = floorMarker.Position;

                MoveToPoint(target);
                document.Dirty = true;
            }
        }

        private void MoveToPoint(Point3D target, bool keepY = true)
        {
            Bounds3D tmpBounds = new Bounds3D();
            if (selectedObjectAdorner != null)
            {
                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    tmpBounds.Add(ob.AbsoluteBounds);
                }

                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    double dAbsX = target.X + (ob.Position.X - tmpBounds.MidPoint().X);

                    double dAbsY = ob.Position.Y;
                    if (!keepY)
                    {
                        dAbsY = target.Y;
                    }
                    double dAbsZ = target.Z + (ob.Position.Z - tmpBounds.MidPoint().Z);
                    ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);

                    ob.Remesh();
                }
                Document.Dirty = true;
                // move the selectors
                selectedObjectAdorner.GenerateAdornments();
                // redraw
                RegenerateDisplayList();
            }
        }

        private void OnAddObject(object param)
        {
            CheckPoint();
            string obType = param.ToString();
            obType = obType.ToLower();
            Color color = Colors.Beige;
            Object3D obj = new Object3D();
            bool added = true;
            DeselectAll();
            obj.Name = Document.NextName;
            obj.Description = obType;

            obj.Scale = new Scale3D(20, 20, 20);
            RecalculateAllBounds();
            // default position of object is at right of everything
            obj.Position = new Point3D(allBounds.Upper.X + obj.Scale.X / 2, obj.Scale.Y / 2, 0);
            // if the user wants it placed at the marker AND there is a marker, put it there instead
            if (Project.SharedProjectSettings.PlaceNewAtMarker && floorMarker != null)
            {
                obj.Position = floorMarker.Position;
            }

            if (obType == "vaseloft" || obType == "shapeloft" || obType == "fuselageloft")
            {
                added = Loft(obj, obType);
            }
            else
            {
                obj.BuildPrimitive(obType);
                obj.ScaleMesh(20.0, 20.0, 20.0);
            }
            if (added)
            {
                obj.PrimType = "Mesh";
                for (int i = 0; i < rotatedPrimitives.GetLength(0); i++)
                {
                    if (rotatedPrimitives[i] == obType)
                    {
                        obj.SwapYZAxies();

                        break;
                    }
                }
                obj.Remesh();
                allBounds += obj.AbsoluteBounds;
                obj.MoveToFloor();
                GeometryModel3D gm = GetMesh(obj);

                Document.Content.Add(obj);
                Document.Dirty = true;
                RegenerateDisplayList();

                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnAlignment(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 0)
            {
                CheckPoint();
                document.Dirty = true;
                string s = param.ToString();
                AlignSelectedObjects(s);
                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
            }
        }

        private void OnBend(object param)
        {
            BendOver(param, false);
        }

        private void OnCameraCommand(object param)
        {
            string p = param.ToString();
            switch (p)
            {
                case "CameraHome":
                    {
                        HomeCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraBack":
                    {
                        BackCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraLeft":
                    {
                        LeftCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraRight":
                    {
                        RightCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraTop":
                    {
                        TopCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraBottom":
                    {
                        BottomCamera();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraLookCenter":
                    {
                        LookToCenter();
                        ReportCameraPosition();
                        cameraMode = CameraModes.CameraMoveLookCenter;
                    }
                    break;

                case "CameraMove":
                    {
                        cameraMode = CameraModes.CameraMove;
                    }
                    break;

                case "CameraMoveLookCenter":
                    {
                        cameraMode = CameraModes.CameraMoveLookCenter;
                        camera.LookAt(new Point3D(0, 0, 0));
                    }
                    break;

                case "CameraLookObject":
                    {
                        cameraMode = CameraModes.CameraMoveLookObject;
                        LookAtObject();
                    }
                    break;

                default:
                    break;
            }
            ReportCameraPosition();
        }

        private void OnCircularPaste(object param)
        {
            CircularPasteDlg dlg = new CircularPasteDlg();
            dlg.Owner = Application.Current.MainWindow;
            if (dlg.ShowDialog() == true)
            {
                if (ObjectClipboard.HasItems())
                {
                    CheckPoint();
                    RecalculateAllBounds();
                    selectedObjectAdorner.Clear();
                    double radius = Convert.ToDouble(dlg.RadiusBox.Text);
                    double altRadius = Convert.ToDouble(dlg.AltBox.Text);
                    if (altRadius == 0)
                    {
                        altRadius = radius;
                    }
                    if (radius > 0 || altRadius > 0)
                    {
                        double cx = allBounds.Upper.X;
                        if (radius > altRadius)
                        {
                            cx += radius;
                        }
                        else
                        {
                            cx += altRadius;
                        }
                        double cy = 0;
                        double cz = 0;
                        double rx;
                        double ry;
                        double rz;
                        int repeats = Convert.ToInt16(dlg.RepeatsBox.Text);
                        if (repeats > 0)
                        {
                            double dTheta = (Math.PI * 2) / repeats;
                            double theta = 0;
                            bool alt = false;
                            double x;
                            double y;

                            while (theta < (Math.PI * 2))
                            {
                                rx = 0;
                                ry = 0;
                                rz = 0;
                                if (!alt)
                                {
                                    x = radius * Math.Cos(theta);
                                    y = radius * Math.Sin(theta);
                                }
                                else
                                {
                                    x = altRadius * Math.Cos(theta);
                                    y = altRadius * Math.Sin(theta);
                                }
                                alt = !alt;
                                foreach (Object3D cl in ObjectClipboard.Items)
                                {
                                    Object3D o = cl.Clone();
                                    o.Name = Document.NextName;

                                    if (o is Group3D)
                                    {
                                        (o as Group3D).Init();
                                    }

                                    if (dlg.DirectionX.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx + o.AbsoluteBounds.Width / 2 + x, cy, cz + o.AbsoluteBounds.Depth / 2 + y);
                                        ry = -theta;
                                    }
                                    if (dlg.DirectionY.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx, cy + o.AbsoluteBounds.Height / 2 + x, cz + o.AbsoluteBounds.Depth / 2 + y);
                                        rx = (Math.PI / 2) + theta;
                                    }
                                    if (dlg.DirectionY.IsChecked == true)
                                    {
                                        o.Position = new Point3D(cx + o.AbsoluteBounds.Width / 2 + x, cy + o.AbsoluteBounds.Height / 2 + y, cz);
                                        rz = -theta;
                                    }
                                    o.CalcScale(false);

                                    o.Remesh();
                                    if (dlg.RotateToCenterBox.IsChecked == true)
                                    {
                                        o.RotateRad(new Point3D(rx, ry, rz));
                                    }
                                    if (dlg.DirectionX.IsChecked == true)
                                    {
                                        o.MoveToFloor();
                                    }
                                    allBounds.Add(o.AbsoluteBounds);
                                    GeometryModel3D gm = GetMesh(o);
                                    Document.Content.Add(o);
                                    Document.Dirty = true;

                                    selectedItems.Add(o);
                                    selectedObjectAdorner.AdornObject(o);
                                }

                                theta += dTheta;
                            }
                            document.Dirty = true;
                            RegenerateDisplayList();
                            UpdateSelectionDisplay();
                        }
                    }
                }
            }
        }

        private void OnCopy(object param)
        {
            if (selectedObjectAdorner != null)
            {
                ObjectClipboard.Clear();
                GC.Collect();
                foreach (Object3D o in selectedObjectAdorner.SelectedObjects)
                {
                    ObjectClipboard.Add(o);
                }
            }
        }

        private void OnCut(object param)
        {
            if (selectedObjectAdorner != null)
            {
                CheckPoint();
                ObjectClipboard.Clear();
                foreach (Object3D o in selectedObjectAdorner.SelectedObjects)
                {
                    ObjectClipboard.Add(o);
                    Document.DeleteObject(o);
                }
                document.Dirty = true;
                selectedObjectAdorner.Clear();
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnDistribute(object param)
        {
            string s = param.ToString();
            if (s == "Optimum")
            {
                OptimisePlacement();
                document.Dirty = true;
            }
            else
            {
                if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count < 3)
                {
                    MessageBox.Show("At least 3 selected objects are need for even distribution");
                }
                else
                {
                    document.Dirty = true;
                    double minx = double.MaxValue;
                    double miny = double.MaxValue;
                    double minz = double.MaxValue;
                    double maxx = double.MinValue;
                    double maxy = double.MinValue;
                    double maxz = double.MinValue;
                    double count = selectedObjectAdorner.SelectedObjects.Count;
                    // Note this is positions, not bounds
                    foreach (Object3D o in selectedObjectAdorner.SelectedObjects)
                    {
                        if (o.Position.X < minx)
                        {
                            minx = o.Position.X;
                        }
                        if (o.Position.X > maxx)
                        {
                            maxx = o.Position.X;
                        }

                        if (o.Position.Y < miny)
                        {
                            miny = o.Position.Y;
                        }
                        if (o.Position.Y > maxy)
                        {
                            maxy = o.Position.Y;
                        }

                        if (o.Position.Z < minz)
                        {
                            minz = o.Position.Z;
                        }
                        if (o.Position.Z > maxz)
                        {
                            maxz = o.Position.Z;
                        }
                    }

                    double dx = (maxx - minx) / (count - 1);
                    double dy = (maxy - miny) / (count - 1);
                    double dz = (maxz - minz) / (count - 1);
                    switch (s)
                    {
                        case "Horizontal":
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    Object3D ob = selectedObjectAdorner.SelectedObjects[i];
                                    Point3D p = new Point3D(minx + (i * dx), ob.Position.Y, ob.Position.Z);
                                    ob.Position = p;
                                }
                            }
                            break;

                        case "Vertical":
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    Object3D ob = selectedObjectAdorner.SelectedObjects[i];
                                    Point3D p = new Point3D(ob.Position.X, miny + (i * dy), ob.Position.Z);
                                    ob.Position = p;
                                }
                            }
                            break;

                        case "Distal":
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    Object3D ob = selectedObjectAdorner.SelectedObjects[i];
                                    Point3D p = new Point3D(ob.Position.X, ob.Position.Y, minz + (i * dz));
                                    ob.Position = p;
                                }
                            }
                            break;
                    }
                    RegenerateDisplayList();
                    UpdateSelectionDisplay();
                }
            }
        }

        private void OnExport(object param)
        {
            string s = param as string;
            if (s != null)
            {
                if (s == "Clean")
                {
                    CleanExports();
                }
                else
                if (s == "AllModels")
                {
                    ExportAllModels();
                    NotificationManager.Notify("RefreshSolution", null);
                }
                else
                {
                    bool all = true;
                    if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 0)
                    {
                        ExportSelectedDialog seldlg = new ExportSelectedDialog();
                        seldlg.Owner = Application.Current.MainWindow;
                        seldlg.ShowDialog();
                        if (seldlg.Result == ExportSelectedDialog.ExportChoice.Selected)
                        {
                            all = false;
                        }
                    }
                    string exportedPath = "";
                    String exportFolderPath = BaseViewModel.Project.BaseFolder + "\\export";
                    Document.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;
                    if (all)
                    {
                        exportedPath = Document.ExportAll(param.ToString(), allBounds, exportFolderPath);
                    }
                    else
                    {
                        exportedPath = Document.ExportSelectedParts(param.ToString(), allBounds, selectedObjectAdorner.SelectedObjects, exportFolderPath);
                    }

                    STLExportedConfirmation dlg = new STLExportedConfirmation();
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.ExportPath = exportedPath;
                    if (dlg.ShowDialog() == true)
                    {
                    }
                }
            }
        }

        private void OnExportParts(object param)
        {
            String exportFolderPath = BaseViewModel.Project.BaseFolder + "\\export";
            string exportedPath = Document.ExportAllPartsSeperately(param.ToString(), allBounds, exportFolderPath);

            STLExportedPartsConfirmation dlg = new STLExportedPartsConfirmation();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ExportPath = exportedPath;
            dlg.ShowDialog();
            NotificationManager.Notify("ExportRefresh", null);
        }

        private void OnFlip(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 0)
            {
                CheckPoint();
                string s = param.ToString();
                FlipSelectedObjects(s);
                document.Dirty = true;
                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
            }
        }

        private void OnFold(object param)
        {
            BendOver(param, true);
        }

        private void OnFuselage(object param)
        {
            FuselageLoftDialog dlg = new FuselageLoftDialog();
            dlg.Owner = Application.Current.MainWindow;
            DisplayModeller(dlg);
        }

        private async Task OnGroup(object param)
        {
            string s = param.ToString().ToLower();
            if (s == "test")
            {
                CheckPoint();
                if (Test())
                {
                    selectedObjectAdorner.Clear();
                    RegenerateDisplayList();
                    NotificationManager.Notify("ObjectNamesChanged", null);
                }
            }
            else
           if (s == "mesh")
            {
                CheckPoint();
                if (GroupToMesh())
                {
                    selectedObjectAdorner.Clear();
                    RegenerateDisplayList();
                    NotificationManager.Notify("ObjectNamesChanged", null);
                }
            }
            else
            if (s == "ungroup")
            {
                CheckPoint();
                if (BreakGroup())
                {
                    selectedObjectAdorner.Clear();
                    RegenerateDisplayList();
                    NotificationManager.Notify("ObjectNamesChanged", null);
                }
            }
            else
            {
                csgCancelation = new CancellationTokenSource();
                await MakeGroup3D(s);
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private async void OnImport(object param)
        {
            if (selectedObjectAdorner != null)
            {
                selectedObjectAdorner.Clear();
            }
            OpenFileDialog dlg = new OpenFileDialog();

            string s = param.ToString();
            switch (s.ToLower())
            {
                case "obj":
                    {
                        dlg.Filter = "Object Model Files (*.obj) | *.obj";
                        if (dlg.ShowDialog() == true)
                        {
                            if (File.Exists(dlg.FileName))
                            {
                                Document.ImportObjs(dlg.FileName);
                                document.Dirty = true;
                                RegenerateDisplayList();
                            }
                        }
                    }
                    break;

                case "stl":
                    {
                        dlg.Filter = "Stereo Lithography Files (*.stl) | *.stl";
                        if (dlg.ShowDialog() == true)
                        {
                            if (File.Exists(dlg.FileName))
                            {
                                Document.ImportStl(dlg.FileName, BaseViewModel.Project.SharedProjectSettings.ImportAxisSwap);
                                RegenerateDisplayList();
                                NotificationManager.Notify("ObjectNamesChanged", null);
                            }
                        }
                    }
                    break;

                case "multistl":
                    {
                        try
                        {
                            var mi = new MultiImport();
                            if (mi.ShowDialog() == true)
                            {
                                GC.Collect();
                                BaseViewModel.Project.Save();
                                BaseViewModel.Document.SaveGlobalSettings();
                                NotificationManager.Notify("ImportRefresh", null);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;
            }
        }

        private void OnKeyRotate(KeyboardRotation rd)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.NumberOfSelectedObjects() == 1)
            {
                double x = 0;
                double y = 0;
                double z = 0;
                switch (rd)
                {
                    case KeyboardRotation.z1:
                        {
                            z = keyrotationz;
                        }
                        break;

                    case KeyboardRotation.z2:
                        {
                            z = -keyrotationz;
                        }
                        break;

                    case KeyboardRotation.x1:
                        {
                            x = -keyrotationx;
                        }
                        break;

                    case KeyboardRotation.x2:
                        {
                            x = keyrotationx;
                        }
                        break;

                    case KeyboardRotation.y1:
                        {
                            y = -keyrotationy;
                        }
                        break;

                    case KeyboardRotation.y2:
                        {
                            y = keyrotationz;
                        }
                        break;
                }
                Point3D pr = new Point3D(x, y, z);
                RotateSelected(selectedObjectAdorner.SelectedObjects[0], pr);
            }
        }

        private void OnLoopSmooth(object param)
        {
            if (selectedObjectAdorner == null || selectedObjectAdorner.SelectedObjects.Count == 0)
            {
                System.Windows.MessageBox.Show("Nothing selected to process");
            }
            else
            {
                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    LoopSmooth(ob);
                }
            }
        }

        private void OnManifoldTest(object param)
        {
            if (selectedObjectAdorner == null || selectedObjectAdorner.SelectedObjects.Count == 0)
            {
                MessageBox.Show("Nothing selected to check");
            }
            else
            {
                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    ManifoldChecker checker = new ManifoldChecker();
                    PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, checker.Points);

                    checker.Indices = ob.TriangleIndices;
                    checker.Check();
                    if (checker.IsManifold)
                    {
                        MessageBox.Show("Manifold");
                    }
                    else
                    {
                        if (checker.NumberOfDuplicatedVertices > 0)
                        {
                            MessageBox.Show($"{checker.NumberOfDuplicatedVertices} duplicate points ");
                        }
                        if (checker.NumberOfBadlyOrientatedEdges > 0)
                        {
                            MessageBox.Show($"{checker.NumberOfBadlyOrientatedEdges} badly orientated edges");
                        }

                        if (checker.NumbeOfUnconnectedFaces > 0)
                        {
                            MessageBox.Show($"{checker.NumbeOfUnconnectedFaces} incompletely connected faces");
                        }

                        if (checker.NumberOfUnReferencedVertices > 0)
                        {
                            MessageBox.Show($"{checker.NumberOfUnReferencedVertices} unreferenced vertices");
                        }
                        if (checker.NumberOfNonExistentVertices > 0)
                        {
                            MessageBox.Show($"{checker.NumberOfNonExistentVertices} references to vertices that don't exist");
                        }
                    }
                }
            }
        }

        private void OnMeshEdit(object param)
        {
            MeshEditorDlg dlg = new MeshEditorDlg();
            dlg.Owner = Application.Current.MainWindow;
            CheckPoint();
            EditorParameters pm = new EditorParameters();
            Object3D editingObj = null;
            bool needToAdd = false;
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                editingObj = selectedObjectAdorner.SelectedObjects[0];
                if (editingObj.IsSizable() && editingObj.PrimType == "Mesh")
                {
                    if (editingObj.EditorParameters.ToolName == dlg.EditorParameters.ToolName)
                    {
                        dlg.EditorParameters = editingObj.EditorParameters;
                        dlg.MeshColour = editingObj.Color;
                        Point3DCollection tmp = new Point3DCollection();
                        PointUtils.P3DToPointCollection(editingObj.RelativeObjectVertices, tmp);
                        dlg.SetInitialMesh(tmp, editingObj.TriangleIndices);
                        editingObj.CalcScale();
                    }
                }
            }
            if (dlg.ShowDialog() == true)
            {
                bool positionAtRight = false;
                if (editingObj == null)
                {
                    editingObj = new Object3D();
                    editingObj.Name = Document.NextName;
                    editingObj.Description = "";
                    needToAdd = true;

                    editingObj.Color = dlg.MeshColour;
                    positionAtRight = true;
                }
                DeselectAll();

                editingObj.EditorParameters = dlg.EditorParameters;
                PointUtils.PointCollectionToP3D(dlg.Vertices, editingObj.RelativeObjectVertices);

                editingObj.TriangleIndices = dlg.Faces;

                RecalculateAllBounds();
                Point3D placement = new Point3D(0, 0, 0);
                editingObj.Position = new Point3D(0, 0, 0);

                editingObj.PrimType = "Mesh";

                editingObj.Remesh();

                editingObj.CalcScale();

                if (positionAtRight)
                {
                    if (allBounds.Upper.X > double.MinValue)
                    {
                        placement = new Point3D(allBounds.Upper.X + editingObj.Scale.X / 2, editingObj.Scale.Y / 2, editingObj.Scale.Z / 2);
                    }
                    else
                    {
                        placement = new Point3D(editingObj.Scale.X / 2, editingObj.Scale.Y / 2, editingObj.Scale.Z / 2);
                    }
                }
                editingObj.Position = placement;

                allBounds += editingObj.AbsoluteBounds;

                GeometryModel3D gm = GetMesh(editingObj);

                if (needToAdd)
                {
                    Document.Content.Add(editingObj);
                }
                Document.Dirty = true;
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnMeshHull(object param)
        {
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() > 0)
                {
                    CheckPoint();
                    foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                    {
                        if (ob is Group3D)
                        {
                            Object3D tmp = Document.GroupToMesh(ob as Group3D);
                            tmp.ConvertToHull();
                        }
                        else
                        {
                            ob.ConvertToHull();
                        }
                    }
                    DeselectAll();
                }
            }
        }

        private void OnMoveObjectToCentre(object param)
        {
            Object3D prm = param as Object3D;
            CheckPoint();
            prm.MoveToCentre();
            // move the selectors
            selectedObjectAdorner.GenerateAdornments();
            // redraw
            RegenerateDisplayList();
        }

        private void OnMoveObjectToFloor(object param)
        {
            Object3D prm = param as Object3D;
            CheckPoint();
            prm.MoveToFloor();
            // move the selectors
            selectedObjectAdorner.GenerateAdornments();
            // redraw
            RegenerateDisplayList();
        }

        private void OnMultiPaste(object param)
        {
            MultiPasteConfig cfg = param as MultiPasteConfig;
            if (cfg != null)
            {
                if (ObjectClipboard.HasItems())
                {
                    CheckPoint();
                    RecalculateAllBounds();
                    selectedObjectAdorner.Clear();
                    double altOff = 0;
                    for (int i = 0; i < cfg.Repeats; i++)
                    {
                        if (i % 2 == 0)
                        {
                            altOff = 0;
                        }
                        else
                        {
                            altOff = cfg.AlternatingOffset;
                        }
                        foreach (Object3D cl in ObjectClipboard.Items)
                        {
                            Object3D o = cl.Clone();
                            o.Name = Document.NextName;

                            if (o is Group3D)
                            {
                                (o as Group3D).Init();
                            }

                            if (cfg.Direction == "X")
                            {
                                o.Position = new Point3D(allBounds.Upper.X + o.AbsoluteBounds.Width / 2 + cfg.Spacing, 0, 0);
                            }
                            if (cfg.Direction == "Y")
                            {
                                o.Position = new Point3D(altOff, allBounds.Upper.Y + o.AbsoluteBounds.Height / 2 + cfg.Spacing, 0);
                            }
                            if (cfg.Direction == "Z")
                            {
                                o.Position = new Point3D(altOff, 0, allBounds.Upper.Z + o.AbsoluteBounds.Depth / 2 + cfg.Spacing);
                            }
                            o.CalcScale(false);

                            o.Remesh();
                            if (cfg.Direction == "X" || cfg.Direction == "Z")
                            {
                                o.MoveToFloor();
                            }

                            allBounds.Add(o.AbsoluteBounds);
                            GeometryModel3D gm = GetMesh(o);
                            Document.Content.Add(o);
                            Document.Dirty = true;

                            selectedItems.Add(o);
                            selectedObjectAdorner.AdornObject(o);
                        }
                    }
                    UpdateSelectionDisplay();
                    RegenerateDisplayList();
                }
            }
        }

        private void OnNewDocument(object param)
        {
            selectedObjectAdorner?.Clear();
            RegenerateDisplayList();
            HomeCamera();
            selectedItems = new List<Object3D>();
            NotificationManager.Notify("ObjectNamesChanged", null);
        }

        private void OnPaste(object param)
        {
            if (ObjectClipboard.HasItems())
            {
                CheckPoint();
                RecalculateAllBounds();
                selectedObjectAdorner?.Clear();
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    Object3D o = cl.Clone();
                    if (Document.ContainsName(o.Name))
                    {
                        o.Name = Document.DuplicateName(o.Name);
                    }
                    if (o is Group3D)
                    {
                        //    (o as Group3D).Init();
                    }
                    o.Position = new Point3D(allBounds.Upper.X + o.AbsoluteBounds.Width / 2, 0, 0);
                    o.Remesh();
                    o.MoveToFloor();
                    o.CalcScale(false);
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;
                }

                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnPasteAt(object param)
        {
            if (ObjectClipboard.HasItems() && floorMarker != null)
            {
                CheckPoint();
                RecalculateAllBounds();
                selectedObjectAdorner.Clear();
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    Object3D o = cl.Clone();
                    if (Document.ContainsName(o.Name))
                    {
                        o.Name = Document.DuplicateName(o.Name);
                    }
                    if (o is Group3D)
                    {
                        //    (o as Group3D).Init();
                    }
                    o.Position = new Point3D(floorMarker.Position.X, floorMarker.Position.Y, floorMarker.Position.Z);
                    o.Remesh();
                    o.MoveToFloor();
                    o.CalcScale(false);
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;
                }

                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnRefresh(object param)
        {
            RegenerateDisplayList();
            NotificationManager.Notify("ObjectNamesChanged", null);
        }

        private void OnRemesh(object param)
        {
        }

        private void OnRemoveDupVertices(object param)
        {
            if (selectedObjectAdorner == null || selectedObjectAdorner.SelectedObjects.Count == 0)
            {
                MessageBox.Show("Nothing selected to check");
            }
            else
            {
                int totalRemoved = 0;
                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    totalRemoved += RemoveDuplicateVertices(ob);
                }
                Document.Dirty = true;
                MessageBox.Show($"Removed {totalRemoved} duplicate vertices");
            }
        }

        private void OnRemoveUnrefVertices(object param)
        {
            if (selectedObjectAdorner == null || selectedObjectAdorner.SelectedObjects.Count == 0)
            {
                MessageBox.Show("Nothing selected to process");
            }
            else
            {
                foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
                {
                    RemoveUnrefVertices(ob);
                }
                Document.Dirty = true;
            }
        }

        private void OnReorigin(object param)
        {
            if (selectedObjectAdorner == null || selectedObjectAdorner.SelectedObjects.Count != 1)
            {
                MessageBox.Show("Must have a single object selected to re-origin it");
            }
            else
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                Reorigin(ob);
            }
        }

        private void OnShowAxies(object param)
        {
            showAxies = (param as bool?) == true;
            RegenerateDisplayList();
        }

        private void OnShowBuildPlate(object param)
        {
            showBuildPlate = (param as bool?) == true;
            RegenerateDisplayList();
        }

        private void OnShowFloor(object param)
        {
            showFloor = (param as bool?) == true;
            RegenerateDisplayList();
        }

        private void OnShowFloorMarker(object param)
        {
            showFloorMarker = (param as bool?) == true;
            RegenerateDisplayList();
        }

        private void OnSize(object param)
        {
            CheckPoint();
            string s = param.ToString();
            if (s == "Decimate")
            {
                Decimate();
            }
            else
            {
                ResizeObjects(s);
            }
        }

        private void OnSlice(object param)
        {
            if (Properties.Settings.Default.SlicerPath != "")
            {
                if (Document.Dirty)
                {
                    if (MessageBox.Show("Save current document first?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (Document.FilePath != "")
                        {
                            Document.Save(Document.FilePath);
                        }
                    }
                }
                string s = param.ToString();
                SliceControl dlg = new SliceControl();
                dlg.ModelMode = s;
                dlg.ModelPath = Document.FilePath;
                dlg.SlicerPath = Properties.Settings.Default.SlicerPath;
                // dlg.ExportDocument = Document;
                dlg.ShowDialog();
            }
        }

        private void OnSplit(object param)
        {
            bool warning = true;
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() == 1)
                {
                    bool confirmed = true;
                    Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                    if (ob is Group3D)
                    {
                        MessageBoxResult res = MessageBox.Show("Some objects will have to be converted to meshes first. Convert now.", "Warning", MessageBoxButton.OKCancel);
                        confirmed = res == MessageBoxResult.OK;
                        if (confirmed)
                        {
                            Document.Content.Remove(ob);

                            Object3D it = ob.ConvertToMesh();
                            it.Remesh();
                            Document.Content.Add(it);
                            Document.Dirty = true;
                            ob = it;
                        }
                    }
                    if (confirmed)
                    {
                        CheckPoint();
                        string ori = param.ToString();
                        SplitObjectInHalf(ob, ori);
                    }
                    warning = false;
                }
            }

            if (warning)
            {
                MessageBox.Show("Split requires a single object to be selected", "Warning");
            }
        }

        private void OnTool(object param)
        {
            String toolName = param.ToString();
            BaseModellerDialog dlg = ToolFactory.MakeTool(toolName);

            if (dlg != null)
            {
                DisplayModeller(dlg);
            }
        }

        private void OnUndo(object param)
        {
            if (selectedObjectAdorner != null)
            {
                selectedObjectAdorner.Clear();
            }
            Undo();
        }

        private void OptimisePlacement()
        {
            CheckPoint();
            Arranger arr = new Arranger();
            foreach (Object3D ob in Document.Content)
            {
                if (ob.Exportable)
                {
                    arr.AddComponent(ob,
                                     new Point(ob.AbsoluteBounds.Lower.X, ob.AbsoluteBounds.Lower.Z),
                                     new Point(ob.AbsoluteBounds.Upper.X, ob.AbsoluteBounds.Upper.Z));
                }
            }
            arr.Clearance = 3;
            arr.Width = 200;
            arr.Height = 200;
            arr.Arrange();

            foreach (PlacementLib.Component c in arr.Results)
            {
                Object3D o = c.Tag as Object3D;
                double dx = c.Position.X - c.OriginalPosition.X;
                double dy = c.Position.Y - c.OriginalPosition.Y;
                o.Position = new Point3D(o.Position.X + dx - arr.Width / 2, o.Position.Y, o.Position.Z + dy - arr.Height / 2);
            }
            MoveAllToCentre();
            RegenerateDisplayList();
            Document.Dirty = true;
        }

        private void RecalculateAllBounds()
        {
            allBounds = new Bounds3D();
            if (Document.Content.Count == 0)
            {
                allBounds.Zero();
            }
            else
            {
                foreach (Object3D ob in Document.Content)
                {
                    allBounds += ob.AbsoluteBounds;
                }
            }
        }

        private void RemoveObjectAdorner()
        {
            if (selectedObjectAdorner != null)
            {
                foreach (Model3D md in selectedObjectAdorner.Adornments)
                {
                    modelItems.Remove(md);
                }
            }
        }

        private void Reorigin(Object3D ob)
        {
            ReoriginDlg dlg = new ReoriginDlg();
            dlg.SetObject(ob);
            if (dlg.ShowDialog() == true)
            {
                ob.Remesh();
                RegenerateDisplayList();
                document.Dirty = true;
            }
        }

        private void ReportCameraPosition()
        {
            String s = $"Camera ({camera.CameraPos.X:F2},{camera.CameraPos.Y:F2},{camera.CameraPos.Z:F2}) => ({lookDirection.X:F2},{lookDirection.Y:F2},{lookDirection.Z:F2}) Zoom {zoomPercent:F1}%";
            NotificationManager.Notify("SetStatusText1", s);
            NotifyPropertyChanged("ModelItems");
            ReportStatistics();
        }

        /// <summary>
        /// Report the memory used, number of faces and vertices.
        /// </summary>
        private void ReportStatistics()
        {
            string s;
            Process currentProcess = Process.GetCurrentProcess();
            long usedMemory = currentProcess.PrivateMemorySize64;
            double mb = usedMemory / (1024 * 1024);
            double gb = mb / 1024;
            if (gb > 1.0)
            {
                s = $"Faces {totalFaces} Vertices {totalFaces * 3}, Mem {gb.ToString("F3")}Gb";
            }
            else
            {
                s = $"Faces {totalFaces} Vertices {totalFaces * 3}, Mem {mb.ToString("F3", CultureInfo.InvariantCulture)}Mb";
            }
            NotificationManager.Notify("SetStatusText3", s);
        }

        private void ResetSelection()
        {
            ResetSelectionColours();
            NotificationManager.Notify("ObjectSelected", null);
            if (selectedObjectAdorner != null)
            {
                // remove the current visible elements of the adorner
                RemoveObjectAdorner();
            }

            MakeSizeAdorner();
            selectedItems.Clear();
            NotificationManager.Notify("GroupSelected", false);
        }

        private void ResetSelectionColours()
        {
            List<GeometryModel3D> tmp = modelItems.OfType<GeometryModel3D>().ToList();
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in tmp)
                {
                    if (md != null)
                    {
                        if (ob.Mesh == md.Geometry)
                        {
                            md.Material = new DiffuseMaterial(new SolidColorBrush(ob.Color));
                        }
                    }
                }
            }
        }

        private void ResizeObjects(string s)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 1)
            {
                CheckPoint();
                Scale3D org = selectedObjectAdorner.SelectedObjects[0].Scale;
                for (int i = 1; i < selectedObjectAdorner.SelectedObjects.Count; i++)
                {
                    double x = selectedObjectAdorner.SelectedObjects[i].Scale.X;
                    double y = selectedObjectAdorner.SelectedObjects[i].Scale.Y;
                    double z = selectedObjectAdorner.SelectedObjects[i].Scale.Z;

                    switch (s)
                    {
                        case "SameLength":
                            {
                                x = selectedObjectAdorner.SelectedObjects[0].Scale.X;
                            }
                            break;

                        case "SameWidth":
                            {
                                z = selectedObjectAdorner.SelectedObjects[0].Scale.Z;
                            }
                            break;

                        case "SameHeight":
                            {
                                y = selectedObjectAdorner.SelectedObjects[0].Scale.Y;
                            }
                            break;

                        case "SameAll":
                            {
                                x = selectedObjectAdorner.SelectedObjects[0].Scale.X;
                                y = selectedObjectAdorner.SelectedObjects[0].Scale.Y;
                                z = selectedObjectAdorner.SelectedObjects[0].Scale.Z;
                            }
                            break;
                    }
                    double sx = x / selectedObjectAdorner.SelectedObjects[i].Scale.X;
                    double sy = y / selectedObjectAdorner.SelectedObjects[i].Scale.Y;
                    double sz = z / selectedObjectAdorner.SelectedObjects[i].Scale.Z;

                    selectedObjectAdorner.SelectedObjects[i].ScaleMesh(sx, sy, sz);
                    selectedObjectAdorner.SelectedObjects[i].Scale = new Scale3D(x, y, z);
                    selectedObjectAdorner.SelectedObjects[i].Remesh();
                    document.Dirty = true;
                }
                // move the selectors
                selectedObjectAdorner.GenerateAdornments();
                // redraw
                RegenerateDisplayList();
            }
        }

        private void RestoreUnselectedColours()
        {
            try
            {
                List<GeometryModel3D> tmp = modelItems.OfType<GeometryModel3D>().ToList();
                foreach (Object3D ob in selectedItems)
                {
                    foreach (GeometryModel3D md in tmp)
                    {
                        if (md != floor.FloorMesh)
                        {
                            if (ob.Mesh == md.Geometry)
                            {
                                md.Material = new DiffuseMaterial(new SolidColorBrush(ob.Color));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void RightCamera()
        {
            ResetSelection();
            camera.HomeRight();
            // true because we are looking from the side
            SetCameraDistance(true);
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, -1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void RotateSelected(Object3D obj, Point3D pr)
        {
            if (obj != null)
            {
                CheckPoint();
                obj.Rotate(pr);
                obj.Remesh();

                NotifyPropertyChanged();
                NotificationManager.Notify("ScaleRefresh", obj);
                NotificationManager.Notify("RefreshAdorners", null);
                Document.Dirty = true;
            }
        }

        private void Select(object param)
        {
            string s = param.ToString();
            s = s.ToLower();
            if (s == "clear")
            {
                DeselectAll();
            }
            else
            if (s == "all")
            {
                SelectAll();
            }
            else
            if (s == "first")
            {
                SelectFirst();
            }
            else
            if (s == "next")
            {
                SelectNext();
            }
            else
            if (s == "previous")
            {
                SelectPrevious();
            }
            else
            if (s == "last")
            {
                SelectLast();
            }
        }

        private void SelectAll()
        {
            NotificationManager.Notify("SetToolsVisibility", false);
            ResetSelection();
            foreach (Object3D ob in Document.Content)
            {
                selectedItems.Add(ob);
                // append the the object to the existing list of
                selectedObjectAdorner.AdornObject(ob);
            }
            UpdateSelectionDisplay();
        }

        private void SelectFirst()
        {
            var v1 = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down);
            var v2 = (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down);
            if ((v1 == KeyStates.None) &&
                (v2 == KeyStates.None))
            {
                ResetSelection();
            }
            if (Document.Content.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D ob = Document.Content[0];
                if (!selectedItems.Contains(ob))
                {
                    selectedItems.Add(ob);
                    if (selectedObjectAdorner == null)
                    {
                        MakeSizeAdorner();
                    }
                    selectedObjectAdorner.AdornObject(ob);
                    SetSelectionColours();
                    NotificationManager.Notify("ObjectSelected", ob);
                    if (selectedItems.Count == 1)
                    {
                        PassOnGroupStatus(ob);
                    }
                    else
                    {
                        NotificationManager.Notify("GroupSelected", false);
                    }

                    EnableTool(ob);
                }
            }
            UpdateSelectionDisplay();
        }

        private void SelectLast()
        {
            var v1 = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down);
            var v2 = (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down);
            if ((v1 == KeyStates.None) &&
                (v2 == KeyStates.None))
            {
                ResetSelection();
            }
            if (Document.Content.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D ob = Document.Content[Document.Content.Count - 1];
                if (!selectedItems.Contains(ob))
                {
                    selectedItems.Add(ob);
                    if (selectedObjectAdorner == null)
                    {
                        MakeSizeAdorner();
                    }
                    selectedObjectAdorner.AdornObject(ob);
                    SetSelectionColours();
                    NotificationManager.Notify("ObjectSelected", ob);
                    if (selectedItems.Count == 1)
                    {
                        PassOnGroupStatus(ob);
                    }
                    else
                    {
                        NotificationManager.Notify("GroupSelected", false);
                    }
                }
                EnableTool(ob);
            }
            UpdateSelectionDisplay();
        }

        private void SelectNext()
        {
            if (selectedItems.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D sel = selectedItems[selectedItems.Count - 1];

                Object3D nxt = null;
                if (sel != null)
                {
                    for (int i = 0; i < Document.Content.Count - 1; i++)
                    {
                        if (Document.Content[i] == sel)
                        {
                            nxt = Document.Content[i + 1];
                            break;
                        }
                    }
                    if (nxt == null)
                    {
                        nxt = Document.Content[0];
                    }
                }
                var v1 = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down);
                var v2 = (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down);
                if ((v1 == KeyStates.None) &&
                    (v2 == KeyStates.None))
                {
                    ResetSelection();
                }

                if (!selectedItems.Contains(nxt))
                {
                    selectedItems.Add(nxt);
                    if (selectedObjectAdorner == null)
                    {
                        MakeSizeAdorner();
                    }
                    selectedObjectAdorner.AdornObject(nxt);

                    SetSelectionColours();
                    NotificationManager.Notify("ObjectSelected", nxt);
                    if (selectedItems.Count == 1)
                    {
                        PassOnGroupStatus(nxt);
                    }
                    else
                    {
                        NotificationManager.Notify("GroupSelected", false);
                    }

                    EnableTool(nxt);
                }
                UpdateSelectionDisplay();
            }
        }

        private void SelectObjectByName(object param)
        {
            if (param != null)
            {
                string nm = param.ToString();

                var v1 = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down);
                var v2 = (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down);
                if ((v1 == KeyStates.None) &&
                    (v2 == KeyStates.None))
                {
                    ResetSelection();
                }

                if (Document.Content.Count > 0)
                {
                    NotificationManager.Notify("SetToolsVisibility", false);
                    foreach (Object3D ob in Document.Content)
                    {
                        if (ob.Name == nm)
                        {
                            if (!selectedItems.Contains(ob))
                            {
                                selectedItems.Add(ob);
                                if (selectedObjectAdorner == null)
                                {
                                    MakeSizeAdorner();
                                }
                                selectedObjectAdorner.AdornObject(ob);
                                SetSelectionColours();
                                NotificationManager.Notify("ObjectSelected", ob);
                                if (selectedItems.Count == 1)
                                {
                                    PassOnGroupStatus(ob);
                                }
                                else
                                {
                                    NotificationManager.Notify("GroupSelected", false);
                                }
                            }
                            EnableTool(ob);
                        }
                    }
                }
                UpdateSelectionDisplay();
            }
        }

        private void SelectPrevious()
        {
            if (selectedItems.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D sel = selectedItems[0];
                var v1 = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down);
                var v2 = (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down);
                if ((v1 == KeyStates.None) &&
                    (v2 == KeyStates.None))
                {
                    ResetSelection();
                }

                Object3D nxt = null;
                if (sel != null)
                {
                    for (int i = 0; i < Document.Content.Count; i++)
                    {
                        if (Document.Content[i] == sel)
                        {
                            if (i > 0)
                            {
                                nxt = Document.Content[i - 1];
                            }
                        }
                    }
                    if (nxt == null)
                    {
                        nxt = Document.Content[Document.Content.Count - 1];
                    }
                }
                if (!selectedItems.Contains(nxt))
                {
                    selectedItems.Add(nxt);
                    if (selectedObjectAdorner == null)
                    {
                        MakeSizeAdorner();
                    }
                    selectedObjectAdorner.AdornObject(nxt);
                    if (selectedItems.Count == 1)
                    {
                        PassOnGroupStatus(nxt);
                    }
                    else
                    {
                        NotificationManager.Notify("GroupSelected", false);
                    }
                    SetSelectionColours();
                    NotificationManager.Notify("ObjectSelected", nxt);
                    EnableTool(nxt);
                }
                UpdateSelectionDisplay();
            }
        }

        private void SetCameraDistance(bool sideView = false)
        {
            double w = allBounds.Upper.X;
            double h = allBounds.Upper.Y;
            double d = allBounds.Upper.Z;

            if (Math.Abs(allBounds.Lower.X) > w)
            {
                w = Math.Abs(allBounds.Lower.X);
            }

            if (Math.Abs(allBounds.Lower.Y) > h)
            {
                h = Math.Abs(allBounds.Lower.Y);
            }

            if (Math.Abs(allBounds.Lower.Z) > d)
            {
                d = Math.Abs(allBounds.Lower.Z);
            }
            if (sideView)
            {
                camera.DistanceToFit(d, h, w * 1.5);
            }
            else
            {
                camera.DistanceToFit(w, h, d * 1.5);
            }
        }

        private void SetSelectionColours()
        {
            bool first = true;
            List<GeometryModel3D> tmp = modelItems.OfType<GeometryModel3D>().ToList();
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in tmp)
                {
                    if (ob.Mesh == md.Geometry)
                    {
                        if (first)
                        {
                            md.Material = new DiffuseMaterial(new SolidColorBrush(Colors.OrangeRed));
                            first = false;
                        }
                        else
                        {
                            md.Material = new DiffuseMaterial(new SolidColorBrush(Colors.LightGreen));
                        }
                    }
                }
            }
        }

        private void ShowCSGProgress(CSGGroupProgress obj)
        {
            InfoWindow.Instance().ShowInfo("CSG Operation");
            //InfoWindow.Instance().ShowText(obj.Text);
            InfoWindow.Instance().UpdateText(obj.Text);
        }

        private void ShowToolForCurrentSelection(bool clear = false)
        {
            if (clear == true)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
            }

            // if nothing was selected turn the all editor tools back on
            if ((selectedObjectAdorner == null) ||
                 ((selectedObjectAdorner != null && selectedObjectAdorner.NumberOfSelectedObjects() == 0)))
            {
                NotificationManager.Notify("SetToolsVisibility", true);
            }
            if (selectedObjectAdorner != null && selectedObjectAdorner.NumberOfSelectedObjects() == 1)
            {
                string s = selectedObjectAdorner.SelectedObjects[0].EditorParameters.Get("ToolName");
                if (s != "")
                {
                    NotificationManager.Notify("SetSingleToolsVisible", s);
                }
            }
        }

        private void SplitObjectInHalf(Object3D ob, string ori)
        {
            if (ob != null)
            {
                // we will need a copy
                Object3D clone = ob.Clone();

                // and a box
                Object3D clipBox = new Object3D();
                clipBox.BuildPrimitive("box");
                clipBox.Position = new Point3D(ob.Position.X, ob.Position.Y, ob.Position.Z);
                // make the clipbox 0.01 mm bigger than the original object in all dimensions
                clipBox.ScaleMesh(ob.Scale.X + 0.01, ob.Scale.Y + 0.01, ob.Scale.Z + 0.01);
                clipBox.CalcScale();
                clipBox.Remesh(); ;

                // where should the clipbox be positioned
                switch (ori)
                {
                    case "X":
                        {
                            // move the clipbox so its front is positioned exactly on the middle x line of the ob.
                            // Note you can't rely on the origin position being in the middle so you have to find it
                            Point3D obMid = ob.AbsoluteBounds.MidPoint();
                            double x = obMid.X;
                            double y = obMid.Y;
                            double z = obMid.Z - (clipBox.Scale.Z / 2);
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            Group3D grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D front = grp.ConvertToMesh();
                            front.Name = ob.Name + "_Front";
                            front.Remesh();
                            document.Content.Add(front);

                            z = obMid.Z + (clipBox.Scale.Z / 2);
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D back = grp.ConvertToMesh();
                            back.Name = ob.Name + "_Back";
                            back.Remesh();
                            document.Content.Add(back);
                            document.Content.Remove(ob);
                            document.Dirty = true;
                            NotificationManager.Notify("ObjectNamesChanged", null);
                            RegenerateDisplayList();
                        }
                        break;

                    case "Y":
                        {
                            Point3D obMid = ob.AbsoluteBounds.MidPoint();
                            double x = obMid.X;
                            double y = obMid.Y - (clipBox.Scale.Y / 2);
                            double z = obMid.Z;
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            Group3D grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D front = grp.ConvertToMesh();
                            front.Name = ob.Name + "_Top";
                            front.Remesh();
                            document.Content.Add(front);

                            y = obMid.Y + (clipBox.Scale.Y / 2);
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D back = grp.ConvertToMesh();
                            back.Name = ob.Name + "_Bottom";
                            back.Remesh();
                            document.Content.Add(back);
                            document.Content.Remove(ob);
                            document.Dirty = true;
                            NotificationManager.Notify("ObjectNamesChanged", null);
                            RegenerateDisplayList();
                        }
                        break;

                    case "Z":
                        {
                            Point3D obMid = ob.AbsoluteBounds.MidPoint();
                            double x = obMid.X - (clipBox.Scale.X / 2);
                            double y = obMid.Y;

                            double z = obMid.Z;
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            Group3D grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D front = grp.ConvertToMesh();
                            front.Name = ob.Name + "_Right";
                            front.Remesh();
                            document.Content.Add(front);

                            x = obMid.X + (clipBox.Scale.X / 2);
                            clipBox.Position = new Point3D(x, y, z);
                            clipBox.Remesh();
                            // now clipbox should sitting over all the points we want to remove.
                            // so do a group difference
                            grp = new Group3D();

                            grp.LeftObject = ob;
                            grp.RightObject = clipBox;
                            grp.PrimType = "groupdifference";
                            grp.Init();
                            Object3D back = grp.ConvertToMesh();
                            back.Name = ob.Name + "_Left";
                            back.Remesh();
                            document.Content.Add(back);
                            document.Content.Remove(ob);
                            document.Dirty = true;
                            NotificationManager.Notify("ObjectNamesChanged", null);
                            RegenerateDisplayList();
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private bool Test()
        {
            return true;
        }

        private void TopCamera()
        {
            ResetSelection();
            camera.HomeTop();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 0, 1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void UpdateSelectionDisplay()
        {
            SetSelectionColours();
            // update the display
            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }

            NotifyPropertyChanged("ModelItems");
        }

        private void XRotationChanged(object param)
        {
            double d = (double)param;
            keyrotationx = d;
        }

        private void YRotationChanged(object param)
        {
            double d = (double)param;
            keyrotationy = d;
        }

        private void Zoom(double v)
        {
            camera.Zoom(v);
            zoomPercent += v;
            ReportCameraPosition();
            NotifyPropertyChanged("CameraPos");
        }

        private void ZoomIn(object param)
        {
            Zoom(1);
        }

        private void ZoomOut(object param)
        {
            if (zoomPercent > 0)
            {
                Zoom(-1);
            }
        }

        private void ZoomReset(object param)
        {
            double diff = 100 - zoomPercent;
            Zoom(diff);
            zoomPercent = 100;
        }

        private void ZRotationChanged(object param)
        {
            double d = (double)param;
            keyrotationz = d;
        }
    }
}