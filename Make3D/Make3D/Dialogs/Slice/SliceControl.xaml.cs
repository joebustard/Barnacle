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
        private List<String> barnaclePrinterNames;
        private bool canClose;
        private bool canSlice;
        private Document exportDoc;

        private List<String> extruders;
        private bool isEditPrinterEnabled;
        private bool isEditProfileEnabled;
        private string modelPath;
        private BarnaclePrinterManager printerManager;
        private List<String> profiles;
        private string resultsText;

        private String selectedPrinter;
        private String selectedUserProfile;

        public SliceControl()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> BarnaclePrinterNames
        {
            get { return barnaclePrinterNames; }
            set
            {
                barnaclePrinterNames = value;
                NotifyPropertyChanged();
            }
        }

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

        public bool IsEditPrinterEnabled
        {
            get { return isEditPrinterEnabled; }
            set {
                if (value != isEditPrinterEnabled)
                {
                    isEditPrinterEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsEditProfileEnabled
        {
            get { return isEditProfileEnabled; }
            set
            {
                if (value != isEditProfileEnabled)
                {
                    isEditProfileEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ModelMode { get; internal set; }
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

   

        public String SelectedPrinter
        {
            get { return selectedPrinter; }
            set
            {
                selectedPrinter = value;
                if (!String.IsNullOrEmpty(selectedPrinter))
                {
                    IsEditPrinterEnabled = true;
                }
                else
                {
                    IsEditPrinterEnabled = false;
                }


                if (selectedPrinter != "" )
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
                    if (!String.IsNullOrEmpty(selectedUserProfile) && selectedUserProfile != "None")
                    {
                        IsEditProfileEnabled = true;
                    }
                    else
                    {
                        IsEditProfileEnabled = false;
                    }
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

        private void EditPrinterClicked(object sender, RoutedEventArgs e)
        {
            if (selectedPrinter != null && selectedPrinter != "")
            {
                BarnaclePrinter bp = printerManager.FindPrinter(selectedPrinter);
                if (bp != null)
                {
                    EditPrinter dlg = new EditPrinter();
                    dlg.PrinterName = bp.Name;
                    dlg.SelectedPrinter = bp.CuraPrinterFile;
                    dlg.SelectedExtruder = bp.CuraExtruderFile;
                    dlg.StartGCode = bp.StartGCode;
                    dlg.EndGCode = bp.EndGCode;
                    if (dlg.ShowDialog() == true)
                    {
                        bp.Name = dlg.PrinterName;
                        bp.CuraPrinterFile = dlg.SelectedPrinter;
                        bp.CuraExtruderFile = dlg.SelectedExtruder;
                        bp.StartGCode = dlg.StartGCode;
                        bp.EndGCode = dlg.EndGCode;
                        printerManager.Save();
                        BarnaclePrinterNames = printerManager.GetPrinterNames();
                        SelectedPrinter = dlg.PrinterName;
                    }
                }
            }
        }

        private void EditProfileClicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedUserProfile))
            {
                EditProfile dlg = new EditProfile();

                String defProfile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\Barnacle\\PrinterProfiles\\{selectedUserProfile}.profile";
                if (File.Exists(defProfile))
                {
                    dlg.LoadFile(defProfile);
                    dlg.ProfileName = selectedUserProfile;
                    dlg.CreatingNewProfile = false;
                    dlg.ShowDialog();
                }
            }
        }

        private void NewPrinterClicked(object sender, RoutedEventArgs e)
        {
            EditPrinter dlg = new EditPrinter();
            dlg.PrinterName = "New Printer";
            dlg.SelectedPrinter = @"creality_ender3pro";
            dlg.SelectedExtruder = @"creality_base_extruder_0";
            dlg.StartGCode =
@"M117 $NAME
; Ender 3 Custom Start G - code
G92 E0; Reset Extruder
G28; Home all axes
G1 Z2.0 F3000; Move Z Axis up little to prevent scratching of Heat Bed
G1 X0.1 Y20 Z0.3 F5000.0; Move to start position
G1 X0.1 Y200.0 Z0.3 F1500.0 E15; Draw the first line
G1 X0.4 Y200.0 Z0.3 F5000.0; Move to side a little
G1 X0.4 Y20 Z0.3 F1500.0 E30; Draw the second line
G92 E0; Reset Extruder
G1 Z2.0 F3000; Move Z Axis up little to prevent scratching of Heat Bed
G1 X5 Y20 Z0.3 F5000.0; Move over to prevent blob squish";
            dlg.EndGCode =
@"G91 ;Relative positioning
G1 E-2 F2700 ;Retract a bit
G1 E-2 Z0.2 F2400 ;Retract and raise Z
G1 X5 Y5 F3000 ;Wipe out
G1 Z10 ;Raise Z more
G90 ;Absolute positioning";

            if (dlg.ShowDialog() == true)
            {
                printerManager.AddPrinter(dlg.PrinterName, dlg.SelectedPrinter, dlg.SelectedExtruder, dlg.StartGCode, dlg.EndGCode);
                printerManager.Save();
                BarnaclePrinterNames = printerManager.GetPrinterNames();
                SelectedPrinter = dlg.PrinterName;
            }
        }

        private void NewProfileClicked(object sender, RoutedEventArgs e)
        {
            EditProfile dlg = new EditProfile();
            String defProfile = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\DefaultPrinter.profile";
            dlg.LoadFile(defProfile);
            dlg.ProfileName = "New Profile";
            dlg.CreatingNewProfile = true;
            dlg.ShowDialog();
            // refresh profiles list
            Profiles = CuraEngineInterface.GetAvailableUserProfiles();

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
            string curaPrinterName;
            string curaExtruderName;
            BarnaclePrinter bp = printerManager.FindPrinter(selectedPrinter);
            curaPrinterName = bp.CuraPrinterFile + ".def.json";
            curaExtruderName = bp.CuraExtruderFile + ".def.json";

            // start and end gcode may have a mcro $NAME in it.
            //Both must be a single line of text when passed to cura engine
            string sg = bp.StartGCode.Replace("$NAME", modelName);
            sg = sg.Replace("\r\n", "\\n");
            sg = sg.Replace("\n", "\\n");
            
            string eg = bp.EndGCode.Replace("$NAME", modelName);
            eg = eg.Replace("\r\n", "\\n");
            eg = eg.Replace("\n", "\\n");

            bool ok;
            ok = await Task.Run(() => (CuraEngineInterface.Slice(exportedPath, gcodePath, logPath, Properties.Settings.Default.SDCardLabel, SlicerPath, curaPrinterName,curaExtruderName, prf,sg, eg)));
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
            printerManager = new BarnaclePrinterManager();
            BarnaclePrinterNames = printerManager.GetPrinterNames();
            if (SlicerPath != null && SlicerPath != "")
            {
                Profiles = CuraEngineInterface.GetAvailableUserProfiles();
                SelectedPrinter = Properties.Settings.Default.SlicerPrinter;

                SelectedUserProfile = Properties.Settings.Default.SlicerProfileName;
            }

            CanClose = true;
            if (selectedPrinter != "" )
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