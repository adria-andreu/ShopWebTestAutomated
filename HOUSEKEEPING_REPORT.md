# Repository Housekeeping Report

Generated: 2025-01-16  
Repository: ShopWebTestAutomated  
Total files analyzed: 1,463

## 1. Duplicate Files

### 🔴 Identical Content Duplicates (High Priority)

**Project Instructions Directory Duplicates:**
- `./Project Instructions/` is entirely duplicated in `./1_Version2_ShopWebTestAutomated/Project Instructions/`
  - All files have identical MD5 hashes
  - Total duplicate files: ~25 files
  - **Recommendation**: Remove entire `./1_Version2_ShopWebTestAutomated/Project Instructions/` directory

**Specific File Duplicates:**
- **docker-compose.yml**
  - `./Project Instructions/docker-compose.yml` (7c20f98e38a18eefdf2057aa6c8a6622)
  - `./1_Version2_ShopWebTestAutomated/Project Instructions/docker-compose.yml` (7c20f98e38a18eefdf2057aa6c8a6622) 
  - Note: Root `./docker-compose.yml` has different content (b1c29f93e701a47eb60f574cd005806d)

- **Dockerfile (Examples)**
  - `./Project Instructions/docs/examples/Dockerfile` (0f22c01f97252137dd1c3bf4dfc3ca81)
  - `./1_Version2_ShopWebTestAutomated/Project Instructions/docs/examples/Dockerfile` (0f22c01f97252137dd1c3bf4dfc3ca81)
  - Note: Root `./Dockerfile` has different content (0c8afc9652b91fb1f61a7c6fe5cdfb5c)

- **README.md**
  - `./Project Instructions/README.md` (24223e03c12494991ecb6c67cc659130)
  - `./1_Version2_ShopWebTestAutomated/Project Instructions/README.md` (24223e03c12494991ecb6c67cc659130)
  - Note: Root `./README.md` has different content (735e85e0823bc9f169418031e851cee9)

- **All Example C# Files** (100% identical duplicates):
  - `AllureIntegrationExample.cs`, `BaseTestExample.cs`, `BrowserFactoryExample.cs`
  - `GateCheckExample.cs`, `MetricsCollectorExample.cs`, `PortabilityPatternExample.cs`
  - `RunMetricsCollectorExample.cs`, `Uso rápido (NUnit BaseTest)Example.cs`
  - All utility classes: `Redactor.cs`, `Waits.cs`, `ISiteProfile.cs`

### 🟡 Other Duplicate Patterns

**Environment Files:**
- `./.env.example` vs `./Project Instructions/docs/examples/.env.example`
- May have different content, needs verification

**Configuration Files:**
- `appsettings.tests.example.json` appears in both Project Instructions directories
- `testdata-structure.json` appears in both Project Instructions directories

## 2. Old / Stale Files

### 🟡 Low Activity Files
Based on git history analysis, these files have minimal commit activity:

**Single-commit files:**
- Most files in `./Project Instructions/` and `./1_Version2_ShopWebTestAutomated/Project Instructions/`
- Most configuration files in `./ShopWeb.E2E.Tests/Config/`
- Tool directories: `./Project Instructions/docs/tools/GateCheck/`

**Note**: Repository appears to be relatively new (all files have recent timestamps), so no files are truly "stale" from an age perspective.

## 3. Unused or Orphaned Files

### 🔴 Likely Unused Assets

**Playwright Dependency Files (1,011 files):**
- `./ShopWeb.E2E.Tests/bin/Debug/net8.0/.playwright/package/` (heavy directory)
- `./ShopWeb.E2E.Tests/bin/Release/net8.0/.playwright/package/` (duplicate heavy directory)
- `./tests/ShopWeb.UnitTests/bin/Release/net8.0/.playwright/package/` (third copy)
- Contains browser binaries, Node.js executables, and framework files
- **Recommendation**: These should be in .gitignore as they're build artifacts

**Image Assets (Low Priority):**
- `appIcon.png` files in Playwright directories (3 copies)
- These are part of Playwright framework, not custom assets

### 🟡 Potentially Unused Configuration

**Example Files:**
- All files in `docs/examples/` directories appear to be templates/examples
- May not be actively used in the running application
- `flaky-issue-template.md`, `runbook.md`, `pom-guidelines.md`

**Duplicate .editorconfig:**
- `./Project Instructions/docs/examples/.editorconfig`
- Only exists in examples, not at repository root where it would be effective

## 4. No-Value / Removable Files

### 🔴 Build Artifacts (High Priority for Removal)

**Visual Studio Files:**
- `./ShopWeb.E2E.Tests/.vs/` directory (entire directory)
- `.vsidx` files (5 files): FileContentIndex artifacts
- `.suo` files (1 file): Solution user options
- `TestStore/` directory with test logs
- `.dtbcache.v2`, `.futdcache.v2` files

**Build Output Directories:**
- `./ShopWeb.E2E.Tests/bin/` (entire directory - 1000+ files)
- `./ShopWeb.E2E.Tests/obj/` (entire directory)
- `./tests/ShopWeb.UnitTests/bin/` (entire directory)
- `./tests/ShopWeb.UnitTests/obj/` (entire directory)

**Cache Files:**
- `.assets.cache` files (4 files)
- `.csproj.AssemblyReference.cache` files (4 files)
- `.csproj.CoreCompileInputs.cache` files (4 files)
- `.genruntimeconfig.cache` files (4 files)
- `project.nuget.cache` files (2 files)

**Temporary Files:**
- `nunit_random_seed.tmp` (1 file)
- `.testlog` files (1 file)
- Various `.tmp` extensions

### 🟡 Legacy/Outdated Content

**Outdated Claude Configuration:**
- `./.claude/CLAUDE2808.md` - appears to be an old version of CLAUDE.md
- Likely superseded by `./.claude/CLAUDE.md`

**Documentation Artifacts:**
- `./IT04_PR_DESCRIPTION.md` - appears to be a specific PR description file
- May be safe to remove after PR is merged

## Summary and Recommendations

### Immediate Actions (High Priority):
1. **Remove entire duplicate directory**: `./1_Version2_ShopWebTestAutomated/Project Instructions/`
2. **Add to .gitignore and remove**: All `bin/`, `obj/`, `.vs/` directories
3. **Clean build artifacts**: Remove all cache, temporary, and VS-specific files
4. **Remove old Claude config**: `./.claude/CLAUDE2808.md`

### Medium Priority:
1. Review and consolidate environment files (`.env.example` duplicates)
2. Verify if examples in `docs/examples/` are still needed
3. Consider removing build artifacts from git history to reduce repository size

### Low Priority:
1. Review orphaned configuration files
2. Consolidate documentation if needed

### Files Safe to Keep:
- Root level configuration files (`./docker-compose.yml`, `./Dockerfile`, `./README.md`)
- All source code in `./ShopWeb.E2E.Tests/` and `./tests/`
- Project memory and documentation in organized folders
- Active workflow files in `.github/workflows/`

**Estimated space savings**: Removing build artifacts and duplicates could reduce repository size by 80%+ and file count by ~1,200 files.