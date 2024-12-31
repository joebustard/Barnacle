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

using MakerLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TextDlg.xaml
    /// </summary>
    public partial class TextDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private string dataPath;
        private bool fontBold;
        private bool fontItalic;
        private bool isCentreAligned;
        private bool isLeftAligned;
        private bool isRightAligned;
        private bool loaded;
        private DispatcherTimer regenTimer;
        private String selectedFont;
        private ObservableCollection<FontFamily> systemFonts = new ObservableCollection<FontFamily>();
        private string text;
        private double textLength;
        private double thickness;
        private string warningText;

        public TextDlg()
        {
            InitializeComponent();
            loaded = false;
            ToolName = "Text";
            DataContext = this;
            text = "A";
            dataPath = "f1M0,0";
            Properties.Settings.Default.Reload();
            regenTimer = new DispatcherTimer();
            regenTimer.Interval = new TimeSpan(0, 0, Properties.Settings.Default.RegenerationDelay);
            regenTimer.Tick += RegenTimer_Tick;
        }

        public bool FontBold
        {
            get
            {
                return fontBold;
            }
            set
            {
                if (fontBold != value)
                {
                    fontBold = value;
                    NotifyPropertyChanged();

                    UpdateDisplay();
                }
            }
        }

        public bool FontItalic
        {
            get
            {
                return fontItalic;
            }
            set
            {
                if (fontItalic != value)
                {
                    fontItalic = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool IsCentreAligned
        {
            get
            {
                return isCentreAligned;
            }
            set
            {
                if (value != isCentreAligned)
                {
                    isCentreAligned = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool IsLeftAligned
        {
            get
            {
                return isLeftAligned;
            }
            set
            {
                if (value != isLeftAligned)
                {
                    isLeftAligned = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public bool IsRightAligned
        {
            get
            {
                return isRightAligned;
            }
            set
            {
                if (value != isRightAligned)
                {
                    isRightAligned = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public String PathData
        {
            get
            {
                return dataPath;
            }
            set
            {
                if (value != dataPath)
                {
                    dataPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String SelectedFont
        {
            get
            {
                return selectedFont;
            }
            set
            {
                if (selectedFont != value)
                {
                    selectedFont = value;
                    NotifyPropertyChanged();
                    UpdateDisplay();
                }
            }
        }

        public ObservableCollection<FontFamily> SystemFonts
        {
            get
            {
                return systemFonts;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    NotifyPropertyChanged();

                    UpdateDisplay();
                }
            }
        }

        public double TextLength
        {
            get
            {
                return textLength;
            }
            set
            {
                if (textLength != value)
                {
                    if (value >= 5)
                    {
                        textLength = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                        WarningText = "";
                    }
                    else
                    {
                        WarningText = "Minimum TextLength is 5";
                    }
                }
            }
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
            set
            {
                if (thickness != value)
                {
                    if (value > 0)
                    {
                        thickness = value;
                        NotifyPropertyChanged();
                        UpdateDisplay();
                        WarningText = "";
                    }
                    else
                    {
                        WarningText = "Thickness must be > 0";
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

        public void LoadSystemFonts()
        {
            systemFonts.Clear();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());

            foreach (var f in fonts)
                systemFonts.Add(f);

            NotifyPropertyChanged("SystemFonts");
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (regenTimer.IsEnabled)
            {
                regenTimer.Stop();
                Regenerate();
            }
            else
            {
                base.SaveSizeAndLocation();
                SaveEditorParmeters();
                DialogResult = true;
                Close();
            }
        }

        private AsyncGeneratorResult GenerateAsync(bool final, int alignment)
        {
            Point3DCollection v1 = new Point3DCollection();
            Int32Collection i1 = new Int32Collection();

            TextMaker mk = new TextMaker(text, selectedFont, textLength, thickness, final, fontBold, fontItalic, alignment);
            string PathCode = mk.Generate(v1, i1);
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

        private async void GenerateShape(bool final = false)
        {
            ClearShape();
            if (text != null && text != "")
            {
                int alignment = 0;
                if (isCentreAligned)
                {
                    alignment = 1;
                }
                if (isRightAligned)
                {
                    alignment = 2;
                }
                Viewer.Busy();
                EditingEnabled = false;
                AsyncGeneratorResult result;
                result = await Task.Run(() => GenerateAsync(final, alignment));
                GetVerticesFromAsyncResult(result);
                CentreVertices();
                Viewer.NotBusy();
                EditingEnabled = true;
            }
        }

        private void LoadEditorParameters()
        {
            if (EditorParameters.Get("Text") != "")
            {
                Text = EditorParameters.Get("Text");
                SelectedFont = EditorParameters.Get("FontName");
                TextLength = EditorParameters.GetDouble("TextLength", 100);
                Thickness = EditorParameters.GetDouble("Thickness", 5);
                FontBold = EditorParameters.GetBoolean("Bold");
                FontItalic = EditorParameters.GetBoolean("Italic");
                IsLeftAligned = EditorParameters.GetBoolean("IsLeftAligned", true);
                IsRightAligned = EditorParameters.GetBoolean("IsRightAligned", false);
                IsCentreAligned = EditorParameters.GetBoolean("IsCentreAligned", false);
            }
        }

        private void Regenerate()
        {
            // building shapes is time consuming,dont do until all
            // the default parameters are set
            if (loaded)
            {
                if (SettingsValid())
                {
                    GenerateShape();
                }
                else
                {
                    ClearShape();
                }
                Viewer.Model = GetModel();
            }
        }

        private void RegenTimer_Tick(object sender, EventArgs e)
        {
            regenTimer.Stop();
            Regenerate();
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            loaded = false;
            SetDefaults();
            loaded = true;
            UpdateDisplay();
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Text", text);
            EditorParameters.Set("FontName", selectedFont);
            EditorParameters.Set("TextLength", textLength);
            EditorParameters.Set("Thickness", thickness.ToString());
            EditorParameters.Set("Bold", fontBold.ToString());
            EditorParameters.Set("Italic", fontItalic.ToString());
            EditorParameters.Set("IsLeftAligned", IsLeftAligned.ToString());
            EditorParameters.Set("IsRightAligned", IsRightAligned.ToString());
            EditorParameters.Set("IsCentreAligned", IsCentreAligned.ToString());
        }

        private void SetDefaults()
        {
            SelectedFont = "Segoe UI";
            TextLength = 100.0;
            FontBold = false;
            FontItalic = false;
            Text = "Text";
            Thickness = 10;
            IsLeftAligned = true;
        }

        private bool SettingsValid()
        {
            bool result = true;
            if (text == null || text == "")
            {
                result = false;
            }
            if (selectedFont == null || selectedFont == "")
            {
                result = false;
            }
            if (textLength < 4.0)
            {
                result = false;
            }
            if (thickness <= 0)
            {
                result = false;
            }
            return result;
        }

        private void UpdateDisplay()
        {
            regenTimer.Stop();
            regenTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemFonts();
            SetDefaults();
            LoadEditorParameters();
            loaded = true;
            warningText = "";

            UpdateCameraPos();
            Viewer.Clear();
            NotifyPropertyChanged("SelectedFont");
            for (int i = 0; i < SystemFonts.Count; i++)
            {
                if (SystemFonts[i].Source == SelectedFont)
                {
                    FontCombo.SelectedIndex = i;
                    break;
                }
            }
            UpdateDisplay();
        }
    }
}