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

using Senilias.StepTypes;
using SeniliasLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Senilias.StepTypes
{
    internal class TouchStep : StepType
    {
        public TouchStep()
        {
            TargetToTouch = "";
            AbsoluteTargetToTouch = TargetToTouch;
        }

        public TouchStep(string n)
        {
            TargetToTouch = n;
        }

        public String AbsoluteTargetToTouch
        {
            get; set;
        }

        public String TargetToTouch
        {
            get; set;
        }

        public override bool Compare(StepType other)
        {
            bool res = false;
            if (other != null)
            {
                if (other is TouchStep)
                {
                    if ((other as TouchStep).TargetToTouch == TargetToTouch)
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        public override bool Execute()
        {
            string script = RelativeToProject(TargetToTouch);
            return true;
        }

        public override void Load(XmlElement ele)
        {
            TargetToTouch = ele.InnerText.Trim();
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement xmlElement = doc.CreateElement("Step");
            xmlElement.SetAttribute("Type", "Touch");
            xmlElement.InnerText = TargetToTouch;
            node.AppendChild(xmlElement);
        }

        public override void ToDo(List<string> res)
        {
            string s = "Touch " + TargetToTouch;
            if (!res.Contains(s))
            {
                res.Add(s);
            }
        }

        public override string ToString()
        {
            return "Touch " + TargetToTouch;
        }

        internal override StepType Clone()
        {
            TouchStep res = new TouchStep();
            res.TargetToTouch = TargetToTouch;
            res.AbsoluteTargetToTouch = AbsoluteTargetToTouch;
            return res;
        }

        internal override void MakePathsAbsolute()
        {
            AbsoluteTargetToTouch = Utils.RelativeToProject(TargetToTouch);
        }

        internal override void ReplaceWildcard(string sub)
        {
            TargetToTouch = TargetToTouch.Replace("*", sub);
        }
    }
}