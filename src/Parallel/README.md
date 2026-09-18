# ![PDFtoImage.Parallel Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_Parallel_128.png) PDFtoImage.Parallel

[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

True parallel PDF rendering for [PDFtoImage](https://www.nuget.org/packages/PDFtoImage/) by distributing PDFium work across isolated worker processes.

PDFium is not thread-safe, so the main PDFtoImage package serializes access to it inside a process. PDFtoImage.Parallel creates multiple worker processes instead, allowing pages and independent PDFs to be rendered concurrently.

## Requirements
* .NET 11
* Windows 10 / Windows Server 2016 or newer, Linux, or macOS
* Permission to start subprocesses

The package references [PDFtoImage](https://www.nuget.org/packages/PDFtoImage/) for the actual PDF rendering implementation.

## Getting started
Create one `ParallelPdfProcessor` and reuse it for multiple conversions:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor(workerCount: 8);

using var image = await converter.ToImageAsync(
    File.OpenRead("document.pdf"),
    page: 0);
```

The same pool can serve concurrent requests for different PDFs:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor(workerCount: 8);

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

Dispose returned `SKBitmap` instances after use.

## Worker pool and lifetime
Workers start on demand and are reused until the processor is disposed. The default worker limit is the processor count; pass an explicit `workerCount` when a smaller process or memory footprint is preferable.

Cancellation and worker failures do not make the processor unusable for later requests. Dispose the processor to terminate its workers. Worker lifetime is also tied to the parent process so orphan workers are cleaned up when the parent exits.

## Deployment
PDFtoImage.Parallel supports:
* framework-dependent apphost executables
* `dotnet app.dll`
* self-contained applications
* trimmed single-file applications
* Native AOT applications

CoreCLR workers enter through a trim-preserved startup hook. Native AOT workers enter through a module initializer.

## Memory considerations
The PDF input is buffered before it is sent to workers, and the same document can be loaded into more than one worker during concurrent rendering. Large PDFs combined with a high worker count can therefore increase memory usage. Choose `workerCount` according to the workload and available memory.

Each IPC message is limited to 1 GiB. A PDF must therefore be slightly smaller than 1 GiB because the load message also contains protocol metadata and the optional password. Known oversized stream lengths are rejected before allocation; unknown-length streams are rejected while they are buffered. A rendered bitmap, including its response metadata, must also fit into one 1 GiB IPC message.
