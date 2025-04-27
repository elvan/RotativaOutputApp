var builder = WebApplication.CreateBuilder(args);

// Configure EPPlus license globally
OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the ReportService
builder.Services.AddScoped<RotativeOutputApp.Services.IReportService, RotativeOutputApp.Services.ReportService>();

var app = builder.Build();

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
