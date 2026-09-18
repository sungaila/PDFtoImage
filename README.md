# ![PDFtoImage Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_64.png) PDFtoImage

[![GitHub Workflow Build Status](https://img.shields.io/github/actions/workflow/status/sungaila/PDFtoImage/dotnet.yml?event=push&style=flat-square&logo=github&logoColor=white)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![GitHub Workflow Test Runs Succeeded](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fgist.githubusercontent.com%2Fsungaila%2F003e8ab2211221897e4b3c0e564ed7b6%2Fraw&query=%24.stats.runs_succ&suffix=%20passed&style=flat-square&logo=github&logoColor=white&label=tests&color=45cc11)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![SonarCloud Quality Gate](https://img.shields.io/sonar/quality_gate/sungaila_PDFtoImage?server=https%3A%2F%2Fsonarcloud.io&style=flat-square&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/dashboard?id=sungaila_PDFtoImage)
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![Website](https://img.shields.io/website?up_message=online&down_message=offline&url=https%3A%2F%2Fwww.sungaila.de%2FPDFtoImage%2F&style=flat-square&label=website)](https://www.sungaila.de/PDFtoImage/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

A .NET library to render [PDF files](https://en.wikipedia.org/wiki/PDF) into images.

This .NET library is built on top of
* [PDFium](https://pdfium.googlesource.com/pdfium/) (native PDF renderer)
* [SkiaSharp](https://github.com/mono/SkiaSharp) (cross-platform 2D graphics API)

## Getting started
Call a static method from `PDFtoImage.Conversion`. Here is an example of how to render the first page of a PDF file as a PNG image:

```csharp
using var pdf = File.OpenRead("document.pdf");

PDFtoImage.Conversion.SavePng(
    imageFilename: "page1.png",
    pdfStream: pdf,
    page: 0);
```

`SaveJpeg`, `SavePng`, `SaveWebp` and `ToImage` for a **single page**.

`ToImages` and `ToImagesAsync` for **multiple pages**.

*Note: [`SkiaSharp.SKBitmap`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap) can be exported with the [`Encode`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?SkiaSharp_SKBitmap_Encode_System_IO_Stream_SkiaSharp_SKEncodedImageFormat_System_Int32_) method.*

### Unity project installation
1. Open your project and navigate to `Window` → `Package Management` → `Package Manager`.
1. Click on the `+` button (top-left corner) and select `Install package from git URL...`.
1. Enter the following URL and confirm with the `Install` button:
```
https://github.com/sungaila/PDFtoImage.git?path=etc/UnityPackage
```

## Supported runtimes
* [.NET (Core)](https://learn.microsoft.com/en-us/dotnet/core/introduction)
* [.NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/get-started/overview)
* [Mono](https://www.mono-project.com)

## Tested and supported frameworks
* [ASP.NET](https://learn.microsoft.com/en-us/aspnet/overview)
* [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)
* [.NET Multi-platform App UI (.NET MAUI)](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui) (excluding iOS, see https://github.com/sungaila/PDFtoImage/issues/141)
* [Unity](https://docs.unity3d.com/Manual/Mono.html) (excluding iOS, see https://github.com/sungaila/PDFtoImage/issues/141)
* [Universal Windows Platform (UWP)](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide)
* [Windows UI Library 3 (WinUI 3)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

## Parallelization
The native PDFium library used by this project for rendering is **not thread-safe**. For that reason, all calls into PDFium are protected with locks, so a single process can only render one PDF page at a time.

[PDFtoImage.Parallel](https://www.nuget.org/packages/PDFtoImage.Parallel) provides true parallel rendering through isolated worker processes and named-pipe IPC. The package works on Windows, Linux and macOS only. Workers are assigned atomically to a Windows Job Object when created, so they are also terminated if the parent process exits unexpectedly, including during worker startup.

```csharp
// start a pool of 8 worker processes
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor(workerCount: 8);

// reuse the same pool for different PDFs, including concurrent requests
using var a = await converter.ToImageAsync(File.OpenRead("a.pdf"), 0);
using var b = await converter.ToImageAsync(File.OpenRead("b.pdf"), 0);

await foreach (var image in converter.ToImagesAsync(File.OpenRead("document.pdf")))
{
    using (image)
    {
        // process pages in their requested order
    }
}
```

Workers start on demand and live until the converter is disposed. The default limit is the processor count. Dispose returned bitmaps; cancellation or a failed worker does not prevent later requests.

If subprocesses are unsuitable, Ghostscript may be an alternative because it can support multiple instances within one process under certain conditions.

## Index and Range for .NET Framework
[PolySharp](https://github.com/Sergio0694/PolySharp) is used to enable the use of `System.Index` and `System.Range` in .NET Framework projects. As a side effect, the following classes are generated and exposed, which **should not be** used directly by your project:
- `System.Index`
- `System.Range`
- `System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute`
- `System.Diagnostics.CodeAnalysis.NotNullWhenAttribute`
- `System.Runtime.CompilerServices.IsExternalInit`
