using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for RibManager.xaml
    /// </summary>
    public partial class RibManager : UserControl, INotifyPropertyChanged
    {
        public int numberOfProfilePoints;
        public CommandHandler OnCommandHandler;

        private bool controlsEnabled;

        private int modelType;
        private ObservableCollection<ImagePathControl> ribs;

        private ImagePathControl selectedRib;

        public RibManager()
        {
            InitializeComponent();
            ribs = new ObservableCollection<ImagePathControl>();
            DataContext = this;
            NextNameLetter = 'A';
            NextNameNumber = 0;
            selectedRib = null;
            controlsEnabled = true;
            numberOfProfilePoints = 80;
            selectedRibIndex = -1;
        }

        public delegate void CommandHandler(string command);

        public delegate void RibAdded(string ribName, ImagePathControl rc);

        public delegate void RibDeleted(ImagePathControl rc);

        public delegate void RibInserted(string ribName, ImagePathControl rc, ImagePathControl after);

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

        public int NumberOfProfilePoints
        {
            get
            {
                return numberOfProfilePoints;
            }
            internal set
            {
                if (value != numberOfProfilePoints)
                {
                    numberOfProfilePoints = value;
                    foreach (ImagePathControl rc in ribs)
                    {
                        rc.NumDivisions = numberOfProfilePoints;
                    }
                    GenerateProfiles();
                }
            }
        }

        public RibAdded OnRibAdded { get; set; }

        public RibDeleted OnRibDeleted { get; set; }

        public RibInserted OnRibInserted { get; set; }

        public RibsRenamed OnRibsRenamed { get; set; }

        public ObservableCollection<ImagePathControl> Ribs
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
                    NotifyPropertyChanged();
                }
            }
        }

        public ImagePathControl SelectedRib
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
            ImagePathControl src = null;
            foreach (ImagePathControl rc in Ribs)
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

        internal void OnForceRibReload(string s)
        {
            foreach (ImagePathControl rc in Ribs)
            {
                if (rc.ImagePath == s)
                {
                    rc.FetchImage(true);
                    if (rc.IsValid)
                    {
                        rc.SetImageSource();
                    }
                }
            }
        }

        private void RenameRibs_Click(object sender, RoutedEventArgs e)
        {
            if (ribs.Count > 0)
            {
                List<NameRec> nameRecs = new List<NameRec>();

                int j = 1;
                for (int i = 0; i < Ribs.Count; i++)
                {
                    NameRec rec = new NameRec();
                    rec.ribIndex = i;
                    rec.originalName = Ribs[i].Header;
                    if (i <= 26)
                    {
                        rec.newName = ((char)('A' + i)).ToString();
                    }
                    else
                    {
                        rec.newName = "Z" + j.ToString();
                        j++;
                    }
                    nameRecs.Add(rec);
                }
                NextNameLetter = (char)('A' + Ribs.Count);
                NextNameNumber = 0;
                if (Ribs.Count > 26)
                {
                    NextNameLetter = 'Z';
                    NextNameNumber = Ribs.Count - 26;
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
        }

        private void AddRib_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ImagePathControl rc = new ImagePathControl();

                    rc.ImagePath = dlg.FileName;
                    rc.FetchImage();
                    if (rc.IsValid)
                    {
                        rc.OnForceReload = OnForceRibReload;
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
                        rc.Width = 500;
                        rc.Height = 500;
                        Ribs.Add(rc);
                        NotifyPropertyChanged("Ribs");
                        if (OnRibAdded != null)
                        {
                            OnRibAdded(name, rc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "AddRib_Clicked");
                }
            }
        }

        private void Copy(ImagePathControl src)
        {
            ImagePathControl clone = src.Clone();
            string nameStart = clone.Header.Substring(0, 1);
            int subName = 0;
            foreach (ImagePathControl rc in Ribs)
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
            foreach (ImagePathControl rc in ribs)
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

        private int selectedRibIndex;

        public int SelectedRibIndex
        {
            get { return selectedRibIndex; }
            set
            {
                if (value != selectedRibIndex)
                {
                    selectedRibIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NextRib_Clicked(object sender, RoutedEventArgs e)
        {
            if (selectedRibIndex < ribs.Count - 1)
            {
                SelectedRibIndex++;
                SelectedRib = ribs[selectedRibIndex];
                RibList.ScrollIntoView(selectedRib);
            }
        }

        private void PreviousRib_CLicked(object sender, RoutedEventArgs e)
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = ribs[selectedRibIndex];
                RibList.ScrollIntoView(selectedRib);
            }
        }
    }
}