# escape=`
FROM mcr.microsoft.com/windows/servercore:ltsc2025
WORKDIR /app

ARG PUBLISH_DIR=artifacts/win-x64-aot
COPY ${PUBLISH_DIR} C:/app/

ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]