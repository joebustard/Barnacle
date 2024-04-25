using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class FolderContextMenuViewModel : INotifyPropertyChanged
    {
#pragma warning disable CS0169
        private ObservableCollection<ContextMenuAction> contextMenuActions;
#pragma warning restore CS0169
        private bool valid;

        public FolderContextMenuViewModel(bool addFolder, bool addFile, bool renFolder, bool offerExplorer, bool offerAddToLibrary)
        {
            Valid = false;
            contextMenuActions = new ObservableCollection<ContextMenuAction>();
            if (addFolder)
            {
                ICommand addFolderCommand = new RelayCommand(HandleAddFolder);
                contextMenuActions.Add(new ContextMenuAction("New Folder", addFolderCommand, "Add a new folder inside this one"));
                Valid = true;
            }
            if (renFolder)
            {
                ICommand renameFolderCommand = new RelayCommand(HandleRenameFolder);
                contextMenuActions.Add(new ContextMenuAction("Rename", renameFolderCommand, "Rename the folder"));
                Valid = true;
            }
            if (offerExplorer)
            {
                ICommand exploreFolderCommand = new RelayCommand(HandleExploreFolder);
                contextMenuActions.Add(new ContextMenuAction("Explore", exploreFolderCommand, "Open the folder in explorer"));
                Valid = true;
            }

            if (addFile)
            {
                contextMenuActions.Add(new ContextMenuSeparator());
                ICommand addFileCommand = new RelayCommand(HandleAddFile);
                contextMenuActions.Add(new ContextMenuAction("New File", addFileCommand, "Add a new file to the project"));
                ICommand addExistingFileCommand = new RelayCommand(HandleAddExistingFile);
                contextMenuActions.Add(new ContextMenuAction("Add Existing File", addExistingFileCommand, "Add an existing file to the project"));
                Valid = true;
            }
            if (offerAddToLibrary)
            {
                contextMenuActions.Add(new ContextMenuSeparator());
                ICommand addLibraryCommand = new RelayCommand(HandleAddToLibrary);
                contextMenuActions.Add(new ContextMenuAction("Add Selected Part To Library", addLibraryCommand, "Add the selected object to the library"));

                Valid = true;
            }
        }

        public delegate void AddExistingFile();

        public delegate void AddObjectToLibrary();

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

        public AddObjectToLibrary OnAddObjectToLibrary { get; set; }

        public CreateFile OnCreateFile { get; set; }

        public CreateFolder OnCreateFolder { get; set; }

        public ExploreFolder OnExploreFolder { get; set; }

        public RenameFolder OnRenameFolder { get; set; }

        public bool Valid
        {
            get { return valid; }

            set
            {
                valid = value;
                NotifyPropertyChanged("Valid");
            }
        }

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

        private void HandleAddToLibrary(object obj)
        {
            if (OnAddObjectToLibrary != null)
            {
                OnAddObjectToLibrary();
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