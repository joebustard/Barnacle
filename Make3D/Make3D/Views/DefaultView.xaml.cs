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

using Barnacle.Dialogs;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using Microsoft.Win32;
using ScriptLanguage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VisualSolutionExplorer;

namespace Barnacle.Views
{
    /// <summary>
    /// Interaction logic for DefaultView.xaml
    /// </summary>
    public partial class DefaultView : UserControl
    {
        private DefaultViewModel vm;

        public DefaultView()
        {
            InitializeComponent();
            NotificationManager.Subscribe("NewFile", NewFile);
            NotificationManager.Subscribe("NewProject", NewProject);
            NotificationManager.Subscribe("SaveAsFile", SaveAsFile);
            NotificationManager.Subscribe("SaveFile", SaveFile);
            NotificationManager.Subscribe("OpenFile", OpenFile);
            NotificationManager.Subscribe("InsertFile", InsertFile);
            NotificationManager.Subscribe("Reference", ReferenceModel);
            NotificationManager.Subscribe("OpenRecentFile", OpenRecentFile);
            NotificationManager.Subscribe("CheckExit", CheckExit);
            NotificationManager.Subscribe("ProjectChanged", ProjectChanged);
            NotificationManager.Subscribe("OpenProject", OpenProject);
            NotificationManager.Subscribe("ReloadProject", ReloadProject);
            NotificationManager.Subscribe("ReloadProjectDontLoadLastFile", ReloadProject);
            NotificationManager.Subscribe("ExportRefresh", RefreshAfterExport);
            NotificationManager.Subscribe("ImportRefresh", RefreshAfterImport);
            NotificationManager.Subscribe("ScriptEditorClosed", ScriptEditorClosed);
            NotificationManager.Subscribe("SolutionPanel", ChangeSolutionPanelVisibility);
            NotificationManager.Subscribe("ObjectSelected", SelectedObjectChanged);
            NotificationManager.Subscribe("SwitchToObjectProperties", SwitchToObjectProperties);
            NotificationManager.Subscribe("AutoRunScript", AutoRunScript);
            vm = DataContext as DefaultViewModel;
        }

        public void CheckPoint()
        {
            if (BaseViewModel.Document != null)
            {
                string s = undoer.GetNextCheckPointName(BaseViewModel.Document.DocumentId.ToString());
                BaseViewModel.Document.Write(s);
            }
        }

        private static void LaunchCmdOrBat(string p)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = p,
                UseShellExecute = false,
                Verb = "open"
            };
            startInfo.EnvironmentVariables.Add("BarnacleProject", BaseViewModel.Project.ProjectName);
            startInfo.EnvironmentVariables.Add("BarnacleFolder", BaseViewModel.Project.BaseFolder);
            string currentFile = System.IO.Path.GetFileName(BaseViewModel.Document.FileName);
            startInfo.EnvironmentVariables.Add("BarnacleFile", currentFile);
            startInfo.EnvironmentVariables.Add("BarnacleSlicer", Properties.Settings.Default.SlicerPath);
            startInfo.EnvironmentVariables.Add("BarnacleSDCard", Properties.Settings.Default.SDCardLabel);
            System.Diagnostics.Process.Start(startInfo);
        }

        private static void OpenFileUsingOS(string p)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = p,
                    UseShellExecute = true,
                    Verb = "open"
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AutoRunScript(object param)
        {
            string fName = param.ToString();
            string p = BaseViewModel.Project.BaseFolder;
            p = p + "\\" + fName;

            // Check its a script
            string ext = System.IO.Path.GetExtension(p);
            ext = ext.ToLower();

            // Is it a limpet script file
            // if so change view and load it
            if (ext == ".lmp")
            {
                // run the script without gathering any solids into a document
                // because we don't have a document to use
                if (RunScript(p, false))
                {
                }
                else
                {
                    MessageBox.Show($"Failed : ${p}");
                }
            }
        }

        private void ChangeSolutionPanelVisibility(object param)
        {
            bool vis = (bool)param;
            if (vis)
            {
                EnableEditorControls(true);
            }
            else
            {
                EnableEditorControls(false);
            }
        }

        private void CheckExit(object sender)
        {
            MessageBoxResult res = MessageBox.Show("Document has changed. Save before exiting?", "Warning", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                SaveFile(sender);
                Application.Current.Shutdown();
            }
            else
            {
                if (res == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void CheckSaveFirst(object sender)
        {
            if (BaseViewModel.Document.Dirty)
            {
                MessageBoxResult res = MessageBox.Show("Document has changed. Save first?", "Warning", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    SaveFile(sender);
                }
            }
        }

        private void EnableEditorControls(bool b)
        {
            ShowGroup.IsEnabled = b;
            SelectionGroup.IsEnabled = b;
            MeshGroup.IsEnabled = b;
            ToolGroup.IsEnabled = b;
            DataGroup.IsEnabled = b;
            ProjectGroup.IsEnabled = b;
            SelectionGroup.IsEnabled = b;
            ObjectPanel.IsEnabled = b;
            EditPanel.IsEnabled = b;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SubViewControl.Width = MyGrid.Width;
            SubViewControl.Height = MyGrid.Height;
        }

        private void InsertFile(object sender)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (Directory.Exists(BaseViewModel.Project.BaseFolder))
            {
                dlg.InitialDirectory = BaseViewModel.Project.BaseFolder;
            }
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                CheckPoint();
                BaseViewModel.Document.InsertFile(dlg.FileName);
                NotificationManager.Notify("Refresh", null);
            }
        }

        private void LoadFileLastOpenedInProject()
        {
            string p = BaseViewModel.Project.BaseFolder;
            p = System.IO.Path.GetDirectoryName(p);
            p = p + BaseViewModel.Project.FirstFile;

            // is it a model file.
            if (System.IO.Path.GetExtension(p) == ".txt")
            {
                if (p != BaseViewModel.Document.FilePath)
                {
                    CheckSaveFirst(null);

                    if (File.Exists(p))
                    {
                        BaseViewModel.Document.Load(p);
                        NotificationManager.Notify("Refresh", null);
                    }
                }
            }
        }

        private void LoadNamedProject(string projName, bool loadLastFile = true)
        {
            BaseViewModel.Project.Open(projName);
            SolutionExplorer.ResetSolutionTree();
            SolutionExplorer.ProjectChanged(BaseViewModel.Project);
            if (BaseViewModel.Project.FirstFile != "" && loadLastFile)
            {
                LoadFileLastOpenedInProject();
            }

            if (BaseViewModel.Document != null)
            {
                BaseViewModel.Document.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;
            }
        }

        private void MainRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void NewFile(object param)
        {
            CheckSaveFirst(null);
            SolutionExplorer.CreateNewFile();
        }

        private void NewProject(object param)
        {
            CheckSaveFirst(null);
            BaseViewModel.Document.Clear();

            NotificationManager.Notify("NewDocument", null);
            NewProjectDlg dlg = new NewProjectDlg();
            if (dlg.ShowDialog() == true)
            {
                if (dlg.ProjectPath != "")
                {
                    if (BaseViewModel.Project.Open(dlg.ProjectPath))
                    {
                        BaseViewModel.RecentlyUsedManager.UpdateRecentFiles(dlg.ProjectPath);
                        SolutionExplorer.ResetSolutionTree();
                        SolutionExplorer.ProjectChanged(BaseViewModel.Project);
                        LoadFileLastOpenedInProject();
                        NotificationManager.Notify("Refresh", null);
                        undoer.ClearUndoFiles();
                    }
                }
            }
        }

        private void OpenFile(object sender)
        {
            CheckSaveFirst(sender);
            OpenFileDialog dlg = new OpenFileDialog();
            if (Directory.Exists(BaseViewModel.Project.BaseFolder))
            {
                dlg.InitialDirectory = BaseViewModel.Project.BaseFolder;
            }
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Document.Load(dlg.FileName);

                NotificationManager.Notify("Refresh", null);
                undoer.ClearUndoFiles();
            }
        }

        private void OpenProject(object param)
        {
            CheckSaveFirst(null);
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = BaseViewModel.Document.ProjectFilter;
            if (dlg.ShowDialog() == true)
            {
                LoadNamedProject(dlg.FileName);
                NotificationManager.Notify("Refresh", null);
                undoer.ClearUndoFiles();
                BaseViewModel.RecentlyUsedManager.UpdateRecentFiles(dlg.FileName);
            }
        }

        private void OpenRecentFile(object sender)
        {
            CheckSaveFirst(sender);
            string f = sender.ToString();
            if (File.Exists(f))
            {
                LoadNamedProject(f);
            }
            else
            {
                MessageBox.Show("Can't find:" + f);
            }
        }

        private void ProjectChanged(object param)
        {
            Project prj = param as Project;
            SolutionExplorer.ProjectChanged(prj);
        }

        private void ReferenceModel(object param)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (Directory.Exists(BaseViewModel.Project.BaseFolder))
            {
                dlg.InitialDirectory = BaseViewModel.Project.BaseFolder;
            }
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                CheckPoint();
                BaseViewModel.Document.ReferenceFile(dlg.FileName);
                NotificationManager.Notify("Refresh", null);
            }
        }

        private void RefreshAfterExport(object param)
        {
            SolutionExplorer.Refresh();
        }

        private void RefreshAfterImport(object param)
        {
            SolutionExplorer.Refresh();
        }

        private void ReloadProject(object param)
        {
            string projName = param.ToString();
            LoadNamedProject(projName);
        }

        private void ReloadProjectDontLoadLastFile(object param)
        {
            string projName = param.ToString();
            LoadNamedProject(projName, false);
        }

        private bool RunScript(string fileName, bool saveSolids = true)
        {
            Dictionary<int, Object3D> content = new Dictionary<int, Object3D>();
            bool result = false;
            NotificationManager.Notify("Select", "clear");
            if (File.Exists(fileName))
            {
                string source = File.ReadAllText(fileName);
                Interpreter interpreter = new Interpreter();
                Script script = new Script();
                if (interpreter.LoadFromText(script, source, fileName))
                {
                    content.Clear();
                    script.SetResultsContent(content);
                    script.SetPartsLibraryRoot(BaseViewModel.PartLibraryProject.BaseFolder);
                    script.SetProjectPathRoot(BaseViewModel.Project.BaseFolder);

                    if (!script.Execute())
                    {
                        MessageBox.Show("Script failed to run");
                    }
                    else
                    {
                        if (saveSolids)
                        {
                            foreach (Object3D obj in content.Values)
                            {
                                BaseViewModel.Document.Content.Add(obj);
                            }
                            content.Clear();
                            GC.Collect();
                        }
                        result = true;
                    }
                }
                else
                {
                    MessageBox.Show("Script failed to load");
                }
            }
            else
            {
                MessageBox.Show($"Script failed to load {fileName}");
            }
            return result;
        }

        private void SaveAsFile(object sender)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if (Directory.Exists(BaseViewModel.Project.BaseFolder))
            {
                dlg.InitialDirectory = BaseViewModel.Project.BaseFolder;
            }
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Document.Save(dlg.FileName);
            }
        }

        private void SaveFile(object sender)
        {
            if (BaseViewModel.Document.FilePath == String.Empty)
            {
                SaveAsFile(sender);
            }
            else
            {
                BaseViewModel.Document.Save(BaseViewModel.Document.FilePath);
            }
        }

        private void ScriptEditorClosed(object param)
        {
            vm.SwitchToView("Editor", false);
        }

        private void SelectedObjectChanged(object param)
        {
            if (param == null)
            {
                LibraryExplorer.LibraryAdd = false;
            }
            else
            {
                LibraryExplorer.LibraryAdd = true;
            }
            LibraryExplorer.Reload();
        }

        private void SetLibraryImageSource(string p)
        {
            BitmapImage bim = null;
            if (File.Exists(p))
            {
                bim = new BitmapImage(new Uri(p));
            }
            else
            {
                bim = new BitmapImage(new Uri(@"pack://application:,,,/Images/NoImage.png", UriKind.Absolute));
            }
            vm.LibraryImageSource = bim;
        }

        private void SolutionChangeRequest(string changeEvent, string parameter1, string parameter2)
        {
            switch (changeEvent)
            {
                case "EditFile":
                    {
                        string fName = parameter1;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        string ext = System.IO.Path.GetExtension(p);
                        ext = ext.ToLower();

                        // Is it a limpet script file
                        // if so change view and load it
                        if (ext == ".lmp")
                        {
                            vm.SwitchToView("Script");
                            NotificationManager.Notify("LimpetLoaded", p);
                        }
                    }
                    break;

                case "InsertFile":
                    {
                        string fName = parameter2;
                        //Wrong
                        string p = vm.GetPartsLibraryPath();
                        p = System.IO.Path.GetDirectoryName(p);
                        //p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        string ext = System.IO.Path.GetExtension(p);
                        ext = ext.ToLower();

                        if (ext == ".txt" && File.Exists(p))
                        {
                            CheckPoint();
                            BaseViewModel.Document.InsertFile(p);
                            NotificationManager.Notify("Refresh", null);
                        }
                    }
                    break;

                case "RunFile":
                    {
                        string fName = parameter2;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        string ext = System.IO.Path.GetExtension(p);
                        ext = ext.ToLower();

                        // Is it a limpet script file
                        // if so change view and load it
                        if (ext == ".lmp")
                        {
                            if (RunScript(p))
                            {
                                NotificationManager.Notify("Refresh", null);
                            }
                        }
                    }
                    break;

                case "SelectLibraryFile":
                    {
                        string fName = parameter1;

                        string p = vm.GetPartsLibraryPath();
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        string ext = System.IO.Path.GetExtension(p);
                        ext = ext.ToLower();
                        if (ext == ".txt" && File.Exists(p))
                        {
                            p = System.IO.Path.ChangeExtension(p, ".png");

                            SetLibraryImageSource(p);
                            NotificationManager.Notify("LibraryImageSource", null);
                        }
                    }
                    break;

                case "SelectFile":
                    {
                        string fName = parameter1;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        string ext = System.IO.Path.GetExtension(p);
                        ext = ext.ToLower();
                        if (ext == ".txt")
                        {
                            vm.SwitchToView("Editor");
                            if (p != BaseViewModel.Document.FilePath)
                            {
                                CheckSaveFirst(null);

                                if (File.Exists(p))
                                {
                                    NotificationManager.Notify("Loading", null);
                                    BaseViewModel.Document.Load(p);
                                    undoer.ClearUndoFiles();
                                    NotificationManager.Notify("Refresh", null);
                                    BaseViewModel.Project.FirstFile = fName;
                                    NotificationManager.Notify("ObjectSelected", null);
                                    NotificationManager.Notify("GroupSelected", false);
                                }
                            }
                        }
                        else
                        {
                            // Is it a limpet script file
                            // if so change view and load it
                            if (ext == ".lmp")
                            {
                                vm.SwitchToView("Script");
                                NotificationManager.Notify("LimpetLoaded", p);
                            }
                            else
                            {
                                OpenFileUsingOS(p);
                            }
                        }
                    }
                    break;

                case "NewFolder":
                    {
                        String fName = parameter1;
                        string p = BaseViewModel.Project.BaseFolder;

                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;
                        try
                        {
                            Directory.CreateDirectory(p);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case "NewLibraryFolder":
                    {
                        String fName = parameter1;
                        string p = vm.GetPartsLibraryPath();

                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;
                        try
                        {
                            Directory.CreateDirectory(p);
                            BaseViewModel.PartLibraryProject.Save();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case "RenameFolder":
                    {
                        String fName = parameter1;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        string old = p + fName;

                        string renamed = p + parameter2;
                        try
                        {
                            // does the old directory contain the document we currently have open?
                            // if so we need to tell the document to sort itself out
                            BaseViewModel.Document.RenameFolder(old, renamed);

                            Directory.Move(old, renamed);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case "RenameLibraryFolder":
                    {
                        String fName = parameter1;
                        string p = vm.GetPartsLibraryPath();
                        p = System.IO.Path.GetDirectoryName(p);
                        string old = p + fName;

                        string renamed = p + parameter2;
                        try
                        {
                            Directory.Move(old, renamed);
                            BaseViewModel.PartLibraryProject.Save();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case "NewFile":
                    {
                        string fName = parameter1;
                        string fileTemplate = parameter2;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        if (System.IO.Path.GetExtension(p) == ".txt")
                        {
                            if (p != BaseViewModel.Document.FilePath)
                            {
                                CheckSaveFirst(null);
                                BaseViewModel.Document.Clear();
                                BaseViewModel.Document.Save(p);
                                if (File.Exists(p))
                                {
                                    BaseViewModel.Document.Load(p);
                                    NotificationManager.Notify("Refresh", null);
                                    NotificationManager.Notify("ObjectSelected", null);
                                    NotificationManager.Notify("GroupSelected", false);
                                }
                            }
                        }
                        else
                        {
                            if (fileTemplate != null)
                            {
                                String templatePath = AppDomain.CurrentDomain.BaseDirectory + "templates\\" + fileTemplate;
                                if (File.Exists(templatePath))
                                {
                                    try
                                    {
                                        File.Copy(templatePath, p);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case "CopyFile":
                    {
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        string fname1 = p + parameter1;
                        string fname2 = p + parameter2;

                        if (fname1 == BaseViewModel.Document.FilePath)
                        {
                            CheckSaveFirst(null);
                        }
                        File.Copy(fname1, fname2);
                    }
                    break;

                case "RenameFile":
                    {
                        String fName = parameter1;
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        string old = p + fName;
                        // string ren = System.IO.Path.GetFileName( parameter2);
                        string ren = p + parameter2;
                        try
                        {
                            if (System.IO.Path.GetExtension(old) == ".txt")
                            {
                                if (old == BaseViewModel.Document.FilePath)
                                {
                                    CheckSaveFirst(null);
                                    // just incase we are renaming the currently open doc
                                    // make sure document knows the name has changed to avoid it saving back into the old
                                    // file name
                                    BaseViewModel.Document.RenameCurrent(old, ren);
                                    BaseViewModel.Project.FirstFile = fName;
                                }
                                if (File.Exists(ren))
                                {
                                    File.Delete(ren);
                                }
                                File.Move(old, ren);
                            }
                            else
                            {
                                if (File.Exists(ren))
                                {
                                    File.Delete(ren);
                                }
                                File.Move(old, ren);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    break;

                case "DeleteFile":
                    {
                        string p = BaseViewModel.Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        String fName = p + parameter1;
                        try
                        {
                            bool deletingCurrent = false;
                            if (fName == BaseViewModel.Document.FilePath)
                            {
                                deletingCurrent = true;
                            }
                            if (File.Exists(fName))
                            {
                                File.Delete(fName);
                                if (deletingCurrent)
                                {
                                    String open = BaseViewModel.Project.DefaultFileToOpen();
                                    if (File.Exists(open))
                                    {
                                        BaseViewModel.Document.Clear();
                                        BaseViewModel.Document.Load(open);
                                        NotificationManager.Notify("Refresh", null);
                                        NotificationManager.Notify("ObjectSelected", null);
                                        NotificationManager.Notify("GroupSelected", false);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;

                case "DeleteLibraryFile":
                    {
                        string p = BaseViewModel.PartLibraryProject.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        String fName = p + parameter1;
                        try
                        {
                            if (File.Exists(fName))
                            {
                                File.Delete(fName);
                            }
                            BaseViewModel.PartLibraryProject.Save();
                            LibraryExplorer.ProjectChanged(BaseViewModel.PartLibraryProject, false);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    break;

                case "DragFile":
                    {
                        String fName = System.IO.Path.GetFileName(parameter1);
                        string targetFile = parameter2;
                        // if file is being dragged to the project root the target will end in \
                        // but if its dragged to a subfolder it wont.
                        if (targetFile.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                        {
                            targetFile += fName;
                        }
                        else
                        {
                            targetFile += System.IO.Path.DirectorySeparatorChar + fName;
                        }
                        if (File.Exists(targetFile))
                        {
                            MessageBox.Show("Unable to move file. A file with the same name already exists.");
                        }
                        else
                        {
                            try
                            {
                                bool movingCurrent = false;
                                if (parameter1 == BaseViewModel.Document.FilePath)
                                {
                                    movingCurrent = true;
                                    CheckSaveFirst(null);
                                }

                                File.Move(parameter1, targetFile);

                                // removee file reference from original projectfolderviewmodel
                                BaseViewModel.Project.RemoveFileFromFolder(parameter1);

                                // add file reference to the target folder
                                BaseViewModel.Project.AddFileToFolder(parameter2, fName);

                                if (movingCurrent)
                                {
                                    // open from the new location
                                    String open = targetFile;
                                    if (File.Exists(open))
                                    {
                                        BaseViewModel.Document.Clear();
                                        BaseViewModel.Document.Load(open);
                                        NotificationManager.Notify("Refresh", null);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    break;

                case "AddObjectToLibrary":
                    {
                        NotificationManager.Notify("AddObjectToLibrary", parameter1);
                        LibraryExplorer.ProjectChanged(BaseViewModel.PartLibraryProject, false);
                    }
                    break;
            }
            BaseViewModel.Project.Save();
            BaseViewModel.Document.SaveGlobalSettings();
        }

        private void SwitchToObjectProperties(object param)
        {
            SolutionPanel.SelectedIndex = 2;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SolutionExplorer.SolutionChanged = SolutionChangeRequest;
            SolutionExplorer.ProjectChanged(BaseViewModel.Project);

            LibraryExplorer.SolutionChanged = SolutionChangeRequest;
            LibraryExplorer.ProjectChanged(BaseViewModel.PartLibraryProject, false);
            NotificationManager.Notify("Refresh", null);
        }
    }
}