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
using System.Windows;
using System.Windows.Media;

namespace Barnacle.Dialogs
{
    /// <summary>
    /// Interaction logic for TextDlg.xaml
    /// </summary>
    public partial class TextDlg : BaseModellerDialog, INotifyPropertyChanged
    {
        private ObservableCollection<FontFamily> _systemFonts = new ObservableCollection<FontFamily>();
        private string dataPath;
        private bool fontBold;
        private bool fontItalic;
        private bool loaded;
        private String selectedFont;
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
            ModelGroup = MyModelGroup;
            text = "A";
            dataPath = "f1M0,0";
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

        public override bool ShowAxies
        {
            get
            {
                return showAxies;
            }
            set
            {
                if (showAxies != value)
                {
                    showAxies = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public override bool ShowFloor
        {
            get
            {
                return showFloor;
            }
            set
            {
                if (showFloor != value)
                {
                    showFloor = value;
                    NotifyPropertyChanged();
                    Redisplay();
                }
            }
        }

        public ObservableCollection<FontFamily> SystemFonts
        {
            get { return _systemFonts; }
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
            _systemFonts.Clear();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());

            foreach (var f in fonts)
                _systemFonts.Add(f);

            NotifyPropertyChanged("SystemFonts");
        }

        protected override void Ok_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorParmeters();
            DialogResult = true;
            Close();
        }

        // private double textLength = 150;

        private void GenerateShape(bool final = false)
        {
            ClearShape();
            if (text != null && text != "")
            {
                ClearShape();

                CentreVertices();
                TextMaker mk = new TextMaker(text, selectedFont, textLength, thickness, final, fontBold, fontItalic);
                string PathCode = mk.Generate(Vertices, Faces);
                //   PathData = PathCode;
            }
            CentreVertices();
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
            }
        }

        private void SaveEditorParmeters()
        {
            EditorParameters.Set("Text", text);
            EditorParameters.Set("FontName", selectedFont);
            EditorParameters.Set("TextLength", textLength);
            EditorParameters.Set("Thickness", thickness.ToString());
            EditorParameters.Set("Bold", fontBold.ToString());
            EditorParameters.Set("Italic", fontItalic.ToString());
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
                Redisplay();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemFonts();
            SelectedFont = "Segoe UI";
            TextLength = 100.0;
            FontBold = false;
            FontItalic = false;
            Text = "Text";
            Thickness = 10;
            LoadEditorParameters();
            loaded = true;
            warningText = "";

            UpdateCameraPos();
            MyModelGroup.Children.Clear();
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