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

using SeniliasLib.Utils;
using System.Xml;

namespace Senilias.StepTypes
{
    public class ScriptStep : StepType
    {
        public ScriptStep()
        {
            ScriptName = "";
            AbsoluteScriptName = ScriptName;
        }

        public ScriptStep(string n)
        {
            ScriptName = n;
        }

        public String AbsoluteScriptName
        {
            get; set;
        }

        public String ScriptName
        {
            get; set;
        }

        public override bool Compare(StepType other)
        {
            bool res = false;
            if (other != null)
            {
                if (other is ScriptStep)
                {
                    if ((other as ScriptStep).AbsoluteScriptName == AbsoluteScriptName)
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        public override bool Execute()
        {
            string script = RelativeToProject(ScriptName);
            return true;
        }

        public override void Load(XmlElement ele)
        {
            ScriptName = ele.InnerText.Trim();
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement xmlElement = doc.CreateElement("Step");
            xmlElement.SetAttribute("Type", "Script");
            xmlElement.InnerText = ScriptName;
            node.AppendChild(xmlElement);
        }

        public override void ToDo(List<string> res)
        {
            string s = "Run " + ScriptName;
            if (!res.Contains(s))
            {
                res.Add(s);
            }
        }

        public override string ToString()
        {
            return "Run " + ScriptName;
        }

        internal override StepType Clone()
        {
            ScriptStep res = new ScriptStep();
            res.ScriptName = ScriptName;
            res.AbsoluteScriptName = AbsoluteScriptName;
            return res;
        }

        internal override void MakePathsAbsolute()
        {
            AbsoluteScriptName = Utils.RelativeToProject(ScriptName);
        }

        internal override void ReplaceWildcard(string sub)
        {
            ScriptName = ScriptName.Replace("*", sub);
        }
    }
}