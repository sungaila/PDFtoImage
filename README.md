# ![PDFtoImage Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_64.png) PDFtoImage

[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/sungaila/dab6d897-d625-40f2-a97a-e985a543e393/5/master?style=flat-square)](https://dev.azure.com/sungaila/PDFtoImage/_build/latest?definitionId=5&branchName=master)
[![Azure DevOps tests (branch)](https://img.shields.io/azure-devops/tests/sungaila/PDFtoImage/5/master?style=flat-square)](https://dev.azure.com/sungaila/PDFtoImage/_build/latest?definitionId=5&branchName=master)
[![SonarCloud Quality Gate](https://img.shields.io/sonar/quality_gate/sungaila_PDFtoImage?server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/dashboard?id=sungaila_PDFtoImage)
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.svg?style=flat-square)](https://www.nuget.org/packages/PDFtoImage/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.svg?style=flat-square)](https://www.nuget.org/packages/PDFtoImage/)
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

*Note: An [`SkiaSharp.SKBitmap`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap) object can be exported with the [`Encode`](https://docs.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?SkiaSharp_SKBitmap_Encode_System_IO_Stream_SkiaSharp_SKEncodedImageFormat_System_Int32_) method.*