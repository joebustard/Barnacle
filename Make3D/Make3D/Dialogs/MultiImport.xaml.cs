using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private Visibility zipVisible;
        private bool importFolderChecked;
        private string importPath;
        private bool importZipChecked;
        private string importZipPath;
        private int maxModelsPerFile;
        private double progressValue;
        private string resultsText;
        private bool startEnabled;
        private double xRotation;
        private double yRotation;
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
            ImportFolderChecked = true;
            
            if (Properties.Settings.Default.LastImportFolder != "")
            {
                ImportPath = Properties.Settings.Default.LastImportFolder;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility FolderVisibility
        {
            get { return folderVisible; }
            set
            {
                if (folderVisible != value)
                {
                    folderVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ZipVisibility
        {
            get { return zipVisible; }
            set
            {
                if (zipVisible != value)
                {
                    zipVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CloseEnabled
        {
            get { return closeEnabled; }
            set
            {
                if (closeEnabled != value)
                {
                    closeEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ImportFolderChecked
        {
            get { return importFolderChecked; }
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
            get { return importPath; }
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
            get { return importZipChecked; }
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
            get { return importZipPath; }
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
            get { return maxModelsPerFile; }
            set
            {
                if (maxModelsPerFile != value)
                {
                    maxModelsPerFile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool OverWrite
        {
            get { return overWrite; }
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
            get { return progressValue; }
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
            get { return resultsText; }
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
            get { return startEnabled; }
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
            get { return xRotation; }
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
            get { return yRotation; }
            set
            {
                if (yRotation != value)
                {
                    yRotation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ZRotation
        {
            get { return zRotation; }
            set
            {
                if (zRotation != value)
                {
                    zRotation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static Task<bool> ImportOneOfMany(string fpath, double xRot, double yRot, double zRot)
        {
            bool result = false;
            string rootName = System.IO.Path.GetFileNameWithoutExtension(fpath);

            string targetPath = BaseViewModel.Project.ProjectPathToAbsPath(rootName + ".txt");
            if (!File.Exists(targetPath) || overWrite)
            {
                Document localDoc = null;
                try
                {
                    localDoc = new Document();
                    string fldr = System.IO.Path.GetDirectoryName(targetPath);

                    localDoc.ImportStl(fpath, BaseViewModel.Project.SharedProjectSettings.ImportAxisSwap);
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
            if (Properties.Settings.Default.LastImportFolder != "")
            {
                dialog.InitialDirectory = Properties.Settings.Default.LastImportFolder;
            }
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImportZipPath = dialog.FileName;
            }
            ProgressValue = 0;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressValue = 0;
            if (importFolderChecked)
            {
                await ImportModelsFromFolder();
            }
            else
            {
                await ImportModelsFromZip();
            }
        }

        private async Task ImportModelsFromFolder()
        {
            if (Directory.Exists(ImportPath))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CloseEnabled = false;
                    StartEnabled = false;
                });

                Properties.Settings.Default.LastImportFolder = ImportPath;
                Properties.Settings.Default.Save();
                ResultsText = "";
                string[] files = System.IO.Directory.GetFiles(ImportPath, "*.stl");
                double prog = 0;
                int numFiles = files.GetLength(0);
                if (numFiles > 0)
                {
                    MaxProgress = numFiles-1;
                }

                foreach (string fpath in files)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressValue = prog;

                        AppendResults("Importing " + System.IO.Path.GetFileName(fpath));
                    });

                    await Task.Run(() => ImportOneOfMany(fpath, xRotation, yRotation, zRotation));
                    prog++;
                }
                AppendResults("Import Complete");
                GC.Collect();
                CloseEnabled = true;
                StartEnabled = true;
            }
        }
        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), extension);
            return Path.Combine(path, fileName);
        }
        private static Task<bool> ImportOneFromZip(string zipPath,string fpath, double xRot, double yRot, double zRot)
        {
            bool result = false;
            if (File.Exists(zipPath))
            {
                string tmpFile = GetTempFilePathWithExtension(".stl");
                if (ZipUtils.ExtractFileFromZip(zipPath, fpath, tmpFile))
                {
                    string rootName = System.IO.Path.GetFileNameWithoutExtension(fpath);
                    string targetPath = BaseViewModel.Project.ProjectPathToAbsPath(rootName + ".txt");
                    if (!File.Exists(targetPath) || overWrite)
                    {
                        Document localDoc = null;
                        try
                        {
                            localDoc = new Document();
                            string fldr = System.IO.Path.GetDirectoryName(targetPath);

                            localDoc.ImportStl(tmpFile, BaseViewModel.Project.SharedProjectSettings.ImportAxisSwap);
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
        private int maxProgress;

        public int MaxProgress
        {
            get { return maxProgress; }
            set 
            {
                if (maxProgress != value )
                {
                    maxProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private async Task ImportModelsFromZip()
        {
            if (File.Exists(ImportZipPath))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CloseEnabled = false;
                    StartEnabled = false;
                });

                // Properties.Settings.Default.LastImportFolder = ImportPath;
                // Properties.Settings.Default.Save();
                ResultsText = "";
                List<string> files = ZipUtils.ListFilesInZip(importZipPath, ".stl");
                double prog = 0;
                int numFiles = files.Count;
                if ( numFiles>0)
                {
                    MaxProgress = numFiles-1;
                }
                foreach (string fpath in files)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProgressValue = prog;

                        AppendResults("Importing " + System.IO.Path.GetFileName(fpath));
                    });

                    await Task.Run(() => ImportOneFromZip(importZipPath, fpath, xRotation, yRotation, zRotation));
                    prog++;
                }
                AppendResults("Import Complete");
                GC.Collect();
                CloseEnabled = true;
                StartEnabled = true;
            }
        }
    }
}