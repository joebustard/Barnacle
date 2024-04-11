using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static Workflow.CuraDefinition;

namespace Workflow
{
    public class SlicerProfile
    {
        //  public List<SettingOverride> Overrides { get; set; }
        // public List<SettingDefinition> Overrides { get; set; }
        public CuraDefinitionFile CuraFile { get; set; }

        public SlicerProfile(string fileName)
        {
            LoadOverrides(fileName);
        }

        public SlicerProfile()
        {
        }

        public void SaveAsXml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlElement docNode = doc.CreateElement("Profile");
            if (CuraFile != null)
            {
                // foreach (SettingOverride so in Overrides)
                foreach (SettingDefinition so in CuraFile.Overrides)
                {
                    XmlElement ovr = doc.CreateElement("Ovr");
                    ovr.SetAttribute("s", so.Section);
                    ovr.SetAttribute("n", so.Name);
                    ovr.SetAttribute("v", so.OverideValue);
                    ovr.SetAttribute("d", so.Description);
                    docNode.AppendChild(ovr);
                }
            }
            doc.AppendChild(docNode);
            doc.Save(fileName);
        }

        public void LoadOverrides(string fileName)
        {
            CuraFile = new CuraDefinitionFile();
            CuraFile.Load(fileName);
        }

        public void SaveOverrides(String fName)
        {
            try
            {
                if (File.Exists(fName))
                {
                    File.Delete(fName);
                }
                if (CuraFile != null)
                {
                    CuraFile.Save(fName);
                }
            }
            catch (Exception ex)
            {
                LoggerLib.Logger.LogException(ex);
            }
        }
    }
}