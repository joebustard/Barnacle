using Barnacle.Dialogs;
using Barnacle.Dialogs.RibbedFuselage.Models;
using Barnacle.RibbedFuselage.Models;
using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barnacle.RibbedFuselage.ViewModels
{
    internal class FuselageViewModel : INotifyPropertyChanged
    {
        private bool dirty;

        private string filePath;

        private FuselageModel fuselageData;

        private RibImageDetailsModel selectedRib;

        private int selectedRibIndex;

        private String sideImage;

        private string sidePath;

        private String topImage;

        private string topPath;

        public FuselageViewModel()
        {
            FuselageData = new FuselageModel();
            RibCommand = new RelayCommand(OnRibComand);
            SelectedRib = null;
            SelectedRibIndex = -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FuselageModel FuselageData
        {
            get { return fuselageData; }
            set
            {
                if (fuselageData != value)
                {
                    fuselageData = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand RibCommand { get; set; }

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get { return fuselageData.Ribs; }
        }

        public RibImageDetailsModel SelectedRib
        {
            get { return selectedRib; }
            set
            {
                if (selectedRib != value)
                {
                    selectedRib = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public String SideImage
        {
            get { return sideImage; }
            set
            {
                if (sideImage != value)
                {
                    sideImage = value;
                    FuselageData?.SetSideImage(sideImage);
                    NotifyPropertyChanged();
                }
            }
        }

        public string SidePath
        {
            get { return sidePath; }
            set
            {
                if (sidePath != value)
                {
                    sidePath = value;
                    FuselageData.SetSidePath(sidePath);
                    NotifyPropertyChanged();
                }
            }
        }

        internal List<LetterMarker> GetMarkers()
        {
            List<LetterMarker> res = new List<LetterMarker>();
            int nextY = 10;
            foreach (MarkerModel m in fuselageData.Markers)
            {
                LetterMarker lm = new LetterMarker(m.Name, new System.Drawing.Point((int)m.Position, nextY));
                res.Add(lm);
                nextY = 40 - nextY;
            }
            return res;
        }

        internal void MoveMarker(string s, int x, bool finishedMove)
        {
            if (finishedMove)
            {
                foreach (MarkerModel m in fuselageData.Markers)
                {
                    if (m.Name == s)
                    {
                        m.Position = (double)x;
                        break;
                    }
                }
                // regenerate the 3d fuselage
            }
        }

        public String TopImage
        {
            get { return topImage; }
            set
            {
                if (topImage != value)
                {
                    topImage = value;
                    FuselageData?.SetTopImage(topImage);
                    NotifyPropertyChanged();
                }
            }
        }

        public string TopPath
        {
            get { return topPath; }
            set
            {
                if (string.Compare(topPath, value) != 0)
                {
                    topPath = value;
                    FuselageData.SetTopPath(topPath);
                    NotifyPropertyChanged();
                }
            }
        }

        public void Load()
        {
            OpenFileDialog opDlg = new OpenFileDialog();
            opDlg.Filter = "Fusalage spar files (*.spr) | *.spr";
            if (opDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filePath = opDlg.FileName;
                    if (fuselageData != null)
                    {
                        fuselageData.Load(filePath);
                        NotifyPropertyChanged("Ribs");
                        if (Ribs.Count > 0)
                        {
                            SelectedRib = Ribs[0];
                            selectedRibIndex = 0;
                        }
                        TopImage = fuselageData.TopImageDetails.ImageFilePath;
                        TopPath = fuselageData.TopImageDetails.FlexiPathText;
                        SideImage = fuselageData.SideImageDetails.ImageFilePath;
                        SidePath = fuselageData.SideImageDetails.FlexiPathText;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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

        public void Save()
        {
            if (String.IsNullOrEmpty(filePath))
            {
                SaveAs();
            }
            else
            {
                Write(filePath);
                dirty = false;
            }
        }

        private void NextRib()
        {
            if (selectedRibIndex < fuselageData.Ribs.Count - 1)
            {
                SelectedRibIndex++;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
        }

        private void OnRibComand(object obj)
        {
            string prm = obj as string;
            if (!string.IsNullOrEmpty(prm))
            {
                switch (prm.ToLower())
                {
                    case "append":
                        {
                            RibImageDetailsModel rib = fuselageData.AddRib();
                            NotifyPropertyChanged("Ribs");
                            SelectedRib = rib;
                            fuselageData.AddMarker(rib.Name);
                        }
                        break;

                    case "copy":
                        {
                            SelectedRib = fuselageData.CloneRib(SelectedRibIndex);
                        }
                        break;

                    case "rename":
                        {
                            fuselageData.RenameAllRibs();
                        }
                        break;

                    case "delete":
                        {
                            if (fuselageData.DeleteRib(selectedRib))
                            {
                                NotifyPropertyChanged("Ribs");
                            }
                        }
                        break;

                    case "load":
                        {
                            Load();
                        }
                        break;

                    case "save":
                        {
                            Save();
                        }
                        break;

                    case "previous":
                        {
                            PreviousRib();
                        }
                        break;

                    case "next":
                        {
                            NextRib();
                        }
                        break;
                }
            }
        }

        private void PreviousRib()
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
            }
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Fuselage spar files (*.spr) | *.spr";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Write(saveFileDialog.FileName);
                filePath = saveFileDialog.FileName;
                dirty = false;
            }
        }

        private void Write(string filePath)
        {
            if (fuselageData != null)
            {
                fuselageData.Save(filePath);
            }
        }
    }
}