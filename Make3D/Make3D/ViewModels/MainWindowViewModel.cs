using Make3D.Views;
using System.ComponentModel;
using System.Windows.Controls;

namespace Make3D.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string caption;
        private Control subview;

        internal MainWindowViewModel()
        {
            Caption = "";
            base.PropertyChanged += MainWindowViewModel_PropertyChanged;
            SubView = new StartupView();
            NotificationManager.Subscribe("StartWithNewProject", StartWithNewProject);
            NotificationManager.Subscribe("NewProjectBack", NewProjectBack);
            NotificationManager.Subscribe("ShowEditor", ShowEditor);
            NotificationManager.Subscribe("StartWithOldProject", StartWithOldProject);
        }

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Control SubView
        {
            get
            {
                return subview;
            }
            set
            {
                if (subview != value)
                {
                    subview = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Caption")
            {
                Caption = Document.Caption;
            }
        }

        private void NewProjectBack(object param)
        {
            SubView = new StartupView();
        }

        private void ShowEditor(object param)
        {
            SubView = new DefaultView();
        }

        private void StartWithNewProject(object param)
        {
            SubView = new NewProjectView();
        }

        private void StartWithOldProject(object param)
        {
            string projPath = param.ToString();
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProject", projPath);
        }
    }
}