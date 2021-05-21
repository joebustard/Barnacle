using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class FileContextMenuViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ContextMenuAction> contextMenuActions;

        public FileContextMenuViewModel()
        {
            contextMenuActions = new ObservableCollection<ContextMenuAction>();
            ICommand renameFile = new RelayCommand(OnRenFile);
            contextMenuActions.Add(new ContextMenuAction("Rename", renameFile, "Rename the file"));

            ICommand deleteFile = new RelayCommand(HandleDeleteFile);
            contextMenuActions.Add(new ContextMenuAction("Delete", deleteFile, "Delete the file from disk and remove it from the project"));

            ICommand copyFile = new RelayCommand(HandleCopyFile);
            contextMenuActions.Add(new ContextMenuAction("Copy", copyFile, "Make a copy of the file"));

            ICommand removeFile = new RelayCommand(HandleRemoveFile);
            contextMenuActions.Add(new ContextMenuAction("Remove", removeFile, "Remove the file from the project but do not delete it"));
        }

        public delegate void CopyFile();

        public delegate void DeleteFile();

        public delegate void RemoveFile();

        public delegate void RenameFile();

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

        public DeleteFile OnCopyFile { get; set; }
        public DeleteFile OnDeleteFile { get; set; }
        public RemoveFile OnRemoveFile { get; set; }
        public RenameFile OnRenameFile { get; set; }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleCopyFile(object obj)
        {
            if (OnCopyFile != null)
            {
                OnCopyFile();
            }
        }

        private void HandleDeleteFile(object obj)
        {
            if (OnDeleteFile != null)
            {
                OnDeleteFile();
            }
        }

        private void HandleRemoveFile(object obj)
        {
            if (OnRemoveFile != null)
            {
                OnRemoveFile();
            }
        }

        private void OnRenFile(object obj)
        {
            if (OnRenameFile != null)
            {
                OnRenameFile();
            }
        }
    }
}