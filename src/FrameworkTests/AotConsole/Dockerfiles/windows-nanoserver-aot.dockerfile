FROM mcr.microsoft.com/windows/nanoserver:ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS sdk
SHELL ["cmd", "/S", "/C"]

RUN powershell -c "Invoke-WebRequest -Uri https://aka.ms/vs/17/release/vs_buildtools.exe -OutFile vs_buildtools.exe"
RUN (start /w vs_buildtools.exe --wait --norestart --nocache \
        --installPath "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools" \
        --add Microsoft.VisualStudio.Workload.VCTools \
        --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 \
        || IF "%ERRORLEVEL%"=="3010" EXIT 0) \
    && del /q vs_buildtools.exe

FROM sdk AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" /p:TargetFramework=net8.0
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]