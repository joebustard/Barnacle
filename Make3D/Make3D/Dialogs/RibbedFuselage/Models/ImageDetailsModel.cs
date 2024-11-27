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

using Barnacle.RibbedFuselage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Barnacle.RibbedFuselage
{
    public class ImageDetailsModel : INotifyPropertyChanged
    {
        private string displayFileName;
        private string flexiPathText;

        private string imageFilePath;

        private string name;

        private Visibility noPathVisibility;

        public ImageDetailsModel()
        {
            imageFilePath = "";
            name = "";
            flexiPathText = "";
            DisplayFileName = "None";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Dirty
        {
            get; set;
        }

        public String DisplayFileName
        {
            get
            {
                return displayFileName;
            }

            set
            {
                if (value != displayFileName)
                {
                    displayFileName = value;
                    NotifyPropertyChanged();
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// The text that defines the flexipath imposed over the image
        /// </summary>
        public String FlexiPathText
        {
            get
            {
                return flexiPathText;
            }

            set
            {
                if (value != flexiPathText)
                {
                    flexiPathText = value;
                    if (String.IsNullOrEmpty(flexiPathText))
                    {
                        NoPathVisibility = Visibility.Visible;
                    }
                    else
                    {
                        NoPathVisibility = Visibility.Hidden;
                    }
                    NotifyPropertyChanged();
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// The path to the image
        /// </summary>
        public String ImageFilePath
        {
            get
            {
                return imageFilePath;
            }

            set
            {
                if (value != imageFilePath)
                {
                    imageFilePath = value;
                    NotifyPropertyChanged();
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// The name to be displayed.
        /// </summary>
        public String Name
        {
            get
            {
                return name;
            }

            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged();
                    Dirty = true;
                }
            }
        }

        public Visibility NoPathVisibility
        {
            get
            {
                return noPathVisibility;
            }

            set
            {
                if (noPathVisibility != value)
                {
                    noPathVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual void Load(XmlElement ele)
        {
            if (ele != null)
            {
                if (ele.HasAttribute("Name"))
                {
                    Name = ele.GetAttribute("Name");
                }
                if (ele.HasAttribute("ImageFilePath"))
                {
                    ImageFilePath = ele.GetAttribute("ImageFilePath");
                }
                if (!String.IsNullOrEmpty(ImageFilePath))
                {
                    DisplayFileName = System.IO.Path.GetFileName(ImageFilePath);
                }
                if (ele.HasAttribute("FlexiPathText"))
                {
                    FlexiPathText = ele.GetAttribute("FlexiPathText");
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void Save(XmlElement ele, XmlDocument doc)
        {
            ele.SetAttribute("Name", Name);
            ele.SetAttribute("ImageFilePath", ImageFilePath);
            ele.SetAttribute("FlexiPathText", FlexiPathText);
        }
    }
}