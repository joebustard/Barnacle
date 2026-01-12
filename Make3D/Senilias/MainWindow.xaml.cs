// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Microsoft.Win32;
using Senilias.StepTypes;
using Senilias.TargetTypes;
using SeniliasLib;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Senilias
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Visibility deleteStepVisible;
        private string barnacleApp = @"C:\Users\joe Bustard\source\repos\Barnacle\Make3D\Make3D\bin\Debug\Barnacle.exe";

        private string cmdTemplate =
        @"""<EXE>"" -m ""<PROJ>"" ""<SCRIPT>""
        ";

        private Visibility controlsVisible;
        private string fileFilter = "Senilia Files|*.sen";
        private string filePath;
        private bool keepGoing;
        private string modelFilter = "Model Files|*.txt";
        private string projectFolder;
        private string projectName;
        private string runResults;
        private string scriptFilter = "Script Files|*.lmp";
        private int selectedStep;
        private SeniliaFile seniliaFile;

        private bool shutdownOnFinish;
        private Visibility stopVisible;

        private ObservableCollection<TargetDependency> targets;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            controlsVisible = Visibility.Visible;
            stopVisible = Visibility.Hidden;
            filePath = "";
            targets = new ObservableCollection<TargetDependency>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Visibility ControlsVisible
        {
            get
            {
                return controlsVisible;
            }
            set
            {
                if (controlsVisible != value)
                {
                    controlsVisible = value;
                    NotifyPropertyChanged();
                    if (value == Visibility.Visible)
                    {
                        StopVisible = Visibility.Hidden;
                    }
                    else
                    {
                        StopVisible = Visibility.Visible;
                    }
                }
            }
        }

        public Visibility DeleteStepVisible
        {
            get
            {
                return deleteStepVisible;
            }
            set
            {
                if (deleteStepVisible != value)
                {
                    deleteStepVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProjectFolder
        {
            get
            {
                return projectFolder;
            }
            set
            {
                if (value != projectFolder)
                {
                    projectFolder = value;
                    NotifyPropertyChanged();
                    seniliaFile.SetProjectPath(projectFolder);
                }
            }
        }

        public string ProjectName
        {
            get
            {
                return projectName;
            }
            set
            {
                if (value != projectName)
                {
                    projectName = value;
                    NotifyPropertyChanged();
                    seniliaFile.SetProjectName(projectName);
                }
            }
        }

        public string RunResults
        {
            get
            {
                return runResults;
            }
            set
            {
                if (runResults != value)
                {
                    runResults = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SelectedStep
        {
            get
            {
                return selectedStep;
            }
            set
            {
                if (value != selectedStep)
                {
                    selectedStep = value;
                    NotifyPropertyChanged();
                    DeselectAll();
                    if (selectedStep != -1)
                    {
                        targets[selectedStep].Selected = true;
                        targets[selectedStep].BrowserVisible = Visibility.Visible;
                        DeleteStepVisible = Visibility.Visible;
                    }
                    else
                    {
                        DeleteStepVisible = Visibility.Hidden;
                    }
                }
            }
        }

        public bool ShutdownOnFinish
        {
            get
            {
                return shutdownOnFinish;
            }
            set
            {
                if (value != shutdownOnFinish)
                {
                    shutdownOnFinish = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility StopVisible
        {
            get
            {
                return stopVisible;
            }
            set
            {
                if (stopVisible != value)
                {
                    stopVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<TargetDependency> Targets
        {
            get
            {
                return targets;
            }
            set
            {
                if (value != targets)
                {
                    targets = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ExecuteCmds(string pt)
        {
            bool result = false;
            if (File.Exists(pt))
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.UseShellExecute = true;
                pi.FileName = pt;
                // pi.WindowStyle = ProcessWindowStyle.Normal;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                //    pi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Barnacle";
                Process? runner = Process.Start(pi);
                runner?.WaitForExit();
                System.Diagnostics.Debug.WriteLine($"Exit:{runner?.ExitCode}");
                if (runner?.ExitCode == 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void
        AddFile_Click(object sender, RoutedEventArgs e)
        {
            seniliaFile.Add(TargetDependency.TypeOfDependency.File, "source", "target", StepType.TypeOfStep.Script, "script");
            UpdateTargets();
        }

        private void Browse_Action(object sender, RoutedEventArgs e)
        {
            if (selectedStep != -1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = scriptFilter;
                ofd.FileName = "*.lmp";
                ofd.InitialDirectory = seniliaFile.GetProjectPath();
                if (ofd.ShowDialog() == true)
                {
                    if (File.Exists(ofd.FileName))
                    {
                        string sub = ofd.FileName.Substring(ofd.InitialDirectory.Length + 1);
                        Targets[selectedStep].Action = new ScriptStep(sub);
                        NotifyPropertyChanged("Targets");
                    }
                }
            }
        }

        private void Browse_Project(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dlg = new OpenFolderDialog();
            if (!String.IsNullOrEmpty(seniliaFile.GetProjectPath()))
            {
                dlg.InitialDirectory = seniliaFile.GetProjectPath();
            }
            if (dlg.ShowDialog() == true)
            {
                if (Directory.Exists(dlg.FolderName))
                {
                    seniliaFile.SetProjectPath(dlg.FolderName);
                    ProjectFolder = dlg.FolderName;
                }
            }
        }

        private void Browse_Source(object sender, RoutedEventArgs e)
        {
            if (selectedStep != -1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = modelFilter;
                ofd.FileName = "*.txt";
                ofd.Multiselect = true;

                ofd.InitialDirectory = seniliaFile.GetProjectPath();
                if (ofd.ShowDialog() == true)
                {
                    Targets[selectedStep].Source = "";
                    foreach (string fl in ofd.FileNames)
                    {
                        if (File.Exists(fl))
                        {
                            string sub = fl.Substring(ofd.InitialDirectory.Length + 1);
                            Targets[selectedStep].Source += sub + ",";
                        }
                    }
                    if (Targets[selectedStep].Source.EndsWith(","))
                    {
                        Targets[selectedStep].Source = Targets[selectedStep].Source.Substring(0, Targets[selectedStep].Source.Length - 1);
                    }
                    NotifyPropertyChanged("Targets");
                }
            }
        }

        private void Browse_Target(object sender, RoutedEventArgs e)
        {
            if (selectedStep != -1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = modelFilter;
                ofd.FileName = "*.txt";
                ofd.InitialDirectory = seniliaFile.GetProjectPath();
                if (ofd.ShowDialog() == true)
                {
                    if (File.Exists(ofd.FileName))
                    {
                        string sub = ofd.FileName.Substring(ofd.InitialDirectory.Length + 1);
                        Targets[selectedStep].Target = sub;
                        NotifyPropertyChanged("Targets");
                    }
                }
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            RunResults = "";
            if (seniliaFile != null)
            {
                seniliaFile.CreateAcyclicDirectedGraph();
                seniliaFile.CheckTriggers();
                //   seniliaFile.SaveGraph("c:\\tmp\\tree.txt");
                List<string> todo = seniliaFile.ToDo();
                foreach (string todoItem in todo)
                {
                    runResults += todoItem + "\r\n";
                }
                NotifyPropertyChanged("RunResults");
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            RunResults = "";
        }

        private void DeleteStep_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStep != -1)
            {
                seniliaFile.RemoveAt(selectedStep);
                UpdateTargets();
            }
        }

        private void DeselectAll()
        {
            foreach (var item in targets)
            {
                item.Selected = false;
                item.BrowserVisible = Visibility.Hidden;
            }
            NotifyPropertyChanged("Targets");
            DataContext = this;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (seniliaFile.Dirty)
            {
                if (MessageBox.Show("Document has changed. Save before exit?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Save();
                }
            }

            Application.Current.Shutdown();
        }

        private void LoadAndUpdate(string fn)
        {
            seniliaFile.LoadFile(fn);
            ProjectFolder = seniliaFile.GetProjectPath();
            ProjectName = seniliaFile.GetProjectName();
            UpdateTargets();
            filePath = fn;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = fileFilter;
            if (ofd.ShowDialog() == true)
            {
                if (File.Exists(ofd.FileName))
                {
                    string fn = ofd.FileName;
                    LoadAndUpdate(fn);
                }
            }
        }

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            keepGoing = true;
            ControlsVisible = Visibility.Hidden;
            RunResults = "";
            if (seniliaFile != null)
            {
                seniliaFile.CreateAcyclicDirectedGraph();
                seniliaFile.CheckTriggers();
                List<string> todo = seniliaFile.ToDo();
                foreach (string todoItem in todo)
                {
                    if (keepGoing)
                    {
                        if (todoItem.StartsWith("Run "))
                        {
                            RunResults += todoItem + "\r\n";
                            string item = todoItem.Substring(4);
                            item = item.Trim();
                            if (File.Exists(SeniliasLib.Utils.Utils.RelativeToProject(item)))
                            {
                                await Task.Run(() => keepGoing = RunSingleStep(item, seniliaFile.GetProjectPath() + "\\" + seniliaFile.GetProjectName()));
                            }
                            else
                            {
                                RunResults += $"Cant find action script :{item}\r\n";
                                keepGoing = false;
                            }
                        }
                        else if (todoItem.StartsWith("Touch "))
                        {
                            RunResults += todoItem + "\r\n";
                            string item = todoItem.Substring(6);
                            item = item.Trim();
                            string fileName = SeniliasLib.Utils.Utils.RelativeToProject(item);
                            if (File.Exists(fileName))
                            {
                                System.IO.File.SetLastWriteTime(fileName, DateTime.Now);
                            }
                            else
                            {
                                RunResults += $"Cant touch :{item}\r\n";
                                keepGoing = false;
                            }
                        }
                        NotifyPropertyChanged("RunResults");
                    }
                }
                RunResults += "done\r\n";
            }
            ControlsVisible = Visibility.Visible;
            if (shutdownOnFinish && keepGoing)
            {
                ShutdownPC();
            }
        }

        private async void RunAll_Click(object sender, RoutedEventArgs e)
        {
            keepGoing = true;
            ControlsVisible = Visibility.Hidden;
            RunResults = "";
            if (seniliaFile != null)
            {
                seniliaFile.CreateAcyclicDirectedGraph();
                seniliaFile.ForceTriggers();
                List<string> todo = seniliaFile.ToDo();
                foreach (string todoItem in todo)
                {
                    if (keepGoing)
                    {
                        if (todoItem.StartsWith("Run "))
                        {
                            RunResults += todoItem + "\r\n";
                            string item = todoItem.Substring(4);
                            item = item.Trim();
                            await Task.Run(() => keepGoing = RunSingleStep(item, seniliaFile.GetProjectPath() + "\\" + seniliaFile.GetProjectName()));
                        }
                        NotifyPropertyChanged("RunResults");
                    }
                }
            }
            RunResults += "done\r\n";
            ControlsVisible = Visibility.Visible;
            StopVisible = Visibility.Hidden;
            if (shutdownOnFinish && keepGoing)
            {
                ShutdownPC();
            }
        }

        private bool RunSingleStep(string item, string proj)
        {
            bool res = false;
            string cmdText = cmdTemplate.Replace("<EXE>", barnacleApp);
            cmdText = cmdText.Replace("<PROJ>", proj);
            cmdText = cmdText.Replace("<SCRIPT>", item);
            System.Diagnostics.Debug.WriteLine(cmdText);
            try
            {
                // we need to construct a cmd file to run the slice Work out where it should be. use
                // different temp names, because we will have async tasks going and we dont want two
                // trying to write to the same cmd or log file
                string tmpCmdFile = System.IO.Path.GetTempFileName();
                tmpCmdFile = System.IO.Path.ChangeExtension(tmpCmdFile, "cmd");
                if (File.Exists(tmpCmdFile))
                {
                    File.Delete(tmpCmdFile);
                }
                File.WriteAllText(tmpCmdFile, cmdText);
                res = ExecuteCmds(tmpCmdFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            System.Diagnostics.Debug.WriteLine($"res ={res}");
            return res;
        }

        private void Save()
        {
            if (filePath == "")
            {
                SaveAs();
            }
            else
            {
                seniliaFile.SaveFile(filePath);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = fileFilter;
            if (dlg.ShowDialog() == true && dlg.FileName != "")
            {
                seniliaFile.SaveFile(dlg.FileName);
                filePath = dlg.FileName;
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void ShutdownPC()
        {
            bool res = false;
            string cmdText = "shutdown -s -f -t 15";
            System.Diagnostics.Debug.WriteLine(cmdText);
            try
            {
                string tmpCmdFile = System.IO.Path.GetTempFileName();
                tmpCmdFile = System.IO.Path.ChangeExtension(tmpCmdFile, "cmd");
                if (File.Exists(tmpCmdFile))
                {
                    File.Delete(tmpCmdFile);
                }
                File.WriteAllText(tmpCmdFile, cmdText);
                res = ExecuteCmds(tmpCmdFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopBuild_Click(object sender, RoutedEventArgs e)
        {
            keepGoing = false;
            StopVisible = Visibility.Hidden;
        }

        private void UpdateTargets()
        {
            targets.Clear();
            foreach (var v in seniliaFile.Targets)
            {
                targets.Add(v);
            }
            NotifyPropertyChanged("Targets");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            seniliaFile = new SeniliaFile();
            selectedStep = -1;
            DeleteStepVisible = Visibility.Hidden;
            foreach (var a in Environment.GetCommandLineArgs())
            {
                if (a.ToLower().EndsWith(".sen"))
                {
                    LoadAndUpdate(a);
                    break;
                }
            }
            ProjectFolder = seniliaFile.GetProjectPath();
            ProjectName = seniliaFile.GetProjectName();
        }
    }
}