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
using System.Windows.Input;

namespace Barnacle.ViewModels
{
    internal class AboutViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private String appIdentity;

        public AboutViewModel()
        {
            AppIdentity = "Barnacle V" + SoftwareVersion;
            EmailText = "Email: Barnacle3D@gmail.com";
            OKCommand = new RelayCommand(OnOk, null);
            String fp = AppDomain.CurrentDomain.BaseDirectory + "Data\\ReadMe.txt";
            if ( System.IO.File.Exists(fp))
            {
                ReadMeText = System.IO.File.ReadAllText(fp);
            }
        }

        private string readMeText;
        public string ReadMeText
        {
            get { return readMeText; }
            set
            {
                if (value != readMeText)
                {
                    readMeText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string emailText;
        public string EmailText
        {
            get { return emailText; }
            set
            {
                if (value != emailText)
                {
                    emailText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String AppIdentity
        {
            get
            {
                return appIdentity;
            }
            set
            {
                if (appIdentity != value)
                {
                    appIdentity = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand OKCommand { get; set; }

        private void OnOk(object obj)
        {
            NotificationManager.Notify("CloseAbout", null);
        }
    }
}