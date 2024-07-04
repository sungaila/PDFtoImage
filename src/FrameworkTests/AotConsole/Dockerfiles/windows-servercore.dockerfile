FROM mcr.microsoft.com/dotnet/runtime:8.0-windowsservercore-ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" /p:TargetFramework=net8.0
COPY . .
WORKDIR "/src/src"

FROM restore AS build
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c %BUILD_CONFIGURATION% -o /app/build --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "PDFtoImage.FrameworkTests.AotConsole.dll"]