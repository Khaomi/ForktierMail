using System.Net;
using ForktierMail.Database;

namespace ForktierMail.Server.Middleware;

public class APIKeyMiddleware
{
    private readonly RequestDelegate _next;

    public APIKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Query.TryGetValue("apiKey", out var extractedApiKey))
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<APIKeyMiddleware>>();
            logger.LogWarning("API Key is missing in the request.");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API Key is missing.");
            return;
        }

        var db = context.RequestServices.GetRequiredService<ServerDbContext>();
        var apiKeyString = extractedApiKey.ToString();
        var fork = db.Forks.FirstOrDefault(x => x.ApiKey == apiKeyString);

        if (fork is null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<APIKeyMiddleware>>();
            logger.LogWarning("Invalid API Key: {ApiKey}", apiKeyString);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("API key is invalid!");
            return;
        }

        context.Items["ForkId"] = fork.Id;
        context.Items["ForkName"] = fork.Name;

        await _next(context);
    }
}