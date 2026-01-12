using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Services;
using SCIMetricAPI.Services.Implementations;
using SCIMetricAPI.Services.Interfaces;
using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();


builder.Services.AddSwaggerGen();
// Configure MySQL database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 31))));

// Register the  repository
builder.Services.AddScoped<IBoaviztaRepository, BoaviztaRepository>();
builder.Services.AddScoped<IGridEmissionRepository, GridEmissionRepository>();
builder.Services.AddScoped<ITdpCoefficientRepository, TdpCoefficientRepository>();
builder.Services.AddScoped<ISciCalculatorService, SciCalculatorService>();
builder.Services.AddScoped<ITdpDataRepository, TdpDataRepository>();
builder.Services.AddScoped<ISciCloudCalculatorService, SciCloudCalculatorService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ISciComputationService, SciComputationService>();
builder.Services.AddScoped<IInstanceOptimizationService, InstanceOptimizationService>();
builder.Services.AddScoped<IAzureRetailPriceService, AzureRetailPriceService>();
builder.Services.AddScoped<ICloudProviderLookupService, CloudProviderLookupService>();
builder.Services.AddScoped<IRegionLookupService, RegionLookupService>();
builder.Services.AddScoped<IInstanceTypeLookupService, InstanceTypeLookupService>();

// New optimization orchestrator
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles(); // Enables serving files from wwwroot
// Serve React build from wwwroot/Frontend/dist
var reactAppPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist");
//Console.WriteLine($"[DEBUG] React build path: {reactAppPath}");

if (Directory.Exists(reactAppPath))
{
   // Console.WriteLine("[DEBUG] React build folder found. Serving static files...");
    //Console.WriteLine($"[DEBUG] React build path: {reactAppPath}");
    // app.UseDefaultFiles(new DefaultFilesOptions
    // {
    //     FileProvider = new PhysicalFileProvider(reactAppPath)
    // });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(reactAppPath),
        RequestPath = ""
    });
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(reactAppPath)
    });
}
else
{
    Console.WriteLine("[ERROR] React build folder NOT found at: " + reactAppPath);
}
app.UseSwagger();

app.UseCors("AllowAll");
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
