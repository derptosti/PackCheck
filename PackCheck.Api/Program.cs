using Microsoft.EntityFrameworkCore;
using PackCheck.Api.Data;
using PackCheck.Api.Infrastructure;
using PackCheck.Api.Repositories;
using PackCheck.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<PackCheckDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PackCheck")));
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IPackingItemRepository, PackingItemRepository>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IPackingItemService, PackingItemService>();
builder.Services.Configure<CustomVisionOptions>(builder.Configuration.GetSection("CustomVision"));
builder.Services.AddHttpClient<IImageRecognitionService, CustomVisionService>(client =>
    client.Timeout = TimeSpan.FromSeconds(30));

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ready", service = "PackCheck.Api" }));

// A small course project starts with an empty persistent database. When the schema
// changes, introduce EF migrations instead of mixing EnsureCreated with migrations.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PackCheckDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program;
