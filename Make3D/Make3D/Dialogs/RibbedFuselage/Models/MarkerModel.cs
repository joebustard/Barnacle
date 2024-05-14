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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Barnacle.Dialogs.RibbedFuselage.Models
{
    internal class MarkerModel
    {
        public String Name
        {
            get;
            set;
        }

        public double Position
        {
            get;
            set;
        }

        public double LowValue
        {
            get;
            set;
        }

        public String HighValue
        {
            get;
            set;
        }

        public RibImageDetailsModel AssociatedRib
        {
            get;
            set;
        }

        public void Load(XmlElement ele)
        {
            if (ele.HasAttribute("Name"))
            {
                Name = ele.GetAttribute("Name");
            }
            if (ele.HasAttribute("Position"))
            {
                string v = ele.GetAttribute("Position");
                Position = Convert.ToDouble(v);
            }
        }

        public void Save(XmlElement ele, XmlDocument doc)
        {
            ele.SetAttribute("Name", Name);
            ele.SetAttribute("Position", Position.ToString());
        }
    }
}