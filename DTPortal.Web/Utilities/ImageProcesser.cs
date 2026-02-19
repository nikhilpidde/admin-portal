using SkiaSharp;
using System.IO;
using System;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Drawing;

namespace DTPortal.Web.Utilities
{
    public class ImageProcesser
    {
        public static string MakeImageTransparentBase64(SKBitmap originalImage)
        {
            int width = originalImage.Width;
            int height = originalImage.Height;
            SKBitmap newImage = new SKBitmap(width, height);
            SKColor transparentColor = SKColors.Transparent;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SKColor pixelColor = originalImage.GetPixel(x, y);
                    if (pixelColor.Alpha == 255 && pixelColor.Blue >= 235 && pixelColor.Green >= 235 && pixelColor.Red >= 235)
                    {
                        newImage.SetPixel(x, y, new SKColor(pixelColor.Red, pixelColor.Green, pixelColor.Blue, 0));
                    }
                    else
                    {
                        newImage.SetPixel(x, y, new SKColor(pixelColor.Red, pixelColor.Green, pixelColor.Blue, pixelColor.Alpha));
                    }
                }
            }

            using (MemoryStream m = new MemoryStream())
            {
                newImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(m);
                byte[] imageBytes = m.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }

        public static string MakeImageTransparent(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (SKBitmap bitmap = SKBitmap.Decode(ms))
                {
                    return MakeImageTransparentBase64(bitmap);
                }
            }
        }
    }
}
