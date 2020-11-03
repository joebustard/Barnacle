using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Make3D.ViewModels
{
    internal class DefaultViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private static Control subView;
        private ObservableCollection<FontFamily> _systemFonts = new ObservableCollection<FontFamily>();
        private bool boldChecked;
        private string caption;
        private bool centerTextAlignment;
        private string fontSize;
        private bool italicChecked;
        private bool leftTextAlignment;
        private Ribbon MainRibbon;
        private static List<MruEntry> recentFilesList;
        private bool rightTextAlignment;
        private bool showGridChecked;
        private bool showMarginsChecked;
        private bool snapMarginChecked;

        private SubViewManager subViewMan;
        private Visibility toolPaletteVisible;
        private bool underLineChecked;

        public DefaultViewModel()
        {
            subViewMan = new SubViewManager();
            NewCommand = new RelayCommand(OnNew);
            OpenCommand = new RelayCommand(OnOpen);
            OpenRecentFileCommand = new RelayCommand(OnOpenRecent);
            SaveCommand = new RelayCommand(OnSave);
            SaveAsCommand = new RelayCommand(OnSaveAs);
            InsertCommand = new RelayCommand(OnInsert);

            UndoCommand = new RelayCommand(OnUndo);
            //   RedoCommand = new RelayCommand(OnRedo);
            CopyCommand = new RelayCommand(OnCopy);
            PasteCommand = new RelayCommand(OnPaste);
            MultiPasteCommand = new RelayCommand(OnMultiPaste);
            CutCommand = new RelayCommand(OnCut);
            //  ViewCommand = new RelayCommand(OnView);
            AddCommand = new RelayCommand(OnAdd);
            //  AddPageCommand = new RelayCommand(OnAddPage);
            ZoomInCommand = new RelayCommand(OnZoomIn);
            ZoomOutCommand = new RelayCommand(OnZoomOut);
            Zoom100Command = new RelayCommand(onZoomReset);
            //  PageCommand = new RelayCommand(OnPageNav);
            AlignCommand = new RelayCommand(OnAlignment);
            DistributeCommand = new RelayCommand(OnDistribute);
            FlipCommand = new RelayCommand(OnFlip);
            SizeCommand = new RelayCommand(OnSize);
            GroupCommand = new RelayCommand(OnGroup);
            SelectCommand = new RelayCommand(OnSelect);
            ImportCommand = new RelayCommand(OnImport);
            ExportCommand = new RelayCommand(OnExport);
            SettingsCommand = new RelayCommand(OnSettings);
            TextAlignmentCommand = new RelayCommand(OnTextAlignment);

            showGridChecked = false;

            ExitCommand = new RelayCommand(OnExit);

            //     ToolPaletteVisible = Visibility.Visible;

            Caption = BaseViewModel.Document.Caption;
            recentFilesList = new List<MruEntry>();

            FillColor = Brushes.White;
            LoadSystemFonts();
            SelectedFont = "Arial";
            FontSize = "14";
            snapMarginChecked = true;
            StatusBlockText1 = "Status Text 1";
            StatusBlockText2 = "Snap Margin On";
            if (recentFilesList != null)
            {
                LoadMru();
            }

            BaseViewModel.Document.PropertyChanged += Document_PropertyChanged;
            NotificationManager.Subscribe("CloseAbout", ReturnToDefaultView);
            NotificationManager.Subscribe("ClosePrintPreview", ReturnToDefaultView);
            NotificationManager.Subscribe("SetFontName", SetFontName);
            NotificationManager.Subscribe("SetFontSize", SetFontSize);
            NotificationManager.Subscribe("SetFontBold", SetFontBold);
            NotificationManager.Subscribe("SetFontItalic", SetFontItalic);
            NotificationManager.Subscribe("SetFontUnderLine", SetFontUnderline);
            NotificationManager.Subscribe("SetTextAlignment", SetTextAlignment);
            NotificationManager.Subscribe("SetStatusText1", SetStatusText1);
            NotificationManager.Subscribe("SetStatusText3", SetStatusText3);
            SubView = subViewMan.GetView("editor");
        }

        private void OnDistribute(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Distribute", s);
        }

        private void OnFlip(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Flip", s);
        }

        private void OnSettings(object obj)
        {
            NotificationManager.Notify("Settings", null);
        }

        private void OnInsert(object obj)
        {
            NotificationManager.Notify("InsertFile", obj);
        }

        private void OnImport(object obj)
        {
            NotificationManager.Notify("Import", obj);
        }

        private void OnExport(object obj)
        {
            NotificationManager.Notify("Export", obj);
        }

        private void SetStatusText1(object param)
        {
            StatusBlockText1 = param.ToString();
        }

        private void SetStatusText3(object param)
        {
            StatusBlockText3 = param.ToString();
        }

        private void OnNew(object obj)
        {
            BaseViewModel.Document.Clear();
            Caption = BaseViewModel.Document.Caption;
            NotificationManager.Notify("NewDocument", null);
            //UndoManager.Clear();
        }

        private void OnOpen(object obj)
        {
            NotificationManager.Notify("OpenFile", null);
            Caption = BaseViewModel.Document.Caption;
        }

        private void OnSave(object obj)
        {
            NotificationManager.Notify("SaveFile", null);
            Caption = BaseViewModel.Document.Caption;
            UpdateRecentFiles(BaseViewModel.Document.FilePath);
        }

        private void UpdateRecentFiles(string fileName)
        {
            bool found = false;
            foreach (MruEntry mru in recentFilesList)
            {
                if (mru.Path == fileName)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                string shortName = AbbreviatePath(fileName, 30);
                MruEntry m = new MruEntry(shortName, fileName);
                recentFilesList.Insert(0, m);
                if (recentFilesList.Count > 6)
                {
                    recentFilesList.RemoveAt(recentFilesList.Count - 1);
                }
                SaveMru();
                CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
            }
        }

        private void SaveMru()
        {
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (!Directory.Exists(mruPath))
            {
                Directory.CreateDirectory(mruPath);
            }
            StreamWriter fout = new StreamWriter(mruPath + "\\Mru.txt");
            foreach (MruEntry me in recentFilesList)
            {
                fout.WriteLine(me.Path);
            }
            fout.Close();
        }

        private void LoadMru()
        {
            String mruPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (Directory.Exists(mruPath))
            {
                StreamReader fin = new StreamReader(mruPath + "\\Mru.txt");
                while (!fin.EndOfStream)
                {
                    String s = fin.ReadLine();
                    string shortName = AbbreviatePath(s, 30);
                    MruEntry m = new MruEntry(shortName, s);
                    recentFilesList.Add(m);
                    //CollectionViewSource.GetDefaultView(RecentFilesList).Refresh();
                }
                fin.Close();
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand AddPageCommand { get; set; }
        public ICommand AlignCommand { get; set; }
        public ICommand DistributeCommand { get; set; }
        public ICommand FlipCommand { get; set; }

        public bool BoldChecked
        {
            get
            {
                return boldChecked;
            }
            set
            {
                if (value != boldChecked)
                {
                    boldChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Bold", boldChecked);
                }
            }
        }

        public String Caption
        {
            get
            {
                return caption;
            }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CenterTextAlignment
        {
            get
            {
                return centerTextAlignment;
            }
            set
            {
                if (value != centerTextAlignment)
                {
                    centerTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand CopyCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand DoNothingCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public String FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("FontSize", fontSize);
                }
            }
        }

        public ICommand GroupCommand { get; set; }

        public bool ItalicChecked
        {
            get
            {
                return italicChecked;
            }
            set
            {
                if (value != italicChecked)
                {
                    italicChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Italic", italicChecked);
                }
            }
        }

        public bool LeftTextAlignment
        {
            get
            {
                return leftTextAlignment;
            }
            set
            {
                if (value != leftTextAlignment)
                {
                    leftTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand NewCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand OpenRecentFileCommand { get; set; }
        public ICommand PageCommand { get; set; }
        public ICommand PasteCommand { get; set; }
        public ICommand MultiPasteCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand PrintPreviewCommand { get; set; }

        public List<MruEntry> RecentFilesList
        {
            get
            {
                return recentFilesList;
            }

            set
            {
                if (recentFilesList != value)
                {
                    recentFilesList = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand RedoCommand { get; set; }

        public bool RightTextAlignment
        {
            get
            {
                return rightTextAlignment;
            }
            set
            {
                if (value != rightTextAlignment)
                {
                    rightTextAlignment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand SaveAsCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand SelectCommand { get; set; }

        public bool ShowGridChecked
        {
            get
            {
                return showGridChecked;
            }
            set
            {
                if (showGridChecked != value)
                {
                    showGridChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ShowGrid", showGridChecked);
                }
            }
        }

        public bool ShowMarginsChecked
        {
            get
            {
                return showMarginsChecked;
            }
            set
            {
                if (showMarginsChecked != value)
                {
                    showMarginsChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ShowMargins", showMarginsChecked);
                }
            }
        }

        public ICommand SizeCommand { get; set; }

        public bool SnapMarginChecked
        {
            get
            {
                return snapMarginChecked;
            }
            set
            {
                if (snapMarginChecked != value)
                {
                    snapMarginChecked = value;
                    if (snapMarginChecked)
                    {
                        StatusBlockText2 = "Snap Margin On";
                    }
                    else
                    {
                        StatusBlockText2 = "Snap Margin Off";
                    }
                    NotifyPropertyChanged();
                    NotificationManager.Notify("SnapMargin", snapMarginChecked);
                }
            }
        }

        public Control SubView
        {
            get
            {
                return subView;
            }
            set
            {
                if (subView != value)
                {
                    subView = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<FontFamily> SystemFonts
        {
            get { return _systemFonts; }
        }

        public ICommand TextAlignmentCommand { get; set; }

        public Visibility ToolPaletteVisible
        {
            get
            {
                return toolPaletteVisible;
            }

            set
            {
                if (toolPaletteVisible != value)
                {
                    toolPaletteVisible = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("ToolPaletteVisible", toolPaletteVisible);
                }
            }
        }

        public bool UnderlineChecked
        {
            get
            {
                return underLineChecked;
            }
            set
            {
                if (value != underLineChecked)
                {
                    underLineChecked = value;
                    NotifyPropertyChanged();
                    NotificationManager.Notify("Underline", underLineChecked);
                }
            }
        }

        public ICommand InsertCommand { get; set; }
        public ICommand UndoCommand { get; set; }
        public ICommand Zoom100Command { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }

        public void LoadSystemFonts()
        {
            _systemFonts.Clear();

            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.ToString());

            foreach (var f in fonts)
                _systemFonts.Add(f);
        }

        internal void SetRibbonMenu(Ribbon mainRibbon)
        {
            MainRibbon = mainRibbon;
        }

        private string AbbreviatePath(string fileName, int max)
        {
            string result = "";
            string[] fldrs = fileName.Split('\\');
            int last = fldrs.GetLength(0) - 1;
            int i = 0;
            while ((result.Length + fldrs[last].Length < max) && i < last)
            {
                result += fldrs[i] + "\\";
                i++;
            }
            if (last != i)
            {
                result += "....\\";
            }
            result += fldrs[last];
            return result;
        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PageNumber" || e.PropertyName == "SelectedPage")
            {
                //           StatusBlockText3 = "Page " + (Document.PageNumber + 1).ToString() + " of " + (Document.NumberOfPages.ToString());
            }
        }

        private void OnAdd(object obj)
        {
            string name = obj.ToString();
            NotificationManager.Notify("AddObject", name);
        }

        private void OnAlignment(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Alignment", s);
        }

        private void OnCopy(object obj)
        {
            NotificationManager.Notify("Copy", null);
        }

        private void OnCut(object obj)
        {
            NotificationManager.Notify("Cut", null);
        }

        private void OnDoNothing(object obj)
        {
        }

        private void OnExit(object obj)
        {
            if (BaseViewModel.Document.Dirty)
            {
                NotificationManager.Notify("CheckExit", null);
            }
            else
            {
                // UndoManager.Clear();
                App.Current.Shutdown();
            }
        }

        private void OnGroup(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Group", s);
        }

        private void OnOpenRecent(object obj)
        {
            string f = obj.ToString();

            NotificationManager.Notify("OpenRecentFile", f);
            Caption = BaseViewModel.Document.Caption;
        }

        private void OnPaste(object obj)
        {
            NotificationManager.Notify("Paste", null);
        }

        private void OnMultiPaste(object obj)
        {
            NotificationManager.Notify("MultiPaste", null);
        }

        private void OnPrint(object obj)
        {
            NotificationManager.Notify("Print", null);
        }

        private void OnRedo(object obj)
        {
            NotificationManager.Notify("Redo", null);
        }

        private void OnSaveAs(object obj)
        {
            NotificationManager.Notify("SaveAsFile", null);
            Caption = BaseViewModel.Document.Caption;
            UpdateRecentFiles(BaseViewModel.Document.FilePath);
        }

        private void OnSelect(object obj)
        {
            NotificationManager.Notify("Select", obj);
        }

        private void OnSize(object obj)
        {
            string s = obj.ToString();
            NotificationManager.Notify("Size", s);
        }

        private void OnTextAlignment(object obj)
        {
            NotificationManager.Notify("TextAlignment", obj);
        }

        private void OnUndo(object obj)
        {
            NotificationManager.Notify("Undo", null);
        }

        private void OnView(object obj)
        {
            String vwName = obj.ToString();
            vwName = vwName.ToLower();
            if (vwName == "pageedit")
            {
                ToolPaletteVisible = Visibility.Visible;
            }
            else
            {
                ToolPaletteVisible = Visibility.Collapsed;
            }
            subViewMan.CloseCurrent();
            SubView = subViewMan.GetView(vwName);
        }

        private void OnZoomIn(object obj)
        {
            NotificationManager.Notify("ZoomIn", null);
        }

        private void OnZoomOut(object obj)
        {
            NotificationManager.Notify("ZoomOut", null);
        }

        private void onZoomReset(object obj)
        {
            NotificationManager.Notify("ZoomReset", null);
            NotificationManager.Notify("UpdateDisplay", null);
        }

        private void ReturnToDefaultView(object param)
        {
            OnView("PageEdit");
        }

        private void SetFontBold(object param)
        {
            bool b = (bool)param;
            BoldChecked = b;
        }

        private void SetFontItalic(object param)
        {
            bool b = (bool)param;
            ItalicChecked = b;
        }

        private void SetFontName(object param)
        {
            SelectedFont = param.ToString();
        }

        private void SetFontSize(object param)
        {
            double fs = (double)param;
            FontSize = fs.ToString();
        }

        private void SetFontUnderline(object param)
        {
            bool b = (bool)param;
            UnderlineChecked = b;
        }

        private void SetTextAlignment(object param)
        {
            TextAlignment ta = (TextAlignment)param;
            switch (ta)
            {
                case TextAlignment.Left:
                    {
                        LeftTextAlignment = true;
                    }
                    break;

                case TextAlignment.Right:
                    {
                        RightTextAlignment = true;
                    }
                    break;

                case TextAlignment.Center:
                    {
                        CenterTextAlignment = true;
                    }
                    break;
            }
        }

        private static string statusBlockText1;

        private static string statusBlockText2;

        private static string statusBlockText3;

        public String StatusBlockText1
        {
            get
            {
                return statusBlockText1;
            }

            set
            {
                if (statusBlockText1 != value)
                {
                    statusBlockText1 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String StatusBlockText2
        {
            get
            {
                return statusBlockText2;
            }

            set
            {
                if (statusBlockText2 != value)
                {
                    statusBlockText2 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String StatusBlockText3
        {
            get
            {
                return statusBlockText3;
            }

            set
            {
                if (statusBlockText3 != value)
                {
                    statusBlockText3 = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}