# ![PDFtoImage Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_128.png) PDFtoImage

[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![Website](https://img.shields.io/website?up_message=online&down_message=offline&url=https%3A%2F%2Fwww.sungaila.de%2FPDFtoImage%2F&style=flat-square&label=website)](https://www.sungaila.de/PDFtoImage/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

A .NET library to render [PDF files](https://en.wikipedia.org/wiki/PDF) into images.

PDFtoImage is built on top of:
* [PDFium](https://pdfium.googlesource.com/pdfium/) (native PDF renderer)
* [SkiaSharp](https://github.com/mono/SkiaSharp) (cross-platform 2D graphics API)

For true multi-process rendering on .NET 11, see [PDFtoImage.Parallel](https://www.nuget.org/packages/PDFtoImage.Parallel/).

## Getting started
Call a static method from `PDFtoImage.Conversion`. Here is an example of how to render the first page of a PDF file as a PNG image:

```csharp
using var pdf = File.OpenRead("document.pdf");

PDFtoImage.Conversion.SavePng(
    imageFilename: "page1.png",
    pdfStream: pdf,
    page: 0);
```

`SaveJpeg`, `SavePng`, `SaveWebp` and `ToImage` render a **single page**.

`ToImages` and `ToImagesAsync` render **multiple pages**.

*Note: [`SkiaSharp.SKBitmap`](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap) can be exported with the [`Encode`](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode) method.*

### Unity project installation
1. Open your project and navigate to `Window` → `Package Management` → `Package Manager`.
1. Click on the `+` button (top-left corner) and select `Install package from git URL...`.
1. Enter the following URL and confirm with the `Install` button:

```
https://github.com/sungaila/PDFtoImage.git?path=etc/UnityPackage
```

## Supported runtimes
* [.NET](https://learn.microsoft.com/en-us/dotnet/core/introduction)
* [.NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/get-started/overview)
* [Mono](https://www.mono-project.com/)

## Tested and supported frameworks
* [ASP.NET](https://learn.microsoft.com/en-us/aspnet/overview)
* [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)
* [.NET Multi-platform App UI (.NET MAUI)](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui) (excluding iOS, see [#141](https://github.com/sungaila/PDFtoImage/issues/141))
* [Unity](https://docs.unity3d.com/Manual/Mono.html) (excluding iOS, see [#141](https://github.com/sungaila/PDFtoImage/issues/141))
* [Universal Windows Platform (UWP)](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide)
* [Windows UI Library 3 (WinUI 3)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

## Parallel rendering
PDFium is **not thread-safe**, so calls into PDFium are protected by locks and a single process renders only one page at a time.

For true parallel rendering through isolated worker processes, install [PDFtoImage.Parallel](https://www.nuget.org/packages/PDFtoImage.Parallel/). It is a separate package built on top of PDFtoImage.

## Index and Range for .NET Framework
[PolySharp](https://github.com/Sergio0694/PolySharp) is used to enable `System.Index` and `System.Range` in .NET Framework projects. As a side effect, the following classes are generated and exposed and **should not be used directly** by your project:
- `System.Index`
- `System.Range`
- `System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute`
- `System.Diagnostics.CodeAnalysis.NotNullWhenAttribute`
- `System.Runtime.CompilerServices.IsExternalInit`