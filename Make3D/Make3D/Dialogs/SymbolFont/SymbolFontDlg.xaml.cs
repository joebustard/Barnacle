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

using MakerLib;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for SymbolFont.xaml
    /// </summary>
    public partial class SymbolDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private const double maxsymbolLength = 200;
        private const double minsymbolLength = 5;
        private bool loaded;
        private string oldFont = "";
        private string oldSymbol = "";
        private Visibility show3D;
        private Visibility showBusy;
        private string symbolCode;
        private string symbolFont;
        private double symbolLength;
        private DispatcherTimer timer;
        private string warningText;

        public SymbolDlg()
        {
            InitializeComponent();
            ToolName = "Symbol";
            DataContext = this;

            loaded = false;
        }

        public Visibility Show3D
        {
            get
            {
                return show3D;
            }

            set
            {
                if (show3D != value)
                {
                    show3D = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility ShowBusy
        {
            get
            {
                return showBusy;
            }

            set
            {
                if (showBusy != value)
                {
                    showBusy = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string SymbolCode
        {
            get
            {
                return symbolCode;
            }

            set
            {
                if (symbolCode != value)
                {
                    symbolCode = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public string SymbolFont
        {
            get
            {
                return symbolFont;
            }

            set
            {
                if (value != symbolFont)
                {
                    symbolFont = value;
                }
            }
        }

        public double SymbolLength
        {
            get
            {
                return symbolLength;
            }

            set
            {
                if (value != symbolLength)
                {
                    if (value >= minsymbolLength && value <= maxsymbolLength)
                    {
                        symbolLength = value;
                        WarningText = "";
                        NotifyPropertyChanged();
                        UpdateDisplay();
                    }
                    else
                    {
                        WarningText = $"Symbol Length must be in the range {minsymbolLength} to {maxsymbolLength}";
                    }
                }
            }
        }

        public string WarningText
        {
            get
            {
                return warningText;
            }

            set
            {
                if (warningText != value)
                {
                    warningText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        private AsyncGeneratorResult GenerateAsync()
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();
            SymbolFontMaker maker = new SymbolFontMaker(symbolCode, symbolFont, symbolLength);
            maker.Generate(v1, i1);
            AsyncGeneratorResult res = new AsyncGeneratorResult();
            // extract the vertices and indices to thread safe arrays
            // while still in the async function
            res.points = new Point3D[v1.Count];
            for (int i = 0; i < v1.Count; i++)
            {
                res.points[i] = new Point3D(v1[i].X, v1[i].Y, v1[i].Z);
            }
            res.indices = new int[i1.Count];
            for (int i = 0; i < i1.Count; i++)
            {
                res.indices[i] = i1[i];
            }
            v1.Clear();
            i1.Clear();
            return (res);
        }

        private async void GenerateShape()
        {
            oldSymbol = symbolCode;
            oldFont = symbolFont;
            ClearShape();
            Viewer.Busy();
            EditingEnabled = false;
            AsyncGeneratorResult result;
            result = await Task.Run(() => GenerateAsync());
            GetVerticesFromAsyncResult(result);
            CentreVertices();
            Viewer.NotBusy();
            EditingEnabled = true;
        }

        private void LoadEditorParameters()
        {
            // load back the tool specific parameters
            SymbolFont = EditorParameters.Get("SymbolFont");
            SymbolCode = EditorParameters.Get("SymbolCode");
            SymbolLength = EditorParameters.GetDouble("SymbolLength", 25);
        }

        private void OnSymbolChanged(string symbol, string fontName)
        {
            SymbolFont = fontName;
            SymbolCode = symbol;
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            // save the parameters for the tool
            EditorParameters.Set("SymbolCode", SymbolCode.ToString());
            EditorParameters.Set("SymbolFont", SymbolFont.ToString());
            EditorParameters.Set("SymbolLength", SymbolLength.ToString());
        }

        private void SetDefaults()
        {
            loaded = false;
            SymbolCode = "A";

            loaded = true;
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            timer.Stop();
            GenerateShape();
            Viewer.Model = GetModel();
            ShowBusy = Visibility.Hidden;
        }

        private void UpdateDisplay()
        {
            if (loaded)
            {
                ClearShape();
                // ShowBusy = Visibility.Visible;

                timer.Start();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WarningText = "";
            LoadEditorParameters();

            UpdateCameraPos();
            Viewer.Clear();
            loaded = true;
            timer = new DispatcherTimer();
            timer.Interval = new System.TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            UpdateDisplay();
            SymbolSelection.OnSymbolChanged = OnSymbolChanged;
            ShowBusy = Visibility.Hidden;
            Show3D = Visibility.Visible;
        }
    }
}