using System.ComponentModel;

namespace Barnacle.ViewModels
{
    internal class RecentProjectViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }

        private string path;

        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                NotifyPropertyChanged();
            }
        }
    }
}