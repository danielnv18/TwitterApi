# Database Migrations Guide

## Migration Files Location

Migrations are stored in: `src/Infrastructure/Migrations/`

Each migration consists of 3 files:
```
src/Infrastructure/Migrations/
├── 20251113223824_InitialCreate.cs              # Migration Up/Down methods
├── 20251113223824_InitialCreate.Designer.cs     # Migration metadata
└── ApplicationDbContextModelSnapshot.cs          # Current model state
```

---

## EF Core Tools Setup

### Install dotnet-ef globally (one-time setup):
```bash
dotnet tool install --global dotnet-ef --version 9.0.0
```

### Add to PATH (add to ~/.bashrc or ~/.bash_profile for persistence):
```bash
export PATH="$PATH:/home/dnoyola/.dotnet/tools"
```

---

## Common Migration Commands

### 1. List All Migrations
```bash
# From project root
dotnet ef migrations list --startup-project src/API --project src/Infrastructure
```

### 2. Create a New Migration
```bash
# From project root
dotnet ef migrations add MigrationName --startup-project src/API --project src/Infrastructure
```

### 3. Apply Migrations to Database
```bash
# Apply all pending migrations
dotnet ef database update --startup-project src/API --project src/Infrastructure

# Apply up to a specific migration
dotnet ef database update MigrationName --startup-project src/API --project src/Infrastructure

# Rollback all migrations (reset database)
dotnet ef database update 0 --startup-project src/API --project src/Infrastructure
```

### 4. Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove --startup-project src/API --project src/Infrastructure
```

### 5. Generate SQL Script (without applying)
```bash
# Generate SQL for all migrations
dotnet ef migrations script --startup-project src/API --project src/Infrastructure

# Generate SQL for specific migration range
dotnet ef migrations script FromMigration ToMigration --startup-project src/API --project src/Infrastructure

# Generate SQL from last applied to latest
dotnet ef migrations script --idempotent --startup-project src/API --project src/Infrastructure
```

---

## Shortcuts (from Infrastructure directory)

```bash
# Navigate to Infrastructure directory
cd src/Infrastructure

# Create migration
dotnet ef migrations add MigrationName --startup-project ../API

# Apply migrations
dotnet ef database update --startup-project ../API

# List migrations
dotnet ef migrations list --startup-project ../API

# Remove last migration
dotnet ef migrations remove --startup-project ../API
```

---

## Verify Migrations in Database

### Using Docker SQL Server:
```bash
# Connect to SQL Server in Docker
docker exec -it twitter-clone-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C

# Then run SQL commands:
USE TwitterCloneDb;
GO

-- Check applied migrations
SELECT * FROM __EFMigrationsHistory;
GO

-- Check tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';
GO
```

### One-line query:
```bash
docker exec twitter-clone-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -Q "USE TwitterCloneDb; SELECT * FROM __EFMigrationsHistory;"
```

---

## Migration Workflow

### Development:
1. Make changes to entities in `src/Domain/Entities/`
2. Create migration: `dotnet ef migrations add DescriptiveName --startup-project src/API --project src/Infrastructure`
3. Review generated migration file
4. Apply migration: `dotnet ef database update --startup-project src/API --project src/Infrastructure`
5. Verify in database

### Production:
1. Generate idempotent SQL script: 
   ```bash
   dotnet ef migrations script --idempotent --startup-project src/API --project src/Infrastructure -o migration.sql
   ```
2. Review the SQL script
3. Apply script to production database using your preferred method

---

## Rollback Migrations

### Rollback to specific migration:
```bash
dotnet ef database update PreviousMigrationName --startup-project src/API --project src/Infrastructure
```

### Rollback all migrations (WARNING: This drops all tables):
```bash
dotnet ef database update 0 --startup-project src/API --project src/Infrastructure
```

---

## Troubleshooting

### "Could not execute because dotnet-ef was not found"
```bash
# Install the tool
dotnet tool install --global dotnet-ef --version 9.0.0

# Add to PATH
export PATH="$PATH:/home/dnoyola/.dotnet/tools"
```

### "Build failed"
```bash
# Build the solution first
dotnet build
```

### "Unable to create an object of type 'ApplicationDbContext'"
- Ensure connection string is correct in `appsettings.json`
- Ensure SQL Server container is running
- Ensure Infrastructure project has EF Core Design package

### Check EF Tools version:
```bash
dotnet ef --version
```

---

## Current Migrations

As of now, we have:
- **InitialCreate** (20251113223824) - ✅ Applied
  - Creates: Users, RefreshTokens, EmailVerificationTokens tables
  - Creates: All indexes and foreign keys

---

## Quick Reference Card

```bash
# Navigate to project root first
cd /home/dnoyola/dev/twitter-clone/TwitterApi

# Set PATH (required for each terminal session)
export PATH="$PATH:/home/dnoyola/.dotnet/tools"

# Common commands
dotnet ef migrations add <Name> --startup-project src/API --project src/Infrastructure
dotnet ef database update --startup-project src/API --project src/Infrastructure
dotnet ef migrations list --startup-project src/API --project src/Infrastructure
dotnet ef migrations remove --startup-project src/API --project src/Infrastructure
```

---

## Environment Variables

For easier usage, add to `~/.bashrc`:
```bash
# .NET Tools
export PATH="$PATH:/home/dnoyola/.dotnet/tools"

# EF Core aliases
alias ef-add='dotnet ef migrations add --startup-project src/API --project src/Infrastructure'
alias ef-update='dotnet ef database update --startup-project src/API --project src/Infrastructure'
alias ef-list='dotnet ef migrations list --startup-project src/API --project src/Infrastructure'
alias ef-remove='dotnet ef migrations remove --startup-project src/API --project src/Infrastructure'
alias ef-script='dotnet ef migrations script --startup-project src/API --project src/Infrastructure'
```

Then use:
```bash
ef-add AddNewFeature
ef-update
ef-list
```
