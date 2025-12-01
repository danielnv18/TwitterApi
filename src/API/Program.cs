using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using TwitterCloneApi.API.Endpoints;
using TwitterCloneApi.Application;
using TwitterCloneApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
// Controllers removed - using Minimal API endpoints instead

// Add Problem Details for better error responses
builder.Services.AddProblemDetails();

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

// Add HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Add OpenAPI documentation (.NET 10 built-in support)
builder.Services.AddOpenApi();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured")))
    };
});

// Add Authorization
builder.Services.AddAuthorization();

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

// Use built-in exception handler for Minimal API
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        if (exception == null)
        {
            return;
        }

        var (statusCode, message) = exception switch
        {
            TwitterCloneApi.Application.Common.Exceptions.ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            TwitterCloneApi.Application.Common.Exceptions.NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message),
            TwitterCloneApi.Application.Common.Exceptions.UnauthorizedException unauthorized => (StatusCodes.Status401Unauthorized, unauthorized.Message),
            TwitterCloneApi.Application.Common.Exceptions.ConflictException conflict => (StatusCodes.Status409Conflict, conflict.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = statusCode,
            Message = message
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map Minimal API endpoints
app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapHealthEndpoints();

app.Run();

public partial class Program { }

