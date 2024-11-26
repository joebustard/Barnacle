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
using System.IO;
using System.Xml;

namespace Barnacle.EditorParameterLib
{
    public class EditorParameter
    {
        public EditorParameter(String n, string v)
        {
            Name = n;
            Value = v;
        }

        public string Name
        {
            get; set;
        }

        public string Value
        {
            get; set;
        }

        internal void ReadBinary(BinaryReader reader)
        {
            Name = reader.ReadString();
            Value = reader.ReadString();
        }

        internal void Write(XmlDocument doc, XmlElement prms)
        {
            XmlElement p = doc.CreateElement("P");
            p.SetAttribute("Name", Name);
            p.SetAttribute("Val", Value);
            prms.AppendChild(p);
        }

        internal void WriteBinary(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }
    }
}