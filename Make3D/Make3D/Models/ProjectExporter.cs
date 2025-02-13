﻿// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using Barnacle.Dialogs;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
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
            doc.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;

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