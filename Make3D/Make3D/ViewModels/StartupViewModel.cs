using Barnacle.Models.Mru;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Barnacle.ViewModels
{
    internal class StartupViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private String appIdentity;
        private Visibility openVisible;
        private List<RecentProjectViewModel> recentProjects;

        private RecentProjectViewModel selectedProject;

        public StartupViewModel()
        {
            AppIdentity = "Barnacle V" + SoftwareVersion;
            NewProjectCommand = new RelayCommand(OnNewProjectCommand);
            OpenProjectCommand = new RelayCommand(OnOpenProjectCommand);
            BrowseProjectCommand = new RelayCommand(OnBrowseProjectCommand);
            recentProjects = new List<RecentProjectViewModel>();
            foreach (MruEntry mu in RecentlyUsedManager.RecentFilesList)
            {
                if (System.IO.Path.GetExtension(mu.Path) == ".bmf")
                {
                    // only add projects if the bmf file still exists.
                    if (System.IO.File.Exists(mu.Path))
                    {
                        RecentProjectViewModel vm = new RecentProjectViewModel();
                        vm.Path = mu.Path;
                        vm.Title = System.IO.Path.GetFileNameWithoutExtension(mu.Path);

                        recentProjects.Add(vm);
                    }
                }
            }

            NotifyPropertyChanged("RecentProjects");
            OpenVisible = Visibility.Hidden;
        }

        public String AppIdentity
        {
            get
            {
                return appIdentity;
            }
            set
            {
                if (appIdentity != value)
                {
                    appIdentity = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand BrowseProjectCommand { get; set; }
        public ICommand NewProjectCommand { get; set; }
        public ICommand OpenProjectCommand { get; set; }

        public Visibility OpenVisible
        {
            get
            {
                return openVisible;
            }
            set
            {
                openVisible = value;
                NotifyPropertyChanged();
            }
        }

        public List<RecentProjectViewModel> RecentProjects
        {
            get
            {
                return recentProjects;
            }
            set
            {
                if (value != recentProjects)
                {
                    recentProjects = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RecentProjectViewModel SelectedProject
        {
            get
            {
                return selectedProject;
            }
            set
            {
                if (value != selectedProject)
                {
                    selectedProject = value;
                    NotifyPropertyChanged();
                    if (selectedProject != null)
                    {
                        OpenVisible = Visibility.Visible;
                    }
                    else
                    {
                        OpenVisible = Visibility.Hidden;
                    }
                }
            }
        }

        private void OnBrowseProjectCommand(object obj)
        {
            string projectRoot = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            projectRoot += "\\Barnacle";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = projectRoot;
            dlg.Filter = Document.ProjectFilter;
            if (dlg.ShowDialog() == true)
            {
                NotificationManager.Notify("StartWithOldProject", dlg.FileName);
            }
        }

        private void OnNewProjectCommand(object obj)
        {
            NotificationManager.Notify("StartWithNewProject", null);
        }

        private void OnOpenProjectCommand(object obj)
        {
            NotificationManager.Notify("StartWithOldProject", selectedProject.Path);
        }
    }
}