using System;
using System.Collections.Generic;
using System.IO;

namespace TemplateLib
{
    internal class ProjectTemplateFolder
    {
        public String Name { get; set; }

        public List<TemplateSubstitution> Substitutions { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        internal List<ProjectTemplateFile> files;

        internal List<ProjectTemplateFile> Files
        {
            get { return files; }
            set
            {
                if (value != files)
                {
                    files = value;
                }
            }
        }

        public ProjectTemplateFolder()
        {
            Files = new List<ProjectTemplateFile>();
            Attributes = new Dictionary<string, string>();
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
    }
}