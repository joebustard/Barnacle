﻿using System;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private static Action EmptyDelegate = delegate () { };
        private static InfoWindow instance;

        private InfoWindow()
        {
            InitializeComponent();
            IsClosed = false;
            Closed += InfoWindow_Closed;
        }

        private void InfoWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        public static InfoWindow Instance()
        {
            if (instance == null)
            {
                instance = new InfoWindow();

            }
            else
            {
                if (instance.IsClosed)
                {
                    instance = new InfoWindow();
                }
            }
            instance.Owner = Application.Current.MainWindow;
            instance.Topmost = true;
            MoveToCenterOfOwner();
            return instance;
        }

        private static void MoveToCenterOfOwner()
        {
            if (instance != null && instance.Owner != null)
            {
                if (instance.Owner.WindowState != WindowState.Minimized)
                {
                    double w = instance.Owner.ActualWidth;
                    double h = instance.Owner.ActualHeight;
                    instance.Top = (h - instance.ActualHeight)/2;
                    instance.Left = (w - instance.ActualWidth) / 2;
                }
            }
        }

        public static void Refresh(System.Windows.UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }

        public void CloseInfo()
        {
            instance.Close();
            instance = null;
        }

        public void ShowInfo()
        {
            Instance().Show();
        }

        public bool IsClosed { get; set; } = false;

        public void ShowInfo(String title)
        {
            Instance().Title = title;
            try
            {
                instance.Show();
            }
            catch
            {
            }
        }

        public void ShowText(String s)
        {
            MoveToCenterOfOwner();
            instance.InfoLabel.Content = s;
            Refresh(instance.InfoLabel);
        }

        public void UpdateText(String s)
        {
            MoveToCenterOfOwner();
            instance.InfoLabel.Content = s;
        }
    }
}