namespace TwitterCloneApi.API.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () =>
        {
            return Results.Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "TwitterCloneApi",
                version = "1.0.0"
            });
        })
        .WithName("HealthCheck")
        .WithSummary("Health check endpoint")
        .WithTags("Health")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
