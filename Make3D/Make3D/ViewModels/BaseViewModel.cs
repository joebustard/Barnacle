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

using Barnacle.Models;
using Barnacle.Models.Mru;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using VisualSolutionExplorer;

namespace Barnacle.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        // only one document shared between all the views
        protected static Document document;

        protected static Project partLibraryProject;
        protected static Project project;
        protected static MostRecentlyUsedManager recentlyUsedManager;

        protected bool lastChangeWasNudge;

        private static SolidColorBrush _fillBrushColor = Brushes.Black;

        private static SolidColorBrush _strokeBrushColor = Brushes.Black;

        private static string selectedFont;

        private string softwareVersion;

        public BaseViewModel()
        {
            if (project == null)
            {
                project = new Project();
                project.CreateDefault();
            }
            if (document == null)
            {
                document = new Document();
                document.ProjectSettings = project.SharedProjectSettings;
                document.ParentProject = project;
            }
            if (recentlyUsedManager == null)
            {
                recentlyUsedManager = new MostRecentlyUsedManager();
                recentlyUsedManager.Name = "Mru";
            }
            document.PropertyChanged += Document_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Document Document
        {
            get { return document; }
        }

        public static Project PartLibraryProject
        {
            get { return partLibraryProject; }
            set { partLibraryProject = value; }
        }

        public static Project Project
        {
            get { return project; }
        }

        public static MostRecentlyUsedManager RecentlyUsedManager
        {
            get { return recentlyUsedManager; }
        }

        public static bool ScriptClearBed { get; protected set; }

        public static object ScriptResults { get; protected set; }

        public SolidColorBrush FillColor
        {
            get
            {
                return _fillBrushColor;
            }
            set
            {
                if (value != _fillBrushColor)
                {
                    _fillBrushColor = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FillColour", _fillBrushColor);
                }
            }
        }

        public String SelectedFont
        {
            get
            {
                return selectedFont;
            }

            set
            {
                if (selectedFont != value)
                {
                    selectedFont = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FontName", selectedFont);
                }
            }
        }

        public String SoftwareVersion
        {
            get
            {
                if (softwareVersion == null || softwareVersion == "")
                {
                    softwareVersion = FindSoftwareVersion();
                }

                return softwareVersion;
            }
        }

        public SolidColorBrush StrokeColor
        {
            get
            {
                return _strokeBrushColor;
            }
            set
            {
                if (value != _strokeBrushColor)
                {
                    _strokeBrushColor = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("StrokeColour", _strokeBrushColor);
                }
            }
        }

        public Viewport3D ViewPort { get; set; }

        public void CheckPoint()
        {
            lastChangeWasNudge = false;
            if (Document != null)
            {
                DateTime start = DateTime.Now;
                string s = undoer.GetNextCheckPointName(Document.DocumentId.ToString());
                Document.Save(s);
                DateTime end = DateTime.Now;
                TimeSpan ts = end - start;
                System.Diagnostics.Debug.WriteLine($"CHeckpoint took {ts.TotalMilliseconds} ms");
            }
        }

        public void CheckPointForNudge()
        {
            if (!lastChangeWasNudge)
            {
                CheckPoint();
            }
            lastChangeWasNudge = true;
        }

        public string GetPartsLibraryPath()
        {
            String pth = Properties.Settings.Default.PartLibraryPath;
            if (pth == "")
            {
                pth = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                pth += "//Barnacle//Library";
            }
            return pth;
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Undo()
        {
            if (undoer.CanUndo(Document.DocumentId.ToString()))
            {
                string s = undoer.GetLastCheckPointName(Document.DocumentId.ToString());
                Document.Load(s);
                NotificationManager.Notify("Refresh", null);
            }
        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Caption")
            {
                NotifyPropertyChanged("Caption");
            }
        }

        private string FindSoftwareVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version}";
        }
    }
}