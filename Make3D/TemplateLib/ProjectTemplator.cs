using FileUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            // Explorer="True" Clean="False" Export="True" AddSubs="True" AddFiles="True"
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
            AddAttribute(backupsFolder, "Export", "False");

            ProjectTemplateFolder exportFolder = AddDesignedFolder(def, rootptf, "Export");
            AddAttribute(exportFolder, "Export", "False");
            AddAttribute(exportFolder, "Clean", "True");
            ProjectTemplateFolder imagesFolder = AddDesignedFolder(def, rootptf, "Images");
            ProjectTemplateFolder kitFolder = AddDesignedFolder(def, rootptf, "Kits");
            ProjectTemplateFolder printerFolder = AddDesignedFolder(def, rootptf, "Printer");
            AddAttribute(printerFolder, "Export", "False");
            ProjectTemplateFolder partsFolder = AddDesignedFolder(def, rootptf, "Parts");
            ProjectTemplateFolder scriptsFolder = AddDesignedFolder(def, rootptf, "Scripts");
            AddAttribute(scriptsFolder, "Export", "False");
            ProjectTemplateFolder subpartsFolder = AddDesignedFolder(def, rootptf, "Subparts");
            AddAttribute(subpartsFolder, "Export", "False");
            ProjectTemplateFolder assemblersScriptsFolder = AddDesignedFolder(def, scriptsFolder, "Assemblers");
            AddAttribute(assemblersScriptsFolder, "Export", "False");
            ProjectTemplateFolder partmakersFolder = AddDesignedFolder(def, scriptsFolder, "PartMakers");
            AddAttribute(partmakersFolder, "Export", "False");
            ProjectTemplateFolder kitmakersFolder = AddDesignedFolder(def, scriptsFolder, "KitMakers");
            AddAttribute(kitmakersFolder, "Export", "False");

            // models
            if (!String.IsNullOrEmpty(modelList))
            {
                string[] partnames = modelList.Split('\n');

                foreach (string fn in partnames)
                {
                    string fn2 = fn.Trim();
                    AddFile(partsFolder, fn2 + ".txt", @"templates/blankmodel1_35.txt");

                    ProjectTemplateFile partmakerfile = AddFile(partmakersFolder, "Make" + fn2 + ".lmp", @"templates/MakePartFromSubParts.lmp");

                    AddSubstitution(partmakerfile, "<PARTNAME>", fn2);
                }

                // sub parts
                foreach (string fn in partnames)
                {
                    string fn2 = fn.Trim();
                    AddFile(subpartsFolder, fn2 + "Subparts.txt", @"templates/blankmodel1_35.txt");
                }

                // Add include file to scripts
                AddFile(scriptsFolder, "scale.inc", @"templates/AircraftScale.inc");
                AddFile(scriptsFolder, "globaldefinitions.inc", @"templates/AircraftGlobalDefinitions.inc");
                AddFile(scriptsFolder, "projectlib.inc", @"templates/AircraftProjectLib.inc");
            }

            // assemblies folder
            if (!String.IsNullOrEmpty(assemblyList))
            {
                string[] assemblies = assemblyList.Split('\n');
                foreach (string fn in assemblies)
                {
                    string fn2 = fn.Trim();
                    // the assembly model file
                    ProjectTemplateFile assemblyFile = AddFile(assemblyFolder, fn2 + "Assembly.txt", @"templates/blankmodel1_35.txt");

                    // the script that makes the assembly
                    ProjectTemplateFile assemblerScriptFile = AddFile(assemblersScriptsFolder, "Assemble" + fn2 + ".lmp", @"templates/MakeAssemblyFromParts.lmp");

                    string dummyDecs = "";
                    string dummyInserts = "";
                    string dummyPositions = "";
                    string dummyBuilder = "";
                    string[] partnames = modelList.Split('\n');
                    bool first = true;
                    foreach (string pn in partnames)
                    {
                        string npn = pn.Trim();
                        dummyPositions += $"  // {npn} positions\r\n";
                        dummyPositions += $"double  {npn}X=0;\r\n";
                        dummyPositions += $"double  {npn}Y=0;\r\n";
                        dummyPositions += $"double  {npn}Z=0;\r\n";
                        dummyPositions += $"double  {npn}RX=0;\r\n";
                        dummyPositions += $"double  {npn}RY=0;\r\n";
                        dummyPositions += $"double  {npn}RZ=0;\r\n";
                        dummyDecs += $"  Solid {npn};\r\n";
                        dummyInserts += "  //\r\n";
                        dummyInserts += $"  {npn} =Insert(src,\"{npn}\",";
                        dummyInserts += $"  {npn}X,{npn}Y,{npn}Z,{npn}RX,{npn}RY,{npn}RZ);";

                        if (first)
                        {
                            dummyBuilder += $"  whole ={npn};\r\n";
                            first = false;
                        }
                        else
                        {
                            dummyBuilder += $"  if (IsValid({npn}))\r\n";
                            dummyBuilder += "  {";
                            dummyBuilder += $"  whole = ForceUnion(whole,{npn});\r\n";
                            dummyBuilder += "  }";
                        }
                    }
                    AddSubstitution(assemblerScriptFile, "<PARTLIST>", dummyInserts);
                    AddSubstitution(assemblerScriptFile, "<PARTPOSITIONS>", dummyPositions);
                    AddSubstitution(assemblerScriptFile, "<ASSEMBLYNAME>", fn2);
                    AddSubstitution(assemblerScriptFile, "<BUILDER>", dummyBuilder);
                    AddSubstitution(assemblerScriptFile, "<DECS>", dummyDecs);
                }
            }

            if (!String.IsNullOrEmpty(scriptIncludeList))
            {
                string[] scincs = scriptIncludeList.Split('\n');
                foreach (string fn in scincs)
                {
                    AddFile(scriptsFolder, fn.Trim() + ".inc", @"templates/LimpetInclude.inc");
                }
            }

            if (!String.IsNullOrEmpty(scriptList))
            {
                string[] scincs = scriptList.Split('\n');
                foreach (string fn in scincs)
                {
                    AddFile(scriptsFolder, fn.Trim() + ".lmp", @"templates/LimpetTemplate.txt");
                }
            }

            // KIT MAKERS
            if (numberOfKits > 0)
            {
                for (int i = 1; i <= numberOfKits; i++)
                {
                    string fn = "kit" + i.ToString();

                    ProjectTemplateFile kitFile = AddFile(kitFolder, fn + ".txt", @"templates/blankmodel1_35.txt");
                    AddSubstitution(kitFile, "<KITNAME>", fn);

                    ProjectTemplateFile kitmakerFile = AddFile(kitmakersFolder, fn + ".lmp", @"templates/MakeKitFromParts.lmp");
                    AddSubstitution(kitmakerFile, "<KITNAME>", fn);

                    string dummy = "";
                    string[] partnames = modelList.Split('\n');
                    foreach (string pn in partnames)
                    {
                        string npn = pn.Trim();
                        dummy += $"    Addpart(\"{npn}\",0,0,0);\r\n";
                    }
                    AddSubstitution(kitmakerFile, "<PARTLIST", dummy);
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

        private static void AddAttribute(ProjectTemplateFolder folder, string v1, string v2)
        {
            folder.Attributes[v1] = v2;
        }

        private static ProjectTemplateFolder AddDesignedFolder(ProjectTemplateDefinition def, ProjectTemplateFolder root, string name)
        {
            ProjectTemplateFolder folder = new ProjectTemplateFolder();
            folder.Name = name;
            folder.Attributes["Name"] = name;
            folder.Substitutions = def.Substitutions;
            root.Folders.Add(folder);
            SetDefaultAttributes(folder);
            return folder;
        }

        private static ProjectTemplateFile AddFile(ProjectTemplateFolder folder, string fileName, string templateName)
        {
            ProjectTemplateFile file = new ProjectTemplateFile();
            file.Name = fileName;
            folder.Files.Add(file);
            file.Attributes["Name"] = fileName;
            file.Attributes["Source"] = templateName;
            file.Source = templateName;
            return file;
        }

        private static void SetDefaultAttributes(ProjectTemplateFolder folder)
        {
            AddAttribute(folder, "Explorer", "True");
            AddAttribute(folder, "Export", "True");
            AddAttribute(folder, "Clean", "False");
            AddAttribute(folder, "AddSubs", "True");
            AddAttribute(folder, "AddFiles", "True");
        }

        private void AddSubstitution(ProjectTemplateFile file, string src, string trg)
        {
            TemplateSubstitution ts = new TemplateSubstitution();
            ts.Original = src;
            ts.Replacement = trg;
            file.Substitutions.Add(ts);
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