using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Workflow;

namespace Barnacle.Dialogs.Slice
{
    /// <summary>
    /// Interaction logic for EditPrinter.xaml
    /// </summary>
    public partial class EditPrinter : Window, INotifyPropertyChanged
    {
        private List<String> curaPrinters;
        private List<String> curaExtruders;
        private String selectedExtruder;
        private String selectedPrinter;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> CuraExtruders
        {
            get { return curaExtruders; }
            set
            {
                curaExtruders = value;
                NotifyPropertyChanged();
            }
        }

        public String SelectedExtruder
        {
            get { return selectedExtruder; }
            set
            {
                selectedExtruder = value;

                NotifyPropertyChanged();
            }
        }

        public String SelectedPrinter
        {
            get { return selectedPrinter; }
            set
            {
                selectedPrinter = value;

                NotifyPropertyChanged();
            }
        }

        private string printerName;

        public String PrinterName
        {
            get { return printerName; }
            set
            {
                if (value != printerName)
                {
                    printerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private String startGCode;

        public String StartGCode
        {
            get { return startGCode; }
            set
            {
                if (value != startGCode)
                {
                    startGCode = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private String endGCode;

        public String EndGCode
        {
            get { return endGCode; }
            set
            {
                if (value != endGCode)
                {
                    endGCode = value;
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

        public List<String> CuraPrinters
        {
            get { return curaPrinters; }
            set
            {
                curaPrinters = value;
                NotifyPropertyChanged();
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public EditPrinter()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            String SlicerPath = Properties.Settings.Default.SlicerPath;

            if (SlicerPath != null && SlicerPath != "")
            {
                CuraPrinters = CuraEngineInterface.GetAvailableCuraPrinterDefinitions(SlicerPath + @"\share\cura\Resources\definitions");
                CuraExtruders = CuraEngineInterface.GetAvailableCuraExtruders(SlicerPath + @"\share\cura\Resources\extruders");
            }
        }
    }
}