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
        private List<String> printers;
        private List<String> extruders;
        private String selectedExtruder;
        private String selectedPrinter;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<String> Extruders
        {
            get { return extruders; }
            set
            {
                extruders = value;
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

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<String> Printers
        {
            get { return printers; }
            set
            {
                printers = value;
                NotifyPropertyChanged();
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            
        }
        private void OKClick(object sender, RoutedEventArgs e)
        {

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
                Printers = CuraEngineInterface.GetAvailableCuraPrinterDefinitions(SlicerPath + @"\Resources\definitions");
                Extruders = CuraEngineInterface.GetAvailableCuraExtruders(SlicerPath + @"\Resources\extruders");
            }
        }
    }
}