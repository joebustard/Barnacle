﻿// **************************************************************************
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using TemplateLib;

namespace Barnacle.ViewModels
{
    internal class NewProjectViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private Dictionary<string, string> descriptions;
        private bool okEnabled;
        private string projectName;
        private string projectRoot;
        private string projPath;
        private string selectedDescription;
        private string selectedTemplate;
        private List<string> templateNames;

        private ProjectTemplator templator;

        public NewProjectViewModel()
        {
            BackCommand = new RelayCommand(OnBack);
            CreateCommand = new RelayCommand(OnCreate);
            projectRoot = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            projectRoot += "\\Barnacle";
            descriptions = new Dictionary<string, string>();
            templator = new ProjectTemplator();
            templator.TemplateDefinitionPath = AppDomain.CurrentDomain.BaseDirectory + "templates";
            templator.ScanForTemplates();
            templateNames = new List<string>();
            for (int i = 0; i < templator.NumberOfTemplates(); i++)
            {
                string n = string.Empty;
                string d = string.Empty;
                templator.GetTemplateDetails(i, ref n, ref d);
                templateNames.Add(n);
                descriptions[n] = d;
            }
            NotifyPropertyChanged("TemplateNames");
            if (templateNames.Count > 0)
            {
                SelectedTemplate = templateNames[0];
            }
            OKEnabled = false;
        }

        public ICommand BackCommand { get; set; }

        public ICommand CreateCommand { get; set; }

        public bool OKEnabled
        {
            get
            {
                return okEnabled;
            }
            set
            {
                if (okEnabled != value)
                {
                    okEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProjectName
        {
            get
            {
                return projectName;
            }
            set
            {
                if (projectName != value)
                {
                    projectName = value;

                    NotifyPropertyChanged();
                    string name = projectName;
                    if (!name.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        name = System.IO.Path.DirectorySeparatorChar + name;
                    }
                    projPath = projectRoot + name;
                    char[] illegal = System.IO.Path.GetInvalidPathChars();
                    bool ok = true;
                    if (projPath.IndexOfAny(illegal) > -1)
                    {
                        ok = false;
                    }
                    illegal = new char[]
                    {
            '$',
            '@',
            '(',
            ')',
            '!',
            '£',
            '$',
            '%',
            '^',
            '&',
            '+',
            '=',
            '[',
            ']',
            '#',
            '~'
                    };

                    if (projPath.IndexOfAny(illegal) > -1)
                    {
                        ok = false;
                    }

                    // can't rcrrd max len
                    if (projPath.Length > 260)
                    {
                        ok = false;
                    }

                    // Does the projectfile already exist
                    if (File.Exists(projPath + name + ".bmf"))
                    {
                        ok = false;
                    }

                    OKEnabled = ok;
                }
            }
        }

        public string SelectedDescription
        {
            get
            {
                return selectedDescription;
            }
            set
            {
                if (selectedDescription != value)
                {
                    selectedDescription = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SelectedTemplate
        {
            get
            {
                return selectedTemplate;
            }
            set
            {
                if (selectedTemplate != value)
                {
                    selectedTemplate = value;
                    NotifyPropertyChanged();
                    UpdateDescription();
                }
            }
        }

        public List<string> TemplateNames
        {
            get
            {
                return templateNames;
            }
            set
            {
                if (templateNames != value)
                {
                    templateNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OnBack(object obj)
        {
            NotificationManager.Notify("NewProjectBack", null);
        }

        private void OnCreate(object obj)
        {
            if (templator.ProcessTemplate(projectName, projPath, selectedTemplate))
            {
                projPath = templator.SolutionPath;
            }
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProject", projPath);
        }

        private void UpdateDescription()
        {
            SelectedDescription = "";
            if (selectedTemplate != null && selectedTemplate != "")
            {
                try
                {
                    SelectedDescription = descriptions[selectedTemplate];
                }
                catch
                {
                }
            }
        }
    }
}