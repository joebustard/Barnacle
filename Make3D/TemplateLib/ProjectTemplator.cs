using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TemplateLib
{
    public class ProjectTemplator
    {
        private string templateDefinitionExtension;
        private List<ProjectTemplateDefinition> templates;

        public ProjectTemplator()
        {
            templates = new List<ProjectTemplateDefinition>();
            TemplateDefinitionPath = AppDomain.CurrentDomain.BaseDirectory;
            TemplateDefinitionExtension = ".def";
        }

        public string ProjectTarget { get; set; }
        public string SolutionPath { get; set; }

        public string TemplateDefinitionExtension
        {
            get
            {
                return templateDefinitionExtension;
            }
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

        public string TemplateDefinitionPath { get; set; }

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

        public int NumberOfTemplates()
        {
            return templates.Count;
        }

        public bool ProcessTemplate(string projName, string pth, string templateName)
        {
            bool res = false;
            ProjectTemplateDefinition def = null;
            foreach (ProjectTemplateDefinition d in templates)
            {
                if (d.Name == templateName)
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

                CreateSolution(projName, pth, def);

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

        private void CreateSolution(string projName, string pth, ProjectTemplateDefinition def)
        {
            XmlDocument solutionDoc = new XmlDocument();
            XmlElement root = solutionDoc.CreateElement("Project");
            root.SetAttribute("ProjectName", projName);
            solutionDoc.AppendChild(root);
            foreach (ProjectTemplateFolder fld in def.Folders)
            {
                fld.CreateSolutionEntry(solutionDoc, root);
            }
            SolutionPath = System.IO.Path.Combine(pth, projName + ".bmf");
            solutionDoc.Save(SolutionPath);
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