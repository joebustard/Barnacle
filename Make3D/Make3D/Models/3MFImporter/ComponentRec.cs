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
using System.Xml;

namespace Barnacle.Dialogs
{
    internal class ComponentRec : ObjectRec
    {
        public ComponentRec()
        {
            ComponentPath = "";
            SrcId = -1;
            Transform = "";
        }

        public String ComponentPath
        {
            get;
            set;
        }

        public int SrcId
        {
            get; set;
        }

        public string Transform
        {
            get; set;
        }

        internal override void LoadFromNode(XmlNode nd)
        {
            SrcId = -1;
            foreach (XmlAttribute attr in nd.Attributes)
            {
                if (attr.Name.ToLower().Contains("path"))
                {
                    ComponentPath = attr.Value;
                }
                if (attr.Name.ToLower() == "objectid")
                {
                    SrcId = Convert.ToInt32(attr.Value);
                }
                if (attr.Name.ToLower() == "transform")
                {
                    Transform = attr.Value;
                }
                if (attr.Name.ToLower() == "id")
                {
                    Id = Convert.ToInt32(attr.Value);
                }
            }
        }
    }
}