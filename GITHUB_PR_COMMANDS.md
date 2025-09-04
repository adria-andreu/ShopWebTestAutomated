# GitHub Pull Request Commands

## Manual Steps to Create PR

Since there are authentication issues with automated push, please run these commands manually:

### 1. Push the Branch
```bash
cd temp_cleanup_repo
git push --set-upstream origin housekeeping/cleanup-duplicates-and-config
```

### 2. Create Pull Request with GitHub CLI
```bash
cd temp_cleanup_repo
gh pr create --title "housekeeping: cleanup duplicate files and outdated configuration" --body-file PR_DESCRIPTION.md
```

### 3. Alternative - Create PR via GitHub Web Interface
If GitHub CLI doesn't work, visit: https://github.com/adria-andreu/ShopWebTestAutomated/pull/new/housekeeping/cleanup-duplicates-and-config

---

## Pull Request Details

**Branch**: `housekeeping/cleanup-duplicates-and-config`  
**Base**: `main`  
**Title**: `housekeeping: cleanup duplicate files and outdated configuration`  
**Files changed**: 27 files  
**Lines removed**: 2,601 lines  
**Type**: Maintenance/Cleanup  

---

## Verification Commands (after PR creation)
```bash
# Verify branch was pushed
gh pr list

# Check PR status  
gh pr view --web

# Merge when ready
gh pr merge --squash
```