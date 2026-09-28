# ![PDFtoImage Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_64.png) PDFtoImage

[![GitHub Workflow Build Status](https://img.shields.io/github/actions/workflow/status/sungaila/PDFtoImage/dotnet.yml?event=push&style=flat-square&logo=github&logoColor=white)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![GitHub Workflow Test Runs Succeeded](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fgist.githubusercontent.com%2Fsungaila%2F003e8ab2211221897e4b3c0e564ed7b6%2Fraw&query=%24.stats.runs_succ&suffix=%20passed&style=flat-square&logo=github&logoColor=white&label=tests&color=45cc11)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![SonarCloud Quality Gate](https://img.shields.io/sonar/quality_gate/sungaila_PDFtoImage?server=https%3A%2F%2Fsonarcloud.io&style=flat-square&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/dashboard?id=sungaila_PDFtoImage)
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![Website](https://img.shields.io/website?up_message=online&down_message=offline&url=https%3A%2F%2Fwww.sungaila.de%2FPDFtoImage%2F&style=flat-square&label=website)](https://www.sungaila.de/PDFtoImage/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

A .NET library for rendering PDF files as images.

PDFtoImage uses [PDFium](https://pdfium.googlesource.com/pdfium/) for rendering and [SkiaSharp](https://github.com/mono/SkiaSharp) for images.

## Getting started
Render the first page of a PDF as PNG with `PDFtoImage.Conversion`:

```csharp
using var pdf = File.OpenRead("document.pdf");

PDFtoImage.Conversion.SavePng(
    imageFilename: "page1.png",
    pdfStream: pdf,
    page: 0);
```

`SaveJpeg`, `SavePng`, `SaveWebp`, and `ToImage` render a single page. `ToImages` and `ToImagesAsync` render multiple pages.

Dispose returned `SKBitmap` instances after use. To save one, use [SKBitmap.Encode](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?view=skiasharp).

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
PDFium is not thread-safe, so PDFtoImage serializes PDFium calls within each process. For parallel rendering on .NET 11 or later, use [PDFtoImage.Parallel](https://www.nuget.org/packages/PDFtoImage.Parallel/). It runs PDFium in separate worker processes; see the [Parallel README](src/Parallel/README.md) for examples.

## Index and Range for .NET Framework
[PolySharp](https://github.com/Sergio0694/PolySharp) provides `System.Index` and `System.Range` for .NET Framework projects. It also exposes the following generated types; avoid using them directly:

- `System.Index`
- `System.Range`
- `System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute`
- `System.Diagnostics.CodeAnalysis.NotNullWhenAttribute`
- `System.Runtime.CompilerServices.IsExternalInit`