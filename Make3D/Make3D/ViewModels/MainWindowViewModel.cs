// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.Views;
using System.ComponentModel;
using System.Windows.Controls;

namespace Barnacle.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string caption;
        private Control subview;

        internal MainWindowViewModel()
        {
            Caption = "";
            base.PropertyChanged += MainWindowViewModel_PropertyChanged;
            SubView = new StartupView();
            NotificationManager.Subscribe("StartWithNewProject", StartWithNewProject);
            NotificationManager.Subscribe("NewProjectBack", NewProjectBack);
            NotificationManager.Subscribe("ShowEditor", ShowEditor);
            NotificationManager.Subscribe("StartWithOldProject", StartWithOldProject);
            NotificationManager.Subscribe("StartWithOldProjectNoLoad", StartWithOldProjectNoLoad);
        }

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Control SubView
        {
            get
            {
                return subview;
            }
            set
            {
                if (subview != value)
                {
                    subview = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Caption")
            {
                Caption = Document.Caption;
            }
        }

        private void NewProjectBack(object param)
        {
            SubView = new StartupView();
        }

        private void ShowEditor(object param)
        {
            SubView = new DefaultView();
        }

        private void StartWithNewProject(object param)
        {
            SubView = new NewProjectView();
        }

        private void StartWithOldProject(object param)
        {
            string projPath = param.ToString();
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProject", projPath);
        }

        private void StartWithOldProjectNoLoad(object param)
        {
            string projPath = param.ToString();
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProjectDontLoadLastFile", projPath);
        }
    }
}