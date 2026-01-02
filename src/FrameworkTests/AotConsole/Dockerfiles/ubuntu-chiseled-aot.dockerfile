FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Packages.props", "src/Directory.Packages.props"]
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" -r linux-x64 -p:TargetFramework=net10.0 -p:PublishAot=true -p:SelfContained=true
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c "$BUILD_CONFIGURATION" -o /app/build -r linux-x64 --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN apt update && apt install -y \
  clang \
  zlib1g-dev
RUN dotnet publish "./FrameworkTests/AotConsole/AotConsole.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish -r linux-x64 --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./PDFtoImage.FrameworkTests.AotConsole"]