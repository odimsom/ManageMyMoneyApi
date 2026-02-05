# ============================================
# ManageMyMoney API - Commit Script by Layer
# ============================================

param(
    [switch]$DryRun = $false,
    [switch]$Push = $false
)

$ErrorActionPreference = "Stop"
$RepoRoot = "c:\Users\Francisco C. Dev\source\repos\ManageMyMoneyApi"

function Write-Step {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Execute-GitCommand {
    param([string]$Command)
    
    if ($DryRun) {
        Write-Host "[DRY-RUN] $Command" -ForegroundColor Yellow
    } else {
        Write-Host "Executing: $Command" -ForegroundColor Green
        Invoke-Expression $Command
        if ($LASTEXITCODE -ne 0) {
            throw "Git command failed: $Command"
        }
    }
}

# Navegar al repositorio
Set-Location $RepoRoot
Write-Host "Working directory: $RepoRoot" -ForegroundColor Gray

# Verificar estado del repositorio
Write-Step "Checking repository status"
git status --short

# ============================================
# COMMIT 1: Domain Layer
# ============================================
Write-Step "COMMIT 1: Domain Layer"

$domainFiles = @(
    "ManageMyMoney.Core.Domain/"
)

$domainMessage = @"
feat(domain): implement complete domain layer with entities and value objects

- Add OperationResult pattern for handling operation results without exceptions
- Add Value Objects: Money, Email, Password, Percentage, PhoneNumber, DateRange
- Add Enums: TransactionType, CategoryType, AccountType, BudgetPeriod, etc.

Auth Module:
- User, RefreshToken, PasswordResetToken, EmailVerificationToken, UserSession

Expenses Module:
- Expense, RecurringExpense, ExpenseAttachment, ExpenseTag, ExpenseSplit

Income Module:
- Income, RecurringIncome, IncomeSource

Categories Module:
- Category, Subcategory, CategoryBudget

Accounts Module:
- Account, PaymentMethod, AccountTransaction, CreditCard

Budgets Module:
- Budget, SavingsGoal, GoalContribution

Notifications Module:
- Notification, Reminder, Alert

System Module:
- Currency, ExchangeRate, TaxRate

Domain Services:
- BudgetCalculator, CurrencyConverter, ExpenseAggregator

Repository Interfaces:
- IUserRepository, IExpenseRepository, IIncomeRepository
- IBudgetRepository, IAccountRepository, ICategoryRepository
- ISavingsGoalRepository, INotificationRepository
"@

foreach ($file in $domainFiles) {
    if (Test-Path $file) {
        Execute-GitCommand "git add $file"
    }
}

# Verificar si hay cambios para commitear
$stagedChanges = git diff --cached --name-only
if ($stagedChanges) {
    Execute-GitCommand "git commit -m '$domainMessage'"
    Write-Host "Domain Layer committed successfully!" -ForegroundColor Green
} else {
    Write-Host "No changes in Domain Layer to commit" -ForegroundColor Yellow
}

# ============================================
# COMMIT 2: Persistence Layer
# ============================================
Write-Step "COMMIT 2: Persistence Layer"

$persistenceFiles = @(
    "ManageMyMoney.Infrastructure.Persistence/"
)

$persistenceMessage = @"
feat(persistence): implement EF Core persistence layer with PostgreSQL

- Add ManageMyMoneyContext with all DbSets
- Add automatic snake_case naming convention for PostgreSQL
- Add StringExtensions for case conversion

Entity Configurations:
- Auth: User, RefreshToken, PasswordResetToken, EmailVerificationToken, UserSession
- Expenses: Expense, RecurringExpense, ExpenseAttachment, ExpenseTag, ExpenseSplit
- Income: Income, RecurringIncome, IncomeSource
- Categories: Category, Subcategory, CategoryBudget
- Accounts: Account, PaymentMethod, AccountTransaction, CreditCard
- Budgets: Budget, SavingsGoal, GoalContribution
- Notifications: Notification, Reminder, Alert
- System: Currency, ExchangeRate, TaxRate

Repositories:
- GenericRepository<T> base with OperationResult pattern
- UserRepository, ExpenseRepository, CategoryRepository
- AccountRepository, BudgetRepository, SavingsGoalRepository

Seeds:
- CurrencySeed with default currencies

Infrastructure:
- ServicesRegistration for DI configuration
- Value Objects mapped as owned entities
"@

foreach ($file in $persistenceFiles) {
    if (Test-Path $file) {
        Execute-GitCommand "git add $file"
    }
}

$stagedChanges = git diff --cached --name-only
if ($stagedChanges) {
    Execute-GitCommand "git commit -m '$persistenceMessage'"
    Write-Host "Persistence Layer committed successfully!" -ForegroundColor Green
} else {
    Write-Host "No changes in Persistence Layer to commit" -ForegroundColor Yellow
}

# ============================================
# COMMIT 3: Documentation
# ============================================
Write-Step "COMMIT 3: Documentation"

$docsFiles = @(
    ".github/copilot-instructions.md",
    "scripts/"
)

$docsMessage = @"
docs: update documentation with domain and persistence layer details

- Update copilot-instructions.md with layer structure
- Add commit script for organized deployments
- Document PostgreSQL conventions and patterns
"@

foreach ($file in $docsFiles) {
    if (Test-Path $file) {
        Execute-GitCommand "git add $file"
    }
}

$stagedChanges = git diff --cached --name-only
if ($stagedChanges) {
    Execute-GitCommand "git commit -m '$docsMessage'"
    Write-Host "Documentation committed successfully!" -ForegroundColor Green
} else {
    Write-Host "No changes in Documentation to commit" -ForegroundColor Yellow
}

# ============================================
# PUSH (if requested)
# ============================================
if ($Push) {
    Write-Step "Pushing to remote repository"
    
    # Detectar rama actual automáticamente
    $currentBranch = git rev-parse --abbrev-ref HEAD
    Write-Host "Current branch: $currentBranch" -ForegroundColor Gray
    
    Execute-GitCommand "git push origin $currentBranch"
    Write-Host "Push completed successfully!" -ForegroundColor Green
}

# ============================================
# Summary
# ============================================
Write-Step "Commit Summary"
git log --oneline -5

Write-Host "`nAll operations completed!" -ForegroundColor Green

if (-not $Push) {
    $currentBranch = git rev-parse --abbrev-ref HEAD
    Write-Host "`nTo push changes, run:" -ForegroundColor Yellow
    Write-Host "  git push origin $currentBranch" -ForegroundColor White
    Write-Host "Or re-run this script with -Push flag" -ForegroundColor White
}
