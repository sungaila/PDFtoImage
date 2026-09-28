# ![PDFtoImage.Parallel Logo](https://raw.githubusercontent.com/sungaila/PDFtoImage/master/etc/Icon_Parallel_64.png) PDFtoImage.Parallel
[![NuGet version](https://img.shields.io/nuget/v/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PDFtoImage.Parallel.svg?style=flat-square&logo=nuget&logoColor=white)](https://www.nuget.org/packages/PDFtoImage.Parallel/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PDFtoImage?style=flat-square)](https://github.com/sungaila/PDFtoImage/blob/master/LICENSE)

True parallel PDF rendering for [PDFtoImage](https://www.nuget.org/packages/PDFtoImage/) by distributing PDFium work across isolated worker processes.

## What this library provides
PDFium is not thread-safe, so parallel rendering requires multiple processes. PDFtoImage.Parallel provides a simple stream-to-[`SKBitmap`](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap?view=skiasharp) API and hides the process orchestration behind it.

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

`WorkerCount` sets the maximum number of worker processes; `null` uses [`Environment.ProcessorCount`](https://learn.microsoft.com/en-us/dotnet/api/system.environment.processorcount).

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

`SlotCount` defaults to `null`, which leaves only `WorkerCount` as the limit. It limits document loading and rendering across concurrent requests; cleanup can proceed without waiting for a render slot. The effective rendering limit is the smaller of `WorkerCount` and `SlotCount`.

This is a concurrency limit, not a memory budget: IPC input streams are buffered before entering the pool, each enumeration has its own bounded look-ahead, and returned bitmaps belong to the caller. Dispose returned images promptly. Dispose manually created async enumerators when stopping early; `await foreach` does this automatically.

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

Workers read the same PDF file. The host copies each mapped bitmap into the returned `SKBitmap` and removes its temporary file before returning it. Throughput depends on PDF size, output size, and temporary-storage performance; benchmark both modes for your workload.

## Technical considerations
### Worker pool and lifetime
Workers start on demand and are reused until the processor is disposed. Set `WorkerCount` to control the pool size. Set `SlotCount` to a positive number to cap simultaneous worker operations across requests; its default `null` leaves the worker count as the only limit. The ordered page scheduler also limits its look-ahead to this setting. This provides backpressure for services with a large worker pool.

Cancellation or a worker failure does not prevent later requests. Workers also exit if the parent process stops.

### Single-worker fault isolation
Using `WorkerCount = 1` does not provide parallel rendering, but still runs PDFium out of process. This can be useful when isolating the host application from native worker failures (e.g. PDFium crashes the process) is more important than parallel throughput.

### Security
Worker processes isolate PDFium crashes, but process isolation is not a security sandbox. Workers run with the host application's user identity and privileges. For publicly supplied, untrusted PDFs, run the service with minimal privileges and use an OS or container sandbox if stronger isolation is required.

In `MemoryMappedFile` mode, set `TempDirectory` to a private, access-controlled directory outside the web root; the default system temporary directory can be shared. Temporary PDFs and raw bitmaps can contain sensitive data. The host creates files with unique names and restricts Unix file permissions, but the service controls the directory and its Windows ACL. Allow only the service account to access it, and provide enough space for concurrent renders.

Reused `FileStream` inputs are reopened by path. Keep uploaded files in a protected location and unchanged until rendering finishes.

The consuming service should validate uploads and set limits for PDF size, page count, render dimensions or DPI, request concurrency, execution time, and temporary storage. Pass request cancellation to the processor. `SlotCount` limits worker operations, not input buffering, queued bitmaps, or disk usage.

### Memory
The default `ProcessorTransferMode.Ipc` mode buffers each PDF in host memory, sends it through local pipes, and receives bitmap pixels the same way. Workers may hold additional PDF copies. `ProcessorTransferMode.MemoryMappedFile` lets workers read a shared PDF file and render directly into a raw, file-backed bitmap. The host copies those pixels once into the returned `SKBitmap` and removes temporary files when they are no longer needed.

Use `Ipc` when:

* PDFs and rendered pages are relatively small.
* Avoiding temporary files is preferred.
* Simpler deployment matters more than maximum throughput.

Use `MemoryMappedFile` when:

* Rendering at high DPI or large dimensions.
* Processing large or many-page PDFs.
* PDFs are supplied as readable, seekable `FileStream` instances, which can be reopened without a PDF copy.
* Multiple pages are rendered concurrently.
* IPC memory pressure or its 1 GiB message limit is relevant.

The file-backed mode needs writable temporary storage and trades disk I/O for lower IPC buffering. Its PDF and bitmap payloads are not subject to the IPC message limit.

### Worker bootstrap
No separate worker executable is deployed. PDFtoImage.Parallel re-launches the consuming application and enters worker mode before `Main`. CoreCLR uses a startup hook, so `System.StartupHookProvider.IsSupported` must not be explicitly disabled; the package explicitly re-enables startup-hook support for trimmed CoreCLR publishes. Native AOT uses a module initializer instead.

### Deployment
Framework-dependent, self-contained, trimmed single-file, and Native AOT applications are supported.