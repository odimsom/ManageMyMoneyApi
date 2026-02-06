-- Emergency Database Migration Fix Script for Railway Production
-- This script should be run directly in Railway's PostgreSQL console
-- when migration state is corrupted and tables already exist

-- Step 1: Create the migrations history table if it doesn't exist
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Step 2: Insert the current migration as "applied"
-- Replace '20260206133152_FixAllOwnedEntitiesKeys' with your actual migration name
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
VALUES ('20260206133152_FixAllOwnedEntitiesKeys', '8.0.0') 
ON CONFLICT DO NOTHING;

-- Step 3: Verify the migration history
SELECT * FROM "__EFMigrationsHistory";

-- Step 4: Check that critical tables exist
SELECT tablename FROM pg_tables 
WHERE schemaname = 'public' 
  AND tablename IN ('account_transactions', 'users', 'expenses', 'categories')
ORDER BY tablename;