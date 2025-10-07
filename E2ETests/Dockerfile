# Use .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

RUN apt-get update && apt-get install -y \
    libglib2.0-0 \
    libnss3 \
    libnspr4 \
    libdbus-1-3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libx11-6 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libxcb1 \
    libxkbcommon0 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2 \
    libatspi2.0-0 \
    && rm -rf /var/lib/apt/lists/*

ENV NUGET_PACKAGES=/root/.nuget/packages
RUN mkdir -p $NUGET_PACKAGES

COPY E2ETests.csproj ./

RUN dotnet restore --packages $NUGET_PACKAGES E2ETests.csproj \
 && dotnet build E2ETests.csproj --configuration Release

ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global Microsoft.Playwright.CLI \
 && playwright install --with-deps

COPY . .

RUN dotnet build E2ETests.csproj --configuration Release

CMD ["dotnet", "test", "E2ETests.csproj", "--verbosity", "detailed", "--filter", "TestCategory!=Failing"]
