using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Style;

using Rotativa.AspNetCore;

using RotativeOutputApp.Services;

using System.Drawing;

namespace RotativeOutputApp.Controllers;

public class ReportController(IReportService reportService) : Controller
{
    public IActionResult Index()
    {
        var reportData = reportService.GetSampleReportData();
        return View(reportData);
    }

    public IActionResult ExportPdf()
    {
        var reportData = reportService.GetSampleReportData();

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

    public IActionResult ExportExcel()
    {
        var reportData = reportService.GetSampleReportData();

        // Set EPPlus license for EPPlus 8 using the new License API
        ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

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