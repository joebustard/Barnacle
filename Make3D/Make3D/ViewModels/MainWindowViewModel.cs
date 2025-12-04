// **************************************************************************
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

using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.Views;
using FileUtils;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using Workflow;

namespace Barnacle.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string caption;
        private Control subview;

        internal MainWindowViewModel()
        {
            Caption = "";
            base.PropertyChanged += MainWindowViewModel_PropertyChanged;
            SubView = new StartupView();
            NotificationManager.Subscribe("StartWithNewProject", StartWithNewProject);
            NotificationManager.Subscribe("NewProjectBack", NewProjectBack);
            NotificationManager.Subscribe("ShowEditor", ShowEditor);
            NotificationManager.Subscribe("StartWithOldProject", StartWithOldProject);
            NotificationManager.Subscribe("StartWithOldProjectNoLoad", StartWithOldProjectNoLoad);
        }

        public string Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Control SubView
        {
            get
            {
                return subview;
            }
            set
            {
                if (subview != value)
                {
                    subview = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal int AutoSlice(string autoSlicePrinter, string autoSliceProfile, string autoModelFile)
        {
            int res = -1;
            if (!String.IsNullOrEmpty(autoModelFile))
            {
                BarnaclePrinterManager printerManager;
                printerManager = new BarnaclePrinterManager();

                Document exportDoc = new Document();
                exportDoc.ParentProject = BaseViewModel.Project;
                string fullPath = BaseViewModel.Project.BaseFolder + "\\" + autoModelFile;
                exportDoc.Load(fullPath);

                exportDoc.ProjectSettings = BaseViewModel.Project.SharedProjectSettings;

                exportDoc.LoadGlobalSettings();

                // if the caller hasn't supplied the printer name or the profile name just use the
                // last one used as per the settings
                if (String.IsNullOrEmpty(autoSlicePrinter))
                {
                    autoSlicePrinter = Properties.Settings.Default.SlicerPrinter;
                }

                if (String.IsNullOrEmpty(autoSliceProfile))
                {
                    autoSliceProfile = Properties.Settings.Default.SlicerProfileName;
                }

                // can only slice if we have a printer name. profile name and a path to the slicer
                if (!String.IsNullOrEmpty(autoSlicePrinter) &&
                     !String.IsNullOrEmpty(autoSliceProfile) &&
                     !String.IsNullOrEmpty(BaseViewModel.Project.SharedProjectSettings.SlicerPath)
                     )
                {
                    String slicerPath = BaseViewModel.Project.SharedProjectSettings.SlicerPath;
                    String exportPath = BaseViewModel.Project.BaseFolder + "\\export";
                    if (!Directory.Exists(exportPath))
                    {
                        Directory.CreateDirectory(exportPath);
                    }
                    string printerPath = BaseViewModel.Project.BaseFolder + "\\printer";
                    if (!Directory.Exists(printerPath))
                    {
                        Directory.CreateDirectory(printerPath);
                    }
                    string modelName = Path.GetFileNameWithoutExtension(autoModelFile);
                    fullPath = Path.GetDirectoryName(fullPath);
                    Bounds3D allBounds = RecalculateAllBounds(exportDoc);

                    string exportedPath = exportDoc.ExportAll("STLSLICE", allBounds, exportPath);
                    exportedPath = Path.Combine(exportPath, modelName + ".stl");

                    string gcodePath = Path.Combine(printerPath, modelName + ".gcode");

                    string logPath = Path.GetTempPath() + modelName + "_slicelog.log";
                    string lastLog = logPath;

                    string prf = PathManager.PrinterProfileFolder() + "\\" + autoSliceProfile + ".profile";

                    string curaPrinterName;
                    string curaExtruderName;
                    BarnaclePrinter bp = printerManager.FindPrinter(autoSlicePrinter);
                    curaPrinterName = bp.CuraPrinterFile + ".def.json";
                    curaExtruderName = bp.CuraExtruderFile + ".def.json";

                    // start and end gcode may have a macro $NAME in it.
                    //Both must be a single line of text when passed to cura engine
                    string sg = bp.StartGCode.Replace("$NAME", modelName);
                    sg = sg.Replace("\r\n", "\\n");
                    sg = sg.Replace("\n", "\\n");

                    string eg = bp.EndGCode.Replace("$NAME", modelName);
                    eg = eg.Replace("\r\n", "\\n");
                    eg = eg.Replace("\n", "\\n");

                    SliceResult sliceRes = CuraEngineInterface.SliceFile(exportedPath, gcodePath, logPath,
                                                                               slicerPath,
                                                                               curaPrinterName,
                                                                               curaExtruderName,
                                                                               prf, sg, eg);

                    if (sliceRes != null)
                    {
                        if (sliceRes.Result)
                        {
                            res = 0;
                        }
                    }
                }
            }
            return res;
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Caption")
            {
                Caption = Document.Caption;
            }
        }

        private void NewProjectBack(object param)
        {
            SubView = new StartupView();
        }

        private Bounds3D RecalculateAllBounds(Document doc)
        {
            Bounds3D allBounds = new Bounds3D();
            if (doc.Content.Count == 0)
            {
                allBounds.Zero();
            }
            else
            {
                foreach (Object3D ob in doc.Content)
                {
                    allBounds += ob.AbsoluteBounds;
                }
            }
            return allBounds;
        }

        private void ShowEditor(object param)
        {
            SubView = new DefaultView();
        }

        private void StartWithNewProject(object param)
        {
            SubView = new NewProjectView();
        }

        private void StartWithOldProject(object param)
        {
            string projPath = param.ToString();
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProject", projPath);
        }

        private void StartWithOldProjectNoLoad(object param)
        {
            string projPath = param.ToString();
            RecentlyUsedManager.UpdateRecentFiles(projPath);
            NotificationManager.Notify("ShowEditor", null);
            NotificationManager.Notify("ReloadProjectDontLoadLastFile", projPath);
        }
    }
}