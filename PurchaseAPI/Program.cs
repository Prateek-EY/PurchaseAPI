using Microsoft.EntityFrameworkCore;
using PurchaseAPI.Configuration;
using PurchaseAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var pgSettings = PostgreSettings.FromEnvironmentOrConfig(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgSettings.BuildConnectionString()));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
