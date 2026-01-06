# Database Seeder

This console application seeds the eSamadhaan database with realistic development data.

## Overview

The seeder creates:
- **8 Departments** (Public Works, Water Supply, Electricity, Municipal, Health, Education, Transport, Revenue)
- **20 Users** (1 Admin, 4 Supervisors, 6 Officers, 9 Citizens)
- **16 Categories** (distributed across departments)
- **50 Grievances** (with various statuses: Submitted, Assigned, InReview, Resolved, Closed)
- **Assignments** (linking officers to grievances)
- **Status History** (tracking status changes)
- **Resolutions** (for resolved grievances)
- **Feedbacks** (for closed grievances)

## Features

- ✅ **Idempotent**: Safe to run multiple times - skips existing data
- ✅ **Realistic Data**: Uses realistic but dummy data (no real PII)
- ✅ **Foreign Key Safe**: Respects all database constraints and relationships
- ✅ **Comprehensive**: Covers all core features and edge cases

## Prerequisites

1. .NET 10.0 SDK installed
2. Database connection string configured in `appsettings.json` or `appsettings.Development.json`
3. **Database schema must exist** - Run migrations first:
   ```bash
   cd backend/src/eSamadhaan.API
   dotnet ef database update
   ```
   
   **Note**: The seeder uses `EnsureCreatedAsync()` which will create the database if it doesn't exist, but it won't run migrations. For production/CI environments, always run migrations first.

## Configuration

The seeder uses the same configuration as the API project. It will look for `appsettings.json` in:
1. Current directory (seeder project)
2. Parent `eSamadhaan.API` directory

Ensure your connection string is configured in one of these files:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
}
```

## Running Locally

### Option 1: From Seeder Project Directory

```bash
cd backend/src/eSamadhaan.DatabaseSeeder
dotnet run
```

### Option 2: From Solution Root

```bash
cd backend/src/eSamadhaan.DatabaseSeeder
dotnet run --project eSamadhaan.DatabaseSeeder.csproj
```

### Option 3: Build and Run Executable

```bash
cd backend/src/eSamadhaan.DatabaseSeeder
dotnet build -c Release
dotnet bin/Release/net10.0/eSamadhaan.DatabaseSeeder.dll
```

## Running in CI/CD

### GitHub Actions Example

```yaml
- name: Seed Database
  run: |
    cd backend/src/eSamadhaan.DatabaseSeeder
    dotnet run --no-build --configuration Release
  env:
    ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
```

### Azure DevOps Example

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Seed Database'
  inputs:
    command: 'run'
    projects: 'backend/src/eSamadhaan.DatabaseSeeder/eSamadhaan.DatabaseSeeder.csproj'
    arguments: '--configuration Release'
  env:
    ConnectionStrings__DefaultConnection: $(DATABASE_CONNECTION_STRING)
```

### Docker Example

```dockerfile
# In your Dockerfile or docker-compose
RUN cd /app/src/eSamadhaan.DatabaseSeeder && \
    dotnet run --no-build --configuration Release
```

## Default Test Credentials

After seeding, you can use these credentials to test the application:

| Role | Email | Password |
|------|-------|----------|
| System Admin | `admin@esamadhaan.test` | `Password123!` |
| Supervisory Officer | `supervisor.pwd@esamadhaan.test` | `Password123!` |
| Department Officer | `officer.pwd1@esamadhaan.test` | `Password123!` |
| Citizen | `citizen.ramesh@test.com` | `Password123!` |

**Note**: All users share the same password for development convenience: `Password123!`

## Data Distribution

- **Grievances by Status**:
  - Submitted: ~20%
  - Assigned: ~25%
  - InReview: ~25%
  - Resolved: ~20%
  - Closed: ~10%

- **Escalated Grievances**: ~20% of all grievances

- **Feedbacks**: ~70% of closed grievances have feedback

## Troubleshooting

### Connection String Not Found

```
ERROR: Connection string 'DefaultConnection' not found in configuration.
```

**Solution**: Ensure `appsettings.json` exists in the seeder project or API project with a valid connection string.

### Foreign Key Violation

```
The INSERT statement conflicted with the FOREIGN KEY constraint
```

**Solution**: The seeder runs in order (Departments → Users → Categories → Grievances → etc.). If you see this error, it may indicate:
- Database schema is out of sync
- Previous partial seed run left inconsistent state
- Try running migrations first: `dotnet ef database update`

### Duplicate Key Error

The seeder is idempotent and checks for existing data. If you see duplicate key errors:
- Clear the database and re-run
- Or modify the seeder to use upsert logic (currently uses insert-only with existence checks)

## Customization

To customize the seed data, edit `DataSeeder.cs` in:
```
backend/src/eSamadhaan.Infrastructure/Data/DataSeeder.cs
```

Key methods to modify:
- `SeedDepartmentsAsync()` - Add/modify departments
- `SeedUsersAsync()` - Add/modify users
- `SeedCategoriesAsync()` - Add/modify categories
- `SeedGrievancesAsync()` - Adjust grievance count or distribution

## Notes

- The seeder uses a fixed random seed (42) for reproducibility
- All timestamps use UTC
- Email addresses use `.test` or `.com` domains (not real addresses)
- Passwords are hashed using the same `PasswordHasher` service as the API

