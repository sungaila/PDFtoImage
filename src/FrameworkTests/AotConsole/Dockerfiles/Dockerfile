FROM mcr.microsoft.com/dotnet/runtime:8.0
USER app
WORKDIR /app
COPY ["AotConsole", "."]
ENTRYPOINT ["dotnet", "net8.0/PDFtoImage.FrameworkTests.AotConsole.dll"]