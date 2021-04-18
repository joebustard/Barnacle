using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace VisualSolutionExplorer
{
    public class ProjectFileViewModel : TreeViewItemViewModel
    {
        private readonly ProjectFile _projectFile;
        private FileContextMenuViewModel contextMenu;
        private BitmapImage icon;
        private IconType iconToShow;
        private bool isEditing;

        public ProjectFileViewModel(ProjectFile projfile, ProjectFolderViewModel parent)
            : base(parent, true)
        {
            _projectFile = projfile;
            contextMenu = new FileContextMenuViewModel();
            contextMenu.OnRenameFile = RenameFile;
            contextMenu.OnRemoveFile = RemoveFile;
            FileClickCommand = new RelayCommand(OnFileClickCommand);
            isEditing = false;
            StopEditing = new RelayCommand(OnStopEditing);
            if (_projectFile != null)
            {
                IconToShow = (IconType)_projectFile.IconNumber;
            }
            else
            {
                IconToShow = IconType.TextIcon;
            }
            SetIcon();
        }

        public delegate void SolutionChangedDelegate(string changeEvent, string parameter1, string parameter2);

        public enum IconType
        {
            TextIcon,
            ImageIcon
        }

        public FileContextMenuViewModel ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                if (contextMenu != value)
                {
                    contextMenu = value;
                    OnPropertyChanged("ContextMenu");
                }
            }
        }

        public ICommand FileClickCommand { get; set; }

        public string FileName
        {
            get
            {
                return _projectFile.FileName;
            }
            set
            {
                if (value != _projectFile.FileName)
                {
                    _projectFile.FileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        public string FileToolTip
        {
            get
            {
                string str = "";
                str = _projectFile?.FilePath;
                return str;
            }
        }

        public BitmapImage Icon
        {
            get
            {
                return icon;
            }
        }

        public IconType IconToShow
        {
            get
            {
                return iconToShow;
            }
            set
            {
                if (value != iconToShow)
                {
                    iconToShow = value;
                    SetIcon();
                }
            }
        }

        public bool IsEditing
        {
            get
            {
                return isEditing;
            }
            set
            {
                if (isEditing != value)
                {
                    isEditing = value;
                    OnPropertyChanged("IsEditing");
                }
            }
        }

        public SolutionChangedDelegate SolutionChanged { get; set; }

        public ICommand StopEditing { get; set; }

        public void NotifySolutionChanged(string e, string p1, string p2)
        {
            if (SolutionChanged != null)
            {
                SolutionChanged(e, p1, p2);
            }
        }

        private Uri ImageUri(string p)
        {
            return new Uri(@"pack://application:,,,/VisualSolutionExplorerLib;component/Images/" + p);
        }

        private void OnFileClickCommand(object obj)
        {
            NotifySolutionChanged("SelectFile", _projectFile.FilePath, "");
        }

        private void OnStopEditing(object obj)
        {
            IsEditing = false;
            if (_projectFile.FileName != _projectFile.OldName)
            {
                string oldPath = _projectFile.FilePath;
                _projectFile.UpdatePath();
                NotifySolutionChanged("RenameFile", oldPath, _projectFile.FilePath);
            }
        }

        private void RenameFile()
        {
            _projectFile.RecordOldName();
            IsEditing = true;
        }

        private void RemoveFile()
        {
            // can't remove yourself, have to ask the containing folder to do it
            NotifySolutionChanged("RemoveFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private void SetIcon()
        {
            switch (iconToShow)
            {
                case IconType.ImageIcon:
                    {
                        icon = new BitmapImage(ImageUri("ImageFile.png"));
                    }
                    break;

                default:
                    {
                        icon = new BitmapImage(ImageUri("File.png"));
                    }
                    break;
            }
            OnPropertyChanged("Icon");
        }
    }
}