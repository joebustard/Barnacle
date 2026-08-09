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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Barnacle.Dialogs
{
    internal class ObjectRec
    {
        public ObjectRec()
        {
            Id = -1;
            SrcPath = "";
            Vertices = new Point3DCollection();
            Faces = new Int32Collection();
        }

        public Int32Collection Faces
        {
            get; set;
        }

        public int Id
        {
            get;
            set;
        }

        public String SrcPath
        {
            get;
            set;
        }

        public Point3DCollection Vertices
        {
            get; set;
        }

        internal void LoadFromFile(string tfn)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(tfn);

            XmlNode objectNodes = doc.ChildNodes[0];
            if (objectNodes != null)
            {
                XmlNode modelNode = objectNodes.NextSibling;
                if (modelNode != null)
                {
                    foreach (XmlNode nd in modelNode.ChildNodes)
                    {
                        if (nd.Name.ToLower() == "resources")
                        {
                            foreach (XmlNode nd2 in nd.ChildNodes)
                            {
                                if (nd2.Name == "object")
                                {
                                    XmlElement ele = nd2 as XmlElement;
                                    if (ele.HasAttribute("id"))
                                    {
                                        if (Id == Convert.ToInt16(ele.GetAttribute("id")))
                                        {
                                            foreach (XmlNode nd3 in nd2.ChildNodes)
                                            {
                                                if (nd3.Name == "mesh")
                                                {
                                                    // must load using the base
                                                    LoadFromNode(nd3);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal virtual void LoadFromNode(XmlNode nd)
        {
            foreach (XmlNode verticesNode in nd.ChildNodes)
            {
                if (verticesNode.Name.ToLower() == "vertices")
                {
                    foreach (XmlNode cn in verticesNode.ChildNodes)
                    {
                        if (cn.Name.ToLower() == "vertex")
                        {
                            string sx = (cn as XmlElement).GetAttribute("x");
                            string sy = (cn as XmlElement).GetAttribute("y");
                            string sz = (cn as XmlElement).GetAttribute("z");

                            Point3D p = new Point3D(
                             Convert.ToDouble(sx),
                                Convert.ToDouble(sy),
                                Convert.ToDouble(sz)
                            );
                            Vertices.Add(p);
                        }
                    }
                }
                if (verticesNode.Name.ToLower() == "triangles")
                {
                    foreach (XmlNode cn in verticesNode.ChildNodes)
                    {
                        if (cn.Name.ToLower() == "triangle")
                        {
                            string v1 = (cn as XmlElement).GetAttribute("v1");
                            string v2 = (cn as XmlElement).GetAttribute("v2");
                            string v3 = (cn as XmlElement).GetAttribute("v3");
                            Faces.Add(Convert.ToInt32(v1));
                            Faces.Add(Convert.ToInt32(v2));
                            Faces.Add(Convert.ToInt32(v3));
                        }
                    }
                }
            }
        }
    }
}