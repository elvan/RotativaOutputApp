using RotativaOutputApp.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Configure EPPlus license globally
OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("Aegis Ultima Teknologi");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Database Services
builder.Services.AddScoped<RotativaOutputApp.Data.IDbContext, RotativaOutputApp.Data.SqlDbContext>();
builder.Services.AddScoped<RotativaOutputApp.Data.Repositories.IReportRepository, RotativaOutputApp.Data.Repositories.ReportRepository>();

// Register Application Services
builder.Services.AddScoped<RotativaOutputApp.Services.IReportService, RotativaOutputApp.Services.ReportService>();

// Register and configure DatabaseInitializer
builder.Services.AddTransient<DatabaseInitializer>();

var app = builder.Build();

// Initialize the database (create if not exists, setup tables and stored procedures)
try
{
    // Perform database initialization during application startup
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        dbInitializer.Initialize();
    }
}
catch (SqlException ex)
{
    Console.WriteLine($"Database initialization error: {ex.Message}");
    // Application can continue with fallback data if database is not available
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Configure Rotativa
Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

app.Run();
