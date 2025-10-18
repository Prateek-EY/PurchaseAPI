using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Purchase.Infrastructure.Configuration;
using Purchase.Infrastructure.Data;
using PurchaseAPI;
using PurchaseAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<PostgreSettings>>();
var pgSettings = PostgreSettings.FromEnvironmentOrConfig(builder.Configuration,logger);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgSettings.BuildConnectionString()));

builder.Services.AddInfrastructureServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
