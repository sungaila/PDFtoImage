# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=10.0

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine-aot AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src

COPY . .
WORKDIR /src/src

# Keep restore and publish in the same cache-mount lifetime. GitHub Actions
# exports normal BuildKit layers, but not the contents of exec cache mounts.
# Restrict the multi-targeted wrapper to net10.0 so mobile workloads aren't
# evaluated in these Linux smoke-test images.
RUN --mount=type=cache,id=nuget-alpine-aot,target=/root/.nuget/packages,sharing=locked \
    case "$TARGETARCH" in \
      amd64) rid=linux-musl-x64 ;; \
      arm64) rid=linux-musl-arm64 ;; \
      *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
    esac && \
    dotnet restore FrameworkTests/AotConsole/AotConsole.csproj \
      -r "$rid" \
      -p:TargetFrameworks=net10.0 \
      -p:PublishAot=true \
      -p:SelfContained=true && \
    dotnet publish FrameworkTests/AotConsole/AotConsole.csproj \
      -c "$BUILD_CONFIGURATION" \
      -f net10.0 \
      -r "$rid" \
      -o /app/publish \
      --no-restore \
      -p:TargetFrameworks=net10.0 \
      -p:PublishAot=true \
      -p:SelfContained=true \
      -p:StripSymbols=true

# Use the regular runtime-deps flavor because third-party native libraries can
# still require libstdc++, even though the managed application is Native AOT.
FROM mcr.microsoft.com/dotnet/runtime-deps:${DOTNET_VERSION}-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["./PDFtoImage.FrameworkTests.AotConsole"]
