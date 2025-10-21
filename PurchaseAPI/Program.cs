using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Purchase.Core.Interfaces;
using Purchase.Infrastructure.Configuration;
using Purchase.Infrastructure.Data;
using Purchase.Infrastructure.Services;
using PurchaseAPI;
using PurchaseAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<PostgreSettings>>();
var pgSettings = PostgreSettings.FromEnvironmentOrConfig(builder.Configuration,logger);
logger.LogInformation($"Environment : {builder.Environment.EnvironmentName}");
logger.LogInformation($"Configuration : {pgSettings.Host}");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgSettings.BuildConnectionString()));

builder.Services.Configure<ExchangeRatesSettings>(
    builder.Configuration.GetSection("ExchangeRates"));
builder.Services.AddHttpClient();

builder.Services.AddInfrastructureServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



string certPath;
string certPassword;

if (builder.Environment.IsDevelopment())
{
    certPath = builder.Configuration["Kestrel:Certificates:Default:Path"] ?? "certs/Certificates.p12";
    certPassword = builder.Configuration["Kestrel:Certificates:Default:Password"] ?? "";
    logger.LogInformation("Development environment: using certificate from appsettings");
}
else
{
   logger.LogInformation($"CERT environment: {Environment.GetEnvironmentVariable("CERT_PATH")}");
    certPath = Environment.GetEnvironmentVariable("CERT_PATH") ?? throw new Exception("CERT_PATH env variable not set");
    certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD") ?? "";
    logger.LogInformation("Production environment: using certificate from environment variables");
}

logger.LogInformation("Certificate path: {certPath}", certPath);

if (!File.Exists(certPath))
{
    logger.LogError("Certificate file not found at {certPath}", certPath);
    throw new FileNotFoundException($"Certificate file not found at path: {certPath}");
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps(certPath, certPassword);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
