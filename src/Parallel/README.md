# ![PDFtoImage.Parallel Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_Parallel_64.png) PDFtoImage.Parallel
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

True parallel PDF rendering for [PDFtoImage](https://www.nuget.org/packages/PDFtoImage/) by distributing PDFium work across isolated worker processes.

## What this project provides

PDFium is not thread-safe, so parallel rendering requires multiple processes. PDFtoImage.Parallel provides a simple stream-to-`SKBitmap` API and hides the process orchestration behind it.

It handles:

* Starting, monitoring, and reusing worker processes
* Sending requests and results through local IPC pipes
* Loading PDFs in workers and reusing documents across page jobs
* Scheduling page jobs and returning images in the requested order
* Detecting worker crashes and creating replacements for later requests
* Cancelling work and cleaning up workers, documents, and temporary files

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

Dispose returned `SKBitmap` instances after use. To save one, use [`SKBitmap.Encode`](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap.encode?view=skiasharp).

## ASP.NET Core dependency injection
Register one processor as a singleton so requests share its worker pool. The public `IParallelPdfProcessor` interface can be used for injection:

```csharp
builder.Services.AddSingleton<PDFtoImage.Parallel.IParallelPdfProcessor>(_ =>
    new PDFtoImage.Parallel.ParallelPdfProcessor(new PDFtoImage.Parallel.ProcessorOptions
    {
        WorkerCount = 8
    }));
```

`WorkerCount` sets the maximum number of worker processes; `null` uses [`Environment.ProcessorCount`](https://learn.microsoft.com/en-us/dotnet/api/system.environment.processorcount). `ProcessorOptions` implements `IProcessorOptions` and provides room for future settings.

## Limit parallelism for memory backpressure

A service may keep a large worker pool but limit simultaneous operations to control the memory used by active renders and queued page results:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor(
    new PDFtoImage.Parallel.ProcessorOptions
    {
        WorkerCount = 16,
        SlotCount = 4
    });
```

`SlotCount` defaults to `null`, which leaves only `WorkerCount` as the limit. It limits work entering the worker pool across concurrent requests; input streams in the default IPC mode are still buffered before they reach that limit.

## File-backed transfer for throughput

For workloads where copying large PDFs and bitmaps through IPC is expensive, use temporary PDF files and raw file-backed memory maps for bitmap pixels:

```csharp
await using var converter = new PDFtoImage.Parallel.ParallelPdfProcessor(
    new PDFtoImage.Parallel.ProcessorOptions
    {
        WorkerCount = 8,
        TransferMode = PDFtoImage.Parallel.ProcessorTransferMode.MemoryMappedFile
    });
```

Workers read the same temporary PDF file. The host copies each mapped bitmap into the returned `SKBitmap` and removes its temporary file before returning it. Throughput depends on PDF size, output size, and temporary-storage performance; benchmark both modes for your workload.

## Technical considerations
### Worker pool and lifetime
Workers start on demand and are reused until the processor is disposed. Set `WorkerCount` to control the pool size. Set `SlotCount` to a positive number to cap simultaneous worker operations across requests; its default `null` leaves the worker count as the only limit. The ordered page scheduler also limits its look-ahead to this setting. This provides backpressure for services with a large worker pool.

Cancellation or a worker failure does not prevent later requests. Workers also exit if the parent process stops.

### Single-worker fault isolation
Using `WorkerCount = 1` does not provide parallel rendering, but still runs PDFium out of process. This can be useful when isolating the host application from native worker failures (e.g. a PDFium process crash) is more important than parallel throughput.

Worker process isolation can protect the host from native PDFium crashes, but it is not a security sandbox. Workers normally run with the same user security context as the host application.

### Deployment
Framework-dependent, self-contained, trimmed single-file, and Native AOT applications are supported.

### Memory
With the default `ProcessorTransferMode.Ipc`, the processor buffers each PDF in memory, copies it over IPC, and receives bitmap pixels over IPC. Workers may load additional copies. Large PDFs and concurrent requests can use substantial memory.

`ProcessorTransferMode.MemoryMappedFile` buffers each PDF in a temporary file shared read-only by workers. Bitmap pixels are written into a raw, file-backed memory map and copied into the returned `SKBitmap` by the host. The host deletes bitmap files before returning each image and deletes PDF files when the request completes or is cancelled. Disposing the processor also removes PDF files held by unfinished enumerations. This mode needs writable temporary storage and trades disk I/O for lower IPC buffering.

In IPC mode, PDF data and rendered bitmaps must each fit in a 1 GiB IPC message, including protocol metadata. The file-backed mode does not use this IPC payload limit for PDF data or bitmap pixels.

### Worker bootstrap
No separate worker executable is deployed. PDFtoImage.Parallel re-launches the consuming application and enters worker mode before `Main`. CoreCLR uses a startup hook, so `System.StartupHookProvider.IsSupported` must not be explicitly disabled; the package explicitly re-enables startup-hook support for trimmed CoreCLR publishes. Native AOT uses a module initializer instead.