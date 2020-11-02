using Make3D.Adorners;
using Make3D.Dialogs;
using Make3D.Models;
using MeshDecimator;
using Microsoft.Win32;
using PlacementLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Make3D.ViewModels
{
    internal class EditorViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private PolarCamera camera;

        private Point3D CameraLookObject = new Point3D(0, 0, 0);
        private Point3D CameraScrollDelta = new Point3D(1, 1, 0);

        private double onePercentZoom;
        private double zoomPercent = 100;

        //     private Point3D cameraPos;
        private CameraModes cameraMode;

        private GeometryModel3D floor;
        private Vector3DCollection floorNormals;
        private Point3DCollection floorObjectVertices;
        private Int32Collection floorTriangleIndices;
        private Point lastMouse;
        private Vector3D lookDirection;
        private int totalFaces;
        private Model3DCollection modelItems;

        private List<Object3D> selectedItems;

        private Bounds3D allBounds;

        private SizeAdorner selectedObjectAdorner;

        public EditorViewModel()
        {
            FloorObjectVertices = FloorPoints3D;
            FloorTriangleIndices = FloorPointsIndices;
            camera = new PolarCamera();

            onePercentZoom = camera.Distance / 100.0;
            LookToCenter();

            cameraMode = CameraModes.CameraMoveLookCenter;
            //  LoadObject("teapot.obj");

            modelItems = new Model3DCollection();
            floor = GetFloor();
            modelItems.Add(floor);
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
            NotificationManager.Subscribe("DoMultiPaste", OnMultiPaste);
            NotificationManager.Subscribe("Export", OnExport);
            NotificationManager.Subscribe("Import", OnImport);
            NotificationManager.Subscribe("MoveObjectToFloor", OnMoveObjectToFloor);
            NotificationManager.Subscribe("MoveObjectToCentre", OnMoveObjectToCentre);
            NotificationManager.Subscribe("Alignment", OnAlignment);
            NotificationManager.Subscribe("Distribute", OnDistribute);
            NotificationManager.Subscribe("Flip", OnFlip);
            NotificationManager.Subscribe("Size", OnSize);
            NotificationManager.Subscribe("Undo", OnUndo);
            ReportCameraPosition();
            selectedItems = new List<Object3D>();
            allBounds = new Bounds3D();
            allBounds.Adjust(new Point3D(0, 0, 0));
            totalFaces = 0;
        }

        private void OnDistribute(object param)
        {
            string s = param.ToString();
            if (s == "Optimum")
            {
                OptimisePlacement();
            }
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

        private void OnUndo(object param)
        {
            Undo();
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

                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
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
                confirmed = res == MessageBoxResult.Yes;
            }
            List<Object3D> tmp = new List<Object3D>();
            foreach (Object3D ob in selectedObjectAdorner.SelectedObjects)
            {
                Object3D it = ob.ConvertToMesh();

                Document.Content.Remove(ob);
                it.Remesh();
                tmp.Add(it);
            }
            selectedObjectAdorner.Clear();
            foreach (Object3D ob in tmp)
            {
                switch (s)
                {
                    case "Horizontal":
                        {
                            ob.FlipX();
                            ob.Remesh();
                        }
                        break;

                    case "Vertical":
                        {
                            ob.FlipY();
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
                Document.Content.Add(ob);
            }
        }

        private void OnAlignment(object param)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.SelectedObjects.Count > 1)
            {
                CheckPoint();
                string s = param.ToString();
                AlignSelectedObjects(s);
                selectedObjectAdorner.Refresh();
                RegenerateDisplayList();
            }
        }

        private void AlignSelectedObjects(string s)
        {
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
                            }
                            break;

                        case "Right":
                            {
                                //dAbsX = ob.Position.X + (selectedObjectAdorner.Bounds.Upper.X - ob.AbsoluteBounds.Upper.X);
                                dAbsX = ob.Position.X + (bns.Upper.X - ob.AbsoluteBounds.Upper.X);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, ob.Position.Z);
                            }
                            break;

                        case "Top":
                            {
                                //dAbsY = ob.Position.Y + (selectedObjectAdorner.Bounds.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                dAbsY = ob.Position.Y + (bns.Upper.Y - ob.AbsoluteBounds.Upper.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                            }
                            break;

                        case "Bottom":
                            {
                                //dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - selectedObjectAdorner.Bounds.Lower.Y);
                                dAbsY = ob.Position.Y - (ob.AbsoluteBounds.Lower.Y - bns.Lower.Y);
                                ob.Position = new Point3D(ob.Position.X, dAbsY, ob.Position.Z);
                            }
                            break;

                        case "Back":
                            {
                                //dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - selectedObjectAdorner.Bounds.Lower.Z);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.Lower.Z - bns.Lower.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                            }
                            break;

                        case "Front":
                            {
                                //                                dAbsZ = ob.Position.Z + (selectedObjectAdorner.Bounds.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                dAbsZ = ob.Position.Z + (bns.Upper.Z - ob.AbsoluteBounds.Upper.Z);
                                ob.Position = new Point3D(ob.Position.X, ob.Position.Y, dAbsZ);
                            }
                            break;

                        case "Centre":
                            {
                                dAbsX = ob.Position.X - (ob.AbsoluteBounds.MidPoint().X - bns.MidPoint().X);
                                dAbsZ = ob.Position.Z - (ob.AbsoluteBounds.MidPoint().Z - bns.MidPoint().Z);
                                ob.Position = new Point3D(dAbsX, ob.Position.Y, dAbsZ);
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

                        case "Floor":
                            {
                                ob.MoveToFloor();
                            }
                            break;
                    }
                }
            }
        }

        internal void KeyUp(Key key, bool shift, bool ctrl)
        {
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
            }
        }

        private void OnPaste(object param)
        {
            if (ObjectClipboard.HasItems())
            {
                CheckPoint();
                selectedObjectAdorner.Clear();
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    Object3D o = cl.Clone();
                    o.Name = Document.NextName;

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
                    modelItems.Add(gm);
                }

                RegenerateDisplayList();
            }
        }

        private void OnMultiPaste(object param)
        {
            MultiPasteConfig cfg = param as MultiPasteConfig;
            if (cfg != null)
            {
                if (ObjectClipboard.HasItems())
                {
                    CheckPoint();
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
                            //o.Position = new Point3D(allBounds.Upper.X + o.AbsoluteBounds.Width / 2, 0, 0);
                            o.Remesh();
                            //o.MoveToFloor();
                            allBounds.Add(o.AbsoluteBounds);
                            GeometryModel3D gm = GetMesh(o);
                            Document.Content.Add(o);
                            Document.Dirty = true;
                            modelItems.Add(gm);
                        }
                    }
                    RegenerateDisplayList();
                }
            }
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

        private void OnMoveObjectToFloor(object param)
        {
            Object3D prm = param as Object3D;
            CheckPoint();
            prm.MoveToFloor();
            selectedObjectAdorner.Clear();
            RegenerateDisplayList();
        }

        private void OnMoveObjectToCentre(object param)
        {
            Object3D prm = param as Object3D;
            CheckPoint();
            prm.MoveToCentre();
            selectedObjectAdorner.Clear();
            RegenerateDisplayList();
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

        private void OnExport(object param)
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
                exportedPath = Document.ExportParts(param.ToString(), allBounds, selectedObjectAdorner.SelectedObjects);
            }

            STLExportedConfirmation dlg = new STLExportedConfirmation();
            dlg.ExportPath = exportedPath;
            if (dlg.ShowDialog() == true)
            {
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
            }
        }

        private void OnGroup(object param)
        {
            string s = param.ToString().ToLower();
            if (s == "mesh")
            {
                CheckPoint();
                if (GroupToMesh())
                {
                    selectedObjectAdorner.Clear();
                    RegenerateDisplayList();
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
                }
            }
            else
            if (MakeGroup3D(s))
            {
                RegenerateDisplayList();
            }
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
                    grp.Name = Document.NextName;
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

        /*
        private void CalculateCameraPlane()
        {
            if (cameraDistance > 0)
            {
                double theta = Math.Acos(cameraPos.Z / cameraDistance);
                double phi = Math.Atan2(cameraPos.Y, cameraPos.X);
                Vector3D t = new Vector3D();
                t.X = Math.Sin(theta) * Math.Cos(phi);
                t.Y = Math.Sin(theta) * Math.Sin(phi);
                t.Z = Math.Cos(theta);
                t.Normalize();
                cameraPlane = t;
            }
        }
        */

        private void OnRemesh(object param)
        {
            throw new NotImplementedException();
        }

        private void ZoomReset(object param)
        {
            double diff = 100 - zoomPercent;
            Zoom(diff);
            zoomPercent = 100;
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

        public GeometryModel3D Floor
        {
            get { return floor; }
        }

        public Point3DCollection FloorObjectVertices
        {
            get { return floorObjectVertices; }
            set
            {
                if (floorObjectVertices != value)
                {
                    floorObjectVertices = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Point3DCollection FloorPoints3D
        {
            get
            {
                double x = 210.0; // floor width / 2
                double z = 210.0; // floor length / 2
                double floorDepth = -.1; // give the floor some depth so it's not a 2 dimensional plane

                Point3DCollection points = new Point3DCollection(20);
                Point3D point;
                //top of the floor
                point = new Point3D(-x, 0, z);// Floor Index - 0
                points.Add(point);
                point = new Point3D(x, 0, z);// Floor Index - 1
                points.Add(point);
                point = new Point3D(x, 0, -z);// Floor Index - 2
                points.Add(point);
                point = new Point3D(-x, 0, -z);// Floor Index - 3
                points.Add(point);
                //front side
                point = new Point3D(-x, 0, z);// Floor Index - 4
                points.Add(point);
                point = new Point3D(-x, floorDepth, z);// Floor Index - 5
                points.Add(point);
                point = new Point3D(x, floorDepth, z);// Floor Index - 6
                points.Add(point);
                point = new Point3D(x, 0, z);// Floor Index - 7
                points.Add(point);
                //right side
                point = new Point3D(x, 0, z);// Floor Index - 8
                points.Add(point);
                point = new Point3D(x, floorDepth, z);// Floor Index - 9
                points.Add(point);
                point = new Point3D(x, floorDepth, -z);// Floor Index - 10
                points.Add(point);
                point = new Point3D(x, 0, -z);// Floor Index - 11
                points.Add(point);
                //back side
                point = new Point3D(x, 0, -z);// Floor Index - 12
                points.Add(point);
                point = new Point3D(x, floorDepth, -z);// Floor Index - 13
                points.Add(point);
                point = new Point3D(-x, floorDepth, -z);// Floor Index - 14
                points.Add(point);
                point = new Point3D(-x, 0, -z);// Floor Index - 15
                points.Add(point);
                //left side
                point = new Point3D(-x, 0, -z);// Floor Index - 16
                points.Add(point);
                point = new Point3D(-x, floorDepth, -z);// Floor Index - 17
                points.Add(point);
                point = new Point3D(-x, floorDepth, z);// Floor Index - 18
                points.Add(point);
                point = new Point3D(-x, 0, z);// Floor Index - 19
                points.Add(point);
                return points;
            }
        }

        public Int32Collection FloorPointsIndices
        {
            get
            {
                int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13,
       14, 15, 16, 17, 19, 17, 18, 19 };

                return new Int32Collection(indices);
            }
        }

        public Int32Collection FloorTriangleIndices
        {
            get { return floorTriangleIndices; }
            set
            {
                if (floorTriangleIndices != value)
                {
                    floorTriangleIndices = value;
                    NotifyPropertyChanged();
                }
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

        private Vector3DCollection FloorNormals
        {
            get { return floorNormals; }
            set
            {
                if (floorNormals != value)
                {
                    floorNormals = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void OnRefreshAdorners(object param)
        {
            allBounds = new Bounds3D();
            allBounds.Zero();
            modelItems.Clear();
            modelItems.Add(floor);
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

        public void CountFaces()
        {
            totalFaces = 0;
            foreach (Object3D ob in Document.Content)
            {
                totalFaces += ob.TotalFaces;
            }
        }

        public void RegenerateDisplayList()
        {
            allBounds = new Bounds3D();
            allBounds.Zero();
            modelItems.Clear();
            modelItems.Add(floor);
            totalFaces = 0;
            foreach (Object3D ob in Document.Content)
            {
                totalFaces += ob.TotalFaces;

                GeometryModel3D gm = GetMesh(ob);
                modelItems.Add(gm);
                allBounds += ob.AbsoluteBounds;
            }

            NotifyPropertyChanged("ModelItems");
        }

        internal void MouseDown(System.Windows.Point lastMousePos, MouseButtonEventArgs e)
        {
            lastMouse = lastMousePos;
        }

        internal void MouseMove(System.Windows.Point newPos, MouseEventArgs e)
        {
            if (selectedObjectAdorner != null && selectedObjectAdorner.MouseMove(lastMouse, newPos, e) == true)
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

        private void LookToObject()
        {
            Vector3D v = new Vector3D(CameraLookObject.X - camera.CameraPos.X,
                  CameraLookObject.Y - camera.CameraPos.Y,
                  CameraLookObject.Z - camera.CameraPos.Z);
            v.Normalize();
            LookDirection = v;
            NotifyPropertyChanged("LookDirection");
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

        internal void Select(GeometryModel3D geo, bool append = false)
        {
            bool handled = false;
            /*
            // if user has just clicked on floor then delect everything
            if (geo.Geometry == Floor.Geometry)
            {
                if (selectedObjectAdorner != null)
                {
                    // remove the currnt visible elements of the adorner
                    RemoveObjectAdorner();
                    DeselectAll();
                }
                handled = true;
            }
            else
            */
            {
                if (selectedObjectAdorner != null)
                {
                    handled = selectedObjectAdorner.Select(geo);
                }
                if (!handled)
                {
                    CheckIfContentSelected(geo, append);
                }
            }
        }

        private void CheckIfContentSelected(GeometryModel3D geo, bool append)
        {
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
                        break;
                    }
                }
                if (selectedItems.Count > 0)
                {
                    CameraLookObject = selectedItems[0].Position;
                    RemoveObjectAdorner();
                    GenerateSelectionBox(selectedItems[0]);
                    NotificationManager.Notify("ObjectSelected", selectedItems[0]);
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
        }

        private void RestoreUnselectedColours()
        {
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in modelItems)
                {
                    if (md != floor)
                    {
                        if (ob.Mesh == md.Geometry)
                        {
                            md.Material = new DiffuseMaterial(new SolidColorBrush(ob.Color));
                        }
                    }
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

        private void GenerateSelectionBox(Object3D object3D)
        {
            selectedObjectAdorner = new SizeAdorner(camera);
            selectedObjectAdorner.AdornObject(object3D);

            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }
            NotifyPropertyChanged("ModelItems");
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

        private GeometryModel3D GetFloor()
        {
            GeometryModel3D gm = new GeometryModel3D();
            MeshGeometry3D fl = new MeshGeometry3D();
            fl.Positions = FloorPoints3D;
            fl.TriangleIndices = FloorPointsIndices;
            gm.Geometry = fl;

            DiffuseMaterial mt = new DiffuseMaterial();
            mt.Color = Colors.LightGray;
            mt.Brush = new SolidColorBrush(Color.FromArgb(60, 200, 200, 200));
            gm.Material = mt;

            DiffuseMaterial mtb = new DiffuseMaterial();
            mtb.Color = Colors.Gray;
            mtb.Brush = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)); ;
            gm.BackMaterial = mtb;
            return gm;
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

        private void BackCamera()
        {
            camera.HomeBack();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(-1, 1, 0);
            LookToCenter();
            zoomPercent = 100;
        }

        private void RightCamera()
        {
            camera.HomeRight();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(0, 1, -1);
            LookToCenter();
            zoomPercent = 100;
        }

        private void TopCamera()
        {
            camera.HomeTop();
            NotifyPropertyChanged("CameraPos");
            CameraScrollDelta = new Point3D(1, 0, 1);
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

        private void LookToCenter()
        {
            lookDirection.X = -camera.CameraPos.X;
            lookDirection.Y = -camera.CameraPos.Y;
            lookDirection.Z = -camera.CameraPos.Z;
            lookDirection.Normalize();
            NotifyPropertyChanged("LookDirection");
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

        // some of the primitives need to be rotated when they are first created so they match the
        // orientaion shown on the icons
        private static string[] rotatedPrimitives =
        {
            "roof","cone","pyramid","roundroof","cap","polygon","rightangle","pointy"
        };

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
                modelItems.Add(gm);

                CountFaces();
                NotifyPropertyChanged("ModelItems");
            }
        }

        private bool Loft(Object3D obj, string obType)
        {
            bool res = false;
            if (obType == "vaseloft")
            {
                LinearLoftDialog dlg = new LinearLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    obj.RelativeObjectVertices = dlg.GetVertices();
                    obj.TriangleIndices = dlg.GetFaces();
                    obj.CalcScale(false);
                    res = true;
                }
            }
            else if (obType == "shapeloft")
            {
                ShapeLoftDialog dlg = new ShapeLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    obj.RelativeObjectVertices = dlg.GetVertices();
                    obj.TriangleIndices = dlg.GetFaces();
                    obj.CalcScale(false);
                    res = true;
                }
            }
            else if (obType == "fuselageloft")
            {
                FuselageLoftDialog dlg = new FuselageLoftDialog();
                if (dlg.ShowDialog() == true)
                {
                    obj.RelativeObjectVertices = dlg.GetVertices();
                    obj.TriangleIndices = dlg.GetFaces();
                    obj.CalcScale(false);
                    res = true;
                }
            }

            return res;
        }

        private void OnCameraCommand(object param)
        {
            string p = param.ToString();
            switch (p)
            {
                case "CameraHome":
                    {
                        HomeCamera();
                    }
                    break;

                case "CameraBack":
                    {
                        BackCamera();
                    }
                    break;

                case "CameraLeft":
                    {
                        LeftCamera();
                    }
                    break;

                case "CameraRight":
                    {
                        RightCamera();
                    }
                    break;

                case "CameraTop":
                    {
                        TopCamera();
                    }
                    break;

                case "CameraBottom":
                    {
                        BottomCamera();
                    }
                    break;

                case "CameraLookCenter":
                    {
                        LookToCenter();
                        ReportCameraPosition();
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
                    }
                    break;

                case "CameraMoveLookObject":
                    {
                        cameraMode = CameraModes.CameraMoveLookObject;
                    }
                    break;

                default:
                    break;
            }
            ReportCameraPosition();
        }

        private void OnNewDocument(object param)
        {
            RegenerateDisplayList();
            HomeCamera();
            selectedItems = new List<Object3D>();
        }

        private void OnRefresh(object param)
        {
            RegenerateDisplayList();
        }

        private void ReportCameraPosition()
        {
            String s = $"Camera ({camera.CameraPos.X:F2},{camera.CameraPos.Y:F2},{camera.CameraPos.Z:F2}) => ({lookDirection.X:F2},{lookDirection.Y:F2},{lookDirection.Z:F2}) Zoom {zoomPercent:F1}%";
            NotificationManager.Notify("SetStatusText1", s);
            s = $"Faces {totalFaces}";
            NotificationManager.Notify("SetStatusText3", s);
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

        private void SelectLast()
        {
            ResetSelection();
            if (Document.Content.Count > 0)
            {
                Object3D ob = Document.Content[Document.Content.Count - 1];
                selectedItems.Add(ob);
                selectedObjectAdorner.AdornObject(ob);
                NotificationManager.Notify("ObjectSelected", ob);
            }
            UpdateSelectionDisplay();
        }

        private void SelectPrevious()
        {
            if (selectedItems.Count == 1)
            {
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
                UpdateSelectionDisplay();
            }
        }

        internal void DeselectAll()
        {
            RemoveObjectAdorner();
            ResetSelectionColours();
            selectedObjectAdorner = new SizeAdorner(camera);
            selectedItems.Clear();
            NotificationManager.Notify("ObjectSelected", null);
        }

        private void ResetSelectionColours()
        {
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in modelItems)
                {
                    if (ob.Mesh == md.Geometry)
                    {
                        md.Material = new DiffuseMaterial(new SolidColorBrush(ob.Color));
                    }
                }
            }
        }

        private void SetSelectionColours()
        {
            foreach (Object3D ob in selectedItems)
            {
                foreach (GeometryModel3D md in modelItems)
                {
                    if (ob.Mesh == md.Geometry)
                    {
                        md.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                    }
                }
            }
        }

        private void SelectNext()
        {
            if (selectedItems.Count == 1)
            {
                ResetSelectionColours();
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
                UpdateSelectionDisplay();
            }
        }

        private void SelectFirst()
        {
            ResetSelection();
            if (Document.Content.Count > 0)
            {
                Object3D ob = Document.Content[0];
                selectedItems.Add(ob);
                selectedObjectAdorner.AdornObject(ob);
                NotificationManager.Notify("ObjectSelected", ob);
            }
            UpdateSelectionDisplay();
        }

        private void UpdateSelectionDisplay()
        {
            SetSelectionColours();
            // update the display
            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }
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

        private void SelectAll()
        {
            ResetSelection();
            foreach (Object3D ob in Document.Content)
            {
                selectedItems.Add(ob);
                // append the the object to the existing list of
                selectedObjectAdorner.AdornObject(ob);
            }
            UpdateSelectionDisplay();
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
    }
}