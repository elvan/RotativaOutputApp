using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Rotativa.AspNetCore;
using RotativaOutputApp.Models;
using RotativaOutputApp.Services;

using System.Drawing;

using RotativaOutputApp.Data.Repositories;

namespace RotativaOutputApp.Controllers;

public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    public IActionResult Index(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortColumn = "Date",
        string? sortDirection = "DESC",
        string? searchTerm = null,
        decimal? minAmount = null,
        decimal? maxAmount = null)
    {
        // Ensure valid pagination parameters
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

        // Store filter, sort, and pagination params in ViewBag for UI
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.SortColumn = sortColumn;
        ViewBag.SortDirection = sortDirection;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.MinAmount = minAmount;
        ViewBag.MaxAmount = maxAmount;

        // Handle reverse sort direction for toggling in UI
        ViewBag.SortDirectionReversed = sortDirection?.ToUpper() == "ASC" ? "DESC" : "ASC";

        PaginatedResult<ReportItem> paginatedData;

        if (startDate.HasValue && endDate.HasValue)
        {
            // If date range is provided, filter by date range with pagination
            paginatedData = _reportService.GetReportsByDateRange(
                startDate.Value,
                endDate.Value,
                pageNumber,
                pageSize,
                sortColumn,
                sortDirection ?? "ASC",
                searchTerm,
                minAmount,
                maxAmount);

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
        }
        else
        {
            // Otherwise get all reports with pagination
            // Set default date range for the filter (last 30 days)
            DateTime defaultStartDate = DateTime.Now.AddDays(-30);
            DateTime defaultEndDate = DateTime.Now;

            ViewBag.StartDate = defaultStartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = defaultEndDate.ToString("yyyy-MM-dd");

            paginatedData = _reportService.GetSampleReportData(
                pageNumber,
                pageSize,
                sortColumn,
                sortDirection ?? "DESC",
                searchTerm,
                minAmount,
                maxAmount,
                startDate,
                endDate);
        }

        return View(paginatedData);
    }

    public IActionResult ExportPdf(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? sortColumn = "Date",
        string? sortDirection = "DESC")
    {
        // Get all data with applied filters and sorting but without pagination (page size = int.MaxValue)
        var paginatedResult = startDate.HasValue && endDate.HasValue
            ? _reportService.GetReportsByDateRange(startDate.Value, endDate.Value, 1, int.MaxValue,
                sortColumn ?? "Date", sortDirection ?? "DESC", searchTerm, minAmount, maxAmount)
            : _reportService.GetSampleReportData(1, int.MaxValue,
                sortColumn ?? "Date", sortDirection ?? "DESC", searchTerm, minAmount, maxAmount, startDate, endDate);

        var reportData = paginatedResult.Items.ToList();

        try
        {
            // Check if wkhtmltopdf.exe exists
            string wkhtmltopdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Rotativa",
                "wkhtmltopdf.exe");
            if (!System.IO.File.Exists(wkhtmltopdfPath))
            {
                // Provide warning message to download wkhtmltopdf
                TempData["ErrorMessage"] =
                    "wkhtmltopdf.exe not found in wwwroot/Rotativa directory. Please download it from https://wkhtmltopdf.org/downloads.html and place it in the wwwroot/Rotativa directory.";
                return RedirectToAction("Index");
            }

            // Using Rotativa to generate PDF
            return new ViewAsPdf("PdfReport", reportData)
            {
                FileName = $"Report-{DateTime.Now:yyyyMMdd}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                CustomSwitches = "--page-offset 0 --footer-center [page] --footer-font-size 8"
            };
        }
        catch (Exception ex)
        {
            // Log the error and redirect back with a message
            TempData["ErrorMessage"] = $"Error generating PDF: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    public IActionResult ExportExcel(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? searchTerm = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        string? sortColumn = "Date",
        string? sortDirection = "DESC")
    {
        // Get all data with applied filters and sorting but without pagination (page size = int.MaxValue)
        var paginatedResult = startDate.HasValue && endDate.HasValue
            ? _reportService.GetReportsByDateRange(startDate.Value, endDate.Value, 1, int.MaxValue,
                sortColumn ?? "Date", sortDirection ?? "DESC", searchTerm, minAmount, maxAmount)
            : _reportService.GetSampleReportData(1, int.MaxValue,
                sortColumn ?? "Date", sortDirection ?? "DESC", searchTerm, minAmount, maxAmount, startDate, endDate);

        var reportData = paginatedResult.Items.ToList();

        // Set EPPlus license for EPPlus 8 using the new License API
        ExcelPackage.License.SetNonCommercialOrganization("Aegis Ultima Teknologi");

        using var package = new ExcelPackage();
        // Add a new worksheet to the workbook
        var worksheet = package.Workbook.Worksheets.Add("Report");

        // Set header style
        using (var range = worksheet.Cells["A1:E1"])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Set headers
        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Cells[1, 3].Value = "Description";
        worksheet.Cells[1, 4].Value = "Amount";
        worksheet.Cells[1, 5].Value = "Date";

        // Fill data
        int row = 2;
        foreach (var item in reportData)
        {
            worksheet.Cells[row, 1].Value = item.Id;
            worksheet.Cells[row, 2].Value = item.Name;
            worksheet.Cells[row, 3].Value = item.Description;
            worksheet.Cells[row, 4].Value = item.Amount;
            worksheet.Cells[row, 5].Value = item.Date;

            // Format cells
            worksheet.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
            worksheet.Cells[row, 5].Style.Numberformat.Format = "yyyy-mm-dd";

            row++;
        }

        // Auto fit columns
        worksheet.Cells.AutoFitColumns();

        // Generate file name
        string fileName = $"Report-{DateTime.Now:yyyyMMdd}.xlsx";

        // Return the Excel file
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var content = package.GetAsByteArray();

        return File(content, contentType, fileName);
    }
}
