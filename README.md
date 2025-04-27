# Rotative Output App

A .NET Core application that generates custom Excel and PDF reports using EPPlus and Rotativa.AspNetCore.

## Features

- Generate Excel reports with custom formatting
- Generate PDF reports with custom templates
- Web interface to view and download reports

## Setup Instructions

1. Restore NuGet packages: `dotnet restore`
2. **Important**: Download wkhtmltopdf.exe for Rotativa to work
   - Download from: https://wkhtmltopdf.org/downloads.html
   - Place the wkhtmltopdf.exe file in the `wwwroot/Rotativa` directory

## Running the Application

1. Execute `dotnet run` in the project directory
2. Open a browser and navigate to `https://localhost:5000`
3. Navigate to the Reports page and test the Excel/PDF export functionality

## Technical Details

- .NET Core MVC application
- EPPlus for Excel generation
- Rotativa.AspNetCore for PDF generation

## Configuration

For production environments, update the following:
- Configure EPPlus license context in ReportController.cs if using commercially
- Customize PDF templates in Views/Report/PdfReport.cshtml
- Customize Excel template in ReportController.cs ExportExcel method
