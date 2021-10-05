using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace VisualSolutionExplorer
{
    public class FileContextMenuViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ContextMenuAction> contextMenuActions;

        public FileContextMenuViewModel(bool addEdit = false, bool addRun = false)
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

            if (addEdit || addRun)
            {
                contextMenuActions.Add(new ContextMenuSeparator());
            }
            if (addEdit)
            {
                ICommand editFile = new RelayCommand(HandleEditFile);
                contextMenuActions.Add(new ContextMenuAction("Edit", editFile, "Open the script in the editor"));
            }

            if (addRun)
            {
                ICommand runFile = new RelayCommand(HandleRunFile);
                contextMenuActions.Add(new ContextMenuAction("Run", runFile, "Run the script"));
            }
        }

        public delegate void CopyFile();

        public delegate void DeleteFile();

        public delegate void EditFile();

        public delegate void RemoveFile();

        public delegate void RenameFile();

        public delegate void RunFile();

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
        public EditFile OnEditFile { get; set; }
        public RemoveFile OnRemoveFile { get; set; }
        public RenameFile OnRenameFile { get; set; }
        public RunFile OnRunFile { get; set; }

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

        private void HandleEditFile(object obj)
        {
            if (OnEditFile != null)
            {
                OnEditFile();
            }
        }

        private void HandleRemoveFile(object obj)
        {
            if (OnRemoveFile != null)
            {
                OnRemoveFile();
            }
        }

        private void HandleRunFile(object obj)
        {
            if (OnRunFile != null)
            {
                OnRunFile();
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