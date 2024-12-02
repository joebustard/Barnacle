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

using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using ImageUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for LibrarySnapShotDlg.xaml
    /// </summary>
    public partial class LibrarySnapShotDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string[] notAllowed =
        {
            "http",
            "www",
            ".com",
            "mail"
        };

        private Object3D part;

        private string partDescription;

        private string partName;

        private DispatcherTimer snapShotTimer;

        public LibrarySnapShotDlg()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Object3D Part
        {
            get
            {
                return part;
            }

            set
            {
                if (value != part)
                {
                    part = value;
                    if (part != null)
                    {
                        MeshColour = part.Color;
                        Vertices.Clear();
                        Faces.Clear();
                        foreach (Point3D p in part.AbsoluteObjectVertices)
                        {
                            Vertices.Add(p);
                        }
                        foreach (int f in part.TriangleIndices)
                        {
                            Faces.Add(f);
                        }
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        public String PartDescription
        {
            get
            {
                return partDescription;
            }

            set
            {
                if (value != partDescription)
                {
                    partDescription = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String PartName
        {
            get
            {
                return partName;
            }

            set
            {
                if (value != partName)
                {
                    partName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string PartPath
        {
            get; internal set;
        }

        public string PartProjectSection
        {
            get; internal set;
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (NameIsValid())
            {
                Part.Name = PartName;
            }
            if (DescriptionIsValid())
            {
                PartDescription = Part.Description;
            }

            Document localDoc = new Document();
            localDoc.ParentProject = BaseViewModel.PartLibraryProject;
            localDoc.Content.Add(Part);

            localDoc.Save(PartPath + System.IO.Path.DirectorySeparatorChar + PartName + ".txt");
            BaseViewModel.PartLibraryProject.AddFileToFolder(PartProjectSection, PartName + ".txt", false);
            BaseViewModel.PartLibraryProject.Save();
            Viewer.ShowFloor = false;
            Viewer.ShowAxies = false;
            Viewer.Model = GetModel();
            // need to give GUI a chance to repaint.
            TimeSpan interval = new TimeSpan(0, 0, 0, 0, 500);
            snapShotTimer = new DispatcherTimer();
            snapShotTimer.Interval = interval;
            snapShotTimer.Tick += SnapShotTImer_Tick;
            snapShotTimer.Start();
        }

        private void BaseModellerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Viewer.Model = GetModel();
            Camera.HomeFront();
            SetCameraDistance();
            UpdateCameraPos();
            if (Part != null)
            {
                PartName = Part.Name;
                PartDescription = Part.Description;
            }
        }

        private bool DescriptionIsValid()
        {
            bool result = true;
            if (PartDescription.Length > 128)
            {
                result = false;
            }
            String low = PartDescription.ToLower();
            foreach (string s in notAllowed)
            {
                if (low.IndexOf(s) > -1)
                {
                    result = false;
                }
            }
            return result;
        }

        private bool NameIsValid()
        {
            bool result = true;
            if (PartName == "" || PartName.Length > 32)
            {
                result = false;
            }
            String low = PartName.ToLower();
            foreach (string s in notAllowed)
            {
                if (low.IndexOf(s) > -1)
                {
                    result = false;
                }
            }
            return result;
        }

        private void SnapShotTImer_Tick(object sender, EventArgs e)
        {
            snapShotTimer.Stop();
            ImageCapture.ScreenCaptureElement(Viewer.Visual, PartPath + System.IO.Path.DirectorySeparatorChar + PartName + ".png", true);
            try
            {
                DialogResult = true;
                Close();
            }
            catch
            {
            }
        }
    }
}