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

    -- For dynamic sorting, prepare the order by clause
    DECLARE @OrderByClause NVARCHAR(100);
    SET @OrderByClause = 
        CASE 
            WHEN @SortColumn = 'Id' THEN 'Id'
            WHEN @SortColumn = 'Name' THEN 'Name'
            WHEN @SortColumn = 'Description' THEN 'Description'
            WHEN @SortColumn = 'Amount' THEN 'Amount'
            ELSE 'Date'
        END + ' ' + 
        CASE 
            WHEN @SortDirection = 'ASC' THEN 'ASC'
            ELSE 'DESC'
        END;

    -- Prepare search condition
    DECLARE @SearchCondition NVARCHAR(500);
    SET @SearchCondition = 
        CASE 
            WHEN @SearchTerm IS NULL THEN '1=1'
            ELSE 'Name LIKE ''%' + @SearchTerm + '%'' OR Description LIKE ''%' + @SearchTerm + '%'''
        END;

    -- Prepare amount conditions
    DECLARE @MinAmountCondition NVARCHAR(100);
    SET @MinAmountCondition = 
        CASE 
            WHEN @MinAmount IS NULL THEN '1=1'
            ELSE 'Amount >= ' + CAST(@MinAmount AS NVARCHAR)
        END;

    DECLARE @MaxAmountCondition NVARCHAR(100);
    SET @MaxAmountCondition = 
        CASE 
            WHEN @MaxAmount IS NULL THEN '1=1'
            ELSE 'Amount <= ' + CAST(@MaxAmount AS NVARCHAR)
        END;

    -- Prepare date conditions
    DECLARE @StartDateCondition NVARCHAR(100);
    SET @StartDateCondition = 
        CASE 
            WHEN @StartDate IS NULL THEN '1=1'
            ELSE 'Date >= ''' + CONVERT(NVARCHAR, @StartDate, 121) + ''''
        END;

    DECLARE @EndDateCondition NVARCHAR(100);
    SET @EndDateCondition = 
        CASE 
            WHEN @EndDate IS NULL THEN '1=1'
            ELSE 'Date <= ''' + CONVERT(NVARCHAR, @EndDate, 121) + ''''
        END;

    -- Build and execute dynamic SQL
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = N'
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
            (' + @SearchCondition + ') AND
            (' + @MinAmountCondition + ') AND
            (' + @MaxAmountCondition + ') AND
            (' + @StartDateCondition + ') AND
            (' + @EndDateCondition + ')
    ),
    OrderedReportsCTE AS (
        SELECT 
            *,
            ROW_NUMBER() OVER (ORDER BY ' + @OrderByClause + ') AS RowNum
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
        RowNum';
    
    EXEC sp_executesql @SQL;
END
GO
