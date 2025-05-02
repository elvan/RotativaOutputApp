USE ReportDB;
GO

-- Stored procedure to get reports with paging, sorting, filtering, and searching
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_GetAllReports')
    DROP PROCEDURE usp_GetAllReports;
GO

CREATE PROCEDURE usp_GetAllReports
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'Date',
    @SortDirection NVARCHAR(4) = 'DESC',
    @SearchTerm NVARCHAR(100) = NULL,
    @MinAmount DECIMAL(18, 2) = NULL,
    @MaxAmount DECIMAL(18, 2) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate total records (for pagination info)
    SELECT COUNT(*) AS TotalRecords
    FROM Reports
    WHERE 
        (@SearchTerm IS NULL OR 
            Name LIKE '%' + @SearchTerm + '%' OR 
            Description LIKE '%' + @SearchTerm + '%') AND
        (@MinAmount IS NULL OR Amount >= @MinAmount) AND
        (@MaxAmount IS NULL OR Amount <= @MaxAmount) AND
        (@StartDate IS NULL OR Date >= @StartDate) AND
        (@EndDate IS NULL OR Date <= @EndDate);
    
    -- Dynamic sorting with CTE
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = '
    WITH FilteredReportsCTE AS (
        SELECT 
            Id, 
            Name, 
            Description,
            Amount,
            Date
        FROM 
            Reports
        WHERE 
            (' + CASE WHEN @SearchTerm IS NULL THEN '1=1' ELSE 
                'Name LIKE ''%' + @SearchTerm + '%'' OR Description LIKE ''%' + @SearchTerm + '%''' END + ') AND
            (' + CASE WHEN @MinAmount IS NULL THEN '1=1' ELSE 'Amount >= ' + CAST(@MinAmount AS NVARCHAR) END + ') AND
            (' + CASE WHEN @MaxAmount IS NULL THEN '1=1' ELSE 'Amount <= ' + CAST(@MaxAmount AS NVARCHAR) END + ') AND
            (' + CASE WHEN @StartDate IS NULL THEN '1=1' ELSE 'Date >= ''' + CONVERT(NVARCHAR, @StartDate, 121) + '''' END + ') AND
            (' + CASE WHEN @EndDate IS NULL THEN '1=1' ELSE 'Date <= ''' + CONVERT(NVARCHAR, @EndDate, 121) + '''' END + ')
    ),
    OrderedReportsCTE AS (
        SELECT 
            *,
            ROW_NUMBER() OVER (ORDER BY ' + 
            CASE 
                WHEN @SortColumn = 'Id' THEN 'Id'
                WHEN @SortColumn = 'Name' THEN 'Name'
                WHEN @SortColumn = 'Description' THEN 'Description'
                WHEN @SortColumn = 'Amount' THEN 'Amount'
                ELSE 'Date'
            END + ' ' + 
            CASE 
                WHEN @SortDirection IN ('ASC', 'asc') THEN 'ASC'
                ELSE 'DESC'
            END + ') AS RowNum
        FROM 
            FilteredReportsCTE
    )
    
    SELECT 
        Id, 
        Name, 
        Description,
        Amount,
        Date
    FROM 
        OrderedReportsCTE
    WHERE
        RowNum BETWEEN ' + CAST((@PageNumber - 1) * @PageSize + 1 AS NVARCHAR) + ' AND ' + CAST(@PageNumber * @PageSize AS NVARCHAR) + '
    ORDER BY 
        RowNum'
    
    EXEC sp_executesql @SQL;
END
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

-- Stored procedure to get reports by date range with paging and sorting
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_GetReportsByDateRange')
    DROP PROCEDURE usp_GetReportsByDateRange;
GO

CREATE PROCEDURE usp_GetReportsByDateRange
    @StartDate DATETIME,
    @EndDate DATETIME,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'Date',
    @SortDirection NVARCHAR(4) = 'ASC',
    @SearchTerm NVARCHAR(100) = NULL,
    @MinAmount DECIMAL(18, 2) = NULL,
    @MaxAmount DECIMAL(18, 2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calculate total records (for pagination info)
    SELECT COUNT(*) AS TotalRecords
    FROM Reports
    WHERE 
        Date BETWEEN @StartDate AND @EndDate AND
        (@SearchTerm IS NULL OR 
            Name LIKE '%' + @SearchTerm + '%' OR 
            Description LIKE '%' + @SearchTerm + '%') AND
        (@MinAmount IS NULL OR Amount >= @MinAmount) AND
        (@MaxAmount IS NULL OR Amount <= @MaxAmount);
    
    -- Dynamic sorting with CTE
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = '
    WITH FilteredDateRangeReportsCTE AS (
        SELECT 
            Id, 
            Name, 
            Description,
            Amount,
            Date
        FROM 
            Reports
        WHERE 
            Date BETWEEN ''' + CONVERT(NVARCHAR, @StartDate, 121) + ''' AND ''' + CONVERT(NVARCHAR, @EndDate, 121) + ''' AND
            (' + CASE WHEN @SearchTerm IS NULL THEN '1=1' ELSE 
                'Name LIKE ''%' + @SearchTerm + '%'' OR Description LIKE ''%' + @SearchTerm + '%''' END + ') AND
            (' + CASE WHEN @MinAmount IS NULL THEN '1=1' ELSE 'Amount >= ' + CAST(@MinAmount AS NVARCHAR) END + ') AND
            (' + CASE WHEN @MaxAmount IS NULL THEN '1=1' ELSE 'Amount <= ' + CAST(@MaxAmount AS NVARCHAR) END + ')
    ),
    OrderedReportsCTE AS (
        SELECT 
            *,
            ROW_NUMBER() OVER (ORDER BY ' + 
            CASE 
                WHEN @SortColumn = 'Id' THEN 'Id'
                WHEN @SortColumn = 'Name' THEN 'Name'
                WHEN @SortColumn = 'Description' THEN 'Description'
                WHEN @SortColumn = 'Amount' THEN 'Amount'
                ELSE 'Date'
            END + ' ' + 
            CASE 
                WHEN @SortDirection IN (''ASC'', ''asc'') THEN 'ASC'
                ELSE 'DESC'
            END + ') AS RowNum
        FROM 
            FilteredDateRangeReportsCTE
    )
    
    SELECT 
        Id, 
        Name, 
        Description,
        Amount,
        Date
    FROM 
        OrderedReportsCTE
    WHERE
        RowNum BETWEEN ' + CAST((@PageNumber - 1) * @PageSize + 1 AS NVARCHAR) + ' AND ' + CAST(@PageNumber * @PageSize AS NVARCHAR) + '
    ORDER BY 
        RowNum'
    
    EXEC sp_executesql @SQL;
END
GO
