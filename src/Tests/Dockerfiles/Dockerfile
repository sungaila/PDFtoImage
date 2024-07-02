FROM mcr.microsoft.com/dotnet/sdk:8.0
USER app
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "test", "/logger:console;verbosity=detailed", "/logger:trx;verbosity=detailed"]