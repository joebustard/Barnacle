/**************************************************************************
*   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
*                                                                         *
*   This file is part of the Barnacle 3D application.                     *
*                                                                         *
*   This application is free software; you can redistribute it and/or     *
*   modify it under the terms of the GNU Library General Public           *
*   License as published by the Free Software Foundation; either          *
*   version 2 of the License, or (at your option) any later version.      *
*                                                                         *
*   This application is distributed in the hope that it will be useful,   *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
*   GNU Library General Public License for more details.                  *
*                                                                         *
**************************************************************************/

using Barnacle.Models;
using Barnacle.Object3DLib;
using Barnacle.ViewModels;
using FixLib;
using HoleLibrary;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for AutoFixDlg.xaml
    /// </summary>
    public partial class AutoFixDlg : Window, INotifyPropertyChanged
    {
        private bool canClose;
        private bool canFix;
        private bool removeDuplicates;
        private bool removeHoles;
        private string resultsText;

        public AutoFixDlg()
        {
            InitializeComponent();
            DataContext = this;
            removeHoles = true;
            removeDuplicates = true;
            CanFix = true;
            CanClose = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanClose
        {
            get { return canClose; }

            set
            {
                canClose = value;
                NotifyPropertyChanged();
            }
        }

        public bool CanFix
        {
            get { return canFix; }

            set
            {
                canFix = value;
                NotifyPropertyChanged();
            }
        }

        public bool RemoveDuplicates
        {
            get { return removeDuplicates; }

            set
            {
                if (removeDuplicates != value)
                {
                    removeDuplicates = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoveHoles
        {
            get { return removeHoles; }

            set
            {
                if (removeHoles != value)
                {
                    removeHoles = value;
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

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AppendResults(string s, bool crlf = true)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ResultsText += s;
                    if (crlf)
                    {
                        ResultsText += "\n";
                    }
                    ResultsBox.CaretIndex = ResultsBox.Text.Length;
                    ResultsBox.ScrollToEnd();
                }));
        }

        private async Task AutoFixDocument(string fullPath)
        {
            AppendResults($"{fullPath}");
            Document doc = new Document();

            doc.Load(fullPath);
            foreach (Object3D ob in doc.Content)
            {
                AppendResults($" {ob.Name}");
                if (removeDuplicates) RemoveDuplicateVertices(ob);
                if (removeHoles) FixHoles(ob);
            }
            AppendResults($"");
            doc.Save(fullPath);
            doc.Clear();
        }

        private void ClearResults()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    ResultsText = "";
                }));
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FixHoles(Object3D ob)
        {
            HoleFinder hf = new HoleFinder(ob.RelativeObjectVertices, ob.TriangleIndices);
            Tuple<int, int> res = hf.FindHoles();

            ob.Remesh();

            ob.CalcScale(false);
            if (res.Item1 > 0)
            {
                AppendResults($"    Found {res.Item1.ToString()} holes, Filled {res.Item2.ToString()}");
            }
        }

        private void RemoveDuplicateVertices(Object3D ob)
        {
            int numberRemoved = ob.RelativeObjectVertices.Count;
            Fixer checker = new Fixer();
            Point3DCollection points = new Point3DCollection();
            PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, points);

            checker.RemoveDuplicateVertices(points, ob.TriangleIndices);
            PointUtils.PointCollectionToP3D(checker.Vertices, ob.RelativeObjectVertices);
            ob.TriangleIndices = checker.Faces;
            ob.Remesh();
            numberRemoved -= ob.RelativeObjectVertices.Count;
            if (numberRemoved > 0)
            {
                AppendResults($"    Removed {numberRemoved} vertices");
            }
        }

        private async void StartClicked(object sender, RoutedEventArgs e)
        {
            ClearResults();
            CanClose = false;
            CanFix = false;
            string original = BaseViewModel.Document.FilePath;
            if (BaseViewModel.Document.Dirty)
            {
                MessageBoxResult res = MessageBox.Show("Open document has changed. Save first?", "Warning", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    BaseViewModel.Document.Save(BaseViewModel.Document.FilePath);
                }
            }
            BaseViewModel.Document.Clear();
            string[] filenames = BaseViewModel.Project.GetExportFiles(".txt");
            String pth = BaseViewModel.Project.BaseFolder;
            foreach (string fullPath in filenames)
            {
                await Task.Run(() => AutoFixDocument(fullPath));
            }

            BaseViewModel.Document.Load(original);
            AppendResults("Done");
            CanClose = true;
            CanFix = true;
        }
    }
}