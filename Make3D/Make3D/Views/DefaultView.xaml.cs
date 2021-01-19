using Make3D.Dialogs;
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
            NotificationManager.Subscribe("NewFile", NewFile);
            NotificationManager.Subscribe("NewProject", NewProject);
            NotificationManager.Subscribe("SaveAsFile", SaveAsFile);
            NotificationManager.Subscribe("SaveFile", SaveFile);
            NotificationManager.Subscribe("OpenFile", OpenFile);
            NotificationManager.Subscribe("InsertFile", InsertFile);
            NotificationManager.Subscribe("Reference", ReferenceModel);
            NotificationManager.Subscribe("OpenRecentFile", OpenRecentFile);
            NotificationManager.Subscribe("CheckExit", CheckExit);
        }

        private void NewFile(object param)
        {
            CheckSaveFirst(null);
            BaseViewModel.Document.Clear();
            //   Caption = BaseViewModel.Document.Caption;
            NotificationManager.Notify("NewDocument", null);
        }

        private void NewProject(object param)
        {
            CheckSaveFirst(null);
            BaseViewModel.Document.Clear();

            NotificationManager.Notify("NewDocument", null);
            NewProjectDlg dlg = new NewProjectDlg();
            if (dlg.ShowDialog() == true)
            {
            }
        }

        private void ReferenceModel(object param)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                CheckPoint();
                BaseViewModel.Document.ReferenceFile(dlg.FileName);
                NotificationManager.Notify("Refresh", null);
            }
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

        private void CheckSaveFirst(object sender)
        {
            if (BaseViewModel.Document.Dirty)
            {
                MessageBoxResult res = MessageBox.Show("Document has changed. Save first?", "Warning", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    SaveFile(sender);
                }
            }
        }

        private void OpenFile(object sender)
        {
            CheckSaveFirst(sender);
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                BaseViewModel.Document.Load(dlg.FileName);

                NotificationManager.Notify("Refresh", null);
                undoer.ClearUndoFiles();
            }
        }

        private void InsertFile(object sender)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = BaseViewModel.Document.FileFilter;
            if (dlg.ShowDialog() == true)
            {
                CheckPoint();
                BaseViewModel.Document.InsertFile(dlg.FileName);
                NotificationManager.Notify("Refresh", null);
            }
        }

        public void CheckPoint()
        {
            if (BaseViewModel.Document != null)
            {
                string s = undoer.GetNextCheckPointName();
                BaseViewModel.Document.Write(s);
            }
        }

        private void OpenRecentFile(object sender)
        {
            CheckSaveFirst(sender);
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

        private void MainRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}