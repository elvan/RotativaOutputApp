USE ReportDB;
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
    
    -- Convert date parameters to string format
    DECLARE @StartDateStr NVARCHAR(30) = CONVERT(NVARCHAR, @StartDate, 121);
    DECLARE @EndDateStr NVARCHAR(30) = CONVERT(NVARCHAR, @EndDate, 121);

    -- Build and execute dynamic SQL
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = N'
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
            Date BETWEEN ''' + @StartDateStr + ''' AND ''' + @EndDateStr + ''' AND
            (' + @SearchCondition + ') AND
            (' + @MinAmountCondition + ') AND
            (' + @MaxAmountCondition + ')
    ),
    OrderedReportsCTE AS (
        SELECT 
            *,
            ROW_NUMBER() OVER (ORDER BY ' + @OrderByClause + ') AS RowNum
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
        RowNum';
    
    EXEC sp_executesql @SQL;
END
GO
