# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project files
COPY E2ETests.csproj . 

# Set NuGet package location inside the container
ENV NUGET_PACKAGES=/root/.nuget/packages
RUN mkdir -p $NUGET_PACKAGES

# Restore NuGet packages in the correct location
RUN dotnet restore --packages $NUGET_PACKAGES --force

# Copy the remaining source files
COPY . .

# Build the test project
RUN dotnet build --no-restore --configuration Release

# Install Playwright CLI and Browsers
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN /root/.dotnet/tools/playwright install

# Run Playwright E2E tests
CMD ["dotnet", "test", "--no-build", "--verbosity", "normal"]
