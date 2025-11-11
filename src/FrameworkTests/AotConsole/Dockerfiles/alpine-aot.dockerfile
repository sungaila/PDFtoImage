FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
ARG BUILD_CONFIGURATION=Release
RUN apk update \
    && apk add build-base zlib-dev
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" -r linux-musl-x64 -p:TargetFramework=net10.0
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-musl-x64 --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrameworkTests/AotConsole/AotConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-musl-x64 --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./PDFtoImage.FrameworkTests.AotConsole"]