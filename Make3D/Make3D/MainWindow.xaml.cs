using asdflibrary;
using Barnacle.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Barnacle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PrepareUndo();
        }

        private void PrepareUndo()
        {
            string fld = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BarnacleUndo";
            undoer.Initialise(fld);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            IInputElement ele = Keyboard.FocusedElement;
            if (!(ele is TextBox))
            {
                NotificationManager.Notify("KeyDown", e);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            NotificationManager.Notify("KeyUp", e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // check if we have been opened with a project file name on the command line
            string[] args = Environment.GetCommandLineArgs();
            string startProject = "";
            foreach (string s in args)
            {
                if (s.EndsWith(".bmf"))
                {
                    startProject = s;
                    break;
                }
            }
            if (startProject != "")
            {
                NotificationManager.Notify("ReloadProject", startProject);
            }

            InfoWindow.Instance().Owner = this;
        }
    }
}