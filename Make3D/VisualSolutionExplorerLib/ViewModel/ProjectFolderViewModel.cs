using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class ProjectFolderViewModel : TreeViewItemViewModel
    {
        private readonly ProjectFolder _folder;
        private List<ProjectFolderViewModel> _folders;
        private FolderContextMenuViewModel contextMenu;

        private bool isEditing;

        public ProjectFolderViewModel(ProjectFolder folder)
                    : base(null, true)
        {
            _folder = folder;

            contextMenu = new FolderContextMenuViewModel(folder.SupportsSubFolders, folder.SupportsFiles);
            contextMenu.OnCreateFolder = CreateNewFolder;
            contextMenu.OnCreateFile = CreateNewFile;
            contextMenu.OnRenameFolder = RenameFolder;
            contextMenu.OnExploreFolder = ExploreFolder;
            contextMenu.OnAddExistingFile = AddExistingFile;
            isEditing = false;
            StopEditing = new RelayCommand(OnStopEditing);
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

        public string FolderToolTip
        {
            get
            {
                string str = "";
                str = _folder?.FolderPath;
                return str;
            }
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
            if (dlg.ShowDialog() == true)
            {
                String fName = dlg.FileName;
                string ren = System.IO.Path.GetFileName(fName);
                string ext = System.IO.Path.GetExtension(fName);
                if (ext == ".txt")
                {
                    string p = Project.BaseFolder;
                    p = System.IO.Path.GetDirectoryName(p);

                    string target = p + _folder.FolderPath + "\\" + ren;
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

        public void CreateNewFile()
        {
            string f = _folder.CreateNewFile();
            NotifySolutionChanged("NewFile", f, "");
            IsExpanded = true;
            LoadChildren();
        }

        public void CreateNewFolder()
        {
            string f = _folder.CreateNewFolder();
            NotifySolutionChanged("NewFolder", f, "");
            IsExpanded = true;
            LoadChildren();
        }

        public void ExploreFolder()
        {
            NotifySolutionChanged("ExploreFolder", _folder.FolderPath, "");
        }

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (e == "RemoveFile")
            {
                _folder.RemoveFile(p1);
                LoadChildren();
            }

            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
        }

        internal void Collapse()
        {
            IsExpanded = false;
            //Sort();
            foreach (TreeViewItemViewModel ti in base.Children)
            {
                ti.IsExpanded = false;
            }
        }

        internal void Expand()
        {
            IsExpanded = true;
            //Sort();
            foreach (TreeViewItemViewModel ti in base.Children)
            {
                ti.IsExpanded = true;
            }
        }

        internal void Sort()
        {
            _folder.ProjectFiles.Sort();
            /*
            foreach (ProjectFolder fld in _folder.ProjectFolders)
            {
                fld.ProjectFiles.Sort();
            }
            */
            LoadChildren();
        }

        protected override void LoadChildren()
        {
            base.Children.Clear();
            foreach (ProjectFolder fld in _folder.ProjectFolders)
            {
                ProjectFolderViewModel pfo = new ProjectFolderViewModel(fld);
                pfo.SolutionChanged = NotifySolutionChanged;
                base.Children.Add(pfo);
            }
            foreach (ProjectFile state in _folder.ProjectFiles)
            {
                ProjectFileViewModel pfi = new ProjectFileViewModel(state, this);
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
                NotifySolutionChanged("RenameFolder", oldPath, _folder.FolderPath);
            }
        }

        private void RenameFolder()
        {
            _folder.RecordOldName();
            IsEditing = true;
        }
    }
}