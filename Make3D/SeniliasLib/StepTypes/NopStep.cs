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

using System.Xml;

namespace Senilias.StepTypes
{
    public class NopStep : StepType
    {
        public override bool Compare(StepType other)
        {
            bool res = false;
            if (other != null)
            {
                if (other is NopStep)
                {
                    res = true;
                }
            }
            return res;
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement xmlElement = doc.CreateElement("Step");
            xmlElement.SetAttribute("Type", "Nop");
            node.AppendChild(xmlElement);
        }

        public override string ToString()
        {
            return "Nop";
        }

        internal override NopStep Clone()
        {
            NopStep res = new NopStep();

            return res;
        }
    }
}