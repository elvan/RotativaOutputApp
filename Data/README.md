# Repository Pattern with ADO.NET Implementation

This document explains the implementation of the Repository Pattern using ADO.NET (without ORM) for the RotativaOutputApp.

## Architecture Overview

The application follows a multi-layered architecture with clear separation of concerns:

1. **Presentation Layer** - Controllers and Views
2. **Service Layer** - Business logic and coordination
3. **Data Access Layer** - Repository pattern implementation with ADO.NET
4. **Database** - SQL Server with stored procedures using CTEs

## Components

### Data Access Layer

- **IDbContext** - Interface defining database operations
- **SqlDbContext** - Concrete implementation of IDbContext using ADO.NET for SQL Server
- **IReportRepository** - Interface defining data access methods for reports
- **ReportRepository** - Implementation of the repository using ADO.NET

### Service Layer

- **IReportService** - Interface for the report business logic
- **ReportService** - Implementation that uses the repository

### Database

- **DatabaseInitializer** - Utility to set up the database, tables, and stored procedures
- **SQL Scripts** - Scripts for database schema and stored procedures with CTEs

## Data Flow

1. The controller receives a request and calls the appropriate service
2. The service uses the repository to access data
3. The repository uses the DbContext to execute stored procedures
4. The stored procedures utilize CTEs for efficient data retrieval
5. Results are mapped to domain models and returned through the layers

## Stored Procedures with CTEs

The application uses Common Table Expressions (CTEs) in stored procedures for:

- Flexible querying and clear organization of complex queries
- Improved readability and maintainability
- Efficient filtering and sorting of data

Example CTE usage:

```sql
WITH ReportsCTE AS (
    SELECT 
        Id, 
        Name, 
        Description,
        Amount,
        Date,
        ROW_NUMBER() OVER (ORDER BY Date DESC) AS RowNum
    FROM 
        Reports
)
SELECT * FROM ReportsCTE;
```

## Error Handling

The implementation includes comprehensive error handling:

- Try-catch blocks at the service layer
- Fallback data if database operations fail
- Logging of database errors
- User-friendly error messages in the UI

## Database Setup

The database is automatically initialized during application startup:

1. The database is created if it doesn't exist
2. Tables are created if they don't exist
3. Stored procedures are set up
4. Sample data is added if the tables are empty
