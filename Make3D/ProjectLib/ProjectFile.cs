using System;
using System.Xml;

namespace ProjectLib
{
    public class ProjectFile
    {
        public String Name { get; set; }
        public String Source { get; set; }

        // should this file be exported when an export all command is issued
        public bool Export { get; set; }

        // should this file be added to the backup when a backup command is issued
        public bool Backup { get; set; }

        public ProjectFile()
        {
            Name = String.Empty;
            Source = String.Empty;
            Backup = false;
            Export = false;
        }

        internal void Load(XmlDocument doc, XmlNode nd)
        {
            XmlElement ele = nd as XmlElement;
            if (ele != null)
            {
                if (ele.HasAttribute("Name"))
                {
                    Name = ele.GetAttribute("Name");
                }

                if (ele.HasAttribute("Source"))
                {
                    Source = ele.GetAttribute("Source");
                }
                if (ele.HasAttribute("Backup"))
                {
                    Backup = Convert.ToBoolean(ele.GetAttribute("Backup"));
                }
                if (ele.HasAttribute("Export"))
                {
                    Export = Convert.ToBoolean(ele.GetAttribute("Export"));
                }
            }
        }
    }
}