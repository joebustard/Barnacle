using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Make3D.ViewModels;

namespace Make3D.Views
{
    /// <summary>
    /// Interaction logic for DefaultView.xaml
    /// </summary>
    public partial class DefaultView : UserControl
    {
        public DefaultView()
        {
            InitializeComponent();
            NotificationManager.Subscribe("SaveAsFile", SaveAsFile);
            NotificationManager.Subscribe("SaveFile", SaveFile);
            NotificationManager.Subscribe("OpenFile", OpenFile);
            NotificationManager.Subscribe("OpenRecentFile", OpenRecentFile);
            NotificationManager.Subscribe("CheckExit", CheckExit);
        }

        private void CheckExit(object sender)
        {
            MessageBoxResult res = MessageBox.Show("Document has changed. Save before exiting?", "Warning", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                SaveFile(sender);
                Application.Current.Shutdown();
            }
            else
            {
                if (res == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void OpenFile(object sender)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Document.Load(dlg.FileName);
                //   UndoManager.Clear();
            }
        }
        private void OpenRecentFile(object sender)
        {
            string f = sender.ToString();
            if (File.Exists(f))
            {
                BaseViewModel.Document.Load(f);
                // UndoManager.Clear();
            }
            else
            {
                MessageBox.Show("Can't find:" + f);
            }
        }
        private void SaveAsFile(object sender)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Document.Save(dlg.FileName);
            }
        }

        private void SaveFile(object sender)
        {
            if (BaseViewModel.Document.FilePath == String.Empty)
            {
                SaveAsFile(sender);
            }
            else
            {
                BaseViewModel.Document.Save(BaseViewModel.Document.FilePath);
            }
        }
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Scroller.Width = MyGrid.Width;
            Scroller.Height = MyGrid.Height;
        }
    }
}
