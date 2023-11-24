using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Barnacle.ViewModels
{
    public class ToolDef : INotifyPropertyChanged
    {
        private string _commandParam;
        private bool _isActive;
        private BitmapImage imagesource;
        private string imageName;

        public ToolDef(string n, bool a, string cp, string tl, string imname = "")
        {
            Name = n;
            IsActive = a;
            CommandParam = cp;
            ToolTip = tl;
            ToolCommand = new RelayCommand(OnCommand, null);
            imagesource = null;
            imageName = cp;
            if (imname != "")
            {
                imageName = imname;
            }
            string imp = "Images/Buttons/" + cp + ".png";
            try
            {
                imagesource = LoadBitmapFromResource(imp);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine($"Failed to find icon {imp}");
            }
        }

        public BitmapImage ToolImage
        {
            get { return imagesource; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string CommandParam { get => _commandParam; set => _commandParam = value; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                NotifyPropertyChanged();
            }
        }

        public string Name { get; set; }

        public ICommand ToolCommand
        { get; set; }

        public string ToolTip { get; set; }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnCommand(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Tool", s);
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
        }
    }
}