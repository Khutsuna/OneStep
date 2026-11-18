using OneStep.Api.Middleware;
using OneStep.Application;
using OneStep.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (what the app can use) ----------

// Logging: Serilog writes to the console and to daily files in the logs folder.
// Its settings are in appsettings.json under "Serilog".
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

// Each layer registers its own services.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Error handling: every error becomes the same ProblemDetails JSON shape.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();       // builds the API description (OpenAPI) that Swagger UI shows
builder.Services.AddHealthChecks();

// CORS: the Angular app runs on another port (a different "origin"), so the browser
// blocks its calls unless the API explicitly allows that origin.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

// ---------- Request pipeline (what happens to every request, in this order) ----------

app.UseExceptionHandler();           // first, so it catches errors from everything after it
app.UseSerilogRequestLogging();      // one log line per request: method, path, status, time

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // /openapi/v1.json
    app.UseSwaggerUI(options =>      // /swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OneStep API v1");
        options.DocumentTitle = "OneStep API";
    });
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
