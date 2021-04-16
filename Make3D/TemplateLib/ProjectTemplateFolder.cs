using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TemplateLib
{
    internal class ProjectTemplateFolder
    {
        internal List<ProjectTemplateFile> files;

        public ProjectTemplateFolder()
        {
            Files = new List<ProjectTemplateFile>();
            Attributes = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Attributes { get; set; }
        public String Name { get; set; }

        public List<TemplateSubstitution> Substitutions { get; set; }

        internal List<ProjectTemplateFile> Files
        {
            get
            {
                return files;
            }
            set
            {
                if (value != files)
                {
                    files = value;
                }
            }
        }

        internal void CreateFiles(string path)
        {
            foreach (ProjectTemplateFile pf in Files)
            {
                string trg = System.IO.Path.Combine(path, pf.Name);
                if (pf.Source != String.Empty)
                {
                    string src = pf.Source;
                    // if it looks like an absolute path leave it alone
                    if (!System.IO.Path.IsPathRooted(src))
                    {
                        src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src);
                    }
                    if (File.Exists(src) && !File.Exists(trg))
                    {
                        File.Copy(src, trg, true);
                    }
                }
                else
                {
                    if (!File.Exists(trg))
                    {
                        File.Create(trg);
                    }
                }
            }
        }

        internal void CreateSolutionEntry(XmlDocument solutionDoc, XmlElement root)
        {
            bool appendToRoot = false;
            if (Attributes.ContainsKey("Name"))
            {
                String n = Attributes["Name"];
                if (n == ".")
                {
                    appendToRoot = true;
                }
            }
            XmlElement fldEl = solutionDoc.CreateElement("Folder");
            if (!appendToRoot)
            {
                root.AppendChild(fldEl);
            }
            foreach (string s in Attributes.Keys)
            {
                string v = Attributes[s];
                fldEl.SetAttribute(s, v);
            }

            foreach (ProjectTemplateFile fi in Files)
            {
                XmlElement fil = solutionDoc.CreateElement("File");
                foreach (string s in fi.Attributes.Keys)
                {
                    if (s != "Source")
                    {
                        string v = fi.Attributes[s];
                        fil.SetAttribute(s, v);
                    }
                }
                if (!appendToRoot)
                {
                    fldEl.AppendChild(fil);
                }
                else
                {
                    root.AppendChild(fil);
                }
            }
        }
    }
}