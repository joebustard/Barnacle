using System;
using System.Collections.Generic;

namespace ProjectLib
{
    internal class ProjectFolder
    {
        public String Name { get; set; }

        // should this folder appear  on the project tree
        public bool Explorer { get; set; }

        // should this folder be cleared when a clean command is issued
        public bool Clean { get; set; }

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
            Clean = false;
            Explorer = false;
        }
    }
}