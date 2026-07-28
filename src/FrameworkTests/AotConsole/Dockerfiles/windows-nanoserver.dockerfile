# syntax=docker/dockerfile:1
# escape=`

ARG DOTNET_VERSION=10.0
ARG WINDOWS_VERSION=ltsc2025

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-windowsservercore-${WINDOWS_VERSION} AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR C:/src

COPY ["src/Directory.Packages.props", "src/Directory.Packages.props"]
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "src/FrameworkTests/AotConsole/AotConsole.csproj" -r win-x64 -p:TargetFramework=net10.0 -p:PublishAot=false -p:SelfContained=false

COPY . .
WORKDIR C:/src/src
RUN dotnet publish "FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -r win-x64 -o C:/app/publish --no-restore -p:TargetFramework=net10.0 -p:PublishAot=false -p:SelfContained=false -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}-nanoserver-${WINDOWS_VERSION} AS final
WORKDIR C:/app
COPY --from=publish C:/app/publish .
USER ContainerUser
ENTRYPOINT ["dotnet", "PDFtoImage.FrameworkTests.AotConsole.dll"]
