# ClientNotifier Version 1.0.0 - Database & Deployment Improvements

**Date:** September 14, 2025  
**Version:** 1.0.0  
**Type:** Major Database Refactoring & Deployment Setup

## 🎯 Summary

Major refactoring to switch from SQL Server to SQLite for easier deployment, enhanced data models with validation, externalized configuration, and created deployment infrastructure for non-technical users.

## 🔄 Breaking Changes

### Database Migration
- **FROM:** SQL Server (requires server installation)
- **TO:** SQLite (file-based, no installation required)
- **Impact:** All existing SQL Server migrations removed, new SQLite migration created

## 📝 Detailed Changes

### 1. Database Layer Changes

#### Switched Database Provider
- **File:** `ClientNotifier.Data.csproj`
  - Removed: `Microsoft.EntityFrameworkCore.SqlServer v9.0.9`
  - Added: `Microsoft.EntityFrameworkCore.Sqlite v9.0.0`

#### Updated Connection Strings
- **File:** `appsettings.json`
  ```json
  // OLD
  "DefaultConnection": "Server=localhost;Database=ClientNotifierDb;Trusted_Connection=True;TrustServerCertificate=True"
  
  // NEW
  "DefaultConnection": "Data Source=Data/ClientNotifier.db"
  ```

#### Enhanced DbContext
- **File:** `NotifierContext.cs`
  - Added automatic timestamp tracking (UpdatedAt field)
  - Added comprehensive indexes for performance
  - Added SQLite-specific configurations
  - Renamed `Mapping` DbSet to `NamedayMappings` for clarity

### 2. Model Enhancements

#### People Model (`People.cs`)
- Added data annotations for validation:
  - FirstName: Required, 2-100 characters
  - LastName: Optional, max 100 characters
  - EGN: Required, exactly 10 digits with regex validation
  - Email: Email format validation
  - PhoneNumber: Phone format validation
  - Notes: Max 1000 characters
- Added new properties:
  - `UpdatedAt`: Nullable DateTime for tracking modifications
  - `FullName`: Computed property for display
  - `NotificationsEnabled`: Boolean flag to control notifications per person

#### NamedayMapping Model (`NamedayMapping.cs`)
- Restructured to use Month/Day integers instead of DateTime
- Added validation attributes
- Added helper methods:
  - `GetNamedayForYear(int year)`: Returns DateTime for specific year
  - `DateDisplay`: Formatted string property for UI display

### 3. Business Logic Improvements

#### EGN Utilities (`EGNUtils.cs`)
- Added comprehensive EGN validation with checksum verification
- Added helper methods:
  - `IsValidEgn()`: Full validation including checksum
  - `GetGender()`: Extract gender from EGN
  - `GetAge()`: Calculate current age
- Improved date extraction with proper validation

#### New NamedayService (`NamedayService.cs`)
- Service for managing nameday lookups and queries
- Methods include:
  - `GetNamedayForPerson()`: Smart matching with fallback to partial matches
  - `GetPeopleWithNamedaysToday()`: Query for today's celebrations
  - `GetPeopleWithBirthdaysToday()`: Query for today's birthdays

### 4. Configuration & Deployment

#### Configuration Structure
- **File:** `appsettings.json`
  - Added structured configuration sections:
    - DatabaseSettings: Path configuration, auto-migration flags
    - EmailSettings: SMTP configuration for notifications
    - NotificationSettings: Timing and behavior settings

- **New File:** `appsettings.Production.json`
  - Production-specific settings with appropriate defaults
  - Different database path for production use

#### Nameday Data Externalization
- **Removed:** Hardcoded nameday data from code
- **Added:** `Data/bulgarian-namedays.json`
  - Contains 290+ Bulgarian nameday entries
  - Easily editable without recompilation
  - Loaded dynamically at startup

#### Database Initialization
- **New File:** `DbInitializer.cs`
  - Handles automatic database creation
  - Loads nameday data from JSON
  - Seeds test data in development
  - Updates existing records with namedays

#### Deployment Infrastructure
- **New File:** `DEPLOYMENT_GUIDE.md`
  - Comprehensive guide for non-technical users
  - Step-by-step installation instructions
  - Troubleshooting section
  - Daily usage guide

- **New File:** `SETUP.bat`
  - One-click initial setup
  - Checks for .NET runtime
  - Creates necessary directories

- **New File:** `START_APP.bat`
  - Daily startup script
  - Opens browser automatically
  - Sets production environment

### 5. Program.cs Enhancements
- Added comprehensive logging setup
- Implemented flexible database path configuration
- Added automatic directory creation
- Conditional test data seeding based on configuration
- Better error handling during initialization

## 📁 Files Modified

### Modified Files:
1. `ClientNotifier.Data/ClientNotifier.Data.csproj`
2. `ClientNotifier.Data/NotifierContext.cs`
3. `ClientNotifier.Core/Models/People.cs`
4. `ClientNotifier.Core/Models/NamedayMapping.cs`
5. `ClientNotifier.Core/Services/EGNUtils.cs`
6. `ClientNotifier.API/Program.cs`
7. `ClientNotifier.API/appsettings.json`
8. `ClientNotifier.API/ClientNotifier.API.csproj`

### New Files Created:
1. `ClientNotifier.Core/Services/NamedayService.cs`
2. `ClientNotifier.Data/DbInitializer.cs`
3. `ClientNotifier.API/Data/bulgarian-namedays.json`
4. `ClientNotifier.API/appsettings.Production.json`
5. `ClientNotifier.API/SETUP.bat`
6. `ClientNotifier.API/START_APP.bat`
7. `DEPLOYMENT_GUIDE.md`
8. `VERSION_NOTES_v1.0.0.md` (this file)

### Deleted Files:
1. `ClientNotifier.Data/Migrations/20250914121716_InitialCreate.cs`
2. `ClientNotifier.Data/Migrations/20250914121716_InitialCreate.Designer.cs`
3. `ClientNotifier.Data/Migrations/NotifierContextModelSnapshot.cs` (old version)

### New Migration Files:
1. `ClientNotifier.Data/Migrations/20250914162341_InitialSqliteMigration.cs`
2. `ClientNotifier.Data/Migrations/20250914162341_InitialSqliteMigration.Designer.cs`
3. `ClientNotifier.Data/Migrations/NotifierContextModelSnapshot.cs` (new SQLite version)

## 🚀 Deployment Benefits

1. **Zero Dependencies**: SQLite requires no server installation
2. **Portable**: Database is a single file, easy to backup/restore
3. **User-Friendly**: Batch files for setup and daily use
4. **Configurable**: All settings in JSON files, no code changes needed
5. **Maintainable**: Nameday data can be updated by editing JSON

## ⚠️ Migration Notes

For existing installations:
1. Export any existing data from SQL Server
2. Delete old migration files
3. Run the new SQLite migration
4. Import data into the new database

## 🔮 Future Considerations

1. Add data import/export functionality
2. Implement automatic backup scheduling
3. Add nameday data validation on load
4. Consider adding logging to file for production

---

**Developer Notes:**
- All package versions are compatible with .NET 9.0
- SQLite version uses looser versioning (9.0.0) for better compatibility
- Test data seeding can be controlled via configuration
- Database path is configurable and supports both relative and absolute paths
