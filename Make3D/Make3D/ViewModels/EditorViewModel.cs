using Make3D.Adorners;
using Make3D.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private Model3DCollection modelItems;

        private List<Object3D> selectedItems;

        private Bounds3D allBounds;

        private ObjectAdorner selectedObjectAdorner;

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
            NotificationManager.Subscribe("NewDocument", OnNewDocument);
            NotificationManager.Subscribe("Remesh", OnRemesh);
            NotificationManager.Subscribe("Select", Select);
            NotificationManager.Subscribe("Group", OnGroup);
            NotificationManager.Subscribe("Cut", OnCut);
            NotificationManager.Subscribe("Copy", OnCopy);
            NotificationManager.Subscribe("Paste", OnPaste);
            NotificationManager.Subscribe("Export", OnExport);
            NotificationManager.Subscribe("MoveObjectToFloor", OnMoveObjectToFloor);
            NotificationManager.Subscribe("MoveObjectToCentre", OnMoveObjectToCentre);
            ReportCameraPosition();
            selectedItems = new List<Object3D>();
            allBounds = new Bounds3D();
            allBounds.Adjust(new Point3D(0, 0, 0));
        }

        private void OnPaste(object param)
        {
            if (ObjectClipboard.HasItems())
            {
                selectedObjectAdorner.Clear();
                foreach (Object3D cl in ObjectClipboard.Items)
                {
                    Object3D o = cl.Clone();
                    o.Name = Document.NextName;

                    
                    if (o is Group3D)
                    {
                        (o as Group3D).Init();
                    }
                    o.Position = new Point3D(allBounds.Upper.X + o.AbsoluteBounds.Width/2 , 0, 0);
                    o.Remesh();
                    o.MoveToFloor();
                    allBounds += o.AbsoluteBounds;
                    GeometryModel3D gm = GetMesh(o);
                    Document.Content.Add(o);
                    Document.Dirty = true;
                    modelItems.Add(gm);
                }

                RegenerateDisplayList();
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
            prm.MoveToFloor();
            selectedObjectAdorner.Clear();
            RegenerateDisplayList();
        }

        private void OnMoveObjectToCentre(object param)
        {
            Object3D prm = param as Object3D;
            prm.MoveToCentre();
            selectedObjectAdorner.Clear();
            RegenerateDisplayList();
        }
        private void OnExport(object param)
        {
            Document.Export(param.ToString());
        }

        private void OnCut(object param)
        {
            if (selectedObjectAdorner != null)
            {
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
            if (s == "ungroup")
            {
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

        private bool BreakGroup()
        {
            bool res = false;
            if (selectedObjectAdorner != null)
            {
                if (selectedObjectAdorner.NumberOfSelectedObjects() == 1 && selectedObjectAdorner.SelectedObjects[0] is Group3D)
                {
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
                Object3D leftie = selectedObjectAdorner.SelectedObjects[0];
                int i = 1;
                while (i < selectedObjectAdorner.NumberOfSelectedObjects())
                {
                    Group3D grp = new Group3D();
                    grp.Name = Document.NextName;
                    grp.LeftObject = leftie;
                    grp.RightObject = selectedObjectAdorner.SelectedObjects[i];
                    grp.PrimType = s;
                    grp.Init();
                    Document.ReplaceObjectsByGroup(grp);
                    leftie = grp;
                    i++;
                    res = true;
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
                double x = 100.0; // floor width / 2
                double z = 100.0; // floor length / 2
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

        public void RegenerateDisplayList()
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

            NotifyPropertyChanged("ModelItems");
        }

        internal void DeselectAll()
        {
            RemoveObjectAdorner();
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
            selectedItems.Clear();
            NotificationManager.Notify("ObjectSelected", null);
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
            selectedObjectAdorner = new ObjectAdorner( camera);
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
            mtb.Color = Colors.CornflowerBlue;
            mtb.Brush = new SolidColorBrush(Colors.CornflowerBlue);
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
            "roof","cone","pyramid","roundroof","cap","polygon"
        };
        private void OnAddObject(object param)
        {
            string pth = AppDomain.CurrentDomain.BaseDirectory;
            string obType = param.ToString();
            obType = obType.ToLower();
            Color color = Colors.Beige;
            Object3D obj = new Object3D();

            DeselectAll();
            obj.Name = Document.NextName;
            obj.Description = obType;

            obj.Scale = new Scale3D(20, 20, 20);
            obj.Position = new Point3D(allBounds.Upper.X + obj.Scale.X / 2, obj.Scale.Y / 2, 0);
            obj.BuildPrimitive(obType);
            for (int i = 0; i < rotatedPrimitives.GetLength(0); i++)
            {
                if (rotatedPrimitives[i] == obType)
                {
                    obj.Rotation = new Point3D(-90.0, 0, 0);
                    break;
                }
            }
            allBounds += obj.AbsoluteBounds;

            GeometryModel3D gm = GetMesh(obj);

            Document.Content.Add(obj);
            Document.Dirty = true;
            modelItems.Add(gm);
            NotifyPropertyChanged("ModelItems");
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
        }

        private void SelectAll()
        {
            NotificationManager.Notify("ObjectSelected", null);
            if (selectedObjectAdorner != null)
            {
                // remove the currnt visible elements of the adorner
                RemoveObjectAdorner();
            }

            selectedObjectAdorner = new ObjectAdorner( camera);
            selectedItems.Clear();
            foreach (Object3D ob in Document.Content)
            {
                selectedItems.Add(ob);
                // append the the object to the existing list of
                selectedObjectAdorner.AdornObject(ob);

            }
            foreach (GeometryModel3D gm in modelItems)
            {
                if (gm != floor)
                {
                    gm.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                }
            }
            // update the display
            foreach (Model3D md in selectedObjectAdorner.Adornments)
            {
                modelItems.Add(md);
            }


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