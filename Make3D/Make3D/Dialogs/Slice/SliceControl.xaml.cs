using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Workflow;
using Path = System.IO.Path;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for SliceControl.xaml
    /// </summary>
    public partial class SliceControl : Window, INotifyPropertyChanged
    {
        private bool canClose;
        private bool canSlice;
        private Document exportDoc;
        private List<String> extruders;
        private string modelPath;

        private List<String> printers;

        private List<String> profiles;

        private string resultsText;

        private String selectedExtruder;

        private String selectedPrinter;

        private String selectedUserProfile;

        public SliceControl()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanClose
        {
            get { return canClose; }
            set
            {
                canClose = value;
                NotifyPropertyChanged();
            }
        }

        public bool CanSlice
        {
            get { return canSlice; }
            set
            {
                canSlice = value;
                NotifyPropertyChanged();
            }
        }

        public Document ExportDocument
        {
            get { return exportDoc; }
            set { exportDoc = value; }
        }

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

        public List<String> Printers
        {
            get { return printers; }
            set
            {
                printers = value;
                NotifyPropertyChanged();
            }
        }

        public List<String> Profiles
        {
            get { return profiles; }
            set
            {
                profiles = value;
                NotifyPropertyChanged();
            }
        }

        public string ResultsText
        {
            get { return resultsText; }
            set
            {
                if (resultsText != value)
                {
                    resultsText = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public String SlicerPath { get; set; }

        internal string ModelPath
        {
            get { return modelPath; }
            set
            {
                modelPath = value;
            }
        }
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AppendResults(string s)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ResultsText += s + "\n";
                }));
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            CanClose = false;
            CanSlice = false;
            ClearResults();
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
                await Task.Run(() => SliceSingleModel(fullPath, exportPath, printerPath));
            }
            else
            {
                string[] filenames = BaseViewModel.Project.GetExportFiles(".txt");
                String pth = VisualSolutionExplorer.Project.BaseFolder;
                foreach (string fullPath in filenames)
                {
                    await Task.Run(() => SliceSingleModel(fullPath, exportPath, printerPath));
                }
            }
            NotificationManager.Notify("ExportRefresh", null);
            CanClose = true;
            CanSlice = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SlicerPrinter = SelectedPrinter;
            Properties.Settings.Default.SlicerExtruder = SelectedExtruder;
            Properties.Settings.Default.SlicerProfileName = SelectedUserProfile;
            Close();
        }

        private void ClearResults()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ResultsText = "";
                }));
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
        private async Task SliceSingleModel(string fullPath, string exportPath, string printerPath)
        {
            exportDoc = new Document();
            exportDoc.Load(fullPath);

            string modelName = Path.GetFileNameWithoutExtension(fullPath);
            fullPath = Path.GetDirectoryName(fullPath);
            Bounds3D allBounds = RecalculateAllBounds();

            string exportedPath = exportDoc.ExportAll("STLSLICE", allBounds, exportPath);
            exportedPath = Path.Combine(exportPath, modelName + ".stl");

            string gcodePath = Path.Combine(printerPath, modelName + ".gcode");

            string logPath = Path.GetTempPath() + modelName + "_slicelog.log";
            string prf = selectedUserProfile;

            AppendResults("Slice " + modelName);
            bool ok;
            ok = await Task.Run(() => (CuraEngineInterface.Slice(exportedPath, gcodePath, logPath, Properties.Settings.Default.SDCardLabel, SlicerPath, selectedPrinter + ".def.json", selectedExtruder + ".def.json", prf)));
            if (ok)
            {
                AppendResults("Completed " + modelName);
            }
            else
            {
                AppendResults("FAILED  " + modelName);
                AppendResults("View Logfile  at " + logPath);
            }
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