# ![PDFtoImage Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_64.png) PDFtoImage

[![GitHub Workflow Build Status](https://img.shields.io/github/actions/workflow/status/sungaila/PDFtoImage/dotnet.yml?event=push&style=flat-square&logo=github&logoColor=white)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![GitHub Workflow Test Runs Succeeded](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fgist.githubusercontent.com%2Fsungaila%2F003e8ab2211221897e4b3c0e564ed7b6%2Fraw&query=%24.stats.runs_succ&suffix=%20passed&style=flat-square&logo=github&logoColor=white&label=tests&color=45cc11)](https://github.com/sungaila/PDFtoImage/actions/workflows/dotnet.yml)
[![SonarCloud Quality Gate](https://img.shields.io/sonar/quality_gate/sungaila_PDFtoImage?server=https%3A%2F%2Fsonarcloud.io&style=flat-square&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/dashboard?id=sungaila_PDFtoImage)
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage/)
[![Website](https://img.shields.io/website?up_message=online&down_message=offline&url=https%3A%2F%2Fwww.sungaila.de%2FPDFtoImage%2F&style=flat-square&label=website)](https://www.sungaila.de/PDFtoImage/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

A Unity package for rendering PDF files as images.

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

Dispose returned `SKBitmap` instances after use. To save one, use [`SKBitmap.Encode`](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?view=skiasharp).