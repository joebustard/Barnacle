using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MakerLib
{
    public class MakeSymbolUtils : ImageMakerUtils
    {
        public void GenerateSymbol(string v, string fontName)
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(v, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                   new Typeface(fontName), 100, System.Windows.Media.Brushes.Black);
            ft.SetFontStretch(FontStretches.Normal);
            Size sz = new Size(ft.Width, ft.Height);

            imageWidth = (int)(Math.Ceiling(sz.Width)) + 20;
            imageHeight = (int)(Math.Ceiling(sz.Height)) + 20;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawText(ft, new System.Windows.Point(10, 10));

            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Default);
            bmp.Render(drawingVisual);
            bitmap = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            WriteableBitmap wrb = new WriteableBitmap(bmp);
            GenerateField(wrb);
        }

        internal void GenerateImage(string plaqueImagePath)
        {
            BitmapImage bitmap;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            BitmapImage plaqueBitmap = new BitmapImage();
            plaqueBitmap.BeginInit();
            plaqueBitmap.UriSource = new Uri(plaqueImagePath);
            plaqueBitmap.EndInit();

            // Create a FormatConvertedBitmap
            FormatConvertedBitmap grayBitmapSource = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            grayBitmapSource.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            grayBitmapSource.Source = plaqueBitmap;

            // Key of changing the bitmap format is DesitnationFormat property of BitmapSource.
            // It is a type of PixelFormat. FixelFormat has dozens of options to set
            // bitmap formatting.
            grayBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            grayBitmapSource.EndInit();

            imageWidth = (int)(Math.Ceiling(grayBitmapSource.Width)) + 2;
            imageHeight = (int)(Math.Ceiling(grayBitmapSource.Height)) + 2;
            drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 1), new Rect(0, 0, imageWidth, imageHeight));
            drawingContext.DrawImage(grayBitmapSource, new Rect(1, 1, grayBitmapSource.Width, grayBitmapSource.Height));

            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Default);
            bmp.Render(drawingVisual);
            bitmap = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            WriteableBitmap wrb = new WriteableBitmap(bmp);
            GenerateField(wrb);
        }
    }
}