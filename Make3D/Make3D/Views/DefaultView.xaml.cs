using Make3D.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
                NotificationManager.Notify("Refresh", null);
                //   UndoManager.Clear();
            }
        }

        private void OpenRecentFile(object sender)
        {
            string f = sender.ToString();
            if (File.Exists(f))
            {
                BaseViewModel.Document.Load(f);
                NotificationManager.Notify("Refresh", null);
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