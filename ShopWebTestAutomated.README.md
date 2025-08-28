# ShopWeb E2E Test Automation Framework

[![CI](https://github.com/adria-andreu/ShopWebTestAutomated/actions/workflows/tests.yml/badge.svg)](https://github.com/adria-andreu/ShopWebTestAutomated/actions/workflows/tests.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Playwright](https://img.shields.io/badge/Playwright-UI%20Testing-2ea44f)](https://playwright.dev/)

A comprehensive .NET 8 + Playwright test automation framework for e-commerce web applications with quality gates, CI/CD integration, and multi-site portability patterns.

## ✨ Key Features

- **POM done right**: Thin pages, reusable components, no assertions in POM, no selectors in tests
- **Portability by config**: Switch sites with `SiteId` (A/B) without changing tests
- **Fast feedback**: Parallel workers, browser matrix (Chromium/Firefox/WebKit)
- **Observability**: Allure reports, traces/screenshots on failure, run metrics as JSON
- **Quality Gates**: `GateCheck` enforces pass-rate and p95 runtime thresholds
- **Reproducible**: Dockerfile + docker-compose; GitHub Actions workflow with caches & nightly

## 🚀 Quick Start

### Prerequisites

- .NET SDK 8.x
- Playwright CLI

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install --with-deps
```

### Run Tests Locally

```bash
# Default: chromium, 4 workers
dotnet test ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj -m:4

# Specify browser & site
dotnet test ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj -m:4 -- \
  TestRunParameters.Parameter(name=Browser,value=firefox) \
  TestRunParameters.Parameter(name=SiteId,value=B)

# Filter by category (e.g., Smoke)
dotnet test ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj -m:4 -- --where "cat == Smoke"
```

### Configuration

Config comes from `ShopWeb.E2E.Tests/Config/appsettings.tests.json` or env vars:

Environment variables override config:
- `BROWSER` → chromium | firefox | webkit
- `HEADED` → 1 for headed mode
- `TRACE_MODE` → On | OnFailure | Off
- `SITE_ID` → site label (A/B/…)
- `GIT_SHA`, `PIPELINE_ID` → injected by CI

## 🐳 Docker & Compose

### Docker

```bash
# Build
docker build -t shop-tests .

# Run (choose browser)
docker run --rm -e BROWSER=chromium \
  -v "$PWD/artifacts:/app/artifacts" \
  -v "$PWD/allure-results:/app/allure-results" \
  shop-tests
```

### Compose

```bash
# Copy environment file
cp .env.example .env

# Run specific browser tests
docker compose run --rm tests-chromium
docker compose run --rm tests-firefox
docker compose run --rm tests-webkit

# Run quality gates check
docker compose run --rm gatecheck

# Start Allure reporting server
docker compose up -d allure-report
# Then visit http://localhost:5050
```

## 🧭 Project Structure

```
ShopWeb.E2E.Tests/
├── Browsers/              # BrowserFactory, context options
├── Pages/                 # POMs (interfaces + per-site implementations)
│   ├── SiteA/            # Site A implementations (demoblaze.com)
│   └── SiteB/            # Site B implementations (future)
├── Flows/                # Business flows (domain-level orchestration)
├── Tests/                # NUnit test fixtures (call Flows only)
├── Utilities/            # Helpers, metrics, waits, assertions
├── Config/               # appsettings.tests.json, configuration classes
└── artifacts/            # run-metrics.json, test-metrics.jsonl, traces

tools/GateCheck/          # Quality gate CLI tool
.github/workflows/        # CI workflows (tests.yml)
```

## 🧪 Test Architecture

### Layer Dependencies
- **Tests** → **Flows** → **Pages (POM)** → **Playwright**
- Tests contain **no selectors or waits** - only call Flows
- POMs contain **no assertions** - only page interactions
- Flows handle **business logic** and orchestration

### Test Categories
- `Smoke`: Critical functionality
- `Regression`: Full feature coverage
- `Negative`: Error scenarios
- `Edge`: Boundary conditions
- `Flaky`: Tests with known instability

## 📊 Metrics & Reporting

- **Per-test**: `artifacts/test-metrics.jsonl`
- **Per-suite**: `artifacts/run-metrics.json`
- **Allure**: `allure-results/` with attachments (traces, screenshots)
- **History**: `artifacts/history/run-*.json` (≥30 days retention in CI)

### Quality Gates (Default Thresholds)

| Environment | Pass Rate | P95 Duration | Flaky Ratio |
|-------------|-----------|--------------|-------------|
| PR          | ≥ 90%     | ≤ 12 min     | ≤ 5%        |
| Main        | ≥ 95%     | ≤ 10 min     | ≤ 5%        |
| Nightly     | ≥ 95%     | ≤ 10 min     | ≤ 5%        |

## 🔧 Development Commands

```bash
# Build projects
dotnet build

# Run specific test
dotnet test --filter "FullyQualifiedName~AddToCart_WhenSelectingProduct"

# Run with headed browser (for debugging)
HEADED=1 dotnet test ShopWeb.E2E.Tests/ShopWeb.E2E.Tests.csproj

# Check quality gates locally
dotnet run --project tools/GateCheck/GateCheck.csproj -- --verbose

# View traces
npx playwright show-trace artifacts/TestName_20250828_143022123/trace.zip
```

## 🏗️ CI/CD Pipeline

- **Triggers**: Push to main/develop, PRs, scheduled nightly, manual dispatch
- **Matrix**: 3 browsers × 1 site (expandable to multiple sites)
- **Artifacts**: Test results, traces, Allure reports, metrics
- **Quality Gates**: Automatic pass/fail based on configurable thresholds
- **PR Comments**: Automatic test results summary

## 🎯 Multi-Site Portability

Switch target sites via `SiteId` parameter:
- **Site A**: demoblaze.com (implemented)
- **Site B**: Future implementation

Tests remain unchanged - only configuration and page implementations differ.

## 🛡️ Security & Best Practices

- No hardcoded credentials in tests
- Environment variables for sensitive data
- Secrets managed via CI/CD secrets
- Dependency vulnerability scanning
- Automated code quality checks

## 📚 Documentation

For detailed architecture, policies, and examples, see:
- **Full Documentation**: [`Project Instructions/PROJECT.md`](Project%20Instructions/PROJECT.md)
- **Examples**: [`Project Instructions/docs/examples/`](Project%20Instructions/docs/examples/)
- **Development Guide**: [`Project Instructions/docs/runbook.md`](Project%20Instructions/docs/runbook.md)

## 🧰 Troubleshooting

### Common Issues

1. **Playwright browsers not installed**:
   ```bash
   playwright install --with-deps
   ```

2. **Tests timing out**:
   - Check network connectivity
   - Increase timeouts in `appsettings.tests.json`
   - Use `TRACE_MODE=On` for debugging

3. **Quality gates failing**:
   ```bash
   # Check metrics
   cat artifacts/run-metrics.json
   
   # Run with verbose output
   dotnet run --project tools/GateCheck/GateCheck.csproj -- --verbose
   ```

4. **Docker issues**:
   ```bash
   # Clean build
   docker compose build --no-cache
   
   # Check logs
   docker compose logs tests-chromium
   ```

## 🤝 Contributing

1. Follow the established patterns (Tests → Flows → Pages)
2. Add appropriate test categories
3. Ensure quality gates pass
4. Include Allure annotations for reporting
5. Update documentation for significant changes

## 📄 License

[MIT License](LICENSE) - see file for details.

---

**Built with ❤️ using .NET 8, Playwright, and NUnit**