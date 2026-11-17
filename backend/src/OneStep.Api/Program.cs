using Microsoft.EntityFrameworkCore;

using OneStep.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register the database: ASP.NET Core creates one AppDbContext per HTTP request
// and gives it to any class that asks for it (dependency injection).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();