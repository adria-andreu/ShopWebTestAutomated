```markdown
# UI Test Framework — .NET 8 + Playwright + NUnit (POM)

[![CI](https://img.shields.io/badge/CI-GitHub%20Actions-blue)](./.github/workflows/tests.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Playwright](https://img.shields.io/badge/Playwright-UI%20Testing-2ea44f)](https://playwright.dev/)

> **ShopWebTestAutomated**:  End-to-end UI automation test project with with **C#/.NET 8**, **Playwright**, **NUnit**, and **POM**.  
> Portable across sites via `SiteId` (A/B), **parallel**, **observable** (Allure + JSON metrics), **gated** by quality checks, and **CI-ready** (GitHub Actions, Docker).
> Designed for **scalability, verificable, and CI/CD integration**.
---

## ✨ Key Features
- **POM done right**: thin pages, reusable components, no assertions in POM, no selectors in tests.
- **Portability by config**: switch sites with `SiteId` (A/B) without changing tests.
- **Fast feedback**: parallel workers, browser matrix (Chromium/Firefox/WebKit).
- **Observability**: Allure reports, traces/screenshots on failure, run metrics as JSON.
- **Quality Gates**: `GateCheck` enforces pass-rate and p95 runtime thresholds.
- **Reproducible**: Dockerfile + docker-compose; GitHub Actions workflow with caches & nightly.

> Full policy/canvas: see **`PROJECT.md`** (root).  
> Operational guide: **`docs/runbook.md`**.

---

## 🧱 Repository Layout
```

ShopWebTestAutomated/
Browsers/            # BrowserFactory, context options
Pages/               # POMs (interfaces + per-site implementations)
Flows/               # Business flows (domain-level orchestration)
Tests/               # NUnit test fixtures (call Flows only)
Utilities/           # Helpers, builders, waits, assertions
Config/              # appsettings.tests.json, profiles, TestData
artifacts/           # run-metrics.json, test-metrics.jsonl, traces
tools/GateCheck/     # Quality gate CLI (reads artifacts/run-metrics.json)
.github/workflows/   # CI workflows (tests.yml)
docs/                # Runbook + examples and guidelines


## 🚀 Quick Start (Local)

### Prereqs
- .NET SDK 8.x
- Playwright CLI
  bash
  dotnet tool install --global Microsoft.Playwright.CLI
  playwright install --with-deps


### Run tests
```bash
# Default: chromium, 4 workers
dotnet test -m:4

# Specify browser & site
dotnet test -m:4 -- \
  TestRunParameters.Parameter(name=Browser,value=firefox) \
  TestRunParameters.Parameter(name=SiteId,value=B)

# Filter by category (e.g., Smoke)
dotnet test -m:4 -- --where "cat == Smoke"

### Configuration

Config comes from `Config/appsettings.tests.json` or env vars:

```json
{
  "TestSettings": {
    "BaseUrl": "https://shopwebtest.local",
    "Browser": "chromium",
    "Headed": false,
    "Timeouts": { "Default": 8000, "Navigation": 15000 },
    "Artifacts": {
      "DumpHtmlOnFail": true,
      "LogConsoleOnFail": true,
      "TraceMode": "OnFailure"
    }
  }
}
```

Environment variables override config:

* `BROWSER` → chromium | firefox | webkit
* `HEADED` → 1 for headed mode
* `TRACE_MODE` → On | OnFailure | Off
* `SITE_ID` → site label (A/B/…)
* `GIT_SHA`, `PIPELINE_ID` → injected by CI

---

### Metrics & Reporting

* **Per-test** → `artifacts/test-metrics.jsonl`
* **Per-suite** → `artifacts/run-metrics.json`
* **Allure** → `allure-results/` with attachments (`trace.zip`, `screenshot.png`)
* **History** → `artifacts/history/run-*.json` (≥30 days retention in CI)

> How to inspect traces: `npx playwright show-trace artifacts/<trace>.zip`

---

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

### Compose (aggregate metrics across browsers)

```bash
# Uses docker-compose.yml; SITEID and MAX_WORKERS can be set in .env
docker compose build
docker compose run --rm tests-chromium
docker compose run --rm tests-firefox
docker compose run --rm tests-webkit
docker compose run --rm gatecheck   # Enforce quality gates on aggregated metrics
```

---

## 🤖 CI (GitHub Actions)

* See [`docs/examples/CIDockerWorkflowExample.yml`](docs/examples/CIDockerWorkflowExample.yml).
* Runs in Docker (`mcr.microsoft.com/dotnet/sdk:8.0`).
* Matrix across browsers (chromium, firefox, webkit) and site IDs.
* Publishes artifacts and Allure results.
* Enforces Quality Gates with `jq` + optional `GateCheck` tool.

* Workflow: `.github/workflows/tests.yml`
* **Matrix**: `browser ∈ {chromium, firefox, webkit}` × `siteId ∈ {A, B}`
* **Caches**: NuGet (and optional Playwright browsers)
* **Nightly** schedule included
* **Quality Gates**: `tools/GateCheck` fails the job when thresholds are not met

**Default thresholds**

* Pass-rate ≥ **90%** (PR) / ≥ **95%** (main/nightly)
* p95 runtime ≤ **12 min** (PR) / ≤ **10 min** (main/nightly)
* Flaky ratio ≤ **5%** (optional check)

> Configure via env vars (e.g., `MIN_PASS_RATE`, `MAX_P95_MS`) if you extend GateCheck.

---

## 🧭 Portability (Multi-site)

* Select target site via `SiteId` test parameter (`A` or `B`).
* Site profiles & per-site POMs live under `Profiles/` and `Pages/SiteA_*/SiteB_*`.
* **Tests never change** when switching sites; only config/adapters do.

---

## 🧪 Test Conventions

* Code & comments in **English**
* Naming: `PascalCase` for public APIs, `_camelCase` for private fields, `Async` suffix
* **No** `Thread.Sleep`
* **No** selectors in tests (tests call **Flows**, which call **POMs**)
* POMs **without assertions**; centralize waits in Utilities/`Locator.WaitForAsync`

Categories (examples): `Smoke`, `Regression`, `Negative`, `Edge`, `A11y`, `Visual`, `Flaky`

---

## ✅ Quality & Governance

* **Allure** + JSON metrics per run
* **GateCheck** enforces thresholds in CI
* **Flaky policy**: tag `[Category("Flaky")]`, issue with owner & 7-day TTL, re-enable after 10 green runs
* **Security**: no secrets in repo/logs; use env/GA secrets; `dotnet list package --vulnerable` in CI
* **PRs**: must acknowledge compliance with `CLAUDE.md` & `PROJECT.md` (template enforced); **CODEOWNERS** review required

---

## 🛠️ Developer Commands (cheat-sheet)

* Tests must inherit from `BaseTest`.
* No `Thread.Sleep` (use explicit waits).
* No selectors in test bodies (use POM).
* No assertions inside POM.
* All artifacts must be attached to NUnit/Allure automatically.

```bash
# Format & analyzers
dotnet format
dotnet build -warnaserror

# Single test reproduction (headless)
dotnet test -m:1 -- --where "test == FullyQualifiedName"

# With browser param and visible UI (if supported by config)
HEADLESS=false dotnet test -m:1 -- \
  TestRunParameters.Parameter(name=Browser,value=chromium) \
  --where "cat == Smoke"
```

---

## 📚 Documentation & Examples

* **Canvas & policies**: `PROJECT.md`
* **Runbook**: `docs/runbook.md`
* **Guides & examples**:

  * Metrics & Allure: `docs/examples/RunMetricsCollectorExample.cs`, `docs/examples/AllureIntegrationExample.cs`
  * Portability & POM: `docs/examples/PortabilityPatternExample.cs`, `docs/examples/pom-guidelines.md`
  * Infra & CI: `docs/examples/BrowserFactoryExample.cs`, `docs/examples/CIDockerWorkflowExample.yml`
  * Code style: `docs/examples/editorconfig-dotnet8.txt`
  * Flaky issue template: `docs/examples/flaky-issue-template.md`

---

## 🔒 License

TBD (e.g., MIT). Add `LICENSE` file at repo root.

---