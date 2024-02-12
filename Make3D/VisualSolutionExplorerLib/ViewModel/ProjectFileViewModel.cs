using System;
using System.Windows;
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
            contextMenu = new FileContextMenuViewModel(projfile.EditFile, projfile.RunFile, projfile.IsLibraryFile);
            contextMenu.OnRenameFile = RenameFile;
            contextMenu.OnRemoveFile = RemoveFile;
            contextMenu.OnDeleteFile = DeleteFile;
            contextMenu.OnDeleteLibraryFile = DeleteLibraryFile;
            contextMenu.OnCopyFile = CopyFile;
            contextMenu.OnEditFile = EditFile;
            contextMenu.OnRunFile = RunFile;
            contextMenu.OnInsertFile = InsertFile;
            FileDoubleClickCommand = new RelayCommand(OnFileDoubleClickCommand);
            FileClickCommand = new RelayCommand(OnFileClickCommand);
            isEditing = false;
            StopEditing = new RelayCommand(OnStopEditing);
            if (_projectFile != null)
            {
                SetIconForExt();
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
            ImageIcon,
            StlIcon,
            PrinterIcon,
            ScriptIcon,
            CmdIcon
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
        public ICommand FileDoubleClickCommand { get; set; }

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
                    SetIconForExt();
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

        public override void StopAllEditing()
        {
            IsEditing = false;
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

        public ProjectFile ProjectFile
        {
            get { return _projectFile; }
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

        private void CopyFile()
        {
            // can't Copy yourself, have to ask the containing folder to do it
            NotifySolutionChanged("CopyFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private void DeleteFile()
        {
            if (MessageBox.Show("Permanently delete file:" + _projectFile.FileName, "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // can't remove yourself, have to ask the containing folder to do it
                NotifySolutionChanged("DeleteFile", _projectFile.FilePath, "Y");
            }
        }

        private void DeleteLibraryFile()
        {
            if (MessageBox.Show("Permanently delete library file:" + _projectFile.FileName, "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // can't remove yourself, have to ask the containing folder to do it
                NotifySolutionChanged("DeleteLibraryFile", _projectFile.FilePath, "Y");
            }
        }

        private void EditFile()
        {
            // can't edit yourself, have to ask the containing folder to do it
            NotifySolutionChanged("EditFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private Uri ImageUri(string p)
        {
            return new Uri(@"pack://application:,,,/VisualSolutionExplorerLib;component/Images/" + p);
        }

        private void InsertFile()
        {
            // insert yourself, have to ask the containing folder to do it
            NotifySolutionChanged("InsertFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private void OnFileClickCommand(object obj)
        {
            // single click in the library can trigger changing image
            // but double click is required to do an open
            if (_projectFile.IsLibraryFile)
            {
                NotifySolutionChanged("SelectLibraryFile", _projectFile.FilePath, "");
            }
        }

        private void OnFileDoubleClickCommand(object obj)
        {
            if (_projectFile.IsLibraryFile)
            {
                NotifySolutionChanged("SelectLibraryFile", _projectFile.FilePath, "");
            }
            else
            {
                NotifySolutionChanged("SelectFile", _projectFile.FilePath, "");
                IsSelected = true;
            }
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

        private void RemoveFile()
        {
            // can't remove yourself, have to ask the containing folder to do it
            NotifySolutionChanged("RemoveFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private void RenameFile()
        {
            _projectFile.RecordOldName();
            IsEditing = true;
        }

        private void RunFile()
        {
            NotifySolutionChanged("RunFile", _projectFile.FileName, _projectFile.FilePath);
        }

        private void SetIcon()
        {
            switch (iconToShow)
            {
                case IconType.ScriptIcon:
                    {
                        icon = new BitmapImage(ImageUri("Script.png"));
                    }
                    break;

                case IconType.ImageIcon:
                    {
                        icon = new BitmapImage(ImageUri("ImageFile.png"));
                    }
                    break;

                case IconType.StlIcon:
                    {
                        if (_projectFile.OutOfDate)
                        {
                            icon = new BitmapImage(ImageUri("outofdatestl.png"));
                        }
                        else
                        {
                            icon = new BitmapImage(ImageUri("stl.png"));
                        }
                    }
                    break;

                case IconType.PrinterIcon:
                    {
                        if (_projectFile.OutOfDate)
                        {
                            icon = new BitmapImage(ImageUri("outofdategcode.png"));
                        }
                        else
                        {
                            icon = new BitmapImage(ImageUri("gcode.png"));
                        }
                    }
                    break;

                case IconType.CmdIcon:
                    {
                        icon = new BitmapImage(ImageUri("cmd.png"));
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

        private void SetIconForExt()
        {
            if (_projectFile != null)
            {
                String ext = System.IO.Path.GetExtension(_projectFile.FileName);
                ext = ext.ToLower();
                switch (ext)
                {
                    case ".stl":
                        {
                            IconToShow = IconType.StlIcon;
                        }
                        break;

                    case ".cmd":
                    case ".bat":
                        {
                            IconToShow = IconType.CmdIcon;
                        }
                        break;

                    case ".png":
                        {
                            IconToShow = IconType.ImageIcon;
                        }
                        break;

                    case ".lmp":
                        {
                            IconToShow = IconType.ScriptIcon;
                        }
                        break;

                    case ".gcode":
                    case ".gco":
                    case ".g":
                    case ".photon":
                    case ".ctb":
                        {
                            IconToShow = IconType.PrinterIcon;
                        }
                        break;

                    default:
                        {
                            IconToShow = IconType.TextIcon;
                        }
                        break;
                }
            }
        }
    }
}