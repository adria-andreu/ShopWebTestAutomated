#!/bin/bash

# Install Playwright browsers for ShopWebTestAutomated
# This script ensures consistent browser installation across environments

set -e

echo "🎭 Installing Playwright browsers for ShopWebTestAutomated..."
echo "📍 Working directory: $(pwd)"

# Ensure we're in the project root
if [ ! -f "ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj" ]; then
    echo "❌ Error: Please run this script from the project root directory"
    echo "   Expected to find: ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj"
    exit 1
fi

# Build the project first to ensure Playwright package is available
echo "🔨 Building project..."
dotnet build ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj

# Navigate to test project directory
cd ShopWeb.E2E.Tests

# Install Playwright CLI globally if not already installed
echo "⚙️  Ensuring Playwright CLI is available..."
dotnet tool install --global Microsoft.Playwright.CLI 2>/dev/null || echo "✅ Playwright CLI already installed"

# Install browsers
echo "📥 Installing browsers..."
playwright install

# Install system dependencies for browsers (Linux only)
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "🐧 Installing system dependencies for browsers..."
    playwright install-deps
fi

echo "✅ Browser installation completed!"
echo ""
echo "🧪 You can now run tests with:"
echo "   dotnet test"
echo ""
echo "🌐 Available browsers: chromium, firefox, webkit"
echo "   dotnet test -- NUnit.Browser=firefox"