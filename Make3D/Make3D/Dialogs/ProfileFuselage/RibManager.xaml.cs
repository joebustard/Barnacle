﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Make3D.Dialogs
{
    /// <summary>
    /// Interaction logic for RibManager.xaml
    /// </summary>
    public partial class RibManager : UserControl, INotifyPropertyChanged
    {
        public CommandHandler OnCommandHandler;

        private bool controlsEnabled;

        private int modelType;
        private ObservableCollection<RibControl> ribs;

        private RibControl selectedRib;

        public RibManager()
        {
            InitializeComponent();
            ribs = new ObservableCollection<RibControl>();
            DataContext = this;
            NextNameLetter = 'A';
            NextNameNumber = 0;
            selectedRib = null;
            controlsEnabled = true;
        }

        public delegate void CommandHandler(string command);

        public delegate void RibAdded(string ribName, RibControl rc);

        public delegate void RibDeleted(RibControl rc);

        public delegate void RibInserted(string ribName, RibControl rc, RibControl after);

        public delegate void RibsRenamed(List<NameRec> newNames);

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ControlsEnabled
        {
            get
            {
                return controlsEnabled;
            }
            set
            {
                if (controlsEnabled != value)
                {
                    controlsEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int ModelType
        {
            get
            {
                return modelType;
            }
            set
            {
                if (modelType != value)
                {
                    modelType = value;
                    GenerateProfiles();
                }
            }
        }

        public char NextNameLetter { get; set; }

        public int NextNameNumber { get; set; }

        public RibAdded OnRibAdded { get; set; }

        public RibDeleted OnRibDeleted { get; set; }

        public RibInserted OnRibInserted { get; set; }

        public RibsRenamed OnRibsRenamed { get; set; }

        public ObservableCollection<RibControl> Ribs
        {
            get
            {
                return ribs;
            }
            set
            {
                if (ribs != value)
                {
                    ribs = value;
                }
            }
        }

        public RibControl SelectedRib
        {
            get
            {
                return selectedRib;
            }
            set
            {
                if (selectedRib != value)
                {
                    selectedRib = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void CopyARib(string name)
        {
            selectedRib = null;
            RibControl src = null;
            foreach (RibControl rc in Ribs)
            {
                if (rc.Header == name)
                {
                    src = rc;
                    break;
                }
            }
            if (src != null)
            {
                Copy(src);
            }
        }

        private void AddRib_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    RibControl rc = new RibControl();

                    rc.Width = 200;
                    rc.Height = 200;
                    rc.ImagePath = dlg.FileName;
                    rc.FetchImage();

                    rc.ClearSinglePixels();
                    rc.FindEdge();

                    rc.SetImageSource();

                    string name = "" + NextNameLetter;
                    if (NextNameNumber > 0)
                    {
                        name += NextNameNumber.ToString();
                    }
                    NextNameLetter++;
                    if (NextNameLetter == 'Z' + 1)
                    {
                        NextNameLetter = 'A';
                        NextNameNumber++;
                    }
                    rc.Header = name;
                    Ribs.Add(rc);
                    NotifyPropertyChanged("Ribs");
                    if (OnRibAdded != null)
                    {
                        OnRibAdded(name, rc);
                    }
                }
                catch
                {
                }
            }
        }

        private void Copy(RibControl src)
        {
            RibControl clone = src.Clone();
            string nameStart = clone.Header.Substring(0, 1);
            int subName = 0;
            foreach (RibControl rc in Ribs)
            {
                if (rc.Header.StartsWith(nameStart))
                {
                    subName++;
                }
            }
            clone.Header = nameStart + subName.ToString();
            int index = Ribs.IndexOf(src);
            if (index >= 0 && index < Ribs.Count - 1)
            {
                Ribs.Insert(index + 1, clone);
            }
            else
            {
                Ribs.Add(clone);
            }
            clone.SetImageSource();
            NotifyPropertyChanged("Ribs");
            if (OnRibInserted != null)
            {
                OnRibInserted(clone.Header, clone, src);
            }
            RenameRibs(nameStart);
        }

        private void CopyRib_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRib != null)
            {
                Copy(selectedRib);
            }
        }

        private void DeleteRib_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRib != null)
            {
                if (Ribs.Contains(selectedRib))
                {
                    Ribs.Remove(selectedRib);

                    if (OnRibDeleted != null)
                    {
                        OnRibDeleted(selectedRib);
                    }
                    NotifyPropertyChanged("Ribs");
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke("Export");
        }

        private void GenerateProfiles()
        {
            foreach (RibControl rc in ribs)
            {
                rc.GenerateProfilePoints(modelType);
            }
        }

        private void LoadRibs_Click(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke("LoadProject");
        }

        private void LoadSide_Click(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke("LoadSide");
        }

        private void LoadTop_Click(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke("LoadTop");
        }

        private void RenameRibs(string nameStart)
        {
            List<NameRec> nameRecs = new List<NameRec>();
            int j = 0;
            for (int i = 0; i < Ribs.Count; i++)
            {
                if (Ribs[i].Header.StartsWith(nameStart))
                {
                    NameRec rec = new NameRec();
                    rec.ribIndex = i;
                    rec.originalName = Ribs[i].Header;
                    if (j == 0)
                    {
                        rec.newName = nameStart;
                    }
                    else
                    {
                        rec.newName = nameStart + j.ToString();
                    }
                    nameRecs.Add(rec);
                    j++;
                }
            }

            // now rename the ribs locally
            for (int i = 0; i < nameRecs.Count; i++)
            {
                Ribs[nameRecs[i].ribIndex].Header = nameRecs[i].newName;
                Ribs[nameRecs[i].ribIndex].UpdateHeaderLabel();
            }

            // pass the data on to the main dialog to that the marker names match
            if (OnRibsRenamed != null)
            {
                OnRibsRenamed(nameRecs);
            }
        }

        private void SaveRibs_Click(object sender, RoutedEventArgs e)
        {
            OnCommandHandler?.Invoke("SaveProject");
        }

        public struct NameRec
        {
            public string newName;
            public string originalName;
            public int ribIndex;
        }
    }
}