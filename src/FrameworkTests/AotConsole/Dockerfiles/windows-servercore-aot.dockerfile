# escape=`
FROM mcr.microsoft.com/windows/servercore:ltsc2025 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-windowsservercore-ltsc2025 AS restore
SHELL ["cmd", "/S", "/C"]
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore ".\src\FrameworkTests\AotConsole\AotConsole.csproj" /p:TargetFramework=net9.0
COPY . .

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build ".\src\FrameworkTests\AotConsole\AotConsole.csproj" -c %BUILD_CONFIGURATION% --no-restore -o C:\app\build

FROM build AS vsbuildtools
SHELL ["cmd", "/S", "/C"]
RUN curl. -L -o C:\temp\vs_buildtools.exe https://aka.ms/vs/17/release/vs_buildtools.exe `
 && (start /w C:\temp\vs_buildtools.exe --quiet --wait --norestart --nocache `
       --installPath "C:\BuildTools" `
       --add Microsoft.VisualStudio.Workload.VCTools `
       --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 `
       --add Microsoft.VisualStudio.Component.Windows11SDK.26100 `
       --add Microsoft.Component.MSBuild `
       || IF "%ERRORLEVEL%"=="3010" EXIT 0) `
 && del C:\temp\vs_buildtools.exe

FROM vsbuildtools AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN "C:\BuildTools\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64 `
    && dotnet publish ".\src\FrameworkTests\AotConsole\AotConsole.csproj" `
      -c %BUILD_CONFIGURATION% -r win-x64 -p:PublishAot=true -p:SelfContained=true -p:StripSymbols=true `
      -o C:\app\publish

FROM base AS final
WORKDIR /app
COPY --from=publish C:\app\publish .
ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]