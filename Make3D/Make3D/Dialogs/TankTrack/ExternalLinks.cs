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
using Barnacle.Object3DLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;

namespace Barnacle.Dialogs
{
    internal class ExternalLinks
    {
        public ExternalLinks()
        {
            Links = new List<Link>();
            AddNewLinks(AppDomain.CurrentDomain.BaseDirectory + "Data\\TankTrackLinks\\");
            String userPath = Properties.Settings.Default.UserTankTrackLinks;
            if (!String.IsNullOrEmpty(userPath))
            {
                if (Directory.Exists(userPath))
                {
                    AddNewLinks(userPath);
                }
            }
        }

        public List<Link> Links
        {
            get; set;
        }

        private void AddNewLinks(String folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, "*.txt");
            foreach (string fn in files)
            {
                try
                {
                    Document doc = new Document();
                    doc.Load(fn);
                    foreach (Object3D ob in doc.Content)
                    {
                        if (!FoundLink(ob.Name) && ob.Exportable)
                        {
                            Link lnk = new Link();
                            lnk.Name = ob.Name;
                            lnk.SourceModel = ob;
                            ob.Remesh();
                            ob.CalcScale();
                            ob.ScaleMesh(1 / ob.Scale.X, 1 / ob.Scale.Y, 1 / ob.Scale.Z);
                            ob.CalcScale();
                            Links.Add(lnk);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private bool FoundLink(string name)
        {
            bool res = false;
            foreach (Link lnk in Links)
            {
                if (lnk.Name == name)
                {
                    res = true;
                    break;
                }
            }
            return res;
        }
    }
}