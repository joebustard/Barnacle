using Make3D.Dialogs;
using Make3D.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VisualSolutionExplorer;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for DefaultView.xaml
    /// </summary>
    public partial class DefaultView : UserControl
    {
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
            NotificationManager.Subscribe("ExportRefresh", RefreshAfterExport);
        }

        public void CheckPoint()
        {
            if (BaseViewModel.Document != null)
            {
                string s = undoer.GetNextCheckPointName();
                BaseViewModel.Document.Write(s);
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

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Scroller.Width = MyGrid.Width;
            Scroller.Height = MyGrid.Height;
        }

        private void InsertFile(object sender)
        {
            OpenFileDialog dlg = new OpenFileDialog();
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
            string p = Project.BaseFolder;
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

        private void LoadNamedProject(string projName)
        {
            BaseViewModel.Project.Open(projName);
            SolutionExplorer.ProjectChanged(BaseViewModel.Project);
            if (BaseViewModel.Project.FirstFile != "")
            {
                LoadFileLastOpenedInProject();
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
                        SolutionExplorer.ProjectChanged(BaseViewModel.Project);
                        String initialFile = BaseViewModel.Project.FirstFile;
                        BaseViewModel.Document.Load(initialFile);

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

        private void ReloadProject(object param)
        {
            string projName = param.ToString();
            LoadNamedProject(projName);
        }

        private void SaveAsFile(object sender)
        {
            SaveFileDialog dlg = new SaveFileDialog();
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

        private void SolutionChangeRequest(string changeEvent, string parameter1, string parameter2)
        {
            switch (changeEvent)
            {
                case "SelectFile":
                    {
                        string fName = parameter1;
                        string p = Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        p = p + fName;

                        // is it a model file.
                        if (System.IO.Path.GetExtension(p) == ".txt")
                        {
                            if (p != BaseViewModel.Document.FilePath)
                            {
                                CheckSaveFirst(null);

                                if (File.Exists(p))
                                {
                                    NotificationManager.Notify("Loading", null);
                                    BaseViewModel.Document.Load(p);
                                    NotificationManager.Notify("Refresh", null);
                                    BaseViewModel.Project.FirstFile = fName;
                                    //  UndoManager.Clear();
                                }
                            }
                        }
                        else
                        {
                            // throw the file at the operating system

                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = p,
                                UseShellExecute = true,
                                Verb = "open"
                            });
                        }
                    }
                    break;

                case "NewFolder":
                    {
                        String fName = parameter1;
                        string p = Project.BaseFolder;
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

                case "RenameFolder":
                    {
                        String fName = parameter1;
                        string p = Project.BaseFolder;
                        p = System.IO.Path.GetDirectoryName(p);
                        string old = p + fName;
                        // string ren = System.IO.Path.GetFileName( parameter2);
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

                case "NewFile":
                    {
                        string fName = parameter1;
                        string p = Project.BaseFolder;
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
                                    //  UndoManager.Clear();
                                }
                            }
                        }
                    }
                    break;

                case "CopyFile":
                    {
                        string p = Project.BaseFolder;
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
                        string p = Project.BaseFolder;
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
                                }
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
                        string p = Project.BaseFolder;
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
                                        //  UndoManager.Clear();
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

                case "DragFile":
                    {
                        String fName = System.IO.Path.GetFileName(parameter1);
                        string targetFile = parameter2;
                        // if file is being dragged to the project root the target will end in \
                        // but if its dragged to a subfolder it wont.
                        if (targetFile.EndsWith("\\"))
                        {
                            targetFile += fName;
                        }
                        else
                        {
                            targetFile += "\\" + fName;
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
                                        //  UndoManager.Clear();
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
            }
            BaseViewModel.Project.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SolutionExplorer.SolutionChanged = SolutionChangeRequest;
            SolutionExplorer.ProjectChanged(BaseViewModel.Project);
        }
    }
}