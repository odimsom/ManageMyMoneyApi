# ManageMyMoney API - Copilot Instructions

## Architecture Overview

This is a **.NET 8 Web API** following **Clean Architecture** (Onion Architecture) with strict layer separation:

```
┌─────────────────────────────────────────────────────┐
│  Presentation.Api  (Controllers, API configuration) │
├─────────────────────────────────────────────────────┤
│  Infrastructure.Persistence  │  Infrastructure.Shared│
│  (EF Core, Repositories)     │  (External services)  │
├─────────────────────────────────────────────────────┤
│  Core.Application  (Use cases, DTOs, Interfaces)    │
├─────────────────────────────────────────────────────┤
│  Core.Domain  (Entities, Value Objects, Enums)      │
└─────────────────────────────────────────────────────┘
```

### Layer Dependencies (Critical)
- **Domain** → No dependencies (pure C#)
- **Application** → References Domain only
- **Infrastructure** → References Application (implements interfaces)
- **Presentation** → References Application, Infrastructure (DI registration)

---

## Persistence Layer - Current Structure

### Context
- `ManageMyMoneyContext`: Main DbContext with all DbSets
  - Automatic snake_case naming convention for PostgreSQL
  - Configurations applied from assembly

### Entity Configurations (Fluent API)

#### Auth Module
- `UserEntityConfiguration`: User with owned Email, Password, PhoneNumber
- `RefreshTokenEntityConfiguration`: JWT refresh tokens
- `PasswordResetTokenEntityConfiguration`: Password recovery
- `EmailVerificationTokenEntityConfiguration`: Email verification
- `UserSessionEntityConfiguration`: Session tracking

#### Expenses Module
- `ExpenseEntityConfiguration`: Main expense with owned Money
- `RecurringExpenseEntityConfiguration`: Subscription/recurring expenses
- `ExpenseAttachmentEntityConfiguration`: Receipt attachments
- `ExpenseTagEntityConfiguration`: Custom tags
- `ExpenseSplitEntityConfiguration`: Split expenses

#### Income Module
- `IncomeEntityConfiguration`: Income transactions
- `RecurringIncomeEntityConfiguration`: Recurring income
- `IncomeSourceEntityConfiguration`: Income sources

#### Categories Module
- `CategoryEntityConfiguration`: Expense/income categories
- `SubcategoryEntityConfiguration`: Category subdivisions
- `CategoryBudgetEntityConfiguration`: Per-category budgets

#### Accounts Module
- `AccountEntityConfiguration`: Financial accounts
- `PaymentMethodEntityConfiguration`: Payment methods
- `AccountTransactionEntityConfiguration`: Transfers
- `CreditCardEntityConfiguration`: Credit card details

#### Budgets Module
- `BudgetEntityConfiguration`: Main budgets
- `SavingsGoalEntityConfiguration`: Savings goals
- `GoalContributionEntityConfiguration`: Goal contributions

#### Notifications Module
- `NotificationEntityConfiguration`: System notifications
- `ReminderEntityConfiguration`: Payment reminders
- `AlertEntityConfiguration`: Budget alerts

#### System Module
- `CurrencyEntityConfiguration`: Supported currencies
- `ExchangeRateEntityConfiguration`: Exchange rates
- `TaxRateEntityConfiguration`: Tax rates

### Repositories

#### Base
- `GenericRepository<T>`: Base CRUD operations with OperationResult

#### Implementations
- `UserRepository`: User management
- `ExpenseRepository`: Expense queries with date range, category, account filters
- `CategoryRepository`: Category management with subcategories
- `AccountRepository`: Account management with balance calculations
- `BudgetRepository`: Budget management with active filtering
- `SavingsGoalRepository`: Savings goals with contributions

### Seeds
- `CurrencySeed`: Default currencies (USD, EUR, GBP, etc.)

### Services Registration
- `AddPersistenceServices()`: Extension method for DI registration

---

## Database Conventions (PostgreSQL)

### Naming
- Tables: `snake_case` (e.g., `expense_tags`, `savings_goals`)
- Columns: `snake_case` (e.g., `created_at`, `user_id`)
- Indexes: `ix_{table}_{columns}` (e.g., `ix_expenses_user_date`)

### Data Types
- Money amounts: `decimal(18,2)`
- Exchange rates: `decimal(18,6)`
- Percentages: `decimal(5,2)`
- Currency codes: `varchar(3)`
- Timestamps: `timestamp` with `CURRENT_TIMESTAMP` default

### Value Object Mapping
- `Money` → Owned entity with `amount` and `currency` columns
- `Email` → Owned entity with `value` column
- `Password` → Owned entity with `hashed_value` column
- `Percentage` → Owned entity with `value` column
- `DateRange` → Owned entity with `start_date` and `end_date` columns

---

## Development Commands

```powershell
# Build and run
dotnet build
dotnet run --project ManageMyMoney.Presentation.Api

# EF Core Migrations
dotnet ef migrations add InitialCreate -p ManageMyMoney.Infrastructure.Persistence -s ManageMyMoney.Presentation.Api
dotnet ef database update -p ManageMyMoney.Infrastructure.Persistence -s ManageMyMoney.Presentation.Api

# API available at: http://localhost:5253 (Swagger UI at /swagger)
```

## Connection String

```json
{
  "ConnectionStrings": {
    "ManageMyMoneyConnection": "Host=localhost;Database=managemymoney;Username=postgres;Password=yourpassword"
  }
}
```

---

## Code Conventions

### Patterns Applied
- **OperationResult Pattern**: All repository methods return OperationResult
- **Factory Methods**: Entities use private constructors with static `Create()` methods
- **Value Objects**: Immutable objects mapped as owned entities
- **Rich Domain Model**: Business logic encapsulated in entities
- **Soft Delete**: Entities use `IsActive` flag instead of hard delete

### Repository Pattern
- Interfaces defined in Domain layer
- Implementations in Persistence layer
- All methods async with proper exception handling
- Generic base repository for common operations

---

## Configuration
- Settings: `appsettings.json` / `appsettings.Development.json`
- Launch profiles: `Properties/launchSettings.json`
