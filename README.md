# ShopWebTestAutomated 🛒

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Playwright](https://img.shields.io/badge/Playwright-1.40.0-green.svg)](https://playwright.dev/)
[![NUnit 4](https://img.shields.io/badge/NUnit-4.0+-brightgreen.svg)](https://nunit.org/)
[![C#](https://img.shields.io/badge/C%23-Latest-239120.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![CI/CD](https://github.com/adria-andreu/ShopWebTestAutomated/workflows/E2E%20Tests/badge.svg)](https://github.com/adria-andreu/ShopWebTestAutomated/actions)

**End-to-end UI automation test project** built with **C#/.NET 8**, **Playwright**, **NUnit**, and **Page Object Model (POM)**. 

> 🚀 **Portable**, **parallel**, **observable**, **quality-gated**, and **CI-ready**  
> 📈 Designed for **scalability**, **verifiability**, and seamless **CI/CD integration**

---

## 📋 Table of Contents

- [✨ Features](#-features)
- [🛠 Tech Stack](#-tech-stack)
- [🚀 Quick Start](#-quick-start)
- [📁 Project Structure](#-project-structure)
- [⚙️ Configuration](#️-configuration)
- [🧪 Running Tests](#-running-tests)
- [🔄 CI/CD Integration](#-cicd-integration)
- [📊 Test Reporting](#-test-reporting)
- [🏗 Architecture](#-architecture)
- [🤝 Contributing](#-contributing)
- [🗺 Roadmap](#-roadmap)
- [📄 License](#-license)

---

## ✨ Features

### 🎯 **Core Capabilities**
- **🌐 Multi-browser support**: Chromium, Firefox, WebKit
- **⚡ Parallel test execution**: Configurable concurrency for faster feedback
- **💻 Cross-platform**: Windows, macOS, Linux support
- **📄 Page Object Model**: Maintainable and scalable test architecture
- **⚙️ Configuration-driven**: Environment-specific test settings
- **🔄 Multi-site validation**: Site switching and compatibility testing

### 🔧 **Quality & Observability**
- **📊 Advanced reporting**: Allure integration for rich test reports
- **🔁 Intelligent retry policies**: Smart retry mechanisms for flaky tests
- **📈 Performance monitoring**: Built-in performance metrics collection
- **🚨 Quarantine system**: Automatic isolation of unstable tests
- **🔍 Flaky test detection**: Advanced analytics for test stability
- **📋 Quality gates**: Automated quality checks and thresholds

### 🚀 **DevOps Integration**
- **🔧 CI/CD ready**: GitHub Actions workflows included
- **🐳 Docker support**: Containerized test execution
- **⏰ Scheduled runs**: Nightly test execution
- **🎛 Manual triggers**: On-demand test runs with parameters
- **📦 Artifact management**: Test results, screenshots, videos

---

## 🛠 Tech Stack

### **Core Framework**
- **[.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)**: Latest LTS version with enhanced performance
- **[C#](https://docs.microsoft.com/en-us/dotnet/csharp/)**: Modern language features with nullable reference types
- **[NUnit 4](https://nunit.org/)**: Advanced testing framework with parallel execution
- **[Playwright](https://playwright.dev/)**: Fast, reliable browser automation

### **Testing & Quality**
- **[Allure](https://allurereport.org/)**: Rich test reporting and analytics
- **[Microsoft.Extensions.*](https://docs.microsoft.com/en-us/dotnet/core/extensions/)**: Dependency injection, configuration, logging
- **[Newtonsoft.Json](https://www.newtonsoft.com/json)**: JSON serialization and data handling
- **[Coverlet](https://github.com/coverlet-coverage/coverlet)**: Code coverage collection

### **DevOps & CI/CD**
- **[GitHub Actions](https://github.com/features/actions)**: Automated workflows
- **[Docker](https://www.docker.com/)**: Containerization support
- **Quality Gates**: Custom tooling for test quality assurance

---

## 🚀 Quick Start

### Prerequisites
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (8.0.0 or later)
- **[Git](https://git-scm.com/downloads)**
- **[Docker](https://www.docker.com/get-started)** (optional, for containerized execution)

### 1. Clone the Repository
```bash
git clone https://github.com/adria-andreu/ShopWebTestAutomated.git
cd ShopWebTestAutomated
```

### 2. Install Dependencies
```bash
# Install .NET dependencies
dotnet restore

# Install Playwright browsers
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

### 3. Configure Environment
```bash
# Copy example configuration
cp .env.example .env

# Edit configuration as needed
# Configure test URLs, credentials, and settings in:
# - ShopWeb.E2E.Tests/Config/appsettings.tests.json
# - .env (environment variables)
```

### 4. Run Your First Test
```bash
# Run all E2E tests
dotnet test ShopWeb.E2E.Tests/

# Run unit tests
dotnet test tests/UnitTests/

# Run with specific browser
dotnet test ShopWeb.E2E.Tests/ -- NUnit.Browser=firefox

# Run tests in parallel
dotnet test --parallel
```

---

## 📁 Project Structure

```
ShopWebTestAutomated/
├── 📂 .github/workflows/          # CI/CD pipelines
│   ├── tests.yml                  # Main E2E test workflow
│   ├── tests-simplified.yml       # Lightweight testing
│   └── unit-tests.yml             # Unit test pipeline
├── 📂 ShopWeb.E2E.Tests/         # Main E2E test suite
│   ├── 📂 Browsers/               # Browser factory and management
│   │   ├── BrowserFactory.cs     # Multi-browser support
│   │   └── IBrowserFactory.cs    # Browser abstraction
│   ├── 📂 Config/                 # Test configuration and settings
│   │   ├── appsettings.tests.json # Main configuration
│   │   ├── allureConfig.json      # Reporting settings
│   │   ├── flakyDetection.json    # Flaky test rules
│   │   └── performanceTrending.json # Performance monitoring
│   ├── 📂 Flows/                  # Business flow implementations
│   │   ├── AuthenticationFlow.cs  # Login/authentication flows
│   │   └── ShoppingFlow.cs        # E-commerce workflows
│   ├── 📂 Pages/                  # Page Object Model classes
│   │   ├── PageFactory.cs         # Page object factory
│   │   └── SiteA/                 # Site-specific page objects
│   ├── 📂 Tests/                  # Test implementations
│   │   ├── BaseTest.cs            # Base test class
│   │   ├── AuthenticationTests.cs # Authentication scenarios
│   │   ├── ShoppingTests.cs       # Shopping workflows
│   │   └── SiteSwitchingValidationTest.cs # Multi-site tests
│   └── 📂 Utilities/              # Test utilities and helpers
│       ├── AllureContextManager.cs    # Allure integration
│       ├── FlakyDetectionEngine.cs    # Flaky test detection
│       ├── MetricsCollector.cs        # Performance metrics
│       ├── QuarantineWorkflowEngine.cs # Test quarantine
│       └── RetryPolicy.cs             # Retry mechanisms
├── 📂 tests/                      # Unit tests
│   └── 📂 UnitTests/              # NUnit 4 unit tests with coverage
│       ├── ShopWeb.UnitTests.csproj # Unit test project
│       └── Utilities/             # Unit test implementations
├── 📂 tools/                      # Quality gate tools
│   └── 📂 GateCheck/              # Custom quality gate implementation
├── 📂 project_memory/             # Project documentation and tracking
├── 📂 roadmap/                    # Project roadmap and planning
├── 📂 technical_debt/             # Technical debt tracking
├── 📂 historical_project_memory/  # Project history
├── 📄 docker-compose.yml          # Multi-service orchestration
├── 📄 Dockerfile                  # Container definition
└── 📄 README.md                   # This file
```

### 🏗 Architecture Highlights

- **📄 Page Objects**: Centralized UI element definitions with factory pattern
- **🔄 Business Flows**: Reusable test workflows for complex scenarios
- **⚙️ Configuration**: Layered configuration system (JSON + Environment variables)
- **🔧 Utilities**: Comprehensive test utilities (metrics, retry, quarantine)
- **🏭 Browser Factory**: Multi-browser support with abstraction layer

---

## ⚙️ Configuration

### Environment Configuration
Configuration is managed through multiple layers:

1. **appsettings.tests.json**: Base test configuration
2. **Environment Variables**: Runtime overrides via `.env`
3. **Command Line**: Test-specific parameters

### Key Configuration Files
```bash
ShopWeb.E2E.Tests/Config/
├── appsettings.tests.json         # Main test settings
├── allureConfig.json              # Reporting configuration  
├── flakyDetection.json            # Flaky test detection rules
├── performanceTrending.json       # Performance monitoring
└── quarantineWorkflow.json        # Test quarantine settings
```

### Sample Configuration
```json
{
  "TestSettings": {
    "BaseUrl": "https://your-shop-url.com",
    "Browser": "chromium",
    "Headless": true,
    "Timeout": 30000,
    "Parallel": true,
    "MaxDegreeOfParallelism": 4
  },
  "Reporting": {
    "AllureEnabled": true,
    "ScreenshotsOnFailure": true,
    "VideoRecording": "retain-on-failure"
  }
}
```

---

## 🧪 Running Tests

### Local Execution
```bash
# Run all tests
dotnet test

# Run E2E tests only
dotnet test ShopWeb.E2E.Tests/

# Run unit tests only
dotnet test tests/UnitTests/

# Run specific test class
dotnet test --filter "ClassName~ShoppingTests"

# Run with custom browser
dotnet test -- NUnit.Browser=firefox

# Run in headed mode (visible browser)
dotnet test -- NUnit.Headless=false

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Docker Execution
```bash
# Build and run tests in container
docker-compose up --build

# Run specific browser in container
docker-compose run --rm tests-firefox

# Run with custom environment
docker-compose -f docker-compose.yml -f docker-compose.staging.yml up
```

### Test Categories
- **🛒 Shopping Tests**: End-to-end purchase workflows
- **🔐 Authentication Tests**: Login, registration, session management
- **🔄 Site Switching**: Multi-site validation and compatibility
- **📊 Performance Tests**: Load time and interaction metrics
- **🧪 Unit Tests**: Component and utility testing

---

## 🔄 CI/CD Integration

### GitHub Actions Workflows

#### **Main E2E Pipeline** (`tests.yml`)
- **Triggers**: Push to main/develop, PRs, nightly schedule (2 AM UTC)
- **Matrix Strategy**: Multiple browsers (Chromium, Firefox, WebKit)
- **Artifact Collection**: Test results, screenshots, videos
- **Quality Gates**: Automated pass/fail thresholds

#### **Unit Test Pipeline** (`unit-tests.yml`)
- **Fast Feedback**: Runs on every commit
- **Code Coverage**: Integrated coverage reporting with Coverlet
- **Quality Enforcement**: Blocks PRs on test failures

#### **Simplified Pipeline** (`tests-simplified.yml`)
- **Lightweight**: Quick validation for draft PRs
- **Essential Tests**: Core functionality validation

### Manual Workflow Triggers
```yaml
workflow_dispatch:
  inputs:
    browser:
      description: 'Browser to test with'
      required: false
      default: 'chromium'
      type: choice
      options: [chromium, firefox, webkit]
    environment:
      description: 'Environment to test'
      required: false
      default: 'staging'
```

---

## 📊 Test Reporting

### Allure Integration
- **📈 Rich Reports**: Detailed test execution results with trends
- **📸 Screenshots**: Automatic capture on failures
- **⏱ Performance Metrics**: Response times and page load data
- **📊 Historical Trends**: Test stability over time
- **🏷 Test Categorization**: Organized by feature, severity, and epic

### Quality Metrics & Analytics
- **📋 Test Coverage**: Unit and integration coverage tracking
- **🔍 Flaky Test Detection**: Automated identification of unstable tests
- **📈 Performance Trending**: Page load and interaction time analysis
- **🚨 Quarantine Management**: Automatic isolation of problematic tests
- **⚡ Performance Benchmarking**: Response time baselines and alerts

### Generating Reports
```bash
# Run tests with Allure
dotnet test -- NUnit.Allure=true

# Generate Allure report
allure generate allure-results --clean -o allure-report

# View report
allure open allure-report
```

---

## 🏗 Architecture

### Design Principles
- **🏛 Page Object Model**: Clean separation between test logic and UI elements
- **🔧 Factory Pattern**: Centralized object creation for browsers and pages
- **⚙️ Dependency Injection**: Loosely coupled components with Microsoft.Extensions.DI
- **📊 Observer Pattern**: Event-driven metrics collection and reporting
- **🔄 Strategy Pattern**: Configurable retry and quarantine strategies

### Key Components

#### **Browser Management**
```csharp
public interface IBrowserFactory
{
    Task<IBrowser> CreateBrowserAsync(string browserType);
    Task<IPage> CreatePageAsync(IBrowser browser);
}
```

#### **Page Object Factory**
```csharp
public class PageFactory
{
    public T CreatePage<T>(IPage page) where T : class;
    public Task<T> NavigateToPage<T>(string url) where T : class;
}
```

#### **Flow-Based Testing**
```csharp
public class ShoppingFlow
{
    public async Task<OrderResult> CompleteCheckoutAsync(CheckoutData data);
    public async Task<CartState> AddProductToCartAsync(Product product);
}
```

---

## 🤝 Contributing

### Development Workflow
1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/amazing-feature`
3. **Implement** your changes with tests
4. **Run** the full test suite: `dotnet test`
5. **Commit** with conventional commits: `feat: add amazing feature`
6. **Submit** a Pull Request

### Code Standards
- **C# Coding Conventions**: Follow Microsoft standards
- **Test Naming**: Use descriptive, behavior-focused names (`Method_Scenario_ExpectedResult`)
- **Page Objects**: Maintain clear separation of concerns
- **Configuration**: Use strongly-typed configuration classes
- **Documentation**: Include XML documentation for public APIs

### Testing Requirements
- ✅ **Unit Tests**: All new code requires unit test coverage (80%+ minimum)
- ✅ **E2E Tests**: Critical user paths must have E2E coverage
- ✅ **Code Quality**: All PRs must pass linting and quality gates
- ✅ **Documentation**: Update README and inline docs for changes

---

## 🗺 Roadmap

### Current Iteration (IT04) ✅
- ✅ **Infrastructure**: CI/CD architecture and quality gates
- ✅ **Unit Testing**: NUnit 4 integration with coverage reporting
- ✅ **Housekeeping**: Repository cleanup and standardization
- ✅ **Documentation**: Comprehensive project documentation

### Next Iteration (IT05) 🔄
- 🔄 **Advanced Observability**: Enhanced metrics and monitoring
- 🔄 **Performance Testing**: Load testing integration
- 🔄 **Visual Regression**: UI change detection capabilities
- 🔄 **API Testing**: REST API test integration

### Upcoming Features 📋
- 📱 **Mobile Testing**: iOS and Android test support with Appium
- ☁️ **Cloud Execution**: Integration with BrowserStack/Sauce Labs
- 🤖 **AI-Powered Testing**: Intelligent test generation and maintenance
- 📈 **Advanced Analytics**: ML-powered test optimization

### Long-term Vision 🚀
- **Self-Healing Tests**: Automatic test maintenance and updates
- **Accessibility Testing**: WCAG compliance validation
- **Multi-Language Support**: Test localization capabilities
- **Real User Monitoring**: RUM integration for production insights

---

## 📚 Documentation

### Additional Resources
- **[Contributing Guide](CONTRIBUTING.md)**: Detailed contribution guidelines
- **[API Documentation](docs/api/)**: Generated API documentation
- **[Architecture Decision Records](docs/adr/)**: Design decisions and rationale
- **[Test Strategy](docs/testing-strategy.md)**: Testing approach and methodologies

### Project Memory
- **[Current Memory](project_memory/)**: Active iteration tracking
- **[Historical Memory](historical_project_memory/)**: Past iteration summaries  
- **[Technical Debt](technical_debt/)**: Known issues and improvements
- **[Roadmap](roadmap/)**: Detailed planning and prioritization

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **[Playwright Team](https://playwright.dev/)** - For the excellent browser automation framework
- **[NUnit Community](https://nunit.org/)** - For the robust testing framework  
- **[Allure Framework](https://allurereport.org/)** - For comprehensive test reporting
- **[.NET Team](https://dotnet.microsoft.com/)** - For the modern development platform
- **[Microsoft Extensions](https://docs.microsoft.com/en-us/dotnet/core/extensions/)** - For dependency injection and configuration

---

## 📞 Support & Community

- **🐛 Issues**: [GitHub Issues](https://github.com/adria-andreu/ShopWebTestAutomated/issues)
- **💬 Discussions**: [GitHub Discussions](https://github.com/adria-andreu/ShopWebTestAutomated/discussions)
- **📖 Wiki**: [Project Wiki](https://github.com/adria-andreu/ShopWebTestAutomated/wiki)
- **📧 Contact**: [Open an Issue](https://github.com/adria-andreu/ShopWebTestAutomated/issues/new)

---

<div align="center">

**⭐ Star this repository if you find it useful! ⭐**

[![Stars](https://img.shields.io/github/stars/adria-andreu/ShopWebTestAutomated.svg?style=social&label=Star)](https://github.com/adria-andreu/ShopWebTestAutomated/stargazers)
[![Forks](https://img.shields.io/github/forks/adria-andreu/ShopWebTestAutomated.svg?style=social&label=Fork)](https://github.com/adria-andreu/ShopWebTestAutomated/network/members)
[![Watchers](https://img.shields.io/github/watchers/adria-andreu/ShopWebTestAutomated.svg?style=social&label=Watch)](https://github.com/adria-andreu/ShopWebTestAutomated/watchers)

Made with ❤️ by [adria-andreu](https://github.com/adria-andreu)

</div>