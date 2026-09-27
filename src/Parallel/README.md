# ![PDFtoImage.Parallel Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_Parallel_128.png) PDFtoImage.Parallel

[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

True parallel PDF rendering for [PDFtoImage](https://www.nuget.org/packages/PDFtoImage/) by distributing PDFium work across isolated worker processes.

PDFium is not thread-safe, so the main PDFtoImage package serializes access to it inside a process. PDFtoImage.Parallel creates multiple worker processes instead, allowing pages and independent PDFs to be rendered concurrently.

## Requirements

* .NET 11 or later
* Windows 10 or later / Windows Server 2016 or later / Linux (glibc or musl) / macOS 14 or later
* Ability to launch child processes and use local pipes

## Getting started

Create one `ParallelPdfProcessor` and reuse it for multiple conversions:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor();

using var image = await converter.ToImageAsync(
    File.OpenRead("document.pdf"),
    page: 0);
```

The same pool can serve concurrent requests for different PDFs:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor();

var first = converter.ToImageAsync(File.OpenRead("a.pdf"), 0);
var second = converter.ToImageAsync(File.OpenRead("b.pdf"), 0);

using var imageA = await first;
using var imageB = await second;
```

To render multiple pages while receiving them in the requested order:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor();

await foreach (var image in converter.ToImagesAsync(File.OpenRead("document.pdf")))
{
    using (image)
    {
        // process the page
    }
}
```

Dispose returned `SKBitmap` instances after use. To save one, use [SKBitmap.Encode](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?view=skiasharp).

## ASP.NET Core dependency injection

Register one processor as a singleton so requests share its worker pool:

```csharp
builder.Services.AddSingleton(_ => new PDFtoImage.Parallel.ParallelPdfProcessor(workerCount: 4));
```

Omitting `workerCount` or passing `null` uses [`Environment.ProcessorCount`](https://learn.microsoft.com/en-us/dotnet/api/system.environment.processorcount).

## Worker pool and lifetime

Workers start on demand and are reused until the processor is disposed. Set `workerCount` to control the pool size.

Cancellation or a worker failure does not prevent later requests. Workers also exit if the parent process stops.

## Deployment

Framework-dependent, self-contained, trimmed single-file, and Native AOT applications are supported.

## Memory considerations

The processor buffers each PDF in memory, and workers may load additional copies. Large PDFs and concurrent requests can use substantial memory.

PDF data and rendered bitmaps must each fit in a 1 GiB IPC message, including protocol metadata.

## Worker bootstrap
No separate worker executable is deployed. PDFtoImage.Parallel re-launches the consuming application and enters worker mode before `Main`. CoreCLR uses a startup hook, so `System.StartupHookProvider.IsSupported` must not be explicitly disabled; the package explicitly re-enables startup-hook support for trimmed CoreCLR publishes. Native AOT uses a module initializer instead.