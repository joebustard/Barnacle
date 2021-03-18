using Make3D.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Make3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //  LineUtils.CoplanerTest();
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
    }
}