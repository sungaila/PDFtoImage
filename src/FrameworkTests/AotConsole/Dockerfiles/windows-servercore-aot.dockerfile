FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-windowsservercore-ltsc2022 AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" /p:TargetFramework=net9.0
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/build --no-restore

FROM build AS vsbuildtools
SHELL ["powershell", "-NoLogo", "-NonInteractive", "-Command"]

RUN Invoke-WebRequest -Uri https://aka.ms/vs/17/release/vs_buildtools.exe -OutFile vs_buildtools.exe
RUN .\vs_buildtools.exe --quiet --wait --norestart --nocache \
    --installPath \"%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\" \
    --add Microsoft.VisualStudio.Component.Roslyn.Compiler \
    --add Microsoft.Component.MSBuild \
    --add Microsoft.VisualStudio.Component.CoreBuildTools \
    --add Microsoft.VisualStudio.Workload.MSBuildTools \
    --add Microsoft.VisualStudio.Component.Windows10SDK \
    --add Microsoft.VisualStudio.Component.VC.CoreBuildTools \
    --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 \
    --add Microsoft.VisualStudio.Component.VC.Redist.14.Latest \
    --add Microsoft.VisualStudio.Component.Windows11SDK.22621 \
    --add Microsoft.VisualStudio.Component.TextTemplating \
    --add Microsoft.VisualStudio.Component.VC.CoreIde \
    --add Microsoft.VisualStudio.ComponentGroup.NativeDesktop.Core \
    --add Microsoft.VisualStudio.Workload.VCTools \
    --add Microsoft.Component.MSBuild \
    --add Microsoft.VisualStudio.Workload.VCTools
RUN Remove-Item vs_buildtools.exe

FROM vsbuildtools AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]