using Make3D.Models.Mru;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Make3D.ViewModels
{
    internal class StartupViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<RecentProjectViewModel> recentProjects;

        public List<RecentProjectViewModel> RecentProjects
        {
            get { return recentProjects; }
            set
            {
                if (value != recentProjects)
                {
                    recentProjects = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Visibility openVisible;

        public Visibility OpenVisible
        {
            get { return openVisible; }
            set
            {
                openVisible = value;
                NotifyPropertyChanged();
            }
        }

        private RecentProjectViewModel selectedProject;

        public RecentProjectViewModel SelectedProject
        {
            get { return selectedProject; }
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

        private String appIdentity;

        public StartupViewModel()
        {
            AppIdentity = "Barnacle V" + SoftwareVersion;
            NewProjectCommand = new RelayCommand(OnNewProjectCommand);
            OpenProjectCommand = new RelayCommand(OnOpenProjectCommand);
            recentProjects = new List<RecentProjectViewModel>();
            foreach (MruEntry mu in RecentlyUsedManager.RecentFilesList)
            {
                RecentProjectViewModel vm = new RecentProjectViewModel();
                vm.Path = mu.Path;
                vm.Title = System.IO.Path.GetFileNameWithoutExtension(mu.Path);

                recentProjects.Add(vm);
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

        public ICommand NewProjectCommand { get; set; }
        public ICommand OpenProjectCommand { get; set; }

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