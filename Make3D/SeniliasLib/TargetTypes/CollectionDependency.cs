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
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Senilias.StepTypes;
using SeniliasLib.Utils;

namespace Senilias.TargetTypes
{
    public class CollectionDependency : TargetDependency
    {
        private List<TargetDependency> children;

        public CollectionDependency()
        {
            children = new List<TargetDependency>();
            Triggered = false;
        }

        public CollectionDependency(string source, string target)
        {
            Source = source;
            Target = target;
        }

        public override bool CheckTriggered()
        {
            Triggered = false;
            foreach (TargetDependency dep in children)
            {
                Triggered |= dep.CheckTriggered();
            }
            if (Predecessor != null)
            {
                Triggered |= Predecessor.CheckTriggered();
            }
            return Triggered;
        }

        public void Clear()
        {
            children.Clear();
        }

        public override void ForceTriggers()
        {
            Triggered = true;
            foreach (TargetDependency dep in children)
            {
                dep.ForceTriggers();
            }
            if (Predecessor != null)
            {
                Predecessor.ForceTriggers();
            }
        }

        public override void Load(XmlNode node)
        {
            XmlNodeList nds = node.ChildNodes;
            foreach (XmlNode nd in nds)
            {
                switch (nd.Name.ToLower())
                {
                    case "target":
                        {
                            Target = nd.InnerText.Trim();
                        }
                        break;

                    case "dep":
                        {
                            XmlElement ele = nd as XmlElement;
                            string t = ele.GetAttribute("Type");
                            switch (t.ToLower())
                            {
                                case "folder":
                                    {
                                        FolderDependency fld = new FolderDependency();
                                        fld.Load(ele);
                                        children.Add(fld);
                                    }
                                    break;

                                case "file":
                                    {
                                        FileDependency fld = new FileDependency();
                                        fld.Load(ele);
                                        children.Add(fld);
                                    }
                                    break;

                                case "deps":
                                    {
                                        CollectionDependency dep = new CollectionDependency();
                                        dep.Load(node);
                                        children.Add(dep);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public void MergeChildren(CollectionDependency src)
        {
            foreach (TargetDependency dep in src.children)
            {
                children.Add(dep);
            }
            src.children.Clear();
        }

        public override void Save(XmlDocument doc, XmlNode node)
        {
            XmlElement xmlElement = doc.CreateElement("Deps");
            if (node != null)
            {
                node.AppendChild(xmlElement);
            }
            else
            {
                doc.AppendChild(xmlElement as XmlNode);
            }
            AddTarget(doc, xmlElement);
            if (Action != null)
            {
                Action.Save(doc, xmlElement);
            }

            foreach (TargetDependency dep in children)
            {
                dep.Save(doc, xmlElement);
            }
        }

        public override void SetLevel(int l)
        {
            level = l;
            foreach (TargetDependency dep in children)
            {
                dep.SetLevel(l + 1);
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
                foreach (TargetDependency dep in children)
                {
                    if (dep.Triggered)
                    {
                        dep.ToDo(res);
                    }
                }
                if (Action != null)
                {
                    Action.ToDo(res);
                }
            }
        }

        internal void Add(TargetDependency dep)
        {
            if (children != null)
            {
                children.Add(dep);
            }
        }

        internal override TargetDependency Clone()
        {
            CollectionDependency res = new CollectionDependency(Source, Target);
            res.Action = Action.Clone();
            if (Predecessor != null)
            {
                res.Predecessor = Predecessor.Clone();
            }
            res.children = new List<TargetDependency>();
            foreach (TargetDependency dep in children)
            {
                res.children.Add(dep.Clone());
            }
            return res;
        }

        internal override void Dump()
        {
            if (Predecessor != null)
            {
                Predecessor.Dump();
            }

            System.Diagnostics.Debug.WriteLine($"Collection {Source} -> {Target}");
            foreach (var v in children)
            {
                v.Dump();
            }
        }

        internal override bool IsPredecessor(TargetDependency dep2)
        {
            bool res = false;

            if (Predecessor != null)
            {
                res = Predecessor.IsPredecessor(dep2);
            }
            if (res == false)
            {
                foreach (var v in children)
                {
                    res = res | v.IsPredecessor(dep2);
                }
            }
            return res;
        }
    }
}