using FileUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
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

        public string ProjectTarget
        {
            get; set;
        }

        public string SolutionPath
        {
            get; set;
        }

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

        public string TemplateDefinitionPath
        {
            get; set;
        }

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
                TemplateSubstitution ts = new TemplateSubstitution();
                ts.Original = "<PROJNAME>";
                ts.Replacement = projName;
                def.Substitutions.Add(ts);

                ts = new TemplateSubstitution();
                ts.Original = "<PROJPATH>";
                ts.Replacement = pth;
                def.Substitutions.Add(ts);

                if (!Directory.Exists(pth))
                {
                    Directory.CreateDirectory(pth);
                }
                foreach (ProjectTemplateFolder fld in def.Folders)
                {
                    fld.CreateFilesAndFolders(pth, def.Substitutions);
                }

                // if we are using a user template there may be a
                // zip file containing the files
                if (def.IsUserTemplate)
                {
                    string zipPath = Path.Combine(PathManager.UserTemplatesFolder(), templateName + ".zip");
                    // can only use it if it exists.
                    // Its NOT a problem if the user decided not to create one
                    if (File.Exists(zipPath))
                    {
                        ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
                        var ets = zipArchive.Entries;
                        foreach (ZipArchiveEntry et in ets)
                        {
                            if (Path.HasExtension(et.Name))
                            {
                                // it seems that a file is being held open for a while after creation
                                // so that we can't unzip to it immediately.
                                // Backing off and retrying seems to recover
                                bool failed = true;
                                for (int retry = 0; retry < 5 && failed; retry++)
                                {
                                    try
                                    {
                                        string targetFile = Path.Combine(pth, et.FullName);
                                        et.ExtractToFile(targetFile, true);
                                        failed = false;
                                    }
                                    catch (Exception ex)
                                    {
                                        // MessageBox.Show(ex.Message);
                                        Thread.Sleep(30 * 1000);
                                    }
                                }
                            }
                        }
                        zipArchive.Dispose();
                    }
                }
                // make the actual solution file and view
                CreateSolution(projName, pth, def);
                res = true;
            }
            return res;
        }

        public void ScanForTemplates(string srcFolder)
        {
            if (srcFolder != String.Empty)
            {
                if (Directory.Exists(srcFolder))
                {
                    if (TemplateDefinitionExtension != String.Empty)
                    {
                        string[] files = Directory.GetFiles(srcFolder, "*" + TemplateDefinitionExtension);
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
            solutionDoc.XmlResolver = null;
            XmlElement root = solutionDoc.CreateElement("Project");
            root.SetAttribute("ProjectName", projName);
            root.SetAttribute("Open", "\\" + projName + "\\" + def.InitialFile);
            root.SetAttribute("Created", DateTime.Now.ToString());
            solutionDoc.AppendChild(root);
            foreach (ProjectTemplateFolder fld in def.Folders)
            {
                if (fld.Name != ".")
                {
                    XmlElement fldel = solutionDoc.CreateElement("Folder");

                    fld.CreateSolutionEntry(solutionDoc, fldel);
                    root.AppendChild(fldel);
                }
                else
                {
                    fld.CreateSolutionEntry(solutionDoc, root);
                }
            }
            XmlElement desEle = solutionDoc.CreateElement("Description");
            desEle.InnerText = def.Description;
            root.AppendChild(desEle);

            SolutionPath = System.IO.Path.Combine(pth, projName + ".bmf");
            solutionDoc.Save(SolutionPath);
            solutionDoc = null;
        }

        private void LoadDefinition(string f)
        {
            if (File.Exists(f))
            {
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.Load(f);
                XmlNode root = doc.SelectSingleNode("Defs");

                XmlNodeList defNodes = root.SelectNodes("ProjectDefinition");
                foreach (XmlNode nd in defNodes)
                {
                    ProjectTemplateDefinition def = new ProjectTemplateDefinition();
                    def.Load(doc, nd);
                    templates?.Add(def);
                }
                doc = null;
            }
        }
    }
}