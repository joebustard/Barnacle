using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplator
    {
        private List<ProjectTemplateDefinition> templates;
        public string TemplateDefinitionPath { get; set; }

        private string templateDefinitionExtension;

        public string TemplateDefinitionExtension
        {
            get { return templateDefinitionExtension; }
            set
            {
                if (value != templateDefinitionExtension)
                {
                    templateDefinitionExtension = value;
                    if (templateDefinitionExtension != String.Empty)
                    {
                        if (!templateDefinitionExtension.StartsWith("."))
                        {
                            templateDefinitionExtension = "." + templateDefinitionExtension;
                        }
                    }
                }
            }
        }

        public string ProjectTarget { get; set; }

        public void AddSubstitution(string v1, string v2)
        {
            TemplateSubstitution sub = new TemplateSubstitution();
            sub.Original = v1;
            sub.Replacement = v2;

            foreach (ProjectTemplateDefinition pd in templates)
            {
                pd.Substitutions.Add(sub);
            }
        }

        public ProjectTemplator()
        {
            templates = new List<ProjectTemplateDefinition>();
            TemplateDefinitionPath = AppDomain.CurrentDomain.BaseDirectory;
            TemplateDefinitionExtension = ".def";
        }

        public bool ProcessTemplate(string pth, string name)
        {
            bool res = false;
            ProjectTemplateDefinition def = null;
            foreach (ProjectTemplateDefinition d in templates)
            {
                if (d.Name == name)
                {
                    def = d;
                    break;
                }
            }
            if (def != null)
            {
                if (!Directory.Exists(pth))
                {
                    Directory.CreateDirectory(pth);
                }
                foreach (ProjectTemplateFolder fld in def.Folders)
                {
                    fld.Substitutions = def.Substitutions;
                    string p = System.IO.Path.Combine(pth, fld.Name);
                    if (!Directory.Exists(p))
                    {
                        Directory.CreateDirectory(p);
                    }
                    fld.CreateFiles(p);
                }
                res = true;
            }
            return res;
        }

        public void ScanForTemplates()
        {
            if (TemplateDefinitionPath != String.Empty)
            {
                if (Directory.Exists(TemplateDefinitionPath))
                {
                    if (TemplateDefinitionExtension != String.Empty)
                    {
                        string[] files = Directory.GetFiles(TemplateDefinitionPath, "*" + TemplateDefinitionExtension);
                        foreach (string f in files)
                        {
                            LoadDefinition(f);
                        }
                    }
                }
            }
        }

        public int NumberOfTemplates()
        {
            return templates.Count;
        }

        public void GetTemplateDetails(int i, ref string name, ref string description)
        {
            name = String.Empty;
            description = String.Empty;

            if (i < templates.Count)
            {
                name = templates[i].Name;
                description = templates[i].Description;
            }
        }

        private void LoadDefinition(string f)
        {
            if (File.Exists(f))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(f);
                XmlNode root = doc.SelectSingleNode("Defs");

                XmlNodeList defNodes = root.SelectNodes("ProjectDefinition");
                foreach (XmlNode nd in defNodes)
                {
                    ProjectTemplateDefinition def = new ProjectTemplateDefinition();
                    def.Load(doc, nd);
                    templates?.Add(def);
                }
            }
        }
    }
}