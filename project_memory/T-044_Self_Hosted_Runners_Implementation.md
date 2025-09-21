# T-044: Self-hosted Runners Implementation - Azure VM Strategy

## **Objective**
Implement Azure VM-based self-hosted GitHub Actions runners to resolve TD-99 CI/CD pipeline failures and achieve stable E2E test execution.

## **Background Context**
From TD-99 analysis and T-040 Phase 1 results:
- ✅ **Fixed**: Compilation errors, Ubuntu dependencies, test parameters, BrowserFactory lifecycle
- ❌ **Persistent**: Test execution failures, networking issues, race conditions
- 🎯 **Target**: >95% green build rate, <5min feedback for smoke tests

## **Architecture Overview**

### **Self-hosted Runner Strategy**
```
GitHub Repository
    ↓ (webhook trigger)
Azure VM Runner (ubuntu-22.04)
    ├── Docker Engine
    ├── .NET 8 Runtime
    ├── Playwright browsers pre-installed
    ├── GitHub Actions Runner Agent
    └── Custom test environment optimization
```

## **Implementation Plan**

### **Phase 1: Azure VM Provisioning (Week 1)**

#### **VM Specifications**
- **Size**: Standard_D4s_v3 (4 vCPUs, 16 GB RAM)
- **OS**: Ubuntu 22.04 LTS
- **Storage**: 128 GB Premium SSD
- **Network**: Standard public IP with SSH access
- **Region**: East US (close to GitHub infrastructure)

#### **Cost Analysis**
- **VM Cost**: ~$140/month (Standard_D4s_v3)
- **Storage**: ~$20/month (128 GB Premium SSD)
- **Network**: ~$5/month (estimated)
- **Total**: ~$165/month operational cost

### **Phase 2: Environment Setup (Week 2)**

#### **Base System Configuration**
```bash
# System updates and dependencies
sudo apt update && sudo apt upgrade -y
sudo apt install -y docker.io curl wget git

# .NET 8 installation
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# Docker configuration for runner user
sudo usermod -aG docker $USER
sudo systemctl enable docker
sudo systemctl start docker
```

#### **Playwright Environment**
```bash
# Playwright dependencies (pre-resolved for ubuntu-22.04)
sudo apt install -y \
    libnss3 libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxrandr2 libgbm1 \
    libxss1 libasound2

# Playwright browsers installation
npx playwright install chromium firefox webkit
npx playwright install-deps
```

#### **GitHub Actions Runner Registration**
```bash
# Download and configure GitHub Actions runner
mkdir actions-runner && cd actions-runner
curl -o actions-runner-linux-x64-2.311.0.tar.gz -L \
  https://github.com/actions/runner/releases/download/v2.311.0/actions-runner-linux-x64-2.311.0.tar.gz
tar xzf ./actions-runner-linux-x64-2.311.0.tar.gz

# Configure runner (requires GitHub token)
./config.sh --url https://github.com/adria-andreu/ShopWebTestAutomated \
  --token [RUNNER_TOKEN] --name azure-runner-01 --labels self-hosted,linux,azure

# Install as service
sudo ./svc.sh install
sudo ./svc.sh start
```

### **Phase 3: Workflow Optimization (Week 3)**

#### **Custom GitHub Actions Workflow**
Create `.github/workflows/tests-self-hosted.yml`:

```yaml
name: E2E Tests (Self-hosted)
on:
  push:
    branches: [main, feature/*]
  pull_request:
    branches: [main]

jobs:
  e2e-tests:
    runs-on: [self-hosted, linux, azure]
    timeout-minutes: 30
    
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
        site: [A, B]
      fail-fast: false
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore -c Release
    
    - name: Run E2E Tests
      run: |
        dotnet test ShopWeb.E2E.Tests/ \
          -c Release \
          --no-build \
          --logger "trx;LogFileName=test-results-${{ matrix.browser }}-${{ matrix.site }}.trx" \
          --logger "console;verbosity=detailed" \
          -- TestRunParameters.Parameter\(name=Browser,value=${{ matrix.browser }}\) \
             TestRunParameters.Parameter\(name=SiteId,value=${{ matrix.site }}\)
      env:
        BROWSER: ${{ matrix.browser }}
        SITE_ID: ${{ matrix.site }}
        TRACE_MODE: OnFailure
    
    - name: Upload test results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: test-results-${{ matrix.browser }}-${{ matrix.site }}
        path: |
          **/TestResults/**/*.trx
          **/artifacts/**/*
    
    - name: Quality Gates Check
      if: always()
      run: |
        dotnet run --project tools/GateCheck/ -- \
          --input artifacts/run-metrics.json \
          --threshold-pass-rate 0.90 \
          --threshold-p95-duration 900000
```

## **Success Criteria & Validation**

### **Technical Validation**
- [ ] Azure VM provisioned and accessible via SSH
- [ ] GitHub Actions runner registered and online
- [ ] .NET 8 + Playwright environment functional
- [ ] Docker containerization working
- [ ] Self-hosted workflow executing successfully

### **Performance Metrics**
- [ ] **Pass Rate**: >90% (PR), >95% (main)
- [ ] **Feedback Speed**: <5min (smoke), <15min (full suite)
- [ ] **Stability**: >95% successful pipeline executions
- [ ] **Resource Efficiency**: Optimal VM utilization

### **Cost Management**
- [ ] Monthly cost monitoring (<$200/month target)
- [ ] Automatic shutdown policies for cost optimization
- [ ] Resource scaling based on usage patterns

## **Risk Mitigation**

### **Infrastructure Risks**
- **VM Downtime**: Implement automated health checks and restart policies
- **Cost Overrun**: Set up Azure cost alerts and spending limits
- **Security**: Configure firewall rules, SSH key authentication, regular patching

### **GitHub Integration Risks**
- **Runner Disconnection**: Implement auto-reconnection scripts
- **Workflow Compatibility**: Gradual migration from hosted to self-hosted
- **Secret Management**: Secure handling of GitHub tokens and secrets

## **Monitoring & Maintenance**

### **Health Monitoring**
```bash
# Runner health check script
#!/bin/bash
# check-runner-health.sh
systemctl is-active --quiet actions.runner.* || {
  echo "Runner service down, restarting..."
  sudo systemctl restart actions.runner.*
}
```

### **Automated Maintenance**
- **Weekly**: System updates and security patches
- **Monthly**: Playwright browser updates
- **Quarterly**: VM performance review and optimization

## **Next Steps**
1. **Azure subscription access verification**
2. **VM provisioning and basic setup**
3. **GitHub runner registration and testing**
4. **Workflow migration and validation**
5. **Performance monitoring and optimization**

## **Links & References**
- [T-040 Architecture Analysis](./T-040_CICD_Architecture_Analysis.md)
- [TD-99 CI/CD Pipeline Issues](../technical_debt/TD-99_CICD_Pipeline_Issues.md)
- [GitHub Self-hosted Runners Documentation](https://docs.github.com/en/actions/hosting-your-own-runners)
- [Azure VM Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)