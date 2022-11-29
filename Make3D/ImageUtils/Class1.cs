using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageUtils
{
    public static class ImageCapture
    {
        public static void ScreenCaptureElement(FrameworkElement element, String fileName)

        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(element);

            PngBitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmp));

            FileStream fs = new FileStream(fileName, FileMode.Create);

            encoder.Save(fs);
            fs.Close();
        }
    }
}