using System;
using System.IO;
using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: PdfToImageExtractor.exe <path-to-pdf-file>");
            return;
        }

        string pdfPath = args[0];
        if (!File.Exists(pdfPath))
        {
            Console.WriteLine($"Error: File '{pdfPath}' not found.");
            return;
        }

        string pdfFileName = Path.GetFileNameWithoutExtension(pdfPath);
        string pdfDirectory = Path.GetDirectoryName(pdfPath) ?? Directory.GetCurrentDirectory();
        string outputDirectory = Path.Combine(pdfDirectory, "OutputImages");
        Directory.CreateDirectory(outputDirectory);

        using (var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(1080, 1920)))
        {
            int pageCount = docReader.GetPageCount();
            for (int i = 0; i < pageCount; i++)
            {
                using (var pageReader = docReader.GetPageReader(i))
                {
                    int width = pageReader.GetPageWidth();
                    int height = pageReader.GetPageHeight();
                    var rawBytes = pageReader.GetImage(); // BGRA32

                    // Convert BGRA to RGBA for ImageSharp
                    var rgbaBytes = new byte[width * height * 4];
                    for (int j = 0; j < width * height; j++)
                    {
                        rgbaBytes[j * 4 + 0] = rawBytes[j * 4 + 2]; // R
                        rgbaBytes[j * 4 + 1] = rawBytes[j * 4 + 1]; // G
                        rgbaBytes[j * 4 + 2] = rawBytes[j * 4 + 0]; // B
                        rgbaBytes[j * 4 + 3] = rawBytes[j * 4 + 3]; // A
                    }

                    using (var image = Image.LoadPixelData<Rgba32>(rgbaBytes, width, height))
                    {
                        image.Mutate(ctx =>
                        {
                            ctx.Grayscale();
                            //ctx.BinaryThreshold(0.2f); // Adjust threshold as needed
                        });

                        // Create a new white background image
                        using (var whiteBg = new Image<Rgba32>(image.Width, image.Height, Color.White))
                        {
                            // Draw the processed image on top of the white background
                            whiteBg.Mutate(ctx => ctx.DrawImage(image, 1f));

                            string outputFileName = $"{pdfFileName}_PAGE{i + 1:D3}.png";
                            string outputPath = Path.Combine(outputDirectory, outputFileName);

                            // Save as PNG (will have no transparency, just white background)
                            whiteBg.Save(outputPath, new PngEncoder());
                            Console.WriteLine($"Saved: {outputFileName}");
                        }
                    }
                }
            }
        }
    }
}