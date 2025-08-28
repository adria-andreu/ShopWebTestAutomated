# Use Microsoft's official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install necessary packages for Playwright
RUN apt-get update && apt-get install -y \
    wget \
    curl \
    gnupg \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy project files
COPY ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj ./ShopWeb.E2E.Tests/
COPY tools/GateCheck/GateCheck.csproj ./tools/GateCheck/

# Restore dependencies
RUN dotnet restore ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj
RUN dotnet restore tools/GateCheck/GateCheck.csproj

# Copy source code
COPY . .

# Build the projects
RUN dotnet build ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj --configuration Release --no-restore
RUN dotnet build tools/GateCheck/GateCheck.csproj --configuration Release --no-restore

# Install Playwright browsers
RUN pwsh ShopWeb.E2E.Tests/bin/Release/net8.0/playwright.ps1 install --with-deps

# Create artifacts directory
RUN mkdir -p artifacts allure-results

# Set environment variables
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=
ENV DOTNET_EnableDiagnostics=0

# Default command - can be overridden
CMD ["dotnet", "test", "ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj", "--configuration", "Release", "--no-build", "-m:4"]