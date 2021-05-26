using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisualSolutionExplorerLib;

namespace VisualSolutionExplorer
{
    public partial class SolutionExplorerControl : UserControl
    {
        private Point _lastMouseDown;

        //TreeViewItem draggedItem, _target;
        private TreeViewItem draggedItem, _target;

        private List<ProjectFolder> folders;
        private Point startPoint;
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
            if (e == "DragFile")
            {
                // force rebuilding of tree.
                viewModel.SetContent(Folders);
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

        private void DropTree_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ProjectFileViewModel)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ProjectFileViewModel)))
            {
                var fileViewModel = e.Data.GetData(typeof(ProjectFileViewModel)) as ProjectFileViewModel;
                var treeViewItem = Helper.TryFindParent<TreeViewItem>((DependencyObject)e.OriginalSource);
                if (treeViewItem != null)
                {
                    var dropTarget = treeViewItem.Header as ProjectFolderViewModel;

                    if (dropTarget == null || fileViewModel == null)
                        return;
                    MessageBoxResult res = MessageBox.Show("Move " + fileViewModel.FileName + " to " + dropTarget.FolderName, "Move File", MessageBoxButton.YesNo);

                    if (res == MessageBoxResult.Yes)
                    {
                        // move the file
                        string src = fileViewModel.ProjectFile.FilePath;

                        src = Project.ProjectPathToAbsPath(src);
                        string target = dropTarget.FolderPath;
                        target = Project.ProjectPathToAbsPath(target);
                        NotifySolutionChanged("DragFile", src, target);
                        viewModel.Folders[0].IsExpanded = true;
                    }
                }
            }
        }

        private void Tree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    var treeViewItem = Helper.TryFindParent<TreeViewItem>((DependencyObject)e.OriginalSource);

                    if (treeView == null || treeViewItem == null)
                        return;

                    var fileViewModel = treeView.SelectedItem as ProjectFileViewModel;
                    if (fileViewModel == null)
                        return;

                    var dragData = new DataObject(fileViewModel);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private void Tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
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