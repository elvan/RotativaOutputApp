using RotativaOutputApp.Models;
using System;
using System.Collections.Generic;

namespace RotativaOutputApp.Data.Repositories
{
    /// <summary>
    /// Pagination results model
    /// </summary>
    public class PaginatedResult<T>
    {
        public PaginatedResult()
        {
            Items = new List<T>();
        }

        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Interface for ReportItem repository operations
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Gets all report items with pagination, sorting, filtering, and searching
        /// </summary>
        /// <returns>Paginated collection of ReportItem</returns>
        PaginatedResult<ReportItem> GetAllReports(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? sortColumn = "Date", 
            string? sortDirection = "DESC", 
            string? searchTerm = null, 
            decimal? minAmount = null, 
            decimal? maxAmount = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
        
        /// <summary>
        /// Gets a report by ID
        /// </summary>
        /// <param name="id">The ID of the report to retrieve</param>
        /// <returns>The ReportItem if found, otherwise null</returns>
        ReportItem? GetReportById(int id);
        
        /// <summary>
        /// Gets reports within a specific date range with pagination, sorting, filtering, and searching
        /// </summary>
        /// <returns>Paginated collection of ReportItems filtered by date range</returns>
        PaginatedResult<ReportItem> GetReportsByDateRange(
            DateTime startDate, 
            DateTime endDate, 
            int pageNumber = 1, 
            int pageSize = 10, 
            string? sortColumn = "Date", 
            string? sortDirection = "ASC", 
            string? searchTerm = null, 
            decimal? minAmount = null, 
            decimal? maxAmount = null);
    }
}
