using System.Text.Json.Serialization;
using OneStep.Api.Middleware;
using OneStep.Api.OpenApi;
using OneStep.Api.Services;
using OneStep.Application;
using OneStep.Application.Abstractions;
using OneStep.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (what the app can use) ----------

// Logging: Serilog writes to the console and to daily files in the logs folder.
// Its settings are in appsettings.json under "Serilog".
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

// Each layer registers its own services (database, JWT, policies are in AddInfrastructure).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// The logged-in user, read from the JWT of the current request.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Error handling: every error becomes the same ProblemDetails JSON shape.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Enums travel as text in JSON ("role": "Admin" instead of "role": 2).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
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

// Order matters: the request logger wraps the exception handler,
// so it logs the FINAL status code (e.g. 404), not the raw exception.
app.UseSerilogRequestLogging();      // one log line per request: method, path, status, time
app.UseExceptionHandler();           // turns exceptions into ProblemDetails responses

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // /openapi/v1.json
    app.UseSwaggerUI(options =>      // /swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OneStep API v1");
        options.DocumentTitle = "OneStep API";
        options.EnablePersistAuthorization();   // keep the token after a page refresh
    });
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();             // 1. who are you? (reads and validates the JWT)
app.UseAuthorization();              // 2. are you allowed? (checks [Authorize] and policies)

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
