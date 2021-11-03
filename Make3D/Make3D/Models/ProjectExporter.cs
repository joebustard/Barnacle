using Barnacle.Object3DLib;
using System;

namespace Barnacle.Models
{
    public class ProjectExporter
    {
        public void Export(String[] filePaths, String exportPath, bool versionExport, bool exportEmptyFiles = true, bool clearPrevious = true)
        {
            Document doc;
            foreach (String f in filePaths)
            {
                try
                {
                    doc = new Document();
                    doc.Load(f);
                    Bounds3D allBounds = new Bounds3D();
                    allBounds.Zero();
                    bool hasContent = false;
                    foreach (Object3D ob in doc.Content)
                    {
                        if (ob.Exportable == true)
                        {
                        hasContent =true;
                        }
                    }
                    if (exportEmptyFiles || hasContent)
                    {
                        foreach (Object3D ob in doc.Content)
                        {
                            allBounds += ob.AbsoluteBounds;
                        }
                        string name = System.IO.Path.GetFileNameWithoutExtension(f);
                        if (versionExport)
                        {
                            name += "_V_" + doc.Revision.ToString();
                            if (clearPrevious)
                            {
                                string[] filesToDelete = System.IO.Directory.GetFiles(exportPath, name + "_V_*.stl");
                                foreach (string fn in filesToDelete)
                                {
                                    try
                                    {
                                        System.IO.File.Delete(fn);
                                    }
                                    catch
                                    {
                                    }
                                }

                            }
                        }
                        name += ".stl";
                        name = exportPath + "\\" + name;
                        doc.AutoExport(name, allBounds);
                    }
                }
                catch
                {
                }
            }
        }
    }
}