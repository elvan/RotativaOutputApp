USE ReportDB;
GO

-- Stored procedure to get report by ID
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_GetReportById')
    DROP PROCEDURE usp_GetReportById;
GO

CREATE PROCEDURE usp_GetReportById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Using CTE for demonstration
    WITH ReportDetailCTE AS (
        SELECT 
            Id, 
            Name, 
            Description,
            Amount,
            Date
        FROM 
            Reports
        WHERE 
            Id = @Id
    )
    
    -- Select from the CTE
    SELECT 
        Id, 
        Name, 
        Description,
        Amount,
        Date
    FROM 
        ReportDetailCTE;
END
GO
