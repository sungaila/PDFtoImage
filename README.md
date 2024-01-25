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
* [PdfiumViewer](https://github.com/pvginkel/PdfiumViewer) (wrapper for PDFium)
* [SkiaSharp](https://github.com/mono/SkiaSharp) (cross-platform 2D graphics API)

## Getting started
Call a static method from `PDFtoImage.Conversion`:

`SaveJpeg`, `SavePng`, `SaveWebp` and `ToImage` for a **single page**.

`ToImages` and `ToImagesAsync` for **multiple pages**.

*Note: [`SkiaSharp.SKBitmap`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap) can be exported with the [`Encode`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?SkiaSharp_SKBitmap_Encode_System_IO_Stream_SkiaSharp_SKEncodedImageFormat_System_Int32_) method.*

## Tested and supported frameworks
* [ASP.NET](https://learn.microsoft.com/en-us/aspnet/overview)
* [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)
* [Mono](https://www.mono-project.com)
* [.NET (Core)](https://learn.microsoft.com/en-us/dotnet/core/introduction)
* [.NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/get-started/overview)
* [.NET Multi-platform App UI (.NET MAUI)](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui) (excluding macOS and iOS)
* [Xamarin.Android](https://learn.microsoft.com/en-us/xamarin/android)
* [Universal Windows Platform (UWP)](https://learn.microsoft.com/en-us/windows/uwp/get-started/universal-application-platform-guide)