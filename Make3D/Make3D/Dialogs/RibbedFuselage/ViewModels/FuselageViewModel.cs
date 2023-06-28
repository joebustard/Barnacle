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
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public RelayCommand RibCommand { get; set; }

        public FuselageViewModel()
        {
            FuselageData = new FuselageModel();
            RibCommand = new RelayCommand(OnRibComand);
            SelectedRib = null;
            SelectedRibIndex = -1;
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
                            fuselageData.AddRib();
                            NotifyPropertyChanged("Ribs");
                        }
                        break;

                    case "copy":
                        {
                        }
                        break;

                    case "rename":
                        {
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

        private void NextRib()
        {
            if (selectedRibIndex < fuselageData.Ribs.Count - 1)
            {
                SelectedRibIndex++;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
                // RibList.ScrollIntoView(selectedRib);
            }
        }

        private void PreviousRib()
        {
            if (selectedRibIndex > 0)
            {
                SelectedRibIndex--;
                SelectedRib = fuselageData.Ribs[selectedRibIndex];
                //RibList.ScrollIntoView(selectedRib);
            }
        }

        private FuselageModel fuselageData;

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

        public ObservableCollection<RibImageDetailsModel> Ribs
        {
            get { return fuselageData.Ribs; }
        }

        private RibImageDetailsModel selectedRib;
        private string filePath;
        private bool dirty;

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

        private void Write(string filePath)
        {
            if (fuselageData != null)
            {
                fuselageData.Save(filePath);
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
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            
        }
    }
}