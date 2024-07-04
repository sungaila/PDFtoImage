FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS restore
ARG BUILD_CONFIGURATION=Release
ARG TARGET_FRAMEWORK=net8.0
WORKDIR /src
COPY ["src/Tests/Tests.csproj", "src/Tests/Tests.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/Tests/Tests.csproj" /p:TargetFramework=%TARGET_FRAMEWORK%
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGET_FRAMEWORK=net8.0
RUN dotnet build "./Tests/Tests.csproj" -c %BUILD_CONFIGURATION% -o /app/build/%TARGET_FRAMEWORK% --no-restore -f %TARGET_FRAMEWORK%

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGET_FRAMEWORK=net8.0
RUN dotnet publish "./Tests/Tests.csproj" -c %BUILD_CONFIGURATION% -o /app/publish --no-restore -f %TARGET_FRAMEWORK%

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "test", "publish/PDFtoImage.Tests.dll", "/logger:console;verbosity=detailed", "/logger:trx;verbosity=detailed"]