# Database Migration State Fix Guide

## Issue Description

The production deployment is failing with this error:
```
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (4ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE account_transactions (...)

      Npgsql.PostgresException (0x80004005): 42P07: relation "account_transactions" already exists
```

This happens when:
1. Tables exist in the database (created by `EnsureCreatedAsync()` or manual creation)
2. The `__EFMigrationsHistory` table is missing or empty
3. EF Core tries to run migrations but the tables already exist

## Quick Fix Options

### Option 1: Emergency Environment Variable (Fastest)

Set this environment variable in Railway:
```
FORCE_SEED_MIGRATION_HISTORY=true
```

This will automatically detect existing tables and mark migrations as applied without recreating them.

### Option 2: Manual SQL Fix (Direct Database Access)

If you have direct access to the Railway PostgreSQL console, run:

```sql
-- Create the migrations history table if it doesn't exist
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Mark the current migration as applied
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20260206133152_FixAllOwnedEntitiesKeys', '8.0.0') 
ON CONFLICT DO NOTHING;

-- Verify
SELECT * FROM "__EFMigrationsHistory";
```

### Option 3: Nuclear Option (Data Loss)

If you can afford to lose all data and start fresh:
```
RECREATE_DATABASE=true
```

⚠️ **WARNING**: This will delete ALL data in the database!

## Implementation Details

The application now includes several fixes:

### 1. Enhanced Error Detection
The migration logic now specifically detects "table already exists" errors (PostgreSQL error code 42P07).

### 2. Automatic Migration History Recovery
When tables exist but migration history is missing, the application will:
- Create the `__EFMigrationsHistory` table if needed
- Check if critical tables exist
- Mark all migrations as applied if tables are present

### 3. Emergency Fix Method
A new method `EmergencyFixMigrationHistoryAsync()` handles the specific case of corrupted migration state.

### 4. Database Initialization Service
A comprehensive service (`DatabaseInitializationService`) provides structured database initialization with proper error handling.

## Environment Variables

| Variable | Purpose | Values |
|----------|---------|--------|
| `RUN_MIGRATIONS` | Enable automatic migrations | `true`/`false` |
| `RECREATE_DATABASE` | Drop and recreate database | `true`/`false` |
| `FORCE_SEED_MIGRATION_HISTORY` | Emergency migration history fix | `true`/`false` |

## Database Fixer Tool

A standalone tool is available for advanced database troubleshooting:

```bash
# Check database state
dotnet run --project ManageMyMoney.Tools.DatabaseFixer -- --connection-string "your-connection-string" --action check

# Fix migration state
dotnet run --project ManageMyMoney.Tools.DatabaseFixer -- --connection-string "your-connection-string" --action fix

# Reset migration state (dangerous)
dotnet run --project ManageMyMoney.Tools.DatabaseFixer -- --connection-string "your-connection-string" --action reset
```

## Prevention

To prevent this issue in the future:

1. Always use migrations instead of `EnsureCreatedAsync()` in production
2. Maintain consistent migration history across environments
3. Test migration scenarios in staging before deploying to production
4. Use the provided database initialization service for better error handling

## Recovery Steps for Railway

1. **First, try the emergency fix**:
   - Go to Railway dashboard → Your service → Variables
   - Add: `FORCE_SEED_MIGRATION_HISTORY=true`
   - Redeploy

2. **If that fails, check the database directly**:
   - Railway dashboard → Database → Query
   - Run the SQL commands from Option 2 above

3. **Last resort - data loss acceptable**:
   - Add: `RECREATE_DATABASE=true`
   - Redeploy
   - Remove the variable after successful deployment

## Verification

After applying any fix, verify success by checking:

1. Application starts without migration errors
2. Database contains expected tables
3. Migration history table exists with proper entries:
   ```sql
   SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
   ```

## Support

If none of these solutions work, the logs should provide more specific guidance. The application now includes comprehensive logging for database initialization issues.