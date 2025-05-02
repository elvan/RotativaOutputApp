using RotativaOutputApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RotativaOutputApp.Data.Repositories
{
    /// <summary>
    /// Repository implementation for accessing report data using ADO.NET
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly IDbContext _dbContext;

        public ReportRepository(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets all reports using a stored procedure with CTE, supporting pagination, sorting, filtering, and searching
        /// </summary>
        public PaginatedResult<ReportItem> GetAllReports(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? sortColumn = "Date", 
            string? sortDirection = "DESC", 
            string? searchTerm = null, 
            decimal? minAmount = null, 
            decimal? maxAmount = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var result = new PaginatedResult<ReportItem>
            {
                Items = new List<ReportItem>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            var parameters = new[]
            {
                new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
                new SqlParameter("@SortColumn", SqlDbType.NVarChar, 50) { Value = sortColumn ?? "Date" },
                new SqlParameter("@SortDirection", SqlDbType.NVarChar, 4) { Value = sortDirection ?? "DESC" },
                new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) { Value = searchTerm == null ? DBNull.Value : (object)searchTerm },
                new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = minAmount.HasValue ? (object)minAmount.Value : DBNull.Value },
                new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = maxAmount.HasValue ? (object)maxAmount.Value : DBNull.Value },
                new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate.HasValue ? (object)startDate.Value : DBNull.Value },
                new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate.HasValue ? (object)endDate.Value : DBNull.Value }
            };
            
            using (var command = _dbContext.CreateStoredProcCommand("usp_GetAllReports", parameters))
            using (var reader = _dbContext.ExecuteReader(command))
            {
                // First resultset has total count
                if (reader.Read())
                {
                    result.TotalCount = reader.GetInt32(0);
                }
                
                // Move to the next resultset which contains the actual data
                reader.NextResult();
                
                // Read items
                while (reader.Read())
                {
                    ((List<ReportItem>)result.Items).Add(MapReportItemFromDataReader(reader));
                }
            }
            
            return result;
        }

        /// <summary>
        /// Gets a report by ID
        /// </summary>
        public ReportItem? GetReportById(int id)
        {
            using (var command = _dbContext.CreateStoredProcCommand("usp_GetReportById", 
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }))
            using (var reader = _dbContext.ExecuteReader(command))
            {
                if (reader.Read())
                {
                    return MapReportItemFromDataReader(reader);
                }
            }
            
            return null; // Explicitly returning null for a nullable return type
        }

        /// <summary>
        /// Gets reports by date range using a stored procedure with CTE
        /// </summary>
        public PaginatedResult<ReportItem> GetReportsByDateRange(
            DateTime startDate, 
            DateTime endDate, 
            int pageNumber = 1, 
            int pageSize = 10, 
            string? sortColumn = "Date", 
            string? sortDirection = "ASC", 
            string? searchTerm = null, 
            decimal? minAmount = null, 
            decimal? maxAmount = null)
        {
            var result = new PaginatedResult<ReportItem>
            {
                Items = new List<ReportItem>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            var parameters = new[]
            {
                new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate },
                new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate },
                new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize },
                new SqlParameter("@SortColumn", SqlDbType.NVarChar, 50) { Value = sortColumn ?? "Date" },
                new SqlParameter("@SortDirection", SqlDbType.NVarChar, 4) { Value = sortDirection ?? "ASC" },
                new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 100) { Value = searchTerm == null ? DBNull.Value : (object)searchTerm },
                new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = minAmount.HasValue ? (object)minAmount.Value : DBNull.Value },
                new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = maxAmount.HasValue ? (object)maxAmount.Value : DBNull.Value }
            };
            
            using (var command = _dbContext.CreateStoredProcCommand("usp_GetReportsByDateRange", parameters))
            using (var reader = _dbContext.ExecuteReader(command))
            {
                // First resultset has total count
                if (reader.Read())
                {
                    result.TotalCount = reader.GetInt32(0);
                }
                
                // Move to the next resultset which contains the actual data
                reader.NextResult();
                
                // Read items
                while (reader.Read())
                {
                    ((List<ReportItem>)result.Items).Add(MapReportItemFromDataReader(reader));
                }
            }
            
            return result;
        }

        /// <summary>
        /// Maps a SQL data reader row to a ReportItem object
        /// </summary>
        private ReportItem MapReportItemFromDataReader(SqlDataReader reader)
        {
            return new ReportItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date"))
            };
        }
    }
}
