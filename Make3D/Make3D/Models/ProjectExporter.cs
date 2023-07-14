using Barnacle.Dialogs;
using Barnacle.Object3DLib;
using System;
using System.Threading.Tasks;
using VisualSolutionExplorer;

namespace Barnacle.Models
{
    public class ProjectExporter
    {
        public async Task ExportAsync(Project prj, String[] filePaths, String exportPath, bool versionExport, bool exportEmptyFiles = true, bool clearPrevious = true)
        {
            InfoWindow.Instance().ShowInfo("Export");
            foreach (String f in filePaths)
            {
                try
                {
                    string rfn = System.IO.Path.GetFileName(f);
                    InfoWindow.Instance().ShowText(rfn);
                    await Task.Run(() => ExportOne(prj, exportPath, versionExport, exportEmptyFiles, clearPrevious, f));
                }
                catch
                {
                }
            }
            InfoWindow.Instance().CloseInfo();
        }

        private static void ExportOne(Project prj, string exportPath, bool versionExport, bool exportEmptyFiles, bool clearPrevious, string f)
        {
            Document doc = new Document();
            doc.ParentProject = prj;
            doc.Load(f);
            Bounds3D allBounds = new Bounds3D();
            allBounds.Zero();
            bool hasContent = false;
            foreach (Object3D ob in doc.Content)
            {
                if (ob.Exportable == true)
                {
                    hasContent = true;
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
                name = exportPath + System.IO.Path.DirectorySeparatorChar + name;
                doc.AutoExport(name, allBounds);
            }
        }
    }
}