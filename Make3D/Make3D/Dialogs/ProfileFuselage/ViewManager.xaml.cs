using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewManager.xaml
    /// </summary>
    public partial class ViewManager : UserControl, INotifyPropertyChanged
    {
        private string commandText;
        public string CommandText
        {
            get { return commandText; }
            set
            {
                if (value != commandText)
                {
                    commandText = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string imageFilePath;
        public string ImageFilePath
        {
            get { return imageFilePath; }

            set
            {
                if (value != imageFilePath)
                {
                    imageFilePath = value;
                    PathControl.ImagePath = imageFilePath;
                    PathControl.FetchImage();
                }
            }
        }

        public delegate void CommandHandler(string command);
        public CommandHandler OnCommandHandler;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewManager()
        {
            InitializeComponent();
            CommandText = "Load It";
            DataContext = this;
        }

        private void LoadClicked(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke(CommandText);
        }

        internal void RenderFlexipath(ref Bitmap bmp, out int tlx, out int tly, out int brx, out int bry)
        {
            PathControl.RenderFlexipath(ref bmp, out tlx, out tly, out brx, out bry);
        }
    }
}