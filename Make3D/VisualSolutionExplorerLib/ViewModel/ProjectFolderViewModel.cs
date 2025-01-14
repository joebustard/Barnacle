﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class ProjectFolderViewModel : TreeViewItemViewModel
    {
        private readonly ProjectFolder _folder;
        private List<ProjectFolderViewModel> _folders;
        private FolderContextMenuViewModel contextMenu;
        public Project ParentProject { get; set; }
        private bool isEditing;

        public ProjectFolderViewModel(ProjectFolder folder)
                    : base(null, true)
        {
            _folder = folder;
            _folder.Vm = this;
            MakeContextMenu(folder);
            ParentProject = folder.ParentProject;
            isEditing = false;
            StopEditing = new RelayCommand(OnStopEditing);
        }

        private void MakeContextMenu(ProjectFolder folder)
        {
            contextMenu = new FolderContextMenuViewModel(folder.SupportsSubFolders,
                                                            folder.SupportsFiles,
                                                            folder.CanBeRenamed,
                                                            folder.Explorer, folder.CanAddToLibrary && folder.IsInLibrary);
            contextMenu.OnAddExistingFile = AddExistingFile;
            contextMenu.OnCreateFolder = CreateNewFolder;
            contextMenu.OnCreateFile = CreateNewFile;
            contextMenu.OnRenameFolder = RenameFolder;
            contextMenu.OnExploreFolder = ExploreFolder;
            contextMenu.OnAddObjectToLibrary = AddObjectToLibrary;
            OnPropertyChanged("ContextMenu");
        }

        private void AddObjectToLibrary()
        {
            NotifySolutionChanged("AddObjectToLibrary", FolderPath, "");
        }

        public delegate void SolutionChangedDelegate(string changeEvent, string parameter1, string parameter2);

        public FolderContextMenuViewModel ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                if (contextMenu != value)
                {
                    contextMenu = value;
                    OnPropertyChanged("ContextMenu");
                }
            }
        }

        public string FolderName
        {
            get
            {
                return _folder.FolderName;
            }
            set
            {
                if (_folder.FolderName != value)
                {
                    _folder.FolderName = value;
                    OnPropertyChanged("FolderName");
                }
            }
        }

        public string FolderPath
        {
            get
            {
                if (_folder == null)
                {
                    return "";
                }
                else
                {
                    return _folder.FolderPath;
                }
            }
        }

        public string FolderToolTip
        {
            get
            {
                string str = "";
                str = _folder?.FolderPath;
                return str;
            }
        }

        internal void UpdateMenu()
        {
            MakeContextMenu(_folder);
        }

        public bool IsEditing
        {
            get
            {
                return isEditing;
            }
            set
            {
                if (isEditing != value)
                {
                    isEditing = value;
                    OnPropertyChanged("IsEditing");
                }
            }
        }

        public SolutionChangedDelegate SolutionChanged { get; set; }
        public ICommand StopEditing { get; set; }

        public void AddExistingFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                foreach (String fName in dlg.FileNames)
                {
                    string ren = System.IO.Path.GetFileName(fName);
                    string ext = System.IO.Path.GetExtension(fName);

                    string p = ParentProject.BaseFolder;
                    p = System.IO.Path.GetDirectoryName(p);

                    string target = p + _folder.FolderPath + System.IO.Path.DirectorySeparatorChar + ren;
                    if (fName.ToLower() != target.ToLower())
                    {
                        System.IO.File.Copy(fName, target);
                    }
                    _folder.AddExistingFile(ren);
                    Sort();
                    NotifySolutionChanged("AddExistingFile", target, "");
                }
            }
        }

        internal bool FolderMatch(ProjectFolder fld)
        {
            return _folder.FolderPath == fld.FolderPath;
        }

        public void CreateNewFile()
        {
            string f = _folder.CreateNewFile();
            if (f != "")
            {
                NotifySolutionChanged("NewFile", f, _folder.FileTemplate);
                IsExpanded = true;
                LoadChildren();
            }
        }

        public void CreateNewFolder()
        {
            string f = _folder.CreateNewFolder();
            if (_folder.IsInLibrary)
            {
                NotifySolutionChanged("NewLibraryFolder", f, "");
            }
            else
            {
                NotifySolutionChanged("NewFolder", f, "");
            }
            IsExpanded = true;
            LoadChildren();
        }

        public void ExploreFolder()
        {
            string root = Path.GetDirectoryName(ParentProject.BaseFolder);
            string pth = root + _folder.FolderPath;
            if (!pth.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                pth += Path.DirectorySeparatorChar;
            }
            if (Directory.Exists(pth))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = pth,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        public override void StopAllEditing()
        {
            IsEditing = false;

            foreach (TreeViewItemViewModel it in Children)
            {
                it.StopAllEditing();
            }
        }

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (e == "RemoveFile")
            {
                _folder.RemoveFile(p1);
                LoadChildren();
            }
            else
            if (e == "DeleteFile")
            {
                _folder.DeleteFile(p1);
                LoadChildren();
            }
            else
            if (e == "DeleteLibraryFile")
            {
                _folder.DeleteFile(p1);
                LoadChildren();
            }
            if (e == "CopyFile")
            {
                string old = p2;
                string newish = _folder.CopyFile(p1);
                LoadChildren();
                p1 = old;
                p2 = _folder.FolderPath + System.IO.Path.DirectorySeparatorChar + newish;
            }
            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
        }

        internal void AddFileReference(ProjectFileViewModel fileViewModel)
        {
            ProjectFile pf = fileViewModel.ProjectFile;
            pf.FilePath = ""; // not valid anymore
            _folder.ProjectFiles.Add(pf);
        }

        internal void Collapse()
        {
            IsExpanded = false;
            //Sort();
            foreach (TreeViewItemViewModel ti in base.Children)
            {
                ti.IsExpanded = false;
            }
            if (_folders != null)
            {
                foreach (ProjectFolderViewModel pfm in _folders)
                {
                    pfm.Collapse();
                }
            }
        }

        internal void Expand()
        {
            IsExpanded = true;
            //Sort();
            foreach (TreeViewItemViewModel ti in base.Children)
            {
                ti.IsExpanded = true;
                foreach (TreeViewItemViewModel sub in ti.Children)
                {
                    sub.IsExpanded = true;
                    if (sub is ProjectFolderViewModel)
                    {
                        (sub as ProjectFolderViewModel).Expand();
                    }
                }
            }
        }

        internal void RemoveFileFromFolder(ProjectFile file)
        {
            _folder.ProjectFiles.Remove(file);
        }

        internal void Sort()
        {
            _folder.ProjectFiles.Sort();

            LoadChildren();
        }

        protected override void LoadChildren()
        {
            base.Children.Clear();
            foreach (ProjectFolder fld in _folder.ProjectFolders)
            {
                ProjectFolderViewModel pfo = new ProjectFolderViewModel(fld);
                pfo.ParentProject = ParentProject;

                pfo.SolutionChanged = NotifySolutionChanged;
                base.Children.Add(pfo);
                pfo.LoadChildren();
            }
            foreach (ProjectFile file in _folder.ProjectFiles)
            {
                ProjectFileViewModel pfi = new ProjectFileViewModel(file, this);
                pfi.SolutionChanged = NotifySolutionChanged;
                base.Children.Add(pfi);
            }
        }

        private void OnStopEditing(object obj)
        {
            IsEditing = false;
            if (_folder.FolderName != _folder.OldName)
            {
                string oldPath = _folder.FolderPath;
                _folder.UpdatePath();
                if (_folder.IsInLibrary)
                {
                    NotifySolutionChanged("RenameLibraryFolder", oldPath, _folder.FolderPath);
                }
                else
                {
                    NotifySolutionChanged("RenameFolder", oldPath, _folder.FolderPath);
                }
            }
        }

        private void RenameFolder()
        {
            if (_folder.CanBeRenamed)
            {
                _folder.RecordOldName();
                IsEditing = true;
            }
        }
    }
}