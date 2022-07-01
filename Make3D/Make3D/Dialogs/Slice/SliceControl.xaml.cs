using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
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
using VisualSolutionExplorer;
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

        private List<String> profiles;

        public List<String> Profiles
        {
            get { return profiles; }
            set
            {
                profiles = value;
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
            String exportPath = VisualSolutionExplorer.Project.BaseFolder + "\\export";
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            string printerPath = VisualSolutionExplorer.Project.BaseFolder + "\\printer";
            if (!Directory.Exists(printerPath))
            {
                Directory.CreateDirectory(printerPath);
            }
            if (ModelMode == "SliceModel")
            {
                string fullPath = modelPath;
                SliceSingleModel(fullPath,exportPath, printerPath);
            }
            else
            {
                string[] filenames = BaseViewModel.Project.GetExportFiles(".txt");
                String pth = VisualSolutionExplorer.Project.BaseFolder;
                foreach (string fullPath in filenames)
                {
                    SliceSingleModel(fullPath, exportPath, printerPath);
                }

            }
            NotificationManager.Notify("ExportRefresh", null);
        }

        private void SliceSingleModel(string fullPath, string exportPath, string printerPath)
        {
            CanClose = false;
            exportDoc = new Document();
            exportDoc.Load(fullPath);

            string modelName = Path.GetFileNameWithoutExtension(fullPath);
            fullPath = Path.GetDirectoryName(fullPath);
            Bounds3D allBounds = RecalculateAllBounds();

            string exportedPath = exportDoc.ExportAll("STLSLICE", allBounds, exportPath);
            exportedPath = Path.Combine(exportPath, modelName + ".stl");


            string gcodePath = Path.Combine(printerPath, modelName + ".gcode");

            string logPath = Path.Combine(modelPath, "slicelog.log");
            string prf = selectedUserProfile;

            CuraEngineInterface.Slice(exportedPath, gcodePath, logPath, "Print3D", SlicerPath, selectedPrinter + ".json", selectedExtruder + ".def.json", prf);
            NotificationManager.Notify("ExportRefresh", null);
            CanClose = true;
        }

        private bool canSlice;

        public bool CanSlice
        {
            get { return canSlice; }
            set
            {
                canSlice = value;
                NotifyPropertyChanged();
            }
        }
        private String selectedPrinter;

        public String SelectedPrinter
        {
            get { return selectedPrinter; }
            set
            {
                selectedPrinter = value;
                if (selectedPrinter != "" && selectedExtruder != "")
                {
                    CanSlice = true;
                }
                else
                {
                    CanSlice = false;
                }
                NotifyPropertyChanged();
            }
        }

        private String selectedUserProfile;
        public String SelectedUserProfile
        {
            get { return selectedUserProfile; }
            set
            {
                if (selectedUserProfile != value)
                {
                    selectedUserProfile = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private String selectedExtruder;

        public String SelectedExtruder
        {
            get { return selectedExtruder; }
            set
            {
                selectedExtruder = value;
                if (selectedPrinter != "" && selectedExtruder != "")
                {
                    CanSlice = true;
                }
                else
                {
                    CanSlice = false;
                }
                NotifyPropertyChanged();
            }
        }


        private bool canClose;

        public bool CanClose
        {
            get { return canClose; }
            set
            {
                canClose = value;
                NotifyPropertyChanged();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.SlicerPrinter = SelectedPrinter;
            Properties.Settings.Default.SlicerExtruder = SelectedExtruder;
            Properties.Settings.Default.SlicerProfileName = SelectedUserProfile;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            if (SlicerPath != null && SlicerPath != "")
            {
                Printers = CuraEngineInterface.GetAvailablePrinters(SlicerPath + @"\Resources\definitions");
                Extruders = CuraEngineInterface.GetAvailableExtruders(SlicerPath + @"\Resources\extruders");
                Profiles = CuraEngineInterface.GetAvailableUserProfiles();
                SelectedPrinter = Properties.Settings.Default.SlicerPrinter;
                SelectedExtruder = Properties.Settings.Default.SlicerExtruder;
                SelectedUserProfile = Properties.Settings.Default.SlicerProfileName;
            }

            CanClose = true;
            if (selectedPrinter != "" && selectedExtruder != "")
            {
                CanSlice = true;
            }
            else
            {
                CanSlice = false;
            }
        }
    }
}
