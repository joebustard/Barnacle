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
            String appPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\Links\\";
            string[] files = Directory.GetFiles(appPath, "*.*");
            foreach (string fn in files)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.XmlResolver = null; // security
                    doc.Load(fn);
                    XmlNode allLinks = doc.SelectSingleNode("Links");
                    if (allLinks != null)
                    {
                        XmlNodeList xl = allLinks.SelectNodes("Link");
                        foreach (XmlNode nd in xl)
                        {
                            XmlElement ele = nd as XmlElement;
                            if (ele.HasAttribute("Name"))
                            {
                                String lname = ele.GetAttribute("Name");
                                Link link = new Link();
                                link.Name = lname;
                                XmlNodeList parts = ele.SelectNodes("Part");
                                foreach (XmlNode partnd in parts)
                                {
                                    XmlElement partel = partnd as XmlElement;
                                    if (partel.HasAttribute("Name"))
                                    {
                                        String pname = partel.GetAttribute("Name");
                                        if (partel.HasAttribute("X"))
                                        {
                                            String px = partel.GetAttribute("X");
                                            if (partel.HasAttribute("Y"))
                                            {
                                                String py = partel.GetAttribute("Y");
                                                if (partel.HasAttribute("Z"))
                                                {
                                                    String pz = partel.GetAttribute("Z");
                                                    if (partel.HasAttribute("W"))
                                                    {
                                                        String pw = partel.GetAttribute("W");
                                                        string text = partel.InnerText;
                                                        text = text.Replace("\n", " ");
                                                        text = text.Replace("\r", " ");
                                                        text = text.Replace("  ", " ");
                                                        try
                                                        {
                                                            LinkPart lp = new LinkPart();
                                                            lp.Name = pname;
                                                            lp.X = Convert.ToDouble(px);
                                                            lp.Y = Convert.ToDouble(py);
                                                            lp.Z = Convert.ToDouble(pz);
                                                            lp.W = Convert.ToDouble(pw);
                                                            lp.PathText = text;
                                                            link.Add(lp);
                                                        }
                                                        catch
                                                        {
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                Links.Add(link);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public List<Link> Links { get; set; }
    }
}