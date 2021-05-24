using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
        public static string ProjectFilePath;
        private ObservableCollection<ProjectFolderViewModel> folders;

        public ProjectViewModel()
        {
            folders = null;
            Project = null;
            CollapseTree = new RelayCommand(OnCollapseTree);
            ExpandTree = new RelayCommand(OnExpandTree);
            RefreshTree = new RelayCommand(OnRefreshTree);
        }

        public delegate void SolutionChangedDelegate(string changeEvent, string parameter1, string parameter2);

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand CollapseTree { get; set; }
        public ICommand ExpandTree { get; set; }

        public ObservableCollection<ProjectFolderViewModel> Folders
        {
            get
            {
                return folders;
            }
            set
            {
                if (folders != value)
                {
                    folders = value;
                    NotifyPropertyChanged("Folders");
                }
            }
        }

        public Project Project { get; set; }
        public ICommand RefreshTree { get; set; }

        public SolutionChangedDelegate SolutionChanged { get; set; }

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
            if (e == "RenameFile" || e == "RenameFolder" || e == "CopyFile")
            {
                foreach (ProjectFolderViewModel pfm in Folders)
                {
                    pfm.Sort();
                }
            }
        }

        public void Refresh()
        {
            Project?.Refresh();
            foreach (ProjectFolderViewModel pfm in Folders)
            {
                pfm.Sort();
            }
        }

        public void SetContent(List<ProjectFolder> folders)
        {
            this.folders = new ObservableCollection<ProjectFolderViewModel>();
            Project?.Refresh();
            foreach (var fld in folders)
            {
                fld.RepathSubFolders("");
                ProjectFolderViewModel pvm = new ProjectFolderViewModel(fld);
                pvm.SolutionChanged = NotifySolutionChanged;
                this.folders.Add(pvm);
            }
            NotifyPropertyChanged("Folders");
        }

        internal void CreateNewFile()
        {
            if (folders.Count > 0)
            {
                folders[0].CreateNewFile();
            }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollapseTree(object obj)
        {
            foreach (ProjectFolderViewModel pfm in folders)
            {
                pfm.Collapse();
                pfm.Sort();
            }
        }

        private void OnExpandTree(object obj)
        {
            foreach (ProjectFolderViewModel pfm in folders)
            {
                pfm.Sort();
                pfm.Expand();
            }
        }

        private void OnRefreshTree(object obj)
        {
            Project?.Refresh();
            foreach (ProjectFolderViewModel pfm in folders)
            {
                pfm.Sort();
            }
            NotifyPropertyChanged("Folders");
        }

        internal void RemoveFileFromFolder(ProjectFileViewModel fileViewModel)
        {

            ProjectFolderViewModel pfm = fileViewModel.Parent as  ProjectFolderViewModel;
            pfm.RemoveFileFromFolder(fileViewModel.ProjectFile);
        }
    }
}