using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

            ICommand deleteFile = new RelayCommand(OnDeleteFile);
            contextMenuActions.Add(new ContextMenuAction("Delete", deleteFile, "Delete the file from disk and remove it from the project"));
            ICommand removeFile = new RelayCommand(OnRemoveFile);
            contextMenuActions.Add(new ContextMenuAction("Remove", removeFile, "Remove the file from the project but do not delete it"));
        }

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

        public RenameFile OnRenameFile { get; set; }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnDeleteFile(object obj)
        {
        }

        private void OnRemoveFile(object obj)
        {
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