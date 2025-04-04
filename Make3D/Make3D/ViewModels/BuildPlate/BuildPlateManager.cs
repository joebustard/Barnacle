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
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml;

namespace Barnacle.ViewModel.BuildPlates
{
    internal class BuildPlateManager : INotifyPropertyChanged
    {
        private List<BuildPlate> buildPlates;

        public BuildPlateManager()
        {
            buildPlates = new List<BuildPlate>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<BuildPlate> BuildPlates
        {
            get
            {
                return buildPlates;
            }
            set
            {
                if (value != buildPlates)
                {
                    buildPlates = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void Load(string file)
        {
            buildPlates.Clear();
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(file);
            XmlNode docNode = doc.SelectSingleNode("PlateDefinitions");
            if (docNode != null)
            {
                XmlNodeList nodes = docNode.SelectNodes("BuildPlate");
                foreach (XmlNode nd in nodes)
                {
                    BuildPlate bp = new BuildPlate();
                    XmlElement ele = nd as XmlElement;
                    bp.Read(doc, ele);
                    buildPlates.Add(bp);
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

        public void Save(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("PlateDefinitions");
            foreach (BuildPlate rf in buildPlates)
            {
                rf.Write(doc, docNode);
            }
            doc.AppendChild(docNode);
            doc.Save(file);
        }

        public void SetDefaults()
        {
            buildPlates.Add(new BuildPlate()
            {
                PrinterName = "Ender 3",
                Width = 200,
                Height = 200,
                BorderColour = Colors.CadetBlue,
                BorderThickness = 5
            });

            buildPlates.Add(new BuildPlate()
            {
                PrinterName = "Elegoo Mars 2 Pro",
                Width = 130,
                Height = 82,
                BorderColour = Colors.CadetBlue,
                BorderThickness = 5
            });
        }

        internal BuildPlate FindBuildPlate(string name)
        {
            BuildPlate res = null;
            foreach (BuildPlate bp in buildPlates)
            {
                if (bp.PrinterName == name)
                {
                    res = bp;
                    break;
                }
            }
            return res;
        }

        internal void Initialise()
        {
            String dataPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\BuildPlates.xml";
            if (File.Exists(dataPath))
            {
                Load(dataPath);
            }
            else
            {
                SetDefaults();
            }
        }
    }
}