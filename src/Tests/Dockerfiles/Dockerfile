FROM mcr.microsoft.com/dotnet/sdk:8.0
USER app
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "test", "net8.0/PDFtoImage.Tests.dll", "/logger:console;verbosity=detailed", "/logger:trx;verbosity=detailed"]