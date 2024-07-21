using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageUtils
{
    public static class ImageCapture
    {
        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        public static void ScreenCaptureElement(FrameworkElement element, String fileName, bool cropped = false)

        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (cropped)
                {
                    // save a version of the image that is cropped close to the content
                    CroppedImage(fileName, element, 3, 3);
                }
                else
                {
                    DpiScale dps = VisualTreeHelper.GetDpi(element);
                    // save an uncropped version
                    //RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, dps.DpiScaleX * 96, dps.DpiScaleY * 96, PixelFormats.Pbgra32);
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight,  96, 96, PixelFormats.Pbgra32);
                    bmp.Render(element);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    

                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    FileStream fs = new FileStream(fileName, FileMode.Create);

                    encoder.Save(fs);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void CroppedImage(String fileName, FrameworkElement element, int borderWidth, int borderHeight)
        {
            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(element);
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stream);
            Bitmap bitmap = new Bitmap(stream);
            stream.Close();

            double tlx = bitmap.Width;
            double tly = bitmap.Height;
            double brx = 0;
            double bry = 0;
            for (int px = 0; px < bitmap.Width; px++)
            {
                for (int py = 0; py < bitmap.Height; py++)
                {
                    System.Drawing.Color c = bitmap.GetPixel(px, py);
                    if (c.A != 0)
                    {
                        tlx = Math.Min(px, tlx);
                        tly = Math.Min(tly, py);
                        brx = Math.Max(px, brx);
                        bry = Math.Max(bry, py);
                    }
                }
            }
            if (tlx > borderWidth)
            {
                tlx -= borderWidth;
            }
            if (tly > borderHeight)
            {
                tly -= borderHeight;
            }
            if (brx < bitmap.Width - borderWidth)
            {
                brx += borderWidth;
            }
            if (bry < bitmap.Height - borderHeight)
            {
                bry += borderHeight;
            }
            int w = (int)(brx - tlx);
            int h = (int)(bry - tly);

            Bitmap outbmp = new Bitmap(w, h);
            for (int px = 0; px < w; px++)
            {
                for (int py = 0; py < h; py++)
                {
                    outbmp.SetPixel(px, py, bitmap.GetPixel(px + (int)tlx, py + (int)tly));
                }
            }
            FileStream fs = new FileStream(fileName, FileMode.Create);
            encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(ConvertBitmap(outbmp)));
            // Save to memory stream and create Bitamp from stream
            encoder.Save(fs);
            fs.Close();
        }
    }
}