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

namespace Senilias.TargetTypes
{
    public class FolderDependency : TargetDependency
    {
        public FolderDependency()
        {
        }

        public FolderDependency(string s, string t)
        {
            Triggered = false;
            Source = s;
            Target = t;
        }

        public override bool CheckTriggered()
        {
            Triggered = false;
            string s = RelativeToProject(Source);
            string t = RelativeToProject(Target);
            if (string.IsNullOrEmpty(s))
            {
                if (Directory.Exists(s))
                {
                    // if src exists but target doesn't then we are triggered
                    if (!Directory.Exists(t))
                    {
                        Triggered = true;
                    }
                    else
                    {
                        // Both exist, so check dates
                        DateTime srcStamp = Directory.GetLastWriteTime(s);
                        DateTime dstStamp = Directory.GetLastWriteTime(t);
                        if (srcStamp > dstStamp)
                        {
                            Triggered = true;
                        }
                    }
                }
            }
            if (Predecessor != null)
            {
                Triggered |= Predecessor.CheckTriggered();
            }
            return Triggered;
        }

        public override void Load(XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            XmlNode snode = ele.SelectSingleNode("Source");
            if (snode != null)
            {
                Source = snode.InnerText.Trim();
            }
            XmlNode tnode = ele.SelectSingleNode("Target");
            if (tnode != null)
            {
                Target = tnode.InnerText.Trim();
            }
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            if (!String.IsNullOrEmpty(Source) || !String.IsNullOrEmpty(Target))
            {
                XmlElement ele = doc.CreateElement("Dep");
                ele.SetAttribute("Type", "Folder");
                node.AppendChild(ele);
                AddSource(doc, ele);
                AddTarget(doc, ele);
                if (Action != null)
                {
                    Action.Save(doc, ele);
                }
                if (Predecessor != null)
                {
                    Predecessor.Save(doc, ele);
                }
            }
            else
            {
                if (Predecessor != null)
                {
                    Predecessor.Save(doc, node);
                }
            }
        }

        public override void ToDo(List<string> res)
        {
            if (Triggered)
            {
                if (Predecessor != null)
                {
                    Predecessor.ToDo(res);
                }
                if (Action != null)
                {
                    Action.ToDo(res);
                }
            }
        }

        internal override TargetDependency Clone()
        {
            FileDependency res = new FileDependency(Source, Target);
            res.Action = Action.Clone();
            if (Predecessor != null)
            {
                res.Predecessor = Predecessor.Clone();
            }
            return res;
        }
    }
}