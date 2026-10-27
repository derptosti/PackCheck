using Microsoft.EntityFrameworkCore;
using PackCheck.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var databasePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "packcheck.db");

builder.Services.AddDbContext<PackCheckDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();