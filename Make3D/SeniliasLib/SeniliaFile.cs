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
using Senilias.TargetTypes;
using System.Windows;
using System.Xml;

namespace SeniliasLib
{
    public class SeniliaFile
    {
        private string projectName;
        private List<TargetDependency> targets;

        private string template = @"<?xml version=""1.0"" encoding=""UTF-8""?>
!CONTENT!";

        private CollectionDependency tree;

        public SeniliaFile()
        {
            // the raw target dependencies supplied by the user
            targets = new List<TargetDependency>();

            // the tree used for building
            tree = new CollectionDependency();
            tree.Target = "All";
        }

        public bool Dirty
        {
            get;
            set;
        }

        public List<TargetDependency> Targets
        {
            get
            {
                return targets;
            }
            set
            {
                targets = value;
            }
        }

        public void Add(TargetDependency.TypeOfDependency depType, string source, string target, StepType.TypeOfStep atype, string actionParam)
        {
            StepType? act = MakeStep(atype, actionParam);
            TargetDependency? dep = MakeDependency(depType, source, target);
            if (dep != null)
            {
                dep.Action = act;
                targets.Add(dep);
            }
            Dirty = true;
        }

        public bool CheckTriggers()
        {
            bool res = false;
            if (tree != null)
            {
                res = tree.CheckTriggered();
            }
            return res;
        }

        public void CreateAcyclicDirectedGraph()
        {
            tree.Clear();
            // clone the targetlist just so the original isn't messed about with
            List<TargetDependency> clones = CloneTargets(targets);

            // expand any wildcard file dependencies
            ExpandWildcards(clones);
            MakePathsAbsolute(clones);

            bool changed = true;
            // if multiple sources affect the same target merge them into a collection
            // Start by sorting the list so that identical targets are nect to each other
            SortByTargets(clones);
            while (changed)
            {
                changed = false;
                for (int i = 0; i < clones.Count && changed == false; i++)
                {
                    for (int j = 0; j < clones.Count && changed == false; j++)
                    {
                        if (i != j)
                        {
                            if (clones[i].Target.ToLower() == clones[j].Target.ToLower())
                            {
                                if (ActionsMatch(clones[i].Action, clones[j].Action))
                                {
                                    clones[i] = MergeTargets(clones[i], clones[j]);

                                    clones.RemoveAt(j);
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
            // there may be different steps with the same source but different targets
            // if one of those records  has a source with a predecessor but the others dont
            // attach to the same predecessor

            // order the predecessors
            changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < clones.Count && changed == false; i++)
                {
                    string itarg = clones[i].RelativeToProject(clones[i].Target).ToLower();
                    for (int j = 0; j < clones.Count && changed == false; j++)
                    {
                        if (i != j)
                        {
                            changed = clones[j].IsPredecessor(clones[i]);
                            if (changed)
                            {
                                clones.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < clones.Count && changed == false; i++)
                {
                    for (int j = 0; j < clones.Count && changed == false; j++)
                    {
                        if (i != j)
                        {
                            if (clones[i].Source == clones[j].Source)
                            {
                                if (clones[i].Predecessor != null && clones[j].Predecessor == null)
                                {
                                    clones[j].Predecessor = clones[i].Predecessor;
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }

            // add the final set to the tree
            foreach (var clone in clones)
            {
                tree.Add(clone);
            }
            DumpDependencies(clones);
        }

        public void DeselectAll()
        {
            foreach (TargetDependency target in targets)
            {
                target.Selected = false;
            }
        }

        public void ForceTriggers()
        {
            tree.ForceTriggers();
        }

        public string GetProjectName()
        {
            return projectName;
        }

        public string GetProjectPath()
        {
            return Utils.Utils.ProjectPath;
        }

        public bool LoadFile(string path)
        {
            bool res = false;
            targets.Clear();
            if (File.Exists(path))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode? node = doc.FirstChild;
                node = node?.NextSibling;
                if (node?.Name == "Deps")
                {
                    XmlElement el = (XmlElement)node;
                    if (el.HasAttribute("ProjectFolder"))
                    {
                        String pf = el.GetAttribute("ProjectFolder");
                        SetProjectPath(pf);
                    }

                    if (el.HasAttribute("ProjectName"))
                    {
                        String pf = el.GetAttribute("ProjectName");
                        SetProjectName(pf);
                    }

                    XmlNodeList nds = node.ChildNodes;
                    foreach (XmlNode nd in nds)
                    {
                        switch (nd.Name.ToLower())
                        {
                            case "dep":
                                {
                                    XmlElement? ele = nd as XmlElement;
                                    string t = ele.GetAttribute("Type");
                                    switch (t.ToLower())
                                    {
                                        case "folder":
                                            {
                                                FolderDependency fld = new();
                                                fld.Load(ele);
                                                targets.Add(fld);
                                            }
                                            break;

                                        case "file":
                                            {
                                                FileDependency fld = new FileDependency();
                                                fld.Load(ele);
                                                targets.Add(fld);
                                            }
                                            break;

                                        case "deps":
                                            {
                                                CollectionDependency dep = new CollectionDependency();
                                                dep.Load(node);
                                                targets.Add(dep);
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
                Dirty = false;
            }
            return res;
        }

        public void RemoveAt(int index)
        {
            targets.RemoveAt(index);
        }

        public bool SaveFile(string path)
        {
            bool res = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);
                XmlElement xmlElement = doc.CreateElement("Deps");
                xmlElement.SetAttribute("ProjectFolder", GetProjectPath());
                xmlElement.SetAttribute("ProjectName", projectName);
                doc.AppendChild(xmlElement as XmlNode);
                foreach (TargetDependency t in targets)
                {
                    t.Save(doc, xmlElement);
                }
                doc.Save(path);
                res = true;
                Dirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Diagnostic function to make it easier to check the graph
        /// .It would NOT normally be saved
        /// </summary>
        /// <param name="path"></param>
        public void SaveGraph(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);
                XmlElement xmlElement = doc.CreateElement("Deps");
                doc.AppendChild(xmlElement as XmlNode);
                tree.Save(doc, xmlElement);
                doc.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetProjectName(string v)
        {
            projectName = v;
        }

        public void SetProjectPath(string v)
        {
            Utils.Utils.ProjectPath = v;
        }

        public List<string> ToDo()
        {
            List<string> res = new List<string>();
            tree.ToDo(res);
            return res;
        }

        private static void SortByTargets(List<TargetDependency> clones)
        {
            if (clones.Count > 1)
            {
                bool changed;
                do
                {
                    changed = false;
                    for (int i = 0; i < clones.Count - 1; i++)
                    {
                        if (String.Compare(clones[i].Target.ToLower(), clones[i + 1].Target.ToLower()) > 0)
                        {
                            TargetDependency t = clones[i];
                            clones[i] = clones[i + 1];
                            clones[i + 1] = t;
                            changed = true;
                        }
                    }
                } while (changed);
            }
        }

        private bool ActionsMatch(StepType action1, StepType action2)
        {
            bool res = false;
            if (action1.GetType() == action2.GetType())
            {
                res = action1.Compare(action2);
            }
            return res;
        }

        private List<TargetDependency> CloneTargets(List<TargetDependency> targets)
        {
            List<TargetDependency> res = new List<TargetDependency>();
            foreach (TargetDependency target in targets)
            {
                if (target != null)
                {
                    // does the source contain more than one name
                    if (target.Source.Contains(","))
                    {
                        string[] srcs = target.Source.Split(",");
                        foreach (string src in srcs)
                        {
                            string str = src.Replace("\r\n", "").Trim();

                            TargetDependency t = target.Clone();
                            t.Source = str;
                            res.Add(t);
                        }
                    }
                    else
                    {
                        res.Add(target.Clone());
                    }
                }
            }
            return res;
        }

        private void DumpDependencies(List<TargetDependency> clones)
        {
            foreach (var v in clones)
            {
                v.Dump();
            }
        }

        private void ExpandWildcards(List<TargetDependency> clones)
        {
            bool more = true;
            while (more)
            {
                more = false;
                for (int i = 0; i < clones.Count && more == false; i++)
                {
                    if (clones[i] is FileDependency)
                    {
                        // if source is wildcarded then assume target is wildcarded
                        if (clones[i].Source.Contains('*'))
                        {
                            string projectSource = clones[i].RelativeToProject(clones[i].Source);
                            if (!String.IsNullOrEmpty(projectSource))
                            {
                                string sourceleft = projectSource.Substring(0, projectSource.IndexOf("*")).ToLower();
                                string sourceright = projectSource.Substring(projectSource.IndexOf("*") + 1).ToLower();
                                String? folderName = Path.GetDirectoryName(projectSource);
                                if (Directory.Exists(folderName))
                                {
                                    string[] fileNames = Directory.GetFiles(folderName);
                                    foreach (string file in fileNames)
                                    {
                                        if (file.ToLower().StartsWith(sourceleft) &&
                                            file.ToLower().EndsWith(sourceright))
                                        {
                                            string sub = file.Substring(sourceleft.Length).ToLower();
                                            int index = sub.IndexOf(sourceright);
                                            sub = sub.Substring(0, index);
                                            string target = clones[i].RelativeToProject(clones[i].Target).Replace("*", sub);

                                            FileDependency fd = new FileDependency(file, target);
                                            fd.Action = clones[i].Action.Clone();
                                            fd.Action.ReplaceWildcard(sub);
                                            clones.Add(fd);
                                        }
                                    }
                                    clones.RemoveAt(i);
                                    more = true;
                                }
                            }
                        }
                        else
                        {
                            // source not wildcarded but is target?
                            // i.e. scale.txt -> Parts\*.txt
                            if (clones[i].Target.Contains('*'))
                            {
                                string projectTarget = clones[i].RelativeToProject(clones[i].Target);
                                if (projectTarget != "")
                                {
                                    string targetleft = projectTarget.Substring(0, projectTarget.IndexOf("*")).ToLower();
                                    string targetright = projectTarget.Substring(projectTarget.IndexOf("*") + 1).ToLower();
                                    String? folderName = Path.GetDirectoryName(projectTarget);
                                    if (Directory.Exists(folderName))
                                    {
                                        string[] fileNames = Directory.GetFiles(folderName);
                                        foreach (string file in fileNames)
                                        {
                                            if (file.ToLower().StartsWith(targetleft) &&
                                                file.ToLower().EndsWith(targetright))
                                            {
                                                string sub = file.Substring(targetleft.Length).ToLower();
                                                int index = sub.IndexOf(targetright);
                                                sub = sub.Substring(0, index);
                                                string target = clones[i].RelativeToProject(clones[i].Target).Replace("*", sub);
                                                string source = clones[i].RelativeToProject(clones[i].Source);
                                                FileDependency fd = new FileDependency(source, target);
                                                fd.Action = clones[i].Action.Clone();
                                                fd.Action.ReplaceWildcard(sub);
                                                clones.Add(fd);
                                            }
                                        }
                                        clones.RemoveAt(i);
                                        more = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private TargetDependency? MakeDependency(TargetDependency.TypeOfDependency depType, string source, string target)
        {
            TargetDependency? dep = null;
            switch (depType)
            {
                case TargetDependency.TypeOfDependency.File:
                    {
                        dep = new FileDependency(source, target);
                    }
                    break;

                case TargetDependency.TypeOfDependency.Folder:
                    {
                        dep = new FolderDependency(source, target);
                    }
                    break;

                case TargetDependency.TypeOfDependency.Collection:
                    {
                        dep = new CollectionDependency(source, target);
                    }
                    break;
            }
            return dep;
        }

        private void MakePathsAbsolute(List<TargetDependency> clones)
        {
            foreach (var cl in clones)
            {
                cl.MakePathsAbsolue();
            }
        }

        private StepType? MakeStep(StepType.TypeOfStep atype, string actionParam)
        {
            StepType? res = null;
            switch (atype)
            {
                case StepType.TypeOfStep.Script:
                    {
                        res = new ScriptStep(actionParam);
                    }
                    break;
            }
            return res;
        }

        private TargetDependency? MergeTargets(TargetDependency td1, TargetDependency td2)
        {
            TargetDependency? res = null;

            // if td1 is a collection and td2 isnt then add td2 into td1 as a child
            if ((td1 is CollectionDependency) &&
                ((td2 is FileDependency)) ||
                ((td2 is FolderDependency)))
            {
                td2.Action = new NopStep();
                (td1 as CollectionDependency)?.Add(td2);
                res = td1;
            }
            else
            // if td2 is a collection and td1 isnt then add td1 into td2 as a child
            if ((td2 is CollectionDependency) &&
                ((td1 is FileDependency)) ||
                ((td1 is FolderDependency)))
            {
                td1.Action = new NopStep();
                (td2 as CollectionDependency)?.Add(td1);
                res = td2;
            }
            else
            if (((td1 is FileDependency)) &&
                ((td2 is FileDependency)))
            {
                CollectionDependency cld = new CollectionDependency();
                cld.Action = td1.Action;
                cld.Target = td1.Target;
                cld.Source = "";
                td1.Action = new NopStep();
                td2.Action = new NopStep();
                cld.Add(td1);
                cld.Add(td2);
                res = cld;
            }
            else
            if (((td1 is FolderDependency)) &&
                ((td2 is FolderDependency)))
            {
                CollectionDependency cld = new CollectionDependency();
                cld.Action = td1.Action;
                cld.Target = td1.Target;
                cld.Source = "";
                td1.Action = new NopStep();
                td2.Action = new NopStep();
                cld.Add(td1);
                cld.Add(td2);
                res = cld;
            }
            else if ((td1 is CollectionDependency) &&
                     (td2 is CollectionDependency))
            {
                (td1 as CollectionDependency)?.MergeChildren(td2 as CollectionDependency);
                res = td1;
            }

            return res;
        }
    }
}