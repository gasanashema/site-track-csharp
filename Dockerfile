# SiteTrack Construction Manager
# Cross-Platform Build & Automated Test Container
FROM mono:6.12 AS build
WORKDIR /app

# Copy entire repository
COPY . .

# Build the solution in Release mode using Mono's msbuild
RUN msbuild SiteTrack.sln /p:Configuration=Release /t:Build

# Default command: execute the automated test runner
CMD ["mono", "src/SiteTrack.Tests/bin/Release/SiteTrack.Tests.exe"]
