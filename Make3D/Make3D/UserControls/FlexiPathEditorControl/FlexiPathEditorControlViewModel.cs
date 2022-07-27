using Barnacle.LineLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Barnacle.UserControls.FlexiPathEditorControl
{
    internal class FlexiPathEditorControlViewModel : INotifyPropertyChanged
    {
        private bool canCNVDouble;

        private FlexiPath flexiPath;
        private List<Shape> gridMarkers;
        private double gridX = 0;
        private double gridY = 0;

        private bool lineShape;
        private BitmapImage localImage;
        private bool moving;
        private string pathText;
        private ObservableCollection<FlexiPoint> polyPoints; // SHOULD BE CALLED FLEXIPAHCONTROLPOINTS
        private double scale;
        private int selectedPoint;
        private SelectionModeType selectionMode;
        private bool showGrid;
        private bool showOrtho;
        private Visibility showWidth;
        private bool snap;

        public FlexiPathEditorControlViewModel()
        {
            AddCubicBezierCommand = new RelayCommand(OnAddCubic);
            AddQuadBezierCommand = new RelayCommand(OnAddQuad);
            AddSegmentCommand = new RelayCommand(OnAddSegment);
            CNVDoubleSegCommand = new RelayCommand(OnCnvDoubleSegment);
            DeleteSegmentCommand = new RelayCommand(OnDeleteSegment);
            MovePathCommand = new RelayCommand(OnMovePath);
            PickCommand = new RelayCommand(OnPickSegment);
            ResetPathCommand = new RelayCommand(OnResetPath);
            ShowAllPointsCommand = new RelayCommand(OnShowAllPoints);
            CopyPathCommand = new RelayCommand(OnCopy);
            PastePathCommand = new RelayCommand(OnPaste);

            flexiPath = new FlexiPath();
            polyPoints = flexiPath.FlexiPoints;
            selectedPoint = -1;
            selectionMode = SelectionModeType.SelectSegmentAtPoint;
            scale = 1.0;
            lineShape = false;
            showOrtho = true;
            showWidth = Visibility.Hidden;
            showGrid = true;
            snap = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public enum SelectionModeType
        {
            StartPoint,
            AppendPoint,
            SelectSegmentAtPoint,
            SplitLine,
            ConvertToCubicBezier,
            DeleteSegment,
            ConvertToQuadBezier,
            MovePath
        };

        public ICommand AddCubicBezierCommand { get; set; }

        public ICommand AddQuadBezierCommand { get; set; }

        public ICommand AddSegmentCommand { get; set; }

        public bool CanCNVDouble
        {
            get { return canCNVDouble; }
            set
            {
                if (canCNVDouble != value)
                {
                    canCNVDouble = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CNVDoubleVisible");
                }
            }
        }

        public ICommand CNVDoubleSegCommand { get; set; }

        public Visibility CNVDoubleVisible
        {
            get
            {
                if (canCNVDouble)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
        }

        public ICommand CopyPathCommand { get; set; }

        public ICommand DeleteSegmentCommand { get; set; }

        public ICommand MovePathCommand { get; set; }

        public ICommand PastePathCommand { get; set; }

        public string PathText
        {
            get
            {
                return pathText;
            }
            set
            {
                if (pathText != value)
                {
                    pathText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand PickCommand { get; set; }

        public ObservableCollection<FlexiPoint> Points
        {
            get
            {
                return polyPoints;
            }
            set
            {
                if (value != polyPoints)
                {
                    polyPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ResetPathCommand { get; set; }

        public int SelectedPoint
        {
            get
            {
                return selectedPoint;
            }
            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    NotifyPropertyChanged();
                    ClearPointSelections();
                    if (polyPoints != null)
                    {
                        if (selectedPoint >= 0 && selectedPoint < polyPoints.Count)
                        {
                            polyPoints[selectedPoint].Selected = true;
                            polyPoints[selectedPoint].Visible = true;
                        }
                    }

                }
            }
        }

        public SelectionModeType SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                if (value != selectionMode)
                {
                    selectionMode = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ShowAllPointsCommand { get; set; }

        
        public bool ShowOrtho
        {
            get
            {
                return showOrtho;
            }
            set
            {
                if (value != showOrtho)
                {
                    showOrtho = value;

                    NotifyPropertyChanged();
                   
                }
            }
        }

        public bool ShowPolyGrid
        {
            get { return showGrid; }
            set
            {
                if (value != showGrid)
                {
                    showGrid = value;
                    NotifyPropertyChanged();
                    
                }
            }
        }

        public bool Snap
        {
            get { return snap; }
            set
            {
                if (snap != value)
                {
                    snap = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ClearPointSelections()
        {
            if (polyPoints != null)
            {
                for (int i = 0; i < polyPoints.Count; i++)
                {
                    polyPoints[i].Selected = false;
                    polyPoints[i].Visible = false;
                }
            }
        }

        private void OnAddCubic(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToCubicBezier;
        }

        private void OnAddQuad(object obj)
        {
            SelectionMode = SelectionModeType.ConvertToQuadBezier;
        }

        private void OnAddSegment(object obj)
        {
            SelectionMode = SelectionModeType.SplitLine;
        }

        private void OnCnvDoubleSegment(object obj)
        {
        }

        private void OnCopy(object obj)
        {
            PathText = flexiPath.ToPath();
            Clipboard.SetText(PathText);
        }

        private void OnDeleteSegment(object obj)
        {
            SelectionMode = SelectionModeType.DeleteSegment;
        }

        private void OnMovePath(object obj)
        {
        }

        private void OnPaste(object obj)
        {
            string pth = Clipboard.GetText();
            if (pth != null && pth != "" && pth.StartsWith("M"))
            {
                PathText = pth;
                flexiPath.FromTextPath(pth);

                SelectionMode = SelectionModeType.SelectSegmentAtPoint;
            }
        }
        private void OnPickSegment(object obj)
        {
            selectionMode = SelectionModeType.SelectSegmentAtPoint;
        }

        private void OnResetPath(object obj)
        {
        }

        private void OnShowAllPoints(object obj)
        {
            foreach (FlexiPoint p in polyPoints)
            {
                p.Visible = true;
            }
        }
    }
}