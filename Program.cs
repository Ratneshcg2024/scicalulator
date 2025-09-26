using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Services;
using SCIMetricAPI.Services.Implementations;
using SCIMetricAPI.Services.Interfaces;

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
builder.Services.AddScoped<ISciCloudCalculatorService,SciCloudCalculatorService >();

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

app.UseSwagger();

app.UseCors("AllowAll");
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
