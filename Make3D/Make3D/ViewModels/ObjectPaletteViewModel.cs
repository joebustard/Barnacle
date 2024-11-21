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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Barnacle.ViewModels
{
    internal class ObjectPaletteViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ICommand AddCommand { get; set; }
        private Visibility toolPaletteVisible;

        public Visibility ToolPaletteVisible
        {
            get
            {
                return toolPaletteVisible;
            }

            set
            {
                if (toolPaletteVisible != value)
                {
                    toolPaletteVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool editingActive;

        public bool EditingActive
        {
            get
            {
                return editingActive;
            }
            set
            {
                if (editingActive != value)
                {
                    editingActive = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void SuspendEditing(object param)
        {
            bool b = Convert.ToBoolean(param);
            EditingActive = !b;
        }

        public ObjectPaletteViewModel()
        {
            AddCommand = new RelayCommand(OnAdd);
            NotificationManager.Subscribe("ToolPaletteVisible", OnToolPaletteVisibleChanged);
            NotificationManager.Subscribe("ObjectPalette", "SuspendEditing", SuspendEditing);
            editingActive = true;
        }

        private void OnToolPaletteVisibleChanged(object param)
        {
            Visibility v = (Visibility)param;
            ToolPaletteVisible = v;
        }

        private void OnAdd(object obj)
        {
            string name = obj.ToString();
            NotificationManager.Notify("AddObject", name);
        }
    }
}