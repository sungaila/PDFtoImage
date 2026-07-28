# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=10.0

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-noble-aot AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src

COPY src/Directory.Packages.props src/Directory.Packages.props
COPY src/FrameworkTests/AotConsole/AotConsole.csproj src/FrameworkTests/AotConsole/AotConsole.csproj
COPY src/PDFtoImage src/PDFtoImage

RUN --mount=type=cache,id=nuget-ubuntu-chiseled-aot,target=/root/.nuget/packages,sharing=locked \
    case "$TARGETARCH" in \
      amd64) rid=linux-x64 ;; \
      arm64) rid=linux-arm64 ;; \
      *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac && \
    dotnet restore src/FrameworkTests/AotConsole/AotConsole.csproj \
      -r "$rid" \
      -p:TargetFramework=net10.0 \
      -p:PublishAot=true \
      -p:SelfContained=true

COPY . .
WORKDIR /src/src

RUN --mount=type=cache,id=nuget-ubuntu-chiseled-aot,target=/root/.nuget/packages,sharing=locked \
    case "$TARGETARCH" in \
      amd64) rid=linux-x64 ;; \
      arm64) rid=linux-arm64 ;; \
      *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac && \
    dotnet publish FrameworkTests/AotConsole/AotConsole.csproj \
      -c "$BUILD_CONFIGURATION" \
      -r "$rid" \
      -o /app/publish \
      -p:TargetFramework=net10.0 \
      -p:PublishAot=true \
      -p:SelfContained=true \
      -p:StripSymbols=true

# Do not use noble-chiseled-aot here: PDFium/SkiaSharp may need libstdc++.
FROM mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}-noble-chiseled AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["./PDFtoImage.FrameworkTests.AotConsole"]
