using Barnacle.Models;
using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Workflow;
using Path = System.IO.Path;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for SliceControl.xaml
    /// </summary>
    public partial class SliceControl : Window, INotifyPropertyChanged
    {

        string modelPath;
        internal string ModelPath
        {
            get { return modelPath; }
            set
            {
                modelPath = value;
            }

        }
        private Document exportDoc;

        public Document ExportDocument
        {
            get { return exportDoc; }
            set { exportDoc = value; }
        }

        public String SlicerPath { get; set; }
        public SliceControl()
        {
            InitializeComponent();
        }
        private List<String> printers;

        public List<String> Printers
        {
            get { return printers; }
            set
            {
                printers = value;
                NotifyPropertyChanged();
            }
        }


        private List<String> extruders;

        public List<String> Extruders
        {
            get { return extruders; }
            set
            {
                extruders = value;
                NotifyPropertyChanged();
            }
        }


        public string ModelMode { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Bounds3D RecalculateAllBounds()
        {
            Bounds3D allBounds = new Bounds3D();
            if (exportDoc.Content.Count == 0)
            {
                allBounds.Zero();
            }
            else
            {
                foreach (Object3D ob in exportDoc.Content)
                {
                    allBounds += ob.AbsoluteBounds;
                }
            }
            return allBounds;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ModelMode == "SliceModel")
            {
                CanClose = false;

                string modelName = Path.GetFileNameWithoutExtension(modelPath);
                modelPath = Path.GetDirectoryName(modelPath);
                String exportPath = VisualSolutionExplorer.Project.BaseFolder + "\\export";
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                Bounds3D allBounds = RecalculateAllBounds();

                string exportedPath = exportDoc.ExportAll("STLSLICE", allBounds, exportPath);
                exportedPath = Path.Combine(exportPath, modelName + ".stl");

                string gcodePath = Path.Combine(modelPath, "Printer");
                if (!Directory.Exists(gcodePath))
                {
                    Directory.CreateDirectory(gcodePath);
                }
                gcodePath = Path.Combine(gcodePath, modelName + ".gcode");

                string logPath = Path.Combine(modelPath, "slicelog.log");


                CuraEngineInterface.Slice(exportedPath, gcodePath, logPath, "Print3D", SlicerPath);
                CanClose = true;
            }
        }
        private bool canSlice;

        public bool CanSlice
        {
            get { return canSlice; }
            set { canSlice = value; }
        }
        private String selectedPrinter;

        public String SelectedPrinter
        {
            get { return selectedPrinter; }
            set { selectedPrinter = value; 
            if ( selectedPrinter != "" && selectedExtruder != "")
            {
                    CanSlice = true;
            }
            else
            {
                    CanSlice = false;
                }
            NotifyPropertyChanged(); }
        }

        private String selectedExtruder;

        public String SelectedExtruder
        {
            get { return selectedExtruder; }
            set { selectedExtruder = value;
                if (selectedPrinter != "" && selectedExtruder != "")
                {
                    CanSlice = true;
                }
                else
                {
                    CanSlice = false;
                }
                NotifyPropertyChanged(); }
        }


        private bool canClose;

        public bool CanClose
        {
            get { return canClose; }
            set { canClose = value; }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.SlicerPrinter= SelectedPrinter;
            Properties.Settings.Default.SlicerExtruder = SelectedExtruder  ;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            Printers = CuraEngineInterface.GetAvailablePrinters(SlicerPath +@"\Resources\definitions");
            Extruders = CuraEngineInterface.GetAvailableExtruders(SlicerPath +@"\Resources\extruders");
            SelectedPrinter = Properties.Settings.Default.SlicerPrinter;
            SelectedExtruder = Properties.Settings.Default.SlicerExtruder;
            CanClose = true;
        }
    }
}
