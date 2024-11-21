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

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Barnacle.ViewModels
{
    internal class CameraViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private int buttonAreaHeight;

        private Visibility cameraPaletteVisible;

        public CameraViewModel()
        {
            CameraCommand = new RelayCommand(OnCameraCommand);
            NotificationManager.Subscribe("CameraPaletteVisible", OnCameraPaletteVisibleChanged);
            ButtonAreaHeight = 100;
        }

        public int ButtonAreaHeight
        {
            get
            {
                return buttonAreaHeight;
            }
            set
            {
                if (buttonAreaHeight != value)
                {
                    buttonAreaHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand CameraCommand { get; set; }

        public Visibility CameraPaletteVisible
        {
            get
            {
                return cameraPaletteVisible;
            }

            set
            {
                if (cameraPaletteVisible != value)
                {
                    cameraPaletteVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OnCameraCommand(object obj)
        {
            // Don't try to handle it here
            // just post on
            NotificationManager.Notify("CameraCommand", obj);
        }

        private void OnCameraPaletteVisibleChanged(object param)
        {
            Visibility v = (Visibility)param;
            CameraPaletteVisible = v;
        }
    }
}