using RotativaOutputApp.Models;
using RotativaOutputApp.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RotativaOutputApp.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Gets all report data with pagination, sorting, filtering, and searching capabilities
        /// </summary>
        PaginatedResult<ReportItem> GetSampleReportData(
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
        /// Gets report by ID
        /// </summary>
        ReportItem? GetReportById(int id);
        
        /// <summary>
        /// Gets reports within a date range with pagination, sorting, filtering, and searching
        /// </summary>
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

    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        }

        /// <summary>
        /// Gets all reports with pagination, sorting, filtering, and searching
        /// </summary>
        public PaginatedResult<ReportItem> GetSampleReportData(
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
            try
            {
                return _reportRepository.GetAllReports(
                    pageNumber, 
                    pageSize, 
                    sortColumn, 
                    sortDirection, 
                    searchTerm, 
                    minAmount, 
                    maxAmount, 
                    startDate, 
                    endDate);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                Console.WriteLine($"Error retrieving reports: {ex.Message}");
                
                // Fallback to sample data if database access fails
                return new PaginatedResult<ReportItem> 
                { 
                    Items = GetFallbackData(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = GetFallbackData().Count
                };
            }
        }

        /// <summary>
        /// Gets a report by ID
        /// </summary>
        public ReportItem? GetReportById(int id)
        {
            try
            {
                return _reportRepository.GetReportById(id);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                Console.WriteLine($"Error retrieving report with ID {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets reports within a date range with pagination, sorting, filtering, and searching
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
            try
            {
                return _reportRepository.GetReportsByDateRange(
                    startDate, 
                    endDate, 
                    pageNumber, 
                    pageSize, 
                    sortColumn, 
                    sortDirection, 
                    searchTerm, 
                    minAmount, 
                    maxAmount);
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                Console.WriteLine($"Error retrieving reports by date range: {ex.Message}");
                
                // Fallback to sample data with pagination
                return new PaginatedResult<ReportItem> 
                { 
                    Items = new List<ReportItem>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
        }

        /// <summary>
        /// Provides fallback data if database access fails
        /// </summary>
        private List<ReportItem> GetFallbackData()
        {
            return new List<ReportItem>
            {
                new ReportItem
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "Dell XPS 15 (Fallback Data)",
                    Amount = 1500.00m,
                    Date = DateTime.Now.AddDays(-10)
                },
                new ReportItem
                {
                    Id = 2,
                    Name = "Mouse",
                    Description = "Logitech MX Master (Fallback Data)",
                    Amount = 99.99m,
                    Date = DateTime.Now.AddDays(-8)
                }
            };
        }
    }
}
