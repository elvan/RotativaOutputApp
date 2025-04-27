using RotativeOutputApp.Models;

namespace RotativeOutputApp.Services;

public interface IReportService
{
    List<ReportItem> GetSampleReportData();
}

public class ReportService : IReportService
{
    public List<ReportItem> GetSampleReportData()
    {
        return
        [
            new ReportItem
            {
                Id = 1,
                Name = "Laptop",
                Description = "Dell XPS 15",
                Amount = 1500.00m,
                Date = DateTime.Now.AddDays(-10)
            },

            new ReportItem
            {
                Id = 2,
                Name = "Mouse",
                Description = "Logitech MX Master",
                Amount = 99.99m,
                Date = DateTime.Now.AddDays(-8)
            },

            new ReportItem
            {
                Id = 3,
                Name = "Keyboard",
                Description = "Keychron K2",
                Amount = 89.99m,
                Date = DateTime.Now.AddDays(-8)
            },

            new ReportItem
            {
                Id = 4,
                Name = "Monitor",
                Description = "LG 27-inch 4K",
                Amount = 349.99m,
                Date = DateTime.Now.AddDays(-5)
            },

            new ReportItem
            {
                Id = 5,
                Name = "Headphones",
                Description = "Sony WH-1000XM4",
                Amount = 279.99m,
                Date = DateTime.Now.AddDays(-2)
            }
        ];
    }
}