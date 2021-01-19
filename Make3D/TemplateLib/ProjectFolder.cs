using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateLib
{
    internal class ProjectFolder
    {
        public String Name { get; set; }

public List<Substitution> Substitutions { get; set; }

            internal List<ProjectFile> files;
        internal List<ProjectFile> Files
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
        public ProjectFolder()
        {
            Files = new List<ProjectFile>();
        }

        internal void CreateFiles(string path)
        {
            foreach( ProjectFile pf in Files)
            {
                string trg = System.IO.Path.Combine(path, pf.Name);
                if ( pf.Source != String.Empty)
                {
                    string src = pf.Source;
                    // if it looks like an absolute path leave it alone
                    if (!System.IO.Path.IsPathRooted(src) )
                    {
                        src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src);
                    }
                    if (File.Exists(src) && !File.Exists(trg))
                    {
                        String s = File.ReadAllText(src);
                        if ( Substitutions != null)
                        {
                            foreach( Substitution sub in Substitutions)
                            {
                                s =s.Replace(sub.Original, sub.Replacement);
                            }
                        }
                        File.WriteAllText(trg, s);
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
