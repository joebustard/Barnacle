using Barnacle.Models;
using Barnacle.Models.SDCard;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private bool canCopyToSD;
        private bool canSeeLog;
        private bool canSlice;
        private Document exportDoc;

        private List<String> extruders;
        private bool isEditPrinterEnabled;
        private bool isEditProfileEnabled;
        private string lastLog;
        private string modelPath;
        private BarnaclePrinterManager printerManager;
        private List<String> profiles;
        private string resultsText;

        private String selectedPrinter;
        private String selectedUserProfile;
        private DispatcherTimer timer;

        public SliceControl()
        {
            InitializeComponent();
            timer = new DispatcherTimer(new TimeSpan(0, 0, 20), DispatcherPriority.Normal, TimerTick, Dispatcher.CurrentDispatcher);

            //df.SaveSettings("c:\\tmp\\test.json.def");
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

        public bool CanCopyToSD
        {
            get { return canCopyToSD; }
            set
            {
                if (value != canCopyToSD)
                {
                    canCopyToSD = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanSeeLog
        {
            get { return canSeeLog; }
            set
            {
                canSeeLog = value;
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
            set
            {
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

        /// <summary>
        /// Are we doing a single model or all the models in the project?
        /// </summary>
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

                if (selectedPrinter != "")
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

        /// <summary>
        /// If processing  a single document this is were to find it.
        /// </summary>
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

        private void AppendResults(string s, bool crlf = true)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ResultsText += s;
                    if (crlf)
                    {
                        ResultsText += "\n";
                    }
                    ResultsBox.CaretIndex = ResultsBox.Text.Length;
                    ResultsBox.ScrollToEnd();
                }));
        }

        private void CheckIfSDCopyShouldBeEnabled()
        {
            CanCopyToSD = false;
            string cardName = Properties.Settings.Default.SDCardLabel;
            if (!String.IsNullOrEmpty(cardName))
            {
                string root = SDCardUtils.FindSDCard(cardName);
                if (!String.IsNullOrEmpty(root))
                {
                    CanCopyToSD = true;
                }
            }
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

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            Properties.Settings.Default.SlicerPrinter = SelectedPrinter;

            Properties.Settings.Default.SlicerProfileName = SelectedUserProfile;
            Properties.Settings.Default.Save();
            Close();
        }

        private void CopySingleFile(string fn, string trg, string name)
        {
            try
            {
                File.Copy(fn, trg, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateBaseProfile(string selectedPrinter, string srcPrinter)
        {
            String SlicerPath = Properties.Settings.Default.SlicerPath;
            if (SlicerPath != null && SlicerPath != "")
            {
                CuraDefinitionFile df = new CuraDefinitionFile();
                string fname = $"{SlicerPath}\\share\\cura\\resources\\definitions\\{srcPrinter}.def.json";
                //df.Load(fname);
                //df.ProcessSettings();
                SlicerProfile baseSlicerProfile = new SlicerProfile(fname);
                string baseFile = UserProfilePath + selectedPrinter + ".baseprofile";
                baseSlicerProfile.SaveOverrides(baseFile);
            }
        }

        private void DelProfileClicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedUserProfile))
            {
                MessageBoxResult res = MessageBox.Show("The profile will be permanently deleted", "Warning", MessageBoxButton.OKCancel);
                if (res == MessageBoxResult.OK)
                {
                    String defProfile = UserProfilePath + $"{selectedUserProfile}.profile";
                    if (File.Exists(defProfile))
                    {
                        try
                        {
                            File.Delete(defProfile);
                            // refresh profiles list
                            Profiles = CuraEngineInterface.GetAvailableUserProfiles();
                        }
                        catch (Exception ex)
                        {
                            LoggerLib.Logger.LogException(ex);
                        }
                    }
                }
            }
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
                        string srcPrinter = dlg.SelectedPrinter;
                        CreateBaseProfile(SelectedPrinter, srcPrinter);
                    }
                }
            }
        }

        private void EditProfileClicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(selectedUserProfile))
            {
                EditProfile dlg = new EditProfile();
                string fl = UserProfilePath;
                String defProfile = fl + $"{selectedUserProfile}.profile";
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
G1 E-3 F2700 ;Retract filament a bit to prevent oozing
G1 E-2 Z0.2 F2400 ;Retract and raise Z
G1 X5 Y5 F3000 ;Wipe out
G1 Z10 ;Raise Z more
G90 ;Absolute positioning
G1 X10 Y220 F1000 ; offer part for removal
M106 S0 ; Turn off cooling fan
M104 S0 ; Turn off extruder
M140 S0 ; Turn off bed
M107 ; Turn off Fan
M84 ; Disable stepper motors
";

            if (dlg.ShowDialog() == true)
            {
                bool update = true;
                if (printerManager.FindPrinter(dlg.PrinterName) != null)
                {
                    if (MessageBox.Show($"Printer {dlg.PrinterName} already exists. Overwrite it?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        update = false;
                    }
                    printerManager.RemovePrinter(dlg.PrinterName);
                }
                if (update)
                {
                    printerManager.AddPrinter(dlg.PrinterName, dlg.SelectedPrinter, dlg.SelectedExtruder, dlg.StartGCode, dlg.EndGCode);
                    printerManager.Save();
                    BarnaclePrinterNames = printerManager.GetPrinterNames();
                    SelectedPrinter = dlg.PrinterName;
                    string srcPrinter = dlg.SelectedPrinter;

                    CreateBaseProfile(SelectedPrinter, srcPrinter);
                }
            }
        }

        private void NewProfileClicked(object sender, RoutedEventArgs e)
        {
            EditProfile dlg = new EditProfile();
            //   string defProfile = UserProfilePath + SelectedPrinter + ".baseprofile";
            //dlg.LoadFile(defProfile);
            dlg.PrinterName = SelectedPrinter;
            dlg.ProfileName = "New Profile";
            dlg.CreatingNewProfile = true;
            dlg.ShowDialog();
            // refresh profiles list
            Profiles = CuraEngineInterface.GetAvailableUserProfiles();
            SelectedUserProfile = dlg.ProfileName;
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

        private async void SDClicked(object sender, RoutedEventArgs e)
        {
            string cardName = Properties.Settings.Default.SDCardLabel;
            if (!String.IsNullOrEmpty(cardName))
            {
                string root = SDCardUtils.FindSDCard(cardName);
                if (!String.IsNullOrEmpty(root))
                {
                    CanClose = false;
                    CanSlice = false;
                    CanCopyToSD = false;
                    string projName = BaseViewModel.Project.ProjectName;
                    string sdPath = System.IO.Path.Combine(root, projName);
                    if (!Directory.Exists(sdPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(sdPath);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        SDCardUtils.ClearFolder(sdPath);
                    }
                    string printerPath = BaseViewModel.Project.BaseFolder + "\\printer";
                    string[] fileNames = Directory.GetFiles(printerPath, "*.gcode");
                    foreach (string fn in fileNames)
                    {
                        string name = Path.GetFileName(fn);
                        AppendResults("Copy " + name);
                        string trg = Path.Combine(sdPath, name);
                        await Task.Run(() => CopySingleFile(fn, trg, name));
                    }
                    AppendResults("Copy complete");
                }
                else
                {
                    MessageBox.Show("Couldn't find sdcard labelled " + cardName);
                }
                CanClose = true;
                CanSlice = true;
                // re-enable the sdcopy button if appropriate
                CheckIfSDCopyShouldBeEnabled();
            }
            else
            {
                MessageBox.Show("Sdcard label not defined in settings.");
            }
        }

        private void SeeLogClicked(object sender, RoutedEventArgs e)
        {
            if (lastLog != "" && File.Exists(lastLog))
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = "Notepad.exe";
                pi.Arguments = lastLog;
                //  pi.WindowStyle = ProcessWindowStyle.Normal;
                pi.WindowStyle = ProcessWindowStyle.Normal;
                pi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle";
                Process runner = Process.Start(pi);
            }
        }

        private async void SliceClicked(object sender, RoutedEventArgs e)
        {
            CanClose = false;
            CanSlice = false;
            CanCopyToSD = false;
            ClearResults();
            String exportPath = BaseViewModel.Project.BaseFolder + "\\export";
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            string printerPath = BaseViewModel.Project.BaseFolder + "\\printer";
            if (!Directory.Exists(printerPath))
            {
                Directory.CreateDirectory(printerPath);
            }
            if (ModelMode == "SliceModel")
            {
                string fullPath = modelPath;
                if (!String.IsNullOrEmpty(fullPath))
                {
                    await Task.Run(() => SliceSingleModel(fullPath, exportPath, printerPath));
                }
            }
            else
            {
                string[] filenames = BaseViewModel.Project.GetExportFiles(".txt");
                String pth = BaseViewModel.Project.BaseFolder;
                foreach (string fullPath in filenames)
                {
                    await Task.Run(() => SliceSingleModel(fullPath, exportPath, printerPath));
                }
            }
            // Make the solution show the newly exported stl & g-code
            NotificationManager.Notify("ExportRefresh", null);
            CanSeeLog = true;

            CanClose = true;
            CanSlice = true;
            // re-enable the sdcopy button if appropriate
            CheckIfSDCopyShouldBeEnabled();
            AppendResults("Slice complete");
        }

        private async Task SliceSingleModel(string fullPath, string exportPath, string printerPath)
        {
            exportDoc = new Document();
            exportDoc.ParentProject = BaseViewModel.Project;
            exportDoc.Load(fullPath);

            string modelName = Path.GetFileNameWithoutExtension(fullPath);
            fullPath = Path.GetDirectoryName(fullPath);
            Bounds3D allBounds = RecalculateAllBounds();

            string exportedPath = exportDoc.ExportAll("STLSLICE", allBounds, exportPath);
            exportedPath = Path.Combine(exportPath, modelName + ".stl");

            string gcodePath = Path.Combine(printerPath, modelName + ".gcode");

            string logPath = Path.GetTempPath() + modelName + "_slicelog.log";
            lastLog = logPath;
            
            string prf = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\PrinterProfiles\\"+selectedUserProfile+".profile";

            AppendResults(modelName.PadRight(16) + ", ", false);
            string curaPrinterName;
            string curaExtruderName;
            BarnaclePrinter bp = printerManager.FindPrinter(selectedPrinter);
            curaPrinterName = bp.CuraPrinterFile + ".def.json";
            curaExtruderName = bp.CuraExtruderFile + ".def.json";

            // start and end gcode may have a macro $NAME in it.
            //Both must be a single line of text when passed to cura engine
            string sg = bp.StartGCode.Replace("$NAME", modelName);
            sg = sg.Replace("\r\n", "\\n");
            sg = sg.Replace("\n", "\\n");

            string eg = bp.EndGCode.Replace("$NAME", modelName);
            eg = eg.Replace("\r\n", "\\n");
            eg = eg.Replace("\n", "\\n");

            SliceResult sliceRes;
            sliceRes = await Task.Run(() => (CuraEngineInterface.Slice(exportedPath, gcodePath, logPath, Properties.Settings.Default.SDCardLabel, SlicerPath, curaPrinterName, curaExtruderName, prf, sg, eg)));
            if (sliceRes.Result)
            {
                int exportedParts = 0;
                foreach (Object3D obj in exportDoc.Content)
                {
                    if (obj.Exportable)
                    {
                        exportedParts++;
                    }
                }
                string timeFormat = sliceRes.Hours.ToString("00") + ":" + sliceRes.Minutes.ToString("00") + ":" + sliceRes.Seconds.ToString("00");
                AppendResults($"{exportedParts.ToString().PadRight(3)}, {timeFormat}, {sliceRes.Filament}");
            }
            else
            {
                AppendResults(" FAILED View Logfile  at " + logPath);
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            if (canSlice)
            {
                CheckIfSDCopyShouldBeEnabled();
            }
            timer.Start();
        }

        private string UserProfilePath = "";
        private string UserProfilePathNoSlash = "";

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
            CanSeeLog = false;
            if (selectedPrinter != "")
            {
                CanSlice = true;
            }
            else
            {
                CanSlice = false;
            }
            CheckIfSDCopyShouldBeEnabled();
            UserProfilePathNoSlash = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle\\PrinterProfiles";
            UserProfilePath = UserProfilePathNoSlash + "\\";
        }
    }
}