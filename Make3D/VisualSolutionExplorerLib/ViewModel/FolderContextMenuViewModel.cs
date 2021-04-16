using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class FolderContextMenuViewModel : INotifyPropertyChanged
    {
        private ICommand addFolderCommand;

        private ObservableCollection<ContextMenuAction> contextMenuActions;

        public FolderContextMenuViewModel(bool addFolder, bool addFile)
        {
            contextMenuActions = new ObservableCollection<ContextMenuAction>();
            if (addFolder)
            {
                ICommand addFolderCommand = new RelayCommand(HandleAddFolder);
                contextMenuActions.Add(new ContextMenuAction("New Folder", addFolderCommand, "Add a new folder inside this one"));
            }
            ICommand renameFolderCommand = new RelayCommand(HandleRenameFolder);
            contextMenuActions.Add(new ContextMenuAction("Rename", renameFolderCommand, "Rename the folder"));

            ICommand exploreFolderCommand = new RelayCommand(HandleExploreFolder);
            contextMenuActions.Add(new ContextMenuAction("Explore", exploreFolderCommand, "Open the folder in explorer"));
            contextMenuActions.Add(new ContextMenuSeparator());

            if (addFile)
            {
                ICommand addFileCommand = new RelayCommand(HandleAddFile);
                contextMenuActions.Add(new ContextMenuAction("New File", addFileCommand, "Add a new file to the project"));
                ICommand addExistingFileCommand = new RelayCommand(HandleAddExistingFile);
                contextMenuActions.Add(new ContextMenuAction("Add Existing File", addExistingFileCommand, "Add an existing file to the project"));
            }
        }

        public delegate void AddExistingFile();

        public delegate void CreateFile();

        public delegate void CreateFolder();

        public delegate void ExploreFolder();

        public delegate void RenameFolder();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ContextMenuAction> ContextMenuActions
        {
            get
            {
                return contextMenuActions;
            }
            set
            {
                if (contextMenuActions != value)
                {
                    contextMenuActions = value;
                    NotifyPropertyChanged("ContextMenuActions");
                }
            }
        }

        public AddExistingFile OnAddExistingFile { get; set; }
        public CreateFile OnCreateFile { get; set; }

        public CreateFolder OnCreateFolder { get; set; }

        public ExploreFolder OnExploreFolder { get; set; }
        public RenameFolder OnRenameFolder { get; set; }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleAddExistingFile(object obj)
        {
            if (OnAddExistingFile != null)
            {
                OnAddExistingFile();
            }
        }

        private void HandleAddFile(object obj)
        {
            if (OnCreateFile != null)
            {
                OnCreateFile();
            }
        }

        private void HandleAddFolder(object obj)
        {
            if (OnCreateFolder != null)
            {
                OnCreateFolder();
            }
        }

        private void HandleExploreFolder(object obj)
        {
            if (OnExploreFolder != null)
            {
                OnExploreFolder();
            }
        }

        private void HandleRenameFolder(object obj)
        {
            if (OnRenameFolder != null)
            {
                OnRenameFolder();
            }
        }
    }
}