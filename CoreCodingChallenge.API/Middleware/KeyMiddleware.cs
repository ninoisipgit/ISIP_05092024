using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "Authorization";
    private const string ValidApiKey = "your_static_valid_api_key_here"; // Change this to your valid API key

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    public async Task Invoke(HttpContext context)
    {
        //var apiKeyHeaderValue = context.Request.Headers[ApiKeyHeader];

        var apiKeyHeaderValue  = "your_static_valid_api_key_here";
        // Check if the API key is provided in the request header
        if (string.IsNullOrEmpty(apiKeyHeaderValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key is missing.");
            return;
        }

        // Check if the provided API key is valid
        if (!string.Equals(apiKeyHeaderValue, ValidApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        // API key is valid, proceed with the request pipeline
        await _next(context);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline
public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
