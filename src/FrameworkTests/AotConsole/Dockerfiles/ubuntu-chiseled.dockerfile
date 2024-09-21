﻿FROM mcr.microsoft.com/dotnet/runtime:9.0-noble-chiseled AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FrameworkTests/AotConsole/AotConsole.csproj", "src/FrameworkTests/AotConsole/AotConsole.csproj"]
COPY ["src/PDFtoImage", "src/PDFtoImage"]
RUN dotnet restore "./src/FrameworkTests/AotConsole/AotConsole.csproj" /p:TargetFramework=net9.0
COPY . .
WORKDIR "/src/src"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet build "./FrameworkTests/AotConsole/AotConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "PDFtoImage.FrameworkTests.AotConsole.dll"]