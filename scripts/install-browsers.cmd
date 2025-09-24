@echo off
REM Install Playwright browsers for ShopWebTestAutomated (Windows)
REM This script ensures consistent browser installation across environments

echo 🎭 Installing Playwright browsers for ShopWebTestAutomated...
echo 📍 Working directory: %CD%

REM Ensure we're in the project root
if not exist "ShopWeb.E2E.Tests\ShopWeb.E2E.Tests.csproj" (
    echo ❌ Error: Please run this script from the project root directory
    echo    Expected to find: ShopWeb.E2E.Tests\ShopWeb.E2E.Tests.csproj
    exit /b 1
)

REM Build the project first to ensure Playwright package is available
echo 🔨 Building project...
dotnet build ShopWeb.E2E.Tests\ShopWeb.E2E.Tests.csproj
if errorlevel 1 (
    echo ❌ Build failed
    exit /b 1
)

REM Navigate to test project directory
cd ShopWeb.E2E.Tests

REM Install Playwright CLI globally if not already installed
echo ⚙️  Ensuring Playwright CLI is available...
dotnet tool install --global Microsoft.Playwright.CLI 2>nul || echo ✅ Playwright CLI already installed

REM Install browsers
echo 📥 Installing browsers...
playwright install
if errorlevel 1 (
    echo ❌ Browser installation failed
    exit /b 1
)

echo ✅ Browser installation completed!
echo.
echo 🧪 You can now run tests with:
echo    dotnet test
echo.
echo 🌐 Available browsers: chromium, firefox, webkit
echo    dotnet test -- NUnit.Browser=firefox