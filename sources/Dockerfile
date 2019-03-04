FROM mono:latest AS build-env
WORKDIR /source

# Copy everything else and build
COPY . ./
RUN mono --version
RUN nuget restore nys.sln
RUN msbuild nys.sln /p:OutputPath=/app

# Build runtime image
FROM mono:latest
WORKDIR /app
COPY --from=build-env /app .
ENTRYPOINT ["mono", "/app/nys.selfhosted.exe"]