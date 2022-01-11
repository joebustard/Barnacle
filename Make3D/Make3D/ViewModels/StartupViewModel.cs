using Barnacle.Models.Mru;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Barnacle.ViewModels
{
    internal class StartupViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private String appIdentity;
        private Visibility browseProjectVisible;
        private Visibility loadingVisible;
        private Visibility newProjectVisible;
        private Visibility openVisible;
        private List<RecentProjectViewModel> recentProjects;

        private RecentProjectViewModel selectedProject;
        private DispatcherTimer updateTimer;

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
            LoadingVisible = Visibility.Hidden;
            NewProjectVisible = Visibility.Visible;
            BrowseProjectVisible = Visibility.Visible;
            TimeSpan updateDelay = new TimeSpan(0, 0, 1);
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = updateDelay;
            updateTimer.Tick += UpdateTimer_Tick;
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

        public Visibility BrowseProjectVisible
        {
            get
            {
                return browseProjectVisible;
            }
            set
            {
                if (browseProjectVisible != value)
                {
                    browseProjectVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility LoadingVisible
        {
            get
            {
                return loadingVisible;
            }
            set
            {
                if (loadingVisible != value)
                {
                    loadingVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand NewProjectCommand { get; set; }

        public Visibility NewProjectVisible
        {
            get
            {
                return newProjectVisible;
            }
            set
            {
                if (newProjectVisible != value)
                {
                    newProjectVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand OpenProjectCommand { get; set; }

        public Visibility OpenVisible
        {
            get
            {
                return openVisible;
            }
            set
            {
                if (openVisible != value)
                {
                    openVisible = value;

                    NotifyPropertyChanged();
                }
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

        internal void SelectionDoubleClick()
        {
            if (selectedProject != null)
            {
                LoadingVisible = Visibility.Visible;
                OpenVisible = Visibility.Hidden;
                BrowseProjectVisible = Visibility.Hidden;
                NewProjectVisible = Visibility.Hidden;
                updateTimer.Start();
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
            LoadingVisible = Visibility.Visible;
            OpenVisible = Visibility.Hidden;
            BrowseProjectVisible = Visibility.Hidden;
            NewProjectVisible = Visibility.Hidden;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            updateTimer.Stop();
            NotificationManager.Notify("StartWithOldProject", selectedProject.Path);
        }
    }
}