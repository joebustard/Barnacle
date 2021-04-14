using System;

namespace Make3D.Models
{
    public class ProjectExporter
    {
        public void Export(String[] filePaths, String exportPath)
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
                    foreach (Object3D ob in doc.Content)
                    {
                        allBounds += ob.AbsoluteBounds;
                    }
                    string name = System.IO.Path.GetFileNameWithoutExtension(f);
                    name += ".stl";
                    name = exportPath + "\\" + name;
                    doc.AutoExport(name, allBounds);
                }
                catch
                {
                }
            }
        }
    }
}