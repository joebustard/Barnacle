using Barnacle.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Barnacle.UserControls
{
    public class SymbolGridViewModel : INotifyPropertyChanged
    {
        public delegate void SymbolChanged(string symbol, string fontName);

        private string _newItem;
        private BitmapImage bitmap;
        private string fontName;
        private List<string> fontNames;
        private int imageHeight = 75;
        private int imageWidth = 75;
        private ObservableCollection<Symbol> itemSource;
        private string selectedFontName;
        private RelayCommand symbolCommand;
        private string selectedChar;
        private SymbolChanged onSymbolChanged;

        public SymbolChanged OnSymbolChanged
        {
            get { return onSymbolChanged; }
            set
            {
                if (onSymbolChanged != value)
                {
                    onSymbolChanged = value;
                }
            }
        }

        private Visibility showImage;

        public Visibility ShowImage
        {
            get { return showImage; }
            set
            {
                if (showImage == value)
                {
                    showImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Visibility showBusy;

        public Visibility ShowBusy
        {
            get { return showBusy; }
            set
            {
                if (showBusy == value)
                {
                    showBusy = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SymbolGridViewModel()
        {
            ItemsSource = new ObservableCollection<Symbol>();
            symbolCommand = new RelayCommand(SymbolChosen);
            fontNames = new List<string>();
            foreach (System.Windows.Media.FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                // FontFamily.Source contains the font family name.
                fontNames.Add(fontFamily.Source);
            }
            selectedFontName = "Arial";

            SetFont(selectedFontName);
            selectedChar = null;
            fontName = SelectedFontName;
            bitmap = null;
            ShowBusy = Visibility.Hidden;
            ShowImage = Visibility.Visible;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> FontNames
        {
            get { return fontNames; }
            set
            {
                if (fontNames != value)
                {
                    fontNames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<Symbol> ItemsSource
        {
            get { return itemSource; }
            set
            {
                if (value != itemSource)
                {
                    itemSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NameOfFont
        {
            get { return fontName; }
            set
            {
                if (value != fontName)
                {
                    fontName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NewItem
        { get { return _newItem; } set { _newItem = value; } }

        public string SelectedFontName
        {
            get { return selectedFontName; }
            set
            {
                if (selectedFontName != value)
                {
                    selectedFontName = value;

                    SetFont(selectedFontName);
                    NotifyPropertyChanged();
                    NameOfFont = selectedFontName;
                }
            }
        }

        public ICommand SymbolClicked
        {
            get { return symbolCommand; }
        }

        public BitmapImage SymbolSource
        {
            get { return bitmap; }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void DrawChar(string v)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(v, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface(fontName), 60, System.Windows.Media.Brushes.Black);
            ft.SetFontStretch(FontStretches.Normal);
            Size sz = new Size(ft.Width, ft.Height);
            imageWidth = (int)(Math.Ceiling(sz.Width)) + 4;
            imageHeight = (int)(Math.Ceiling(sz.Height)) + 4;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawText(ft, new System.Windows.Point(0, 0));

            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Default);

            bmp.Render(drawingVisual);
            bitmap = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));
            bitmapEncoder.Frames[0].DownloadCompleted += ViewModel_DownloadCompleted;
            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            NotifyPropertyChanged("SymbolSource");
        }

        private void SetFont(string fntName)
        {
            ItemsSource.Clear();
            ObservableCollection<Symbol> tmp = new ObservableCollection<Symbol>();
            var typeface = new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily(fntName),
               FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var isGlyphTypeface = typeface.TryGetGlyphTypeface(out var glyph);
            if (isGlyphTypeface)
            {
                IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;
                foreach (int k in characterMap.Keys)
                {
                    PathGeometry gm = glyph.GetGlyphOutline(characterMap[k], 20, 20) as PathGeometry;
                    if (gm != null && gm.Figures.Count > 0)
                    {
                        string s = Char.ToString((char)k);
                        if (s != " ")
                        {
                            Symbol sym = new Symbol();
                            sym.Text = s;
                            sym.SymbolClicked = symbolCommand;
                            tmp.Add(sym);
                        }
                    }
                }
                ItemsSource = tmp;
                NotifyPropertyChanged("ItemsSource");
                NotifyPropertyChanged("MainItemsSource");
                if (selectedChar != null)
                {
                    foreach (Symbol sym in ItemsSource)
                    {
                        if (sym.Text == selectedChar)
                        {
                            DrawChar(selectedChar);
                            break;
                        }
                    }
                }
            }
        }

        private const int numberOfLayers = 6;

        private void SymbolChosen(object obj)
        {
            selectedChar = obj as String;
            DrawChar(selectedChar);
            if (OnSymbolChanged != null)
            {
                OnSymbolChanged(selectedChar, fontName);
            }
        }

        private void ViewModel_DownloadCompleted(object sender, EventArgs e)
        {
            BitmapEncoder be = sender as BitmapEncoder;
        }
    }
}