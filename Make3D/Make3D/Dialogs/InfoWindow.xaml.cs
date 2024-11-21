/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using System;
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

        public bool IsClosed { get; set; } = false;

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

        private static void MoveToCenterOfOwner()
        {
            if (instance != null && instance.Owner != null)
            {
                if (instance.Owner.WindowState != WindowState.Minimized)
                {
                    double w = instance.Owner.ActualWidth;
                    double h = instance.Owner.ActualHeight;
                    instance.Top = (h - instance.ActualHeight) / 2;
                    instance.Left = (w - instance.ActualWidth) / 2;
                }
            }
        }

        private void InfoWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }
    }
}