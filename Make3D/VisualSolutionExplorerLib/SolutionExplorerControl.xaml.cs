﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace VisualSolutionExplorer
{
    public partial class SolutionExplorerControl : UserControl
    {
        private List<ProjectFolder> folders;
        private ProjectViewModel viewModel;

        public SolutionExplorerControl()
        {
            InitializeComponent();

            //  Folders = Database.GetFolders("");
            viewModel = new ProjectViewModel();
            //   viewModel.SetContent(folders);
            base.DataContext = viewModel;
            viewModel.SolutionChanged = NotifySolutionChanged;
        }

        public delegate void SolutionChangedDelegate(string changeEvent, string parameter1, string parameter2);

        public List<ProjectFolder> Folders
        {
            get
            {
                return folders;
            }
            set
            {
                if (value != folders)
                {
                    folders = value;
                }
            }
        }

        public SolutionChangedDelegate SolutionChanged { get; set; }

        public void CreateNewFile()
        {
            viewModel.CreateNewFile();
        }

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
        }

        public void ProjectChanged(Project prj)
        {
            viewModel.Project = prj;
            List<ProjectFolder> fldrs = prj.ProjectFolders;
            if (fldrs != null)
            {
                Folders = fldrs;
                viewModel.SetContent(Folders);
                ProjectViewModel.ProjectFilePath = Project.BaseFolder;
            }
        }

        public void Refresh()
        {
            viewModel.Refresh();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView item = sender as TreeView;
            if (e.OldValue != null)
            {
                object ob = e.OldValue;

                if (ob is ProjectFolderViewModel)
                {
                    ProjectFolderViewModel pfovm = ob as ProjectFolderViewModel;
                    pfovm.IsEditing = false;
                }
            }
            if (e.NewValue != null)
            {
                object ob = e.NewValue;

                if (ob is ProjectFolderViewModel)
                {
                    ProjectFolderViewModel pfovm = ob as ProjectFolderViewModel;
                    pfovm.IsEditing = false;
                }
            }
        }
    }
}