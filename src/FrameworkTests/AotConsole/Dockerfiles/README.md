# AotConsole container smoke tests

All build contexts are the repository root.

## Linux

The Linux Dockerfiles support `linux/amd64` and `linux/arm64` through BuildKit's
`TARGETARCH` argument. Native AOT builds compile on `TARGETPLATFORM`; use native
ARM64 runners where possible, because AOT compilation under QEMU is much slower.

Examples:

```bash
docker buildx build --load --platform linux/amd64 \
  -f src/FrameworkTests/AotConsole/Dockerfiles/ubuntu.dockerfile \
  -t pdftoimage-smoke:ubuntu .
docker run --rm pdftoimage-smoke:ubuntu
```

```bash
docker buildx build --load --platform linux/arm64 \
  -f src/FrameworkTests/AotConsole/Dockerfiles/alpine-aot.dockerfile \
  -t pdftoimage-smoke:alpine-aot .
docker run --rm --platform linux/arm64 pdftoimage-smoke:alpine-aot
```

The `singlefile` variants enable `IncludeNativeLibrariesForSelfExtract` to test
that PDFium and SkiaSharp native assets remain usable after bundle extraction.

## Windows

Framework-dependent, self-contained and single-file builds are produced inside
Windows SDK containers. Windows Native AOT still requires the MSVC toolchain on
the host; the AOT Dockerfiles therefore only package a previously published
`win-x64` directory.

Nano Server variants are intentionally kept as extended compatibility tests.
They can expose native Win32 imports that are unavailable on Nano Server.
