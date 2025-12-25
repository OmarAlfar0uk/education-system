using System.Net;
using System.Text.Json;

namespace EduocationSystem.Middlewares
{

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var errorObj = new
                {
                    error = ex.Message,
                    details = ex.InnerException?.Message
                };

                var errorJson = JsonSerializer.Serialize(errorObj);

                await context.Response.WriteAsync(errorJson);
            }
        }
    }
}