#!/bin/bash
# test-local.sh - Local testing script (GitHub Actions simulation)
# Usage: ./scripts/test-local.sh [smoke|full]

set -e  # Exit on error

# Configuration
TEST_MODE="${1:-smoke}"
BROWSER="${BROWSER:-chromium}"
SITE_ID="${SITE_ID:-A}"
TRACE_MODE="${TRACE_MODE:-OnFailure}"

echo "🚀 Starting local tests (GitHub Actions simulation)"
echo "📋 Mode: $TEST_MODE | Browser: $BROWSER | Site: $SITE_ID"

# Cleanup previous run
echo "🧹 Cleaning up previous run..."
rm -rf artifacts/ TestResults/ bin/ obj/

# Build solution
echo "📦 Restoring and building solution..."
dotnet restore
dotnet build -c Release --no-restore

# Install Playwright using the same method as GitHub Actions
echo "🎭 Installing Playwright (GitHub Actions compatible method)..."
dotnet tool install --global Microsoft.Playwright.CLI || echo "Playwright CLI already installed"

# Install browser using playwright CLI (same as GitHub Actions)
echo "🔽 Installing Playwright browser: $BROWSER"
if command -v playwright &> /dev/null; then
    playwright install $BROWSER
    playwright install-deps $BROWSER
else
    echo "⚠️  Playwright CLI not found, trying pwsh method..."
    if command -v pwsh &> /dev/null; then
        pwsh bin/Release/net8.0/playwright.ps1 install $BROWSER
        pwsh bin/Release/net8.0/playwright.ps1 install-deps $BROWSER
    else
        echo "❌ Neither playwright CLI nor pwsh available"
        echo "💡 Install PowerShell Core or use: dotnet tool install --global Microsoft.Playwright.CLI"
        exit 1
    fi
fi

# Verify installation
echo "✅ Checking Playwright installation..."
if [ -d "$HOME/.cache/ms-playwright" ]; then
    echo "📁 Playwright cache found at: $HOME/.cache/ms-playwright"
    ls -la "$HOME/.cache/ms-playwright" | head -5
else
    echo "⚠️  Playwright cache not found, but continuing..."
fi

# Run unit tests first (fast feedback)
echo "🧪 Running unit tests..."
dotnet test tests/ShopWeb.UnitTests/ \
    -c Release \
    --no-build \
    --logger "console;verbosity=minimal"

if [ $? -ne 0 ]; then
    echo "❌ Unit tests failed! Stopping execution."
    exit 1
fi

echo "✅ Unit tests passed!"

# Run E2E tests based on mode
if [ "$TEST_MODE" = "smoke" ]; then
    echo "🌐 Running E2E smoke test..."
    TEST_FILTER="Category=Smoke"
elif [ "$TEST_MODE" = "full" ]; then
    echo "🌐 Running full E2E test suite..."
    TEST_FILTER=""
else
    echo "❌ Invalid test mode: $TEST_MODE (use 'smoke' or 'full')"
    exit 1
fi

# Execute E2E tests
dotnet test ShopWeb.E2E.Tests/ \
    -c Release \
    --no-build \
    ${TEST_FILTER:+--filter "$TEST_FILTER"} \
    --logger "console;verbosity=detailed" \
    --logger "trx;LogFileName=local-test-results.trx" \
    -- TestRunParameters.Parameter\(name=Browser,value=$BROWSER\) \
       TestRunParameters.Parameter\(name=SiteId,value=$SITE_ID\)

E2E_EXIT_CODE=$?

# Check results
if [ $E2E_EXIT_CODE -eq 0 ]; then
    echo "✅ E2E tests passed!"
else
    echo "❌ E2E tests failed (exit code: $E2E_EXIT_CODE)"
    
    # Show available artifacts for debugging
    if [ -d "artifacts" ]; then
        echo "🔍 Available debugging artifacts:"
        find artifacts -type f -name "*.zip" -o -name "*.png" -o -name "*.json" | head -10
    fi
    
    # Show test results location
    if [ -d "TestResults" ]; then
        echo "📊 Test results available in:"
        find TestResults -name "*.trx" | head -5
    fi
fi

echo "🎯 Local testing complete!"
echo "📈 To view detailed results:"
echo "   - TRX files: TestResults/"
echo "   - Artifacts: artifacts/"
echo "   - Traces: artifacts/**/*.zip"

exit $E2E_EXIT_CODE