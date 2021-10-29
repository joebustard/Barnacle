using Barnacle.Models;
using Barnacle.Object3DLib;
using ScriptLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Media3D;

namespace Barnacle.ViewModels
{
    internal class ScriptViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private const String defaultSource = @"
program ""Script Name""
{
 Include ""libs\limplib.txt"" ;

 // main starts here
}
";

        private static Action EmptyDelegate = delegate () { };
        private Axies axies;
        private PolarCamera camera;
        private CameraModes cameraMode;
        private Point3D CameraScrollDelta = new Point3D(1, 1, 0);
        private List<Object3D> content;
        private string filePath;
        private Floor floor;
        private FloorMarker floorMarker;
        private FlowDocument flowDocument;
        private Grid3D grid;
        private Interpreter interpreter;
        private Point lastMouse;
        private Vector3D lookDirection;
        private Model3DCollection modelItems;
        private TextBox resultsBox;
        private string resultsText;
        private RichTextBox richTextBox;
        private string rtf;
        private Script script;
        private int searchIndex;
        private string searchText;
        private bool showAxies;
        private bool showFloor;
        private int totalFaces;
        private bool updatedInternally;
        private double zoomPercent = 100;

        public ScriptViewModel()
        {
            filePath = "";
            Dirty = false;
            Source = defaultSource;
            Rtf = Source;
            floor = new Floor();
            showFloor = true;
            axies = new Axies();
            showAxies = true;
            content = new List<Object3D>();
            floorMarker = null;
            grid = new Grid3D();
            interpreter = new Interpreter();
            script = new Script();
            interpreter.LoadFromText(script, defaultSource, filePath);
            Rtf = script.ToRichText();
            ScriptCommand = new RelayCommand(OnScriptCommand);
            FindCommand = new RelayCommand(OnFind);
            camera = new PolarCamera();
            lookDirection.X = -camera.CameraPos.X;
            lookDirection.Y = -camera.CameraPos.Y;
            lookDirection.Z = -camera.CameraPos.Z;
            lookDirection.Normalize();
            cameraMode = CameraModes.CameraMoveLookCenter;
            modelItems = new Model3DCollection();
            searchIndex = 0;
            Log.Instance().UpdateText = UpdateText;
            NotificationManager.Subscribe("Script", "LimpetLoaded", OnLimpetLoaded);
            NotificationManager.Subscribe("Script", "LimpetClosing", OnLimpetClosing);
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
            set { NotifyPropertyChanged(); }
        }

        public bool Dirty { get; set; }

        public ICommand FindCommand { get; set; }

        public FlowDocument FlowDoc
        {
            get
            {
                return flowDocument;
            }

            set
            {
                if (flowDocument != value)
                {
                    flowDocument = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public Canvas Overlay { get; internal set; }

        public string ResultsText
        {
            get
            {
                return resultsText;
            }
            set
            {
                if (resultsText != value)
                {
                    resultsText = value;
                    if (resultsBox != null)
                    {
                        int iStartOfLine = resultsBox.Text.Length;
                        Refresh(resultsBox);
                        resultsBox.Select(iStartOfLine, 1);
                        resultsBox.ScrollToEnd();
                        Refresh(resultsBox);
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public String Rtf
        {
            get
            {
                return rtf;
            }
            set
            {
                if (rtf != value)
                {
                    rtf = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RichTextBox ScriptBox
        {
            get
            {
                return richTextBox;
            }
            set
            {
                if (value != richTextBox)
                {
                    richTextBox = value;
                }
            }
        }

        public ICommand ScriptCommand
        {
            get; set;
        }

        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Source
        {
            get;
            set;
        }

        public bool UpdatedInternally
        {
            get
            {
                return updatedInternally;
            }
            set
            {
                if (updatedInternally != value)
                {
                    updatedInternally = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal String RawText { get; set; }

        public static void Refresh(System.Windows.UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }

        public void ClearResults()
        {
            ResultsText = "";
        }

        public void RegenerateDisplayList()
        {
            modelItems.Clear();
            if (showFloor)
            {
                modelItems.Add(floor.FloorMesh);
                foreach (GeometryModel3D m in grid.Group.Children)
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
            foreach (Object3D ob in content)
            {
                if (ob != null)
                {
                    totalFaces += ob.TotalFaces;

                    GeometryModel3D gm = GetMesh(ob);
                    modelItems.Add(gm);
                }
            }

            NotifyPropertyChanged("ModelItems");
        }

        internal void KeyDown(Key key, bool v1, bool v2)
        {
        }

        internal void KeyUp(Key key, bool v1, bool v2)
        {
        }

        internal void MouseDown(System.Windows.Point lastMousePos, MouseButtonEventArgs e)
        {
            lastMouse = lastMousePos;
        }

        internal void MouseMove(System.Windows.Point newPos, MouseEventArgs e)
        {
            bool ctrlDown = false;

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
                /*
                else if (cameraMode == CameraModes.CameraMoveLookObject)
                {
                    MoveCameraDelta(lastMouse, newPos);
                    LookToObject();
                    lastMouse = newPos;
                }
                */
            }
        }

        internal void MouseUp(System.Windows.Point lastMousePos, MouseButtonEventArgs e)
        {
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

        internal void RunScript()
        {
            if (filePath != "" && Project.SharedProjectSettings.AutoSaveScript == true)
            {
                try
                {
                    File.WriteAllText(filePath, Source);
                    Dirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            content.Clear();
            script.SetResultsContent(content);
            if (script.Execute())
            {
                Log.Instance().AddEntry("Complete");
            }
            else
            {
                Log.Instance().AddEntry("Failed");
            }
            RegenerateDisplayList();
        }

        internal bool ScriptText(string scriptText)
        {
            Source = scriptText;

            bool result = interpreter.LoadFromText(script, Source, filePath);
            if (result)
            {
                Rtf = script.ToRichText();
                RawText = scriptText;
            }
            UpdatedInternally = false;
            return result;
        }

        internal void SetResultsBox(System.Windows.Controls.TextBox resultsBox)
        {
            this.resultsBox = resultsBox;
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

        private TextPointer GetTextPositionAtOffset(TextPointer position, int characterCount)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    int count = position.GetTextRunLength(LogicalDirection.Forward);
                    if (characterCount <= count)
                    {
                        return position.GetPositionAtOffset(characterCount);
                    }

                    characterCount -= count;
                }

                TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                    return position;

                position = nextContextPosition;
            }

            return position;
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
            NotifyPropertyChanged("CameraPos");
        }

        private void OnFind(object obj)
        {
            if (searchText != null && searchText != "" && richTextBox != null)
            {
                searchIndex = richTextBox.Find(searchText, searchIndex + 1);
                if (searchIndex < 0)
                {
                    searchIndex = 0;
                }
            }
        }

        private void OnLimpetClosing(object param)
        {
            if (Dirty == true && filePath != "")
            {
                MessageBoxResult res = MessageBox.Show("Script changed." + System.Environment.NewLine + "Save changes before closing?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.WriteAllText(filePath, Source);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            BaseViewModel.ScriptResults = null;
            if (content.Count > 0)
            {
                MessageBoxResult res = MessageBox.Show("Objects have been created. Do you want to save them in the model?", "Warning", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    BaseViewModel.ScriptResults = content;
                }
            }
        }

        private void OnLimpetLoaded(object param)
        {
            string fileName = param.ToString();
            if (File.Exists(fileName))
            {
                Source = File.ReadAllText(fileName);
                interpreter = new Interpreter();
                script = new Script();
                if (interpreter.LoadFromText(script, Source, fileName))
                {
                    Rtf = script.ToRichText();
                }
                else
                {
                    Rtf = script.ToErrorRichText(Source);
                }
                Dirty = false;
                filePath = fileName;
                NotificationManager.Notify("UpdateScript", null);
                content.Clear();
            }
        }

        private void OnScriptCommand(object obj)
        {
            String com = obj.ToString();
            com = com.ToLower();
            switch (com)
            {
                case "new":
                    {
                        Source = defaultSource;
                        interpreter.LoadFromText(script, Source, filePath);
                        Rtf = script.ToRichText();
                        NotificationManager.Notify("UpdateScript", null);
                        Dirty = true;
                    }
                    break;

                case "clear":
                    {
                        Log.Instance().Clear();
                        content.Clear();
                    }
                    break;

                case "save":
                    {
                        if (filePath != "")
                        {
                            try
                            {
                                File.WriteAllText(filePath, Source);
                                Dirty = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    break;

                case "load":
                    {
                        /*
                        OpenFileDialog dlg = new OpenFileDialog();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                Source = File.ReadAllText(dlg.FileName);
                                interpreter = new Interpreter();
                                script = new Script();
                                if (interpreter.LoadFromText(script, Source))
                                {
                                    Rtf = script.ToRichText();
                                }
                                else
                                {
                                    Rtf = Source;
                                }
                                NotificationManager.Notify("Update", null);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        */
                    }
                    break;
            }
        }

        private void UpdateText(string s)
        {
            ResultsText += s;
            CollectionViewSource.GetDefaultView(ResultsText).Refresh();
        }

        private void Zoom(double v)
        {
            camera.Zoom(v);
            zoomPercent += v;

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