using System;
using System.IO;
using System.Reflection;

namespace PDFtoImage.Console
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            System.Console.WriteLine($"{entryAssembly?.GetName()?.ToString() ?? "PDFtoImage"}");
            System.Console.WriteLine();

            try
            {
                ParseArguments(args, out string? inputPath, out string? outputPath, out int page, out int dpi, out bool withAnnotations, out bool withFormFill);

                if (inputPath == null)
                    throw new InvalidOperationException("There is no PDF file path.");

                if (outputPath == null)
                    throw new InvalidOperationException("There is no output image file path.");

                using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);

#if NET6_0_OR_GREATER
                if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                    throw new PlatformNotSupportedException("Only win-x86, win-x64, win-arm64, linux-x64, linux-arm, linux-arm64, osx-x64 and osx-arm64 are supported for PDF file conversion.");
#endif

                switch (Path.GetExtension(outputPath).ToLower())
                {
                    case ".png":
                        Conversion.SavePng(outputPath, inputStream, page: page - 1, dpi: dpi, withAnnotations: withAnnotations, withFormFill: withFormFill);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        Conversion.SaveJpeg(outputPath, inputStream, page: page - 1, dpi: dpi, withAnnotations: withAnnotations, withFormFill: withFormFill);
                        break;
                    case ".webp":
                        Conversion.SaveWebp(outputPath, inputStream, page: page - 1, dpi: dpi, withAnnotations: withAnnotations, withFormFill: withFormFill);
                        break;
                    default:
                        throw new InvalidOperationException("Only the following file extensions are supported: png, jpg/jpeg and webp.");
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

        private static void ParseArguments(string[] args, out string? inputPath, out string? outputPath, out int page, out int dpi, out bool withAnnotations, out bool withFormFill)
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

            withAnnotations = false;

            if (args.Length >= 5)
            {
                outputPath = args[4];
            }
            else
            {
                System.Console.Write("Should annotations be rendered (y/n): ");

                var input = System.Console.ReadLine();
                if (input?.ToLowerInvariant() == "y")
                {
                    withAnnotations = true;
                }
                else if (input?.ToLowerInvariant() == "n")
                {
                    withAnnotations = false;
                }
                else
                {
                    withAnnotations = false;
                    System.Console.WriteLine($"Annotations not rendered by default.");
                }
            }

            withFormFill = false;

            if (args.Length >= 5)
            {
                outputPath = args[4];
            }
            else
            {
                System.Console.Write("Should form filling be rendered (y/n): ");

                var input = System.Console.ReadLine();
                if (input?.ToLowerInvariant() == "y")
                {
                    withFormFill = true;
                }
                else if (input?.ToLowerInvariant() == "n")
                {
                    withFormFill = false;
                }
                else
                {
                    withFormFill = false;
                    System.Console.WriteLine($"Form filling not rendered by default.");
                }
            }
        }
    }
}