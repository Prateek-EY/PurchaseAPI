using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace PurchaseAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";

            
                var statusCode = ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    DbUpdateException => (int)HttpStatusCode.InternalServerError,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = statusCode;

                var response = new
                {
                    error = ex switch
                    {
                        DbUpdateException => "Database update failed. Please check the request or try again later.",
                        _ => ex.Message
                    },
                    statusCode = statusCode,
                    detail = _env.IsDevelopment() ? ex.StackTrace : null
                };

                var json = JsonSerializer.Serialize(response);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
