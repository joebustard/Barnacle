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
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiImport.xaml
    /// </summary>
    public partial class MultiImport : Window, INotifyPropertyChanged
    {
        private static bool overWrite = false;
        private bool closeEnabled;
        private Visibility folderVisible;
        private bool importFolderChecked;
        private string importPath;
        private bool importZipChecked;
        private string importZipPath;
        private string[] ImportZipPaths;
        private int maxModelsPerFile;
        private int maxProgress;
        private string newFolderName;
        private double progressValue;
        private string resultsText;
        private bool startEnabled;
        private double xRotation;
        private double yRotation;
        private Visibility zipVisible;
        private double zRotation;

        public MultiImport()
        {
            InitializeComponent();
            DataContext = this;
            MaxModelsPerFile = 1;
            XRotation = 0;
            YRotation = 0;
            ZRotation = 0;
            MaxProgress = 100;
            CloseEnabled = true;
            StartEnabled = true;
            OverWrite = false;
            ImportZipChecked = true;

            if (Properties.Settings.Default.LastImportFolder != "")
            {
                ImportPath = Properties.Settings.Default.LastImportFolder;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CloseEnabled
        {
            get
            {
                return closeEnabled;
            }

            set
            {
                if (closeEnabled != value)
                {
                    closeEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility FolderVisibility
        {
            get
            {
                return folderVisible;
            }

            set
            {
                if (folderVisible != value)
                {
                    folderVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ImportFolderChecked
        {
            get
            {
                return importFolderChecked;
            }

            set
            {
                if (importFolderChecked != value)
                {
                    importFolderChecked = value;
                    NotifyPropertyChanged();
                    if (importFolderChecked)
                    {
                        FolderVisibility = Visibility.Visible;
                        ZipVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public string ImportPath
        {
            get
            {
                return importPath;
            }

            set
            {
                if (importPath != value)
                {
                    importPath = value;
                }
                NotifyPropertyChanged();
            }
        }

        public bool ImportZipChecked
        {
            get
            {
                return importZipChecked;
            }

            set
            {
                if (importZipChecked != value)
                {
                    importZipChecked = value;
                    NotifyPropertyChanged();
                    if (importZipChecked)
                    {
                        FolderVisibility = Visibility.Hidden;
                        ZipVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public string ImportZipPath
        {
            get
            {
                return importZipPath;
            }

            set
            {
                if (importZipPath != value)
                {
                    importZipPath = value;
                }
                NotifyPropertyChanged();
            }
        }

        public int MaxModelsPerFile
        {
            get
            {
                return maxModelsPerFile;
            }

            set
            {
                if (maxModelsPerFile != value)
                {
                    maxModelsPerFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int MaxProgress
        {
            get
            {
                return maxProgress;
            }

            set
            {
                if (maxProgress != value)
                {
                    maxProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NewFolderName
        {
            get
            {
                return newFolderName;
            }

            set
            {
                if (value != newFolderName)
                {
                    if (ValidateFolderName(value))
                    {
                        newFolderName = value;
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        public bool OverWrite
        {
            get
            {
                return overWrite;
            }

            set
            {
                if (overWrite != value)
                {
                    overWrite = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ProgressValue
        {
            get
            {
                return progressValue;
            }

            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ResultsText
        {
            get
            {
                return resultsText;
            }

            set
            {
                if (resultsText != value)
                {
                    resultsText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool StartEnabled
        {
            get
            {
                return startEnabled;
            }

            set
            {
                if (startEnabled != value)
                {
                    startEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double XRotation
        {
            get
            {
                return xRotation;
            }

            set
            {
                if (xRotation != value)
                {
                    xRotation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double YRotation
        {
            get
            {
                return yRotation;
            }

            set
            {
                if (yRotation != value)
                {
                    yRotation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ZipVisibility
        {
            get
            {
                return zipVisible;
            }

            set
            {
                if (zipVisible != value)
                {
                    zipVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ZRotation
        {
            get
            {
                return zRotation;
            }

            set
            {
                if (zRotation != value)
                {
                    zRotation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), extension);
            return Path.Combine(path, fileName);
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static Task<bool> ImportOneFromZip(string zipPath, string fpath, double xRot, double yRot, double zRot, string targetSubFolder)
        {
            bool result = false;
            if (File.Exists(zipPath))
            {
                string tmpFile = GetTempFilePathWithExtension(".stl");
                if (ZipUtils.ExtractFileFromZip(zipPath, fpath, tmpFile))
                {
                    string rootName = System.IO.Path.GetFileNameWithoutExtension(fpath);
                    string targetPath = BaseViewModel.Project.ProjectPathToAbsPath(rootName + ".txt");
                    if (targetSubFolder != "")
                    {
                        targetPath = BaseViewModel.Project.ProjectPathToAbsPath(targetSubFolder + "\\" + rootName + ".txt");
                    }
                    if (!File.Exists(targetPath) || overWrite)
                    {
                        Document localDoc = null;
                        try
                        {
                            localDoc = new Document();
                            string fldr = System.IO.Path.GetDirectoryName(targetPath);
                            bool swapYZ = BaseViewModel.Project.SharedProjectSettings.ImportStlAxisSwap;
                            bool setCentroid = BaseViewModel.Project.SharedProjectSettings.SetOriginToCentroid;
                            localDoc.ImportStl(tmpFile, swapYZ, setCentroid);
                            int numObs = 0;
                            foreach (Object3D ob in localDoc.Content)
                            {
                                ob.Name = rootName;
                                if (numObs > 0)
                                {
                                    ob.Name += "_" + numObs.ToString();
                                }
                                // ob.FlipInside();
                                ob.FlipX();
                                ob.Color = BaseViewModel.Project.SharedProjectSettings.DefaultObjectColour;
                                ob.MoveOriginToCentroid();

                                if (xRot != 0 || yRot != 0 || zRot != 0)
                                {
                                    ob.Rotate(new System.Windows.Media.Media3D.Point3D(xRot, yRot, zRot));
                                }
                                ob.MoveToFloor();
                                ob.MoveToCentre();
                                numObs++;
                            }

                            localDoc.Save(targetPath);
                            // Add this file to project but don't allow duplicates
                            BaseViewModel.Project.AddFileToFolder(fldr, rootName + ".txt", false);
                            localDoc.Empty();
                            File.Delete(tmpFile);
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        localDoc?.Empty();
                        localDoc = null;
                        GC.Collect();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("File already exists: " + targetPath, "Error");
                    }
                }
            }
            return Task.FromResult<bool>(result);
        }

        private static Task<bool> ImportOneOfMany(string fpath, double xRot, double yRot, double zRot, string targetSubFolder)
        {
            bool result = false;
            string rootName = System.IO.Path.GetFileNameWithoutExtension(fpath);
            bool flip = false;
            bool swapYZ = BaseViewModel.Project.SharedProjectSettings.ImportStlAxisSwap;
            bool swapObjYZ = BaseViewModel.Project.SharedProjectSettings.ImportObjAxisSwap;
            bool swapOffYZ = BaseViewModel.Project.SharedProjectSettings.ImportOffAxisSwap;
            bool setCentroid = BaseViewModel.Project.SharedProjectSettings.SetOriginToCentroid;

            string targetPath = BaseViewModel.Project.ProjectPathToAbsPath(rootName + ".txt");
            if (targetSubFolder != "")
            {
                targetPath = BaseViewModel.Project.ProjectPathToAbsPath(targetSubFolder + "\\" + rootName + ".txt");
            }
            if (!File.Exists(targetPath) || overWrite)
            {
                Document localDoc = null;
                try
                {
                    localDoc = new Document();
                    string fldr = System.IO.Path.GetDirectoryName(targetPath);

                    if (fpath.ToLower().EndsWith("stl"))
                    {
                        localDoc.ImportStl(fpath, swapYZ, setCentroid);
                        flip = true;
                    }
                    else
                    if (fpath.ToLower().EndsWith("off"))
                    {
                        localDoc.ImportOffs(fpath, swapOffYZ, setCentroid);
                        flip = false;
                    }
                    else
                    if (fpath.ToLower().EndsWith("obj"))
                    {
                        localDoc.ImportObjs(fpath, swapObjYZ, setCentroid);
                        flip = false;
                    }
                    int numObs = 0;
                    foreach (Object3D ob in localDoc.Content)
                    {
                        ob.Name = rootName;
                        if (numObs > 0)
                        {
                            ob.Name += "_" + numObs.ToString();
                        }
                        // ob.FlipInside();
                        if (flip)
                        {
                            ob.FlipX();
                        }
                        ob.Color = BaseViewModel.Project.SharedProjectSettings.DefaultObjectColour;

                        if (xRot != 0 || yRot != 0 || zRot != 0)
                        {
                            ob.Rotate(new System.Windows.Media.Media3D.Point3D(xRot, yRot, zRot));
                        }
                        ob.MoveToFloor();
                        ob.MoveToCentre();
                        numObs++;
                    }

                    localDoc.Save(targetPath);
                    // Add this file to project but don't allow duplicates
                    BaseViewModel.Project.AddFileToFolder(fldr, rootName + ".txt", false);
                    localDoc.Empty();
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                localDoc?.Empty();
                localDoc = null;
                GC.Collect();
            }
            else
            {
                System.Windows.MessageBox.Show("File already exists: " + targetPath, "Error");
            }

            return Task.FromResult<bool>(result);
        }

        private void AppendResults(string v)
        {
            ResultsText += v;
            ResultsText += "\n";
            ResultsBox.CaretIndex = ResultsBox.Text.Length;
            ResultsBox.ScrollToEnd();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (Properties.Settings.Default.LastImportFolder != "")
            {
                dialog.SelectedPath = Properties.Settings.Default.LastImportFolder;
            }
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImportPath = dialog.SelectedPath;
            }
            ProgressValue = 0;
        }

        private void BrowseZipButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = ("Zip files(*.zip)|*.zip");
            dialog.Multiselect = true;
            if (Properties.Settings.Default.LastImportFolder != "")
            {
                dialog.InitialDirectory = Properties.Settings.Default.LastImportFolder;
            }
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // This one used for display
                ImportZipPath = "";
                foreach (string s in dialog.SafeFileNames)
                {
                    ImportZipPath += s + "; ";
                }
                ImportZipPath = ImportZipPath.Substring(0, ImportZipPath.Length - 2);
                // This one is the list for import
                ImportZipPaths = dialog.FileNames;
                CreateNewFolderNameFromZipFile();
            }
            ProgressValue = 0;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CreateNewFolderNameFromZipFile()
        {
            string s = "";
            if (ImportZipPaths.GetLength(0) > 0)
            {
                s = ImportZipPaths[0];
            }
            if (s != "")
            {
                s = System.IO.Path.GetFileNameWithoutExtension(s);
                s = s.Replace(' ', '_');
                NewFolderName = s;
            }
        }

        private async Task ImportModelsFromFolder(string targetSubFolder)
        {
            if (Directory.Exists(ImportPath))
            {
                Application.Current.Dispatcher.Invoke(() => {
                    CloseEnabled = false;
                    StartEnabled = false;
                });

                Properties.Settings.Default.LastImportFolder = ImportPath;
                Properties.Settings.Default.Save();
                ResultsText = "";

                var filteredFiles = Directory.GetFiles(ImportPath, "*.*")
    .Where(file => file.ToLower().EndsWith("stl") || file.ToLower().EndsWith("off"))
    .ToList();
                string[] files = filteredFiles.ToArray();
                double prog = 0;
                int numFiles = files.GetLength(0);
                if (numFiles > 0)
                {
                    MaxProgress = numFiles - 1;
                }

                foreach (string fpath in files)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        ProgressValue = prog;

                        AppendResults("Importing " + System.IO.Path.GetFileName(fpath));
                    });

                    await Task.Run(() => ImportOneOfMany(fpath, xRotation, yRotation, zRotation, targetSubFolder));
                    prog++;
                }
                AppendResults("Import Complete");
                GC.Collect();
                CloseEnabled = true;
                StartEnabled = true;
            }
        }

        private async Task ImportModelsFromZip(string targetSubFolder)
        {
            ResultsText = "";
            if (ImportZipPaths != null && ImportZipPaths.GetLength(0) > 0)
            {
                foreach (string zipname in ImportZipPaths)
                {
                    if (File.Exists(zipname))
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            CloseEnabled = false;
                            StartEnabled = false;
                        });

                        List<string> files = ZipUtils.ListFilesInZip(zipname, ".stl");
                        double prog = 0;
                        int numFiles = files.Count;
                        if (numFiles > 0)
                        {
                            MaxProgress = numFiles - 1;
                        }
                        foreach (string fpath in files)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                ProgressValue = prog;

                                AppendResults("Importing " + System.IO.Path.GetFileName(fpath));
                            });

                            await Task.Run(() => ImportOneFromZip(zipname, fpath, xRotation, yRotation, zRotation, targetSubFolder));
                            prog++;
                        }
                    }
                    AppendResults("Import Complete");
                    GC.Collect();
                    CloseEnabled = true;
                    StartEnabled = true;
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressValue = 0;
            // has the user specified a folder
            string nf = "";
            if (!String.IsNullOrEmpty(newFolderName))
            {
                nf = newFolderName.Trim();

                if (!String.IsNullOrEmpty(nf))
                {
                    string projPath = BaseViewModel.Project.BaseFolder;
                    string subPath = System.IO.Path.Combine(projPath, nf);
                    if (!Directory.Exists(subPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(subPath);

                            // must tell project to add the new folder too
                            BaseViewModel.Project.folders[0].CreateNamedFolder(nf);
                            NotificationManager.Notify("ImportRefresh", null); ;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            if (importFolderChecked)
            {
                await ImportModelsFromFolder(nf);
            }
            else
            {
                await ImportModelsFromZip(nf);
            }
        }

        private bool ValidateFolderName(string v)
        {
            bool res = true;
            if (v.Contains("\\") ||
                v.Contains("/") ||
                v.Contains(" ") ||
                v.Contains("\t") ||
                v.Contains("*") ||
                v.Contains("?") ||
                v.Contains("\"") ||
                v.Contains("\'")
                )
            {
                res = false;
            }
            return res;
        }
    }
}