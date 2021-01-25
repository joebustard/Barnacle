using Make3D.Adorners;
using Make3D.Dialogs;
using Make3D.Models;
using Make3D.Views;
using MeshDecimator;
using Microsoft.Win32;
using PlacementLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Workflow;

namespace Make3D.ViewModels
{
    internal class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        // some of the primitives need to be rotated when they are first created so they match the
        // orientaion shown on the icons
        private static string[] rotatedPrimitives =
        {
            "roof","cone","pyramid","roundroof","cap","polygon","rightangle","pointy"
        };

        private Bounds3D allBounds;
        private PolarCamera camera;

        private Point3D CameraLookObject = new Point3D(0, 0, 0);

        //     private Point3D cameraPos;
        private CameraModes cameraMode;

        private Point3D CameraScrollDelta = new Point3D(1, 1, 0);

        private Floor floor;
        private Axies axies;
        private FloorMarker floorMarker;
        private Point lastMouse;
        private Vector3D lookDirection;
        private Model3DCollection modelItems;
        private double onePercentZoom;
        private List<Object3D> selectedItems;
        private Adorner selectedObjectAdorner;
        private int totalFaces;
        private double zoomPercent = 100;
        private Grid3D grid;
        private bool showAdorners;

        public EditorViewModel()
        {
            floor = new Floor();
            axies = new Axies();
            floorMarker = null;
            grid = new Grid3D();
            //FloorObjectVertices = flr.FloorPoints3D;
            // FloorTriangleIndices = flr.FloorPointsIndices;
            camera = new PolarCamera();

            onePercentZoom = camera.Distance / 100.0;
            LookToCenter();

            cameraMode = CameraModes.CameraMoveLookCenter;
            //  LoadObject("teapot.obj");

            modelItems = new Model3DCollection();

            modelItems.Add(floor.FloorMesh);
            NotificationManager.Subscribe("ZoomIn", ZoomIn);
            NotificationManager.Subscribe("ZoomOut", ZoomOut);
            NotificationManager.Subscribe("ZoomReset", ZoomReset);
            NotificationManager.Subscribe("CameraCommand", OnCameraCommand);
            NotificationManager.Subscribe("AddObject", OnAddObject);
            NotificationManager.Subscribe("Refresh", OnRefresh);
            NotificationManager.Subscribe("RefreshAdorners", OnRefreshAdorners);
            NotificationManager.Subscribe("NewDocument", OnNewDocument);
            NotificationManager.Subscribe("Remesh", OnRemesh);
            NotificationManager.Subscribe("Select", Select);
            NotificationManager.Subscribe("Group", OnGroup);
            NotificationManager.Subscribe("Cut", OnCut);
            NotificationManager.Subscribe("Copy", OnCopy);
            NotificationManager.Subscribe("Paste", OnPaste);
            NotificationManager.Subscribe("PasteAt", OnPasteAt);
            NotificationManager.Subscribe("DoMultiPaste", OnMultiPaste);
            NotificationManager.Subscribe("CircularPaste", OnCircularPaste);

            NotificationManager.Subscribe("Export", OnExport);
            NotificationManager.Subscribe("ExportParts", OnExportParts);
            NotificationManager.Subscribe("Slice", OnSlice);
            NotificationManager.Subscribe("Import", OnImport);
            NotificationManager.Subscribe("MoveObjectToFloor", OnMoveObjectToFloor);
            NotificationManager.Subscribe("MoveObjectToCentre", OnMoveObjectToCentre);
            NotificationManager.Subscribe("Marker", MoveToMarker);
            NotificationManager.Subscribe("Alignment", OnAlignment);
            NotificationManager.Subscribe("Distribute", OnDistribute);
            NotificationManager.Subscribe("Flip", OnFlip);
            NotificationManager.Subscribe("Size", OnSize);
            NotificationManager.Subscribe("Undo", OnUndo);
            NotificationManager.Subscribe("Irregular", OnIrregular);
            NotificationManager.Subscribe("Linear", OnLinear);
            NotificationManager.Subscribe("Doughnut", OnDoughNut);
            NotificationManager.Subscribe("BezierFuselage", OnFuselage);
            NotificationManager.Subscribe("TwoShape", OnTwoShape);
            NotificationManager.Subscribe("SpurGear", OnSpurGear);
            NotificationManager.Subscribe("TankTrack", OnTankTrack);
            NotificationManager.Subscribe("MeshEdit", OnMeshEdit);
            NotificationManager.Subscribe("Stadium", OnStadium);
            NotificationManager.Subscribe("BezierRing", OnBezierRing);
            NotificationManager.Subscribe("ShowFloor", OnShowFloor);
            NotificationManager.Subscribe("ShowFloorMarker", OnShowFloorMarker);
            NotificationManager.Subscribe("ShowAxies", OnShowAxies);
            NotificationManager.Subscribe("SelectObjectName", SelectObjectByName);
            ReportCameraPosition();
            selectedItems = new List<Object3D>();
            allBounds = new Bounds3D();
            allBounds.Adjust(new Point3D(0, 0, 0));
            totalFaces = 0;
            showAxies = true;
            showFloor = true;
            showAdorners = true;
            showFloorMarker = true;
            RegenerateDisplayList();
        }

        private void OnBezierRing(object param)
        {
            BezierRingDlg dlg = new BezierRingDlg();
            DisplayModeller(dlg);
        }

        private void OnMeshEdit(object param)
        {
            MeshEditorDlg dlg = new MeshEditorDlg();
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
                        dlg.SetInitialMesh(editingObj.RelativeObjectVertices, editingObj.TriangleIndices);
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
                editingObj.RelativeObjectVertices = dlg.Vertices;
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

        private void MoveToMarker(object param)
        {
            if (showFloorMarker == true && floorMarker != null)
            {
                Point3D target = floorMarker.Position;

                MoveToPoint(target);
            }
        }

        private void MoveToPoint(Point3D target)
        {
            Bounds3D tmpBounds = new Bounds3D();
            foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
            {
                tmpBounds.Add(ob.AbsoluteBounds);
            }

            foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
            {
                double dAbsX = target.X + (ob.Position.X - tmpBounds.MidPoint().X);
                double dAbsY = ob.Position.Y;
                double dAbsZ = target.Z + (ob.Position.Z - tmpBounds.MidPoint().Z);
                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                ob.RelativeToAbsolute();
            }
            Document.Dirty = true;
            // move the selectors
            selectedObjectAdorner.GenerateAdornments();
            // redraw
            RegenerateDisplayList();
        }

        private void SelectObjectByName(object param)
        {
            string nm = param.ToString();

            ResetSelection();

            if (Document.Content.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                foreach (Object3D ob in Document.Content)
                {
                    if (ob.Name == nm)
                    {
                        selectedItems.Add(ob);
                        selectedObjectAdorner.AdornObject(ob);
                        NotificationManager.Notify("ObjectSelected", ob);
                        EnableTool(ob);
                    }
                }
            }
            UpdateSelectionDisplay();
        }

        private void OnTankTrack(object param)
        {
            TrackDialog dlg = new TrackDialog();
            DisplayModeller(dlg);
        }

        private void OnStadium(object param)
        {
            StadiumDialog dlg = new StadiumDialog();
            DisplayModeller(dlg);
        }

        private void OnCircularPaste(object param)
        {
            CircularPasteDlg dlg = new CircularPasteDlg();
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

                            RegenerateDisplayList();
                            UpdateSelectionDisplay();
                        }
                    }
                }
            }
        }

        private void OnSpurGear(object param)
        {
            SpurGearDialog dlg = new SpurGearDialog();
            DisplayModeller(dlg);
        }

        private void OnSlice(object param)
        {
            string s = param.ToString();
            if (s == "SliceAll")
            {
                string modelPath = Document.FilePath;
                string modelName = Path.GetFileNameWithoutExtension(modelPath);
                modelPath = Path.GetDirectoryName(modelPath);
                string exportPath = Path.Combine(modelPath, "export");
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                exportPath = Path.Combine(exportPath, modelName + ".stl");

                string gcodePath = Path.Combine(modelPath, "gcode");
                if (!Directory.Exists(gcodePath))
                {
                    Directory.CreateDirectory(gcodePath);
                }
                gcodePath = Path.Combine(gcodePath, modelName + ".gcode");

                string logPath = Path.Combine(modelPath, "slicelog.log");

                string exportedPath = Document.ExportAll("STL", allBounds);
                SlicerInterface.Slice(exportPath, gcodePath, logPath, "Print3D");
            }
        }

        private void OnTwoShape(object param)
        {
            ShapeLoftDialog dlg = new ShapeLoftDialog();
            DisplayModeller(dlg);
        }

        private void OnDoughNut(object param)
        {
            TorusDialog torusDialog = new TorusDialog();
            DisplayModeller(torusDialog);
        }

        private void OnFuselage(object param)
        {
            FuselageLoftDialog dlg = new FuselageLoftDialog();
            DisplayModeller(dlg);
        }

        private void OnExportParts(object param)
        {
            string exportedPath = Document.ExportAllPartsSeperately(param.ToString(), allBounds);

            STLExportedPartsConfirmation dlg = new STLExportedPartsConfirmation();
            dlg.ExportPath = exportedPath;
            dlg.ShowDialog();
        }

        private void OnLinear(object param)
        {
            LinearLoftDialog dlg = new LinearLoftDialog();
            DisplayModeller(dlg);
        }

        private void OnIrregular(object param)
        {
            IrregularPolygonDlg dlg = new IrregularPolygonDlg();
            DisplayModeller(dlg);
        }

        private void DisplayModeller(BaseModellerDialog dlg)
        {
            CheckPoint();
            EditorParameters pm = new EditorParameters();
            Object3D editingObj = null;
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
                editingObj.RelativeObjectVertices = dlg.Vertices;
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

        private bool showFloor;
        private bool showFloorMarker;
        private bool showAxies;

        private void OnShowAxies(object param)
        {
            showAxies = (param as bool?) == true;
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

        private enum CameraModes
        {
            None,
            CameraMove,
            CameraMoveLookCenter,
            CameraMoveLookObject
        }

        public Point3D CameraPos
        {
            get { return camera.CameraPos; }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public Vector3D LookDirection
        {
            get { return lookDirection; }
            set
            {
                if (lookDirection != value)
                {
                    lookDirection = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Model3DCollection ModelItems
        {
            get { return modelItems; }
            set
            {
                if (modelItems != value)
                {
                    modelItems = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void CountFaces()
        {
            totalFaces = 0;
            foreach (Object3D ob in Document.Content)
            {
                totalFaces += ob.TotalFaces;
            }
        }

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

        public void RegenerateDisplayList()
        {
            CountFaces();
            allBounds = new Bounds3D();
            allBounds.Zero();
            modelItems.Clear();
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
        }

        internal void DeselectAll()
        {
            NotificationManager.Notify("SetToolsVisibility", true);
            if (selectedObjectAdorner != null)
            {
                selectedObjectAdorner.Clear();
            }
            // RemoveObjectAdorner();
            //  ResetSelectionColours();
            //  selectedObjectAdorner = new SizeAdorner(camera);
            selectedItems.Clear();
            RegenerateDisplayList();
            NotificationManager.Notify("ObjectSelected", null);
        }

        internal void KeyDown(Key key, bool shift, bool ctrl)
        {
            switch (key)
            {
                case Key.Up:
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
                                selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Up, 1.0);
                            }
                        }
                    }
                    break;

                case Key.Down:
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
                                selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Down, 1.0);
                            }
                        }
                    }
                    break;

                case Key.Left:
                    {
                        CheckPointForNudge();
                        if (shift)
                        {
                            selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Left, 0.1);
                        }
                        else
                        {
                            selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Left, 1.0);
                        }
                    }
                    break;

                case Key.Right:
                    {
                        CheckPointForNudge();
                        if (shift)
                        {
                            selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Right, 0.1);
                        }
                        else
                        {
                            selectedObjectAdorner.Nudge(Adorner.NudgeDirection.Right, 1.0);
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
                    }
                    break;

                case Key.H:
                    {
                        showAdorners = false;
                        RegenerateDisplayList();
                    }
                    break;
            }
        }

        private void MoveSelectionToCentre()
        {
            Point3D centre = new Point3D(0, 0, 0);
            MoveToPoint(centre);
        }

        private void FloorAllObjects()
        {
            foreach (Object3D ob in Document.Content)
            {
                ob.MoveToFloor();
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
            if (selectedObjectAdorner != null && selectedObjectAdorner.MouseMove(lastMouse, newPos, e, ctrlDown) == true)
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
            if (selectedObjectAdorner != null)
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

        internal void Select(GeometryModel3D geo, Point3D hitPos, bool size, bool append = false)
        {
            bool handled = false;
            NotificationManager.Notify("SetToolsVisibility", false);

            if (selectedObjectAdorner != null)
            {
                handled = selectedObjectAdorner.Select(geo);
            }
            if (!handled)
            {
                handled = CheckIfContentSelected(geo, append, size);
            }
            if (!handled)
            {
                if (floor.Matches(geo) || grid.Matches(geo))
                {
                    if (selectedObjectAdorner != null)
                    {
                        selectedObjectAdorner.Clear();
                    }
                    floorMarker = new FloorMarker();
                    floorMarker.Position = hitPos;
                    RegenerateDisplayList();
                }
            }

            ShowToolForCurrentSelection();
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
            foreach (Point3D pnt in ob.RelativeObjectVertices)
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
                //foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
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
                                //dAbsX = ob.Position.X - (ob.AbsoluteBounds.Lower.X - selectedObjectAdorner.Bounds.Lower.X);
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.Lower.X - bns.Lower.X);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Right":
                            {
                                //dAbsX = ob.Position.X + (selectedObjectAdorner.Bounds.Upper.X - ob.AbsoluteBounds.Upper.X);
                                dAbsX = ob.Position.X + (bns.Upper.X - ob.AbsoluteBounds.Upper.X);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Top":
                            {
                                //dAbsY = ob.Position.Y + (selectedObjectAdorner.Bounds.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                dAbsY = ob.Position.Y + (bns.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Bottom":
                            {
                                //dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - selectedObjectAdorner.Bounds.Lower.Y);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Lower.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Back":
                            {
                                //dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - selectedObjectAdorner.Bounds.Lower.Z);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Lower.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Front":
                            {
                                //                                dAbsZ = ob.Position.Z + (selectedObjectAdorner.Bounds.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                dAbsZ = ob.Position.Z + (bns.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "Centre":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - bns.MidPoint().Z);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, dAbsZ);
                                ob.RelativeToAbsolute();
                            }
                            break;

                        case "StackY":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Upper.Y) - 0.001;
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackBottom":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = bns.Lower.Y - (ob.AbsoluteBounds.Height / 2) + 0.001;
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackRight":
                            {
                                //dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsX = bns.Upper.X + ob.AbsoluteBounds.Width / 2 - 0.001;
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackLeft":
                            {
                                //dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsX = bns.Lower.X - ob.AbsoluteBounds.Width / 2 + 0.001;
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackZ":
                            {
                                //dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                // dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Upper.Z) - 0.001;
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
                                bns.Add(ob.AbsoluteBounds);
                            }
                            break;

                        case "StackBehind":
                            {
                                //dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - midX);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.MidPoint().Y - midY);
                                // dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - midZ);
                                dAbsZ = bns.Lower.Z - (ob.AbsoluteBounds.Depth / 2) + 0.001;
                                ob.Position = new Point3D(dAbsX, dAbsY, dAbsZ);
                                ob.RelativeToAbsolute();
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
                                    ob.RelativeToAbsolute();
                                }
                            }
                            break;
                    }
                }
            }
            selectedObjectAdorner.GenerateAdornments();
        }

        private void FloorSelectedObjects()
        {
            for (int i = 0; i < selectedObjectAdorner.SelectedObjects.Count; i++)
            {
                Object3D ob = selectedObjectAdorner.SelectedObjects[i];

                ob.MoveToFloor();
                ob.RelativeToAbsolute();
            }
        }

        private void BackCamera()
        {
            camera.HomeBack();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void BottomCamera()
        {
            camera.HomeBottom();
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

        private bool CheckIfContentSelected(GeometryModel3D geo, bool append, bool sizer)
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
                    GenerateSelectionBox(selectedItems[0], sizer);
                    NotificationManager.Notify("ObjectSelected", selectedItems[0]);
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

        private void Decimate()
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count == 1)
            {
                CheckPoint();
                Object3D ob = selectedObjectAdorner.SelectedObjects[0];
                MeshDecimator.Mesh mesh = ObjectMeshToDecimatorMesh(ob);
                DecimatorSettings dlg = new DecimatorSettings();
                dlg.OriginalFaceCount = mesh.TriangleCount;
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
                            Point3D pnt = new Point3D(v.x, v.y, v.z);
                            ob.RelativeObjectVertices.Add(pnt);
                        }
                        ob.TriangleIndices.Clear();
                        for (int i = 0; i < smallerMesh.Indices.GetLength(0); i++)
                        {
                            ob.TriangleIndices.Add(smallerMesh.Indices[i]);
                        }
                        ob.Remesh();
                        RegenerateDisplayList();
                    }
                }
            }
            else
            {
                MessageBox.Show("Decimate requires a single objec to be selected");
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

                        case "Inside":
                            {
                                ob.FlipInside();
                                ob.Remesh();
                            }
                            break;
                    }
                }
                Document.Content.Add(ob);
            }
        }

        private void GenerateSelectionBox(Object3D object3D, bool sizer)
        {
            if (sizer)
            {
                selectedObjectAdorner = new SizeAdorner(camera);
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

        private bool GroupToMesh()
        {
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

        private void HomeCamera()
        {
            camera.HomeFront();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void LeftCamera()
        {
            camera.HomeLeft();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, 1);
            LookToCenter();
            zoomPercent = 100;
        }

        private bool Loft(Object3D obj, string obType)
        {
            bool res = false;
            if (obType == "shapeloft")
            {
                ShapeLoftDialog dlg = new ShapeLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    obj.RelativeObjectVertices = dlg.Vertices;
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
                    obj.RelativeObjectVertices = dlg.Vertices;
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
                //ResetSelectionColours();
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

        private bool MakeGroup3D(string s)
        {
            bool res = false;
            if (selectedObjectAdorner.NumberOfSelectedObjects() >= 2)
            {
                CheckPoint();
                Object3D leftie = selectedObjectAdorner.SelectedObjects[0];
                int i = 1;
                while (i < selectedObjectAdorner.NumberOfSelectedObjects())
                {
                    Group3D grp = new Group3D();
                    grp.Name = leftie.Name;
                    grp.Description = leftie.Description;
                    grp.LeftObject = leftie;
                    grp.RightObject = selectedObjectAdorner.SelectedObjects[i];
                    grp.PrimType = s;

                    if (grp.Init())
                    {
                        Document.ReplaceObjectsByGroup(grp);
                        leftie = grp;
                        i++;
                        res = true;
                    }
                    else
                    {
                        MessageBox.Show("Group operation failed, it produces too many faces");
                        return false;
                    }
                }
            }
            return res;
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
            obj.Position = new Point3D(allBounds.Upper.X + obj.Scale.X / 2, obj.Scale.Y / 2, 0);
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
                string s = param.ToString();
                AlignSelectedObjects(s);
                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
            }
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

        private void OnCopy(object param)
        {
            if (selectedObjectAdorner != null)
            {
                ObjectClipboard.Clear();
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
            }
        }

        private void OnExport(object param)
        {
            string s = param as string;
            if (s != null)
            {
                bool all = true;
                if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 0)
                {
                    ExportSelectedDialog seldlg = new ExportSelectedDialog();
                    seldlg.ShowDialog();
                    if (seldlg.Result == ExportSelectedDialog.ExportChoice.Selected)
                    {
                        all = false;
                    }
                }
                string exportedPath = "";
                if (all)
                {
                    exportedPath = Document.ExportAll(param.ToString(), allBounds);
                }
                else
                {
                    exportedPath = Document.ExportSelectedParts(param.ToString(), allBounds, selectedObjectAdorner.SelectedObjects);
                }

                STLExportedConfirmation dlg = new STLExportedConfirmation();
                dlg.ExportPath = exportedPath;
                if (dlg.ShowDialog() == true)
                {
                }
            }
        }

        private void OnFlip(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 0)
            {
                CheckPoint();
                string s = param.ToString();
                FlipSelectedObjects(s);
                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
            }
        }

        private async void OnGroup(object param)
        {
            string s = param.ToString().ToLower();
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
                MakeGroup3D(s);
                RegenerateDisplayList();
                NotificationManager.Notify("ObjectNamesChanged", null);
            }
        }

        private void OnImport(object param)
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
                                Document.ImportStl(dlg.FileName);

                                RegenerateDisplayList();
                            }
                        }
                    }
                    break;
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
                arr.AddComponent(ob,
                                 new Point(ob.AbsoluteBounds.Lower.X, ob.AbsoluteBounds.Lower.Z),
                                 new Point(ob.AbsoluteBounds.Upper.X, ob.AbsoluteBounds.Upper.Z));
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
            RegenerateDisplayList();
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

        private void ReportCameraPosition()
        {
            String s = $"Camera ({camera.CameraPos.X:F2},{camera.CameraPos.Y:F2},{camera.CameraPos.Z:F2}) => ({lookDirection.X:F2},{lookDirection.Y:F2},{lookDirection.Z:F2}) Zoom {zoomPercent:F1}%";
            NotificationManager.Notify("SetStatusText1", s);
            s = $"Total Faces {totalFaces}";
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
                // selectedObjectAdorner.Clear();
            }

            selectedObjectAdorner = new SizeAdorner(camera);
            selectedItems.Clear();
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
            camera.HomeRight();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, -1);
            LookToCenter();
            zoomPercent = 100;
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
            ResetSelection();

            if (Document.Content.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D ob = Document.Content[0];
                selectedItems.Add(ob);
                selectedObjectAdorner.AdornObject(ob);
                NotificationManager.Notify("ObjectSelected", ob);
                EnableTool(ob);
            }
            UpdateSelectionDisplay();
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

        private void SelectLast()
        {
            ResetSelection();
            if (Document.Content.Count > 0)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D ob = Document.Content[Document.Content.Count - 1];
                selectedItems.Add(ob);
                selectedObjectAdorner.AdornObject(ob);
                NotificationManager.Notify("ObjectSelected", ob);
                EnableTool(ob);
            }
            UpdateSelectionDisplay();
        }

        private void SelectNext()
        {
            if (selectedItems.Count == 1)
            {
                ResetSelectionColours();
                NotificationManager.Notify("SetToolsVisibility", false);
                Object3D sel = selectedItems[0];

                Object3D nxt = null;
                if (sel != null)
                {
                    for (int i = 0; i < Document.Content.Count - 1; i++)
                    {
                        if (Document.Content[i] == sel)
                        {
                            nxt = Document.Content[i + 1];
                        }
                    }
                    if (nxt == null)
                    {
                        nxt = Document.Content[0];
                    }
                }
                ResetSelection();

                selectedItems.Add(nxt);
                selectedObjectAdorner.AdornObject(nxt);
                NotificationManager.Notify("ObjectSelected", nxt);
                EnableTool(nxt);
                UpdateSelectionDisplay();
            }
        }

        private void SelectPrevious()
        {
            if (selectedItems.Count == 1)
            {
                NotificationManager.Notify("SetToolsVisibility", false);
                ResetSelectionColours();
                Object3D sel = selectedItems[0];

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
                ResetSelection();

                selectedItems.Add(nxt);
                selectedObjectAdorner.AdornObject(nxt);
                NotificationManager.Notify("ObjectSelected", nxt);
                EnableTool(nxt);
                UpdateSelectionDisplay();
            }
        }

        private void SetSelectionColours()
        {
            List<GeometryModel3D> tmp = modelItems.OfType<GeometryModel3D>().ToList();
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in tmp)
                {
                    if (ob.Mesh == md.Geometry)
                    {
                        md.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                    }
                }
            }
        }

        private void TopCamera()
        {
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
    }
}