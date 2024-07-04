FROM mcr.microsoft.com/windows/nanoserver:ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS sdk
# Restore the default Windows shell for correct batch processing.
SHELL ["cmd", "/S", "/C"]

RUN `
    # Download the Build Tools bootstrapper.
    curl -SL --output vs_buildtools.exe https://aka.ms/vs/17/release/vs_buildtools.exe `
    `
    # Install Build Tools with the Microsoft.VisualStudio.Workload.VCTools workload
    && (start /w vs_buildtools.exe --quiet --wait --norestart --nocache `
        --installPath "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools" `
        --add Microsoft.VisualStudio.Workload.VCTools `
        --remove Microsoft.VisualStudio.Component.Windows10SDK.10240 `
        --remove Microsoft.VisualStudio.Component.Windows10SDK.10586 `
        --remove Microsoft.VisualStudio.Component.Windows10SDK.14393 `
        --remove Microsoft.VisualStudio.Component.Windows81SDK `
        || IF "%ERRORLEVEL%"=="3010" EXIT 0) `
    `
    # Cleanup
    && del /q vs_buildtools.exe

FROM sdk AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" /p:TargetFramework=net8.0
COPY . .
WORKDIR "/src/src"
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]