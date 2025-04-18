﻿/**************************************************************************
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Barnacle.Dialogs.Figure
{
    /// <summary>
    /// Interaction logic for ModelAssignmentControl.xaml
    /// </summary>
    public partial class ModelAssignmentControl : UserControl, INotifyPropertyChanged
    {
        private List<String> availableFigureNames;
        private String boneName;
        private String figureName;

        private double hScale;

        private double lScale;

        private double wScale;

        public ModelAssignmentControl()
        {
            InitializeComponent();
            availableFigureNames = new List<string>();
            DataContext = this;
            hScale = 1.0;
            wScale = 1.0;
            lScale = 1.0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> AvailableFigureNames
        {
            get
            {
                return availableFigureNames;
            }

            set
            {
                if (availableFigureNames != value)
                {
                    availableFigureNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String BoneName
        {
            get
            {
                return boneName;
            }

            set
            {
                if (boneName != value)
                {
                    boneName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String FigureName
        {
            get
            {
                foreach (String s in availableFigureNames)
                {
                    if (String.Compare(s.ToLower(), figureName) == 0)
                    {
                        return s;
                    }
                }
                return figureName;
            }

            set
            {
                if (figureName != value)
                {
                    figureName = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("SelectedFigure", this);
                }
            }
        }

        public double HScale
        {
            get
            {
                return hScale;
            }

            set
            {
                if (hScale != value)
                {
                    if (value > 0)
                    {
                        hScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
                }
            }
        }

        public double LScale
        {
            get
            {
                return lScale;
            }

            set
            {
                if (lScale != value)
                {
                    if (value > 0)
                    {
                        lScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
                }
            }
        }

        public double WScale
        {
            get
            {
                return wScale;
            }

            set
            {
                if (wScale != value)
                {
                    if (value > 0)
                    {
                        wScale = value;
                        NotifyPropertyChanged();
                        NotificationManager.Notify("FigureScale", this);
                    }
                }
            }
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}