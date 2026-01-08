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
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Senilias.TargetTypes
{
    public class FileDependency : TargetDependency
    {
        public FileDependency()
        {
        }

        public FileDependency(string s, string t)
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
            if (!string.IsNullOrEmpty(s))
            {
                if (File.Exists(s))
                {
                    // if src exists but target doesn't then we are triggered
                    if (!File.Exists(t))
                    {
                        Triggered = true;
                    }
                    else
                    {
                        // Both exist, so check dates
                        DateTime srcStamp = File.GetLastWriteTime(s);
                        DateTime dstStamp = File.GetLastWriteTime(t);
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
            XmlNode stnode = ele.SelectSingleNode("Step");
            if (stnode != null)
            {
                XmlElement actEle = stnode as XmlElement;
                if (actEle != null)
                {
                    string ty = actEle.GetAttribute("Type");
                    switch (ty.ToLower())
                    {
                        case "script":
                            {
                                ScriptStep step = new ScriptStep();
                                step.Load(actEle);
                                Action = step;
                            }
                            break;

                        case "nop":
                            {
                                NopStep step = new NopStep();
                                Action = step;
                            }
                            break;

                        case "touch":
                            {
                                TouchStep step = new TouchStep();
                                step.Load(actEle);
                                Action = step;
                            }
                            break;
                    }
                }
            }
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            if (!String.IsNullOrEmpty(Source) || !String.IsNullOrEmpty(Target))
            {
                XmlElement ele = doc.CreateElement("Dep");
                ele.SetAttribute("Type", "File");
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

        internal override void Dump()
        {
            if (Predecessor != null)
            {
                Predecessor.Dump();
            }

            System.Diagnostics.Debug.WriteLine($"File {Source} -> {Target}");
        }
    }
}