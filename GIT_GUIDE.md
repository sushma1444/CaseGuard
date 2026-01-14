# Git Guide for CaseGuard Project

## Common Git Commands

### 1. **Cloning the Repository** (First Time Setup)
```bash
# Clone the repository
git clone https://github.com/sushma1444/CaseGuard.git

# Navigate to the project directory
cd CaseGuard

# Switch to the test branch (if needed)
git checkout test
```

### 2. **Checking Status**
```bash
# See what files have changed
git status

# See detailed changes
git diff

# See commit history
git log --oneline
```

### 3. **Making Changes and Committing**
```bash
# Stage all changes
git add .

# Or stage specific files
git add filename.cs

# Commit with a message
git commit -m "Your commit message here"

# Commit all tracked files (skip staging)
git commit -a -m "Your commit message"
```

### 4. **Pushing to Remote**
```bash
# Push current branch to remote
git push origin test

# Push and set upstream (first time)
git push -u origin test

# Force push (use with caution!)
git push --force origin test
```

### 5. **Pulling from Remote**
```bash
# Pull latest changes
git pull origin test

# Fetch without merging
git fetch origin test
```

### 6. **Branching**
```bash
# Create a new branch
git checkout -b new-branch-name

# Switch to existing branch
git checkout branch-name

# List all branches
git branch

# List remote branches
git branch -r

# Delete a branch
git branch -d branch-name
```

### 7. **Viewing Changes**
```bash
# See what changed in a file
git diff filename.cs

# See commit details
git show commit-hash

# See file history
git log --follow filename.cs
```

## Setting Up and Running the Project from Git

### Step 1: Clone the Repository
```powershell
git clone https://github.com/sushma1444/CaseGuard.git
cd CaseGuard
git checkout test
```

### Step 2: Set Up Database
```powershell
# Ensure PostgreSQL is running
# Create database (if not exists)
psql -U postgres -c "CREATE DATABASE CaseGuardDb;"

# Run migrations
cd CaseGuard.Backend.Assignment
dotnet ef database update
```

### Step 3: Load Test Data
```powershell
# From project root
.\setup-test-data.ps1

# OR use the complete setup script
.\setup-everything.ps1
```

### Step 4: Build and Run
```powershell
# Build the project
dotnet build

# Run the application
dotnet run --project CaseGuard.Backend.Assignment
```

### Step 5: Run Tests
```powershell
# In a new terminal (keep application running)
.\test-api.ps1
```

## Complete Setup Workflow (New Machine)

```powershell
# 1. Clone repository
git clone https://github.com/sushma1444/CaseGuard.git
cd CaseGuard
git checkout test

# 2. Restore dependencies
dotnet restore

# 3. Set up database
cd CaseGuard.Backend.Assignment
dotnet ef database update
cd ..

# 4. Load test data
.\setup-test-data.ps1

# 5. Build
dotnet build

# 6. Run application
dotnet run --project CaseGuard.Backend.Assignment

# 7. In another terminal, run tests
.\test-api.ps1
```

## Common Git Workflows

### Daily Development Workflow
```bash
# 1. Pull latest changes
git pull origin test

# 2. Make your changes
# ... edit files ...

# 3. Check status
git status

# 4. Stage and commit
git add .
git commit -m "Description of changes"

# 5. Push to remote
git push origin test
```

### Undoing Changes
```bash
# Discard changes in working directory (not staged)
git restore filename.cs

# Unstage a file (keep changes)
git restore --staged filename.cs

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Undo last commit (discard changes)
git reset --hard HEAD~1
```

### Viewing Remote Information
```bash
# See remote repositories
git remote -v

# See remote branches
git branch -r

# Fetch remote branches
git fetch origin
```

## Troubleshooting

### If you have merge conflicts:
```bash
# Pull with rebase
git pull --rebase origin test

# Resolve conflicts, then:
git add .
git rebase --continue
```

### If you need to reset to remote:
```bash
# Fetch latest
git fetch origin

# Reset to match remote (WARNING: loses local changes)
git reset --hard origin/test
```

### If files are locked (build errors):
```bash
# Stop the running application first
# Then rebuild
dotnet clean
dotnet build
```

## Quick Reference

| Command | Description |
|---------|-------------|
| `git status` | Check current status |
| `git add .` | Stage all changes |
| `git commit -m "msg"` | Commit with message |
| `git push origin test` | Push to remote |
| `git pull origin test` | Pull from remote |
| `git log --oneline` | View commit history |
| `git diff` | See changes |
| `git checkout -b branch` | Create new branch |
| `git branch` | List branches |
