using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace PDFtoImage.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            System.Console.WriteLine($"{entryAssembly?.GetName()?.ToString() ?? "PDFtoImage"}");
            System.Console.WriteLine();

            try
            {
                ParseArguments(args, out string? inputPath, out string? outputPath, out int page, out int dpi);

                if (inputPath == null)
                    throw new InvalidOperationException("There is no PDF file path.");

                if (outputPath == null)
                    throw new InvalidOperationException("There is no output image file path.");

                using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);

#if NETCOREAPP3_0_OR_GREATER
                if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                    throw new PlatformNotSupportedException("Only win-x86, win-x64, linux-x64, osx-x64 and osx-arm64 are supported for PDF file conversion.");
#endif

                switch (Path.GetExtension(outputPath).ToLower())
                {
                    case ".bmp":
                        Conversion.SaveBmp(outputPath, inputStream, page: page - 1, dpi: dpi);
                        break;
                    case ".png":
                        Conversion.SavePng(outputPath, inputStream, page: page - 1, dpi: dpi);
                        break;
                    case ".gif":
                        Conversion.SaveGif(outputPath, inputStream, page: page - 1, dpi: dpi);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        Conversion.SaveJpeg(outputPath, inputStream, page: page - 1, dpi: dpi);
                        break;
                    case ".tif":
                    case ".tiff":
                        Conversion.SaveTiff(outputPath, inputStream, page: page - 1, dpi: dpi);
                        break;
                    default:
                        throw new InvalidOperationException("Only the following file extensions are supported: bmp, png, gif, jpg/jpeg and tif/tiff.");
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Failed to render PDF.");
                System.Console.Error.WriteLine(ex);

                return -1;
            }

            return 0;
        }

        private static void ParseArguments(string[] args, out string? inputPath, out string? outputPath, out int page, out int dpi)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length >= 1)
            {
                inputPath = args[0];
            }
            else
            {
                System.Console.Write("Enter the path to the PDF file: ");
                inputPath = System.Console.ReadLine();
            }

            inputPath = inputPath!.Trim('\"');

            if (args.Length >= 2)
            {
                outputPath = args[1];
            }
            else
            {
                System.Console.Write("Enter the output path of the result: ");
                outputPath = System.Console.ReadLine();

                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(inputPath)!, Path.GetFileNameWithoutExtension(inputPath) + ".png");
                    System.Console.WriteLine($"Output defaulting to \"{outputPath}\".");
                }
            }

            outputPath = outputPath.Trim('\"');

            page = 1;

            if (args.Length >= 3)
            {
                outputPath = args[2];
            }
            else
            {
                System.Console.Write("Enter PDF page number: ");

                if (!int.TryParse(System.Console.ReadLine(), out page) || page <= 0)
                {
                    page = 1;
                    System.Console.WriteLine($"PDF page number defaulting to {page}.");
                }
            }

            dpi = 300;

            if (args.Length >= 4)
            {
                outputPath = args[3];
            }
            else
            {
                System.Console.Write("Enter the target resolution in DPI: ");
                
                if (!int.TryParse(System.Console.ReadLine(), out dpi) || dpi <= 0)
                {
                    dpi = 300;
                    System.Console.WriteLine($"Target DPI defaulting to {dpi}.");
                }
            }
        }
    }
}
