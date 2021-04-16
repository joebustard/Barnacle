using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;

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

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
        }

        public void ProjectChanged(object param)
        {
            List<ProjectFolder> fldrs = param as List<ProjectFolder>;
            if (fldrs != null)
            {
                Folders = fldrs;
                viewModel.SetContent(Folders);
            }
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