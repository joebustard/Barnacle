using FileUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Shapes;
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

        /// <summary>
        /// Use the templator to build a project from stringlists rather than a stored template
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projPath"></param>
        /// <param name="modelList"></param>
        /// <param name="assemblyList"></param>
        /// <param name="scriptList"></param>
        /// <param name="scriptIncludeList"></param>
        /// <param name="numberOfKits"></param>
        /// <param name="generateQuickAssembler"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool CreateByDesign(string projectName, string projPath, string modelList, string assemblyList, string scriptList, string scriptIncludeList, int numberOfKits, bool generateQuickAssembler)
        {
            bool res = false;
            ProjectTemplateDefinition def = new ProjectTemplateDefinition();
            TemplateSubstitution ts = new TemplateSubstitution();
            ts.Original = "<PROJNAME>";
            ts.Replacement = projectName;
            def.Substitutions.Add(ts);

            ts = new TemplateSubstitution();
            ts.Original = "<PROJPATH>";
            ts.Replacement = projPath;
            def.Substitutions.Add(ts);

            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }

            // create a root folder
            ProjectTemplateFolder rootptf = new ProjectTemplateFolder();
            rootptf.Name = ".";
            rootptf.Substitutions = def.Substitutions;
            def.Folders.Add(rootptf);
            // add a empty file
            ProjectTemplateFile file1 = new ProjectTemplateFile();
            file1.Name = "untitled.txt";
            rootptf.Files.Add(file1);
            file1.Attributes["Name"] = file1.Name;
            file1.Attributes["Source"] = @"templates/blankmodel1_35.txt";
            file1.Source = @"templates/blankmodel1_35.txt";
            def.InitialFile = "untitled.txt";

            ProjectTemplateFolder assemblyFolder = AddDesignedFolder(def, rootptf, "Assemblies");
            ProjectTemplateFolder backupsFolder = AddDesignedFolder(def, rootptf, "Backups");
            ProjectTemplateFolder exportFolder = AddDesignedFolder(def, rootptf, "Export");
            ProjectTemplateFolder kitFolder = AddDesignedFolder(def, rootptf, "Kits");
            ProjectTemplateFolder printerFolder = AddDesignedFolder(def, rootptf, "Printer");
            ProjectTemplateFolder partsFolder = AddDesignedFolder(def, rootptf, "Parts");
            ProjectTemplateFolder scriptsFolder = AddDesignedFolder(def, rootptf, "Scripts");
            ProjectTemplateFolder subpartsFolder = AddDesignedFolder(def, rootptf, "Subparts");
            ProjectTemplateFolder assemblersScriptsFolder = AddDesignedFolder(def, scriptsFolder, "Assemblers");
            ProjectTemplateFolder partmakersFolder = AddDesignedFolder(def, scriptsFolder, "PartMakers");
            ProjectTemplateFolder kitmakersFolder = AddDesignedFolder(def, scriptsFolder, "KitMakers");

            // models
            if (!String.IsNullOrEmpty(modelList))
            {
                string[] partnames = modelList.Split('\n');
                foreach (string fn in partnames)
                {
                    string fn2 = fn.Trim();
                    ProjectTemplateFile partfile = new ProjectTemplateFile();
                    partfile.Name = fn2 + ".txt";
                    partsFolder.Files.Add(partfile);
                    partfile.Attributes["Name"] = partfile.Name;
                    partfile.Attributes["Source"] = @"templates/blankmodel1_35.txt";
                    partfile.Source = @"templates/blankmodel1_35.txt";

                    ProjectTemplateFile partmakerfile = new ProjectTemplateFile();
                    partmakerfile.Name = "Make" + fn2 + ".lmp";

                    TemplateSubstitution ts2 = new TemplateSubstitution();
                    ts2.Original = "<PARTNAME>";
                    ts2.Replacement = fn2;

                    partmakerfile.Substitutions.Add(ts2);

                    partmakersFolder.Files.Add(partmakerfile);
                    partmakerfile.Attributes["Name"] = partmakerfile.Name;
                    partmakerfile.Attributes["Source"] = @"templates/MakePartFromSubParts.lmp";
                    partmakerfile.Source = @"templates/MakePartFromSubParts.lmp";
                }

                // sub parts
                foreach (string fn in partnames)
                {
                    string fn2 = fn.Trim();
                    ProjectTemplateFile subpartFile = new ProjectTemplateFile();
                    subpartFile.Name = fn2 + "Subparts.txt";
                    subpartsFolder.Files.Add(subpartFile);
                    subpartFile.Attributes["Name"] = subpartFile.Name;
                    subpartFile.Attributes["Source"] = @"templates/blankmodel1_35.txt";
                    subpartFile.Source = @"templates/blankmodel1_35.txt";
                }
            }

            // assemblies folder
            if (!String.IsNullOrEmpty(assemblyList))
            {
                string[] assemblies = assemblyList.Split('\n');
                foreach (string fn in assemblies)
                {
                    string fn2 = fn.Trim();
                    ProjectTemplateFile assemblyFile = new ProjectTemplateFile();
                    assemblyFile.Name = fn2 + ".txt";
                    assemblyFolder.Files.Add(assemblyFile);
                    assemblyFile.Attributes["Name"] = assemblyFile.Name;
                    assemblyFile.Attributes["Source"] = @"templates/blankmodel1_35.txt";
                    assemblyFile.Source = @"templates/blankmodel1_35.txt";

                    ProjectTemplateFile assemblerScriptFile = new ProjectTemplateFile();
                    assemblerScriptFile.Name = "Assemble" + fn2 + ".lmp";
                    assemblersScriptsFolder.Files.Add(assemblerScriptFile);
                    assemblerScriptFile.Attributes["Name"] = assemblerScriptFile.Name;
                    assemblerScriptFile.Attributes["Source"] = @"templates/MakeAssemblyFromParts.lmp";
                    assemblerScriptFile.Source = assemblerScriptFile.Attributes["Source"];
                }
            }

            // KIT MAKERS
            if (numberOfKits > 0)
            {
                for (int i = 1; i <= numberOfKits; i++)
                {
                    ProjectTemplateFile kitFile = new ProjectTemplateFile();
                    string fn = "kit" + i.ToString();
                    kitFile.Name = fn + ".txt";
                    kitFolder.Files.Add(kitFile);
                    kitFile.Attributes["Name"] = kitFile.Name;
                    kitFile.Attributes["Source"] = @"templates/blankmodel1_35.txt";
                    kitFile.Source = @"templates/blankmodel1_35.txt";

                    TemplateSubstitution ts3 = new TemplateSubstitution();
                    ts3.Original = "<KITNAME>";
                    ts3.Replacement = fn;
                    kitFile.Substitutions.Add(ts3);

                    ProjectTemplateFile kitmakerFile = new ProjectTemplateFile();
                    string kn = "kit" + i.ToString();
                    kitmakerFile.Name = kn + ".lmp";
                    kitmakersFolder.Files.Add(kitmakerFile);
                    kitmakerFile.Attributes["Name"] = kitmakerFile.Name;
                    kitmakerFile.Attributes["Source"] = @"templates/MakeKitFromParts.lmp";
                    kitmakerFile.Source = kitmakerFile.Attributes["Source"];

                    string dummy = "";
                    string[] partnames = modelList.Split('\n');
                    foreach (string pn in partnames)
                    {
                        dummy += $"    Addpart(\"{pn}\",0,0,0);\r\n";
                    }
                    TemplateSubstitution ts4 = new TemplateSubstitution();
                    ts4.Original = "<PARTLIST>";
                    ts4.Replacement = dummy;
                    kitmakerFile.Substitutions.Add(ts4);
                }
            }
            foreach (ProjectTemplateFolder fld in def.Folders)
            {
                fld.CreateFilesAndFolders(projPath, def.Substitutions);
            }
            // do the generation
            CreateSolution(projectName, projPath, def);
            res = true;
            return res;
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
                    string zipPath = System.IO.Path.Combine(PathManager.UserTemplatesFolder(), templateName + ".zip");
                    // can only use it if it exists.
                    // Its NOT a problem if the user decided not to create one
                    if (File.Exists(zipPath))
                    {
                        ZipArchive zipArchive = ZipFile.OpenRead(zipPath);
                        var ets = zipArchive.Entries;
                        foreach (ZipArchiveEntry et in ets)
                        {
                            if (System.IO.Path.HasExtension(et.Name))
                            {
                                // it seems that a file is being held open for a while after creation
                                // so that we can't unzip to it immediately.
                                // Backing off and retrying seems to recover
                                bool failed = true;
                                for (int retry = 0; retry < 5 && failed; retry++)
                                {
                                    try
                                    {
                                        string targetFile = System.IO.Path.Combine(pth, et.FullName);
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

        private static ProjectTemplateFolder AddDesignedFolder(ProjectTemplateDefinition def, ProjectTemplateFolder root, string name)
        {
            ProjectTemplateFolder folder = new ProjectTemplateFolder();
            folder.Name = name;
            folder.Attributes["Name"] = name;
            folder.Substitutions = def.Substitutions;
            root.Folders.Add(folder);
            return folder;
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