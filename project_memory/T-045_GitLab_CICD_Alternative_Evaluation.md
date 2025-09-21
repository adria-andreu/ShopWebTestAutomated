# T-045: GitLab CI/CD Alternative Platform Evaluation

## **Objective**
Evaluate GitLab CI/CD as a backup platform strategy to reduce dependency on GitHub Actions and provide redundant CI/CD capability for the project.

## **Background Context**
From TD-99 analysis and enterprise readiness requirements:
- **Primary Strategy**: Azure VM self-hosted runners (T-044)
- **Backup Strategy**: Alternative platform evaluation for redundancy
- **Risk Mitigation**: Platform-independent CI/CD capability

## **GitLab CI/CD Analysis**

### **Platform Advantages**
- ✅ **Docker-first approach**: Native containerization, simpler Playwright setup
- ✅ **Mature .NET support**: Extensive .NET Core/8 templates and examples
- ✅ **Integrated registry**: Built-in container registry for custom images
- ✅ **Pipeline efficiency**: Generally faster execution than GitHub Actions
- ✅ **Cost model**: Generous free tier, competitive pricing

### **Integration Requirements**
- **Repository mirroring**: GitHub → GitLab sync strategy
- **Secret management**: Environment variables and secure files
- **Artifact handling**: Test results, traces, metrics export
- **Quality gates**: Integration with existing GateCheck tool

## **Implementation Strategy**

### **Phase 1: GitLab Setup & Repository Mirror**

#### **Repository Configuration**
```bash
# Add GitLab as secondary remote
git remote add gitlab https://gitlab.com/adria-andreu/ShopWebTestAutomated.git
git push gitlab main

# Set up automated mirroring (GitLab Premium feature alternative)
# Manual sync approach for evaluation phase
```

#### **GitLab CI/CD Pipeline (.gitlab-ci.yml)**
```yaml
stages:
  - build
  - test-unit
  - test-e2e
  - quality-gates

variables:
  DOTNET_VERSION: "8.0"
  PLAYWRIGHT_BROWSERS_PATH: "/opt/playwright"

# Custom Docker image with pre-installed dependencies
.dotnet-playwright-base:
  image: mcr.microsoft.com/dotnet/sdk:8.0
  before_script:
    - apt-get update
    - apt-get install -y wget curl gnupg
    - npx playwright install chromium firefox webkit
    - npx playwright install-deps
  cache:
    key: dotnet-nuget-$CI_COMMIT_REF_SLUG
    paths:
      - ~/.nuget/packages/

build:
  extends: .dotnet-playwright-base
  stage: build
  script:
    - dotnet restore
    - dotnet build -c Release --no-restore
  artifacts:
    paths:
      - "*/bin/Release/"
      - "*/obj/"
    expire_in: 1 hour

unit-tests:
  extends: .dotnet-playwright-base
  stage: test-unit
  script:
    - dotnet test tests/ShopWeb.UnitTests/ -c Release --logger trx --logger "console;verbosity=detailed"
  artifacts:
    when: always
    reports:
      junit: 
        - "**/TestResults/*.trx"
    paths:
      - "**/TestResults/"
    expire_in: 1 week

e2e-tests:
  extends: .dotnet-playwright-base
  stage: test-e2e
  parallel:
    matrix:
      - BROWSER: [chromium, firefox, webkit]
        SITE_ID: [A, B]
  script:
    - |
      dotnet test ShopWeb.E2E.Tests/ \
        -c Release \
        --logger "trx;LogFileName=test-results-${BROWSER}-${SITE_ID}.trx" \
        --logger "console;verbosity=detailed" \
        -- TestRunParameters.Parameter\(name=Browser,value=${BROWSER}\) \
           TestRunParameters.Parameter\(name=SiteId,value=${SITE_ID}\)
  artifacts:
    when: always
    paths:
      - "**/TestResults/"
      - "**/artifacts/"
    expire_in: 1 week
  timeout: 30m

quality-gates:
  extends: .dotnet-playwright-base
  stage: quality-gates
  script:
    - |
      dotnet run --project tools/GateCheck/ -- \
        --input artifacts/run-metrics.json \
        --threshold-pass-rate 0.90 \
        --threshold-p95-duration 900000
  artifacts:
    reports:
      junit: "artifacts/quality-gate-results.xml"
  dependencies:
    - e2e-tests
```

### **Phase 2: Custom Docker Image Optimization**

#### **Dockerfile.gitlab-ci**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install system dependencies
RUN apt-get update && apt-get install -y \
    wget curl gnupg ca-certificates \
    libnss3 libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxrandr2 libgbm1 \
    libxss1 libasound2 && \
    rm -rf /var/lib/apt/lists/*

# Install Node.js for Playwright
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs

# Pre-install Playwright browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/opt/playwright
RUN npx playwright install chromium firefox webkit && \
    npx playwright install-deps

# Create workspace
WORKDIR /workspace

# Pre-warm .NET dependencies
COPY Directory.Build.props ./
COPY */*.csproj ./
RUN find . -name "*.csproj" -exec dirname {} \; | sort -u | \
    xargs -I {} mkdir -p {} && \
    find . -name "*.csproj" -exec cp {} {}/{} \; && \
    dotnet restore && \
    rm -rf */

COPY . .
```

### **Phase 3: Performance Comparison**

#### **Benchmark Metrics**
```yaml
# benchmark-comparison.yml
performance_comparison:
  platforms:
    github_actions:
      - setup_time: "~3min"
      - test_execution: "~12min"
      - total_pipeline: "~15min"
      - success_rate: "65%" # Current TD-99 issue
    
    gitlab_cicd:
      - setup_time: "~1.5min" # Pre-built image
      - test_execution: "~10min" # Docker optimization
      - total_pipeline: "~11.5min"
      - success_rate: "TBD" # Target >95%
```

## **Evaluation Criteria**

### **Technical Criteria**
- [ ] **Pipeline Success Rate**: >95% green builds
- [ ] **Execution Speed**: <12min total pipeline time
- [ ] **Setup Complexity**: Straightforward configuration
- [ ] **Debugging Capability**: Clear error reporting and artifact access

### **Operational Criteria**
- [ ] **Cost Efficiency**: Free tier sufficient or competitive pricing
- [ ] **Maintenance Overhead**: Minimal ongoing configuration needs
- [ ] **Integration Quality**: Smooth workflow with existing tools
- [ ] **Documentation Quality**: Clear setup and troubleshooting guides

### **Strategic Criteria**
- [ ] **Platform Independence**: Reduces GitHub dependency risk
- [ ] **Backup Capability**: Functional secondary CI/CD option
- [ ] **Future Scalability**: Can handle project growth requirements
- [ ] **Team Adoption**: Easy for team members to use and maintain

## **Implementation Timeline**

### **Week 1: Repository Setup & Basic Pipeline**
- [ ] GitLab repository creation and mirroring setup
- [ ] Basic .gitlab-ci.yml implementation
- [ ] Unit tests pipeline validation

### **Week 2: E2E Pipeline & Optimization**
- [ ] E2E tests matrix execution implementation
- [ ] Custom Docker image creation and optimization
- [ ] Performance benchmarking vs GitHub Actions

### **Week 3: Quality Gates & Documentation**
- [ ] GateCheck integration with GitLab pipeline
- [ ] Comprehensive documentation and runbook
- [ ] Decision matrix and recommendation report

## **Risk Assessment**

### **Technical Risks**
- **Docker complexity**: GitLab CI/CD heavily Docker-dependent
- **Learning curve**: Team unfamiliarity with GitLab ecosystem
- **Playwright issues**: Potential browser setup challenges in containers

### **Operational Risks**
- **Dual maintenance**: Managing two CI/CD platforms
- **Secret synchronization**: Keeping environment variables in sync
- **Repository mirroring**: Manual sync overhead without premium features

### **Mitigation Strategies**
- **Gradual adoption**: Start with unit tests, expand to E2E
- **Documentation focus**: Comprehensive setup and troubleshooting guides
- **Automation**: Scripts for common maintenance tasks

## **Success Metrics**
- **Primary**: GitLab pipeline achieves >95% success rate
- **Performance**: Pipeline execution <12min total time
- **Reliability**: 7-day uptime >99% without intervention
- **Usability**: Team can execute and debug GitLab pipelines independently

## **Next Steps**
1. **GitLab account setup and repository creation**
2. **Basic pipeline implementation and validation**
3. **Docker image optimization and performance testing**
4. **Comparative analysis and recommendation documentation**

## **Links & References**
- [T-044 Self-hosted Runners Implementation](./T-044_Self_Hosted_Runners_Implementation.md)
- [TD-99 CI/CD Pipeline Issues](../technical_debt/TD-99_CICD_Pipeline_Issues.md)
- [GitLab CI/CD Documentation](https://docs.gitlab.com/ee/ci/)
- [GitLab .NET Examples](https://gitlab.com/gitlab-examples/dotnet)