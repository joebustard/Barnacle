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
using System.Threading;
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
        private bool alignFaces;
        private bool canClose;
        private bool canFix;
        private bool canStop;
        private bool removeDuplicates;
        private bool removeDuplicatesFaces;
        private bool removeHoles;
        private string resultsText;

        private CancellationToken token;

        private CancellationTokenSource tokenSource;

        public AutoFixDlg()
        {
            InitializeComponent();
            DataContext = this;
            removeHoles = true;
            removeDuplicates = true;
            canFix = true;
            canClose = true;
            canStop = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AlignFaces
        {
            get
            {
                return alignFaces;
            }

            set
            {
                if (alignFaces != value)
                {
                    alignFaces = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanClose
        {
            get
            {
                return canClose;
            }

            set
            {
                if (canClose != value)
                {
                    canClose = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanFix
        {
            get
            {
                return canFix;
            }

            set
            {
                if (canFix != value)
                {
                    canFix = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CanStop
        {
            get
            {
                return canStop;
            }

            set
            {
                if (canStop != value)
                {
                    canStop = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoveDuplicates
        {
            get
            {
                return removeDuplicates;
            }

            set
            {
                if (removeDuplicates != value)
                {
                    removeDuplicates = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoveDuplicatesFaces
        {
            get
            {
                return removeDuplicatesFaces;
            }

            set
            {
                if (removeDuplicatesFaces != value)
                {
                    removeDuplicatesFaces = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RemoveHoles
        {
            get
            {
                return removeHoles;
            }

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
                new Action(() => {
                    ResultsText += s;
                    if (crlf)
                    {
                        ResultsText += "\n";
                    }
                    ResultsBox.CaretIndex = ResultsBox.Text.Length;
                    ResultsBox.ScrollToEnd();
                }));
        }

        private async Task AutoFixDocument(string fullPath, CancellationToken token)
        {
            AppendResults($"{fullPath}");
            Document doc = new Document();
            bool dontReportMissingReferences = false;
            doc.Load(fullPath, dontReportMissingReferences);
            foreach (Object3D ob in doc.Content)
            {
                if (!token.IsCancellationRequested)
                {
                    AppendResults($" {ob.Name}");
                    if (removeDuplicates)
                    {
                        RemoveDuplicateVertices(ob, token);
                    }
                    if (removeDuplicatesFaces)
                    {
                        var numberRemovedFaces = DuplicateTriangles.RemoveBothDuplicateTriangles(ob.RelativeObjectVertices.Count, ob.TriangleIndices);
                        if (numberRemovedFaces > 0)
                        {
                            AppendResults($"    Removed {numberRemovedFaces} duplicate faces");
                        }
                    }
                    if (alignFaces)
                    {
                        var numberReversed = ob.OrientateFaceNormals();
                        if (numberReversed > 0)
                        {
                            AppendResults($"    Reversed {numberReversed} faces");
                        }
                    }
                    if (removeHoles)
                    {
                        FixHoles(ob, token);
                    }
                }
            }
            if (!token.IsCancellationRequested)
            {
                AppendResults($"");
                doc.Save(fullPath);
            }
            doc.Clear();
        }

        private void ClearResults()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => {
                    ResultsText = "";
                }));
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FixHoles(Object3D ob, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                HoleFinder hf = new HoleFinder(ob.RelativeObjectVertices, ob.TriangleIndices, token);
                Tuple<int, int> res = hf.FindHoles(token);

                ob.Remesh();

                ob.CalcScale(false);
                if (res.Item1 > 0)
                {
                    AppendResults($"    Found {res.Item1.ToString()} holes, Filled {res.Item2.ToString()}");
                }
            }
        }

        private void RemoveDuplicateVertices(Object3D ob, CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                int numberRemoved = ob.RelativeObjectVertices.Count;
                Fixer checker = new Fixer();
                if (!token.IsCancellationRequested)
                {
                    Point3DCollection points = new Point3DCollection();
                    PointUtils.P3DToPointCollection(ob.RelativeObjectVertices, points);
                    if (!token.IsCancellationRequested)
                    {
                        checker.RemoveDuplicateVerticesCancellable(points, ob.TriangleIndices, token);
                        if (!token.IsCancellationRequested)
                        {
                            PointUtils.PointCollectionToP3D(checker.Vertices, ob.RelativeObjectVertices);

                            ob.TriangleIndices = checker.Faces;
                            ob.Remesh();
                            numberRemoved -= ob.RelativeObjectVertices.Count;
                            if (numberRemoved > 0)
                            {
                                AppendResults($"   Removed {numberRemoved} vertices");
                            }
                        }
                    }
                }
            }
        }

        private async void StartClicked(object sender, RoutedEventArgs e)
        {
            ClearResults();
            CanClose = false;
            CanFix = false;
            CanStop = true;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
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
                if (!token.IsCancellationRequested)
                {
                    await Task.Run(() => AutoFixDocument(fullPath, token), token);
                }
            }

            BaseViewModel.Document.Load(original);
            AppendResults("All Done");
            CanClose = true;
            CanFix = true;
            CanStop = false;
            e.Handled = true;
        }

        private void StopClicked(object sender, RoutedEventArgs e)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                AppendResults("Cancelled");
            }
        }
    }
}