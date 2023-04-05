
using System.Drawing;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Page = UglyToad.PdfPig.Content.Page;

namespace PdfConverter.Classes
{
    public static class PDFParser
    {
        internal static async Task<string> GetText(IFormFile file, bool parseImages)
        {
            using var memoryStream = new MemoryStream();

            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using PdfDocument pdf = PdfDocument.Open(memoryStream);

            var textBuilder = new StringBuilder();
            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                Page page = pdf.GetPage(i);
                textBuilder.AppendLine(page.Text);
            }

            if (parseImages)
            {
                textBuilder.Append(ParseImages(pdf));
            }

            return textBuilder.ToString();
        }

        private static string ParseImages(PdfDocument pdf)
        {
            StringBuilder output = new();
            
            using var tesseractEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

            foreach (Page page in pdf.GetPages())
            {
                var images = page.GetImages();
                foreach (IPdfImage image in images)
                {
                    if (image.TryGetPng(out byte[] pngBytes))
                    {
                        string imageText = ReadTextFromPNG(tesseractEngine, pngBytes);
                        output.AppendLine(imageText);
                    }
                    else
                    {
                        byte[] bytes = image.RawBytes.ToArray();
                        string imageText = ReadTextFromPNG(tesseractEngine, bytes);
                        // Not 100% sure this works! but at least trying to handle non PNGs
                        output.AppendLine(imageText);
                    }
                }
            }

            return output.ToString();
        }

        private static string ReadTextFromPNG(TesseractEngine tesseractEngine, byte[] pngBytes)
        {
            Stream stream = new MemoryStream(pngBytes);

            Bitmap bitmap = new(stream);

            using var pix = PixConverter.ToPix(bitmap);
            using var pageOcr = tesseractEngine.Process(pix);
            string imageText = pageOcr.GetText();

            return imageText;
        }
    }
}
