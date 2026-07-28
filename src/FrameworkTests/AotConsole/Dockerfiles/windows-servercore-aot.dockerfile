# syntax=docker/dockerfile:1
# escape=`

ARG WINDOWS_VERSION=ltsc2025
FROM mcr.microsoft.com/windows/servercore:${WINDOWS_VERSION}
WORKDIR C:/app

ARG PUBLISH_DIR=artifacts/win-x64-aot
COPY ${PUBLISH_DIR}/ .

USER ContainerUser
ENTRYPOINT ["PDFtoImage.FrameworkTests.AotConsole.exe"]
