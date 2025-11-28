using Scalar.AspNetCore;
using Serilog;
using TwitterCloneApi.API.Middleware;
using TwitterCloneApi.Application;
using TwitterCloneApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Configure URL routing (lowercase, kebab-case)
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add Application services
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add OpenAPI documentation (.NET 10 built-in support)
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();

// Global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

