using Make3D.Object3DLib;
using System;

namespace Make3D.Models
{
    public class ProjectExporter
    {
        public void Export(String[] filePaths, String exportPath, bool versionExport, bool exportEmptyFiles = true)
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
                    if (exportEmptyFiles || doc.Content.Count > 0)
                    {
                        foreach (Object3D ob in doc.Content)
                        {
                            allBounds += ob.AbsoluteBounds;
                        }
                        string name = System.IO.Path.GetFileNameWithoutExtension(f);
                        if (versionExport)
                        {
                            name += "_V_" + doc.Revision.ToString();
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