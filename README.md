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

## Getting started
Call a static method from `PDFtoImage.Conversion`:

`SaveBmp`, `SaveGif`, `SaveJpeg`, `SavePng`, `SaveTiff` and `ToImage` for a **single page**.

`ToImages` and `ToImagesAsync` for **multiple pages**.

*Note: An [`System.Drawing.Image`](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image) object can be exported with the [`Save`](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image.save) method.*

## Prerequisite libgdiplus
On platforms other than Windows you will have to have [libgdiplus](https://www.mono-project.com/docs/gui/libgdiplus/) installed.
### Debian-based Linux distributions
```console
sudo apt-get install libgdiplus
```

### macOS (via [Homebrew](https://brew.sh/))
```console
brew install mono-libgdiplus
```