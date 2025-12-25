using EduocationSystem.Shared.Responses;
using System.Text.Json;

namespace EduocationSystem.Middlewares
{
    public class FormContentTypeValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public FormContentTypeValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("This request does not have a Content-Type header") ||
                                                     ex.Message.Contains("Forms are available from requests with bodies"))
            {
                // Create a ServiceResponse for the error
                var response = new ServiceResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid Content-Type",
                    MessageAr = "نوع المحتوى غير صالح",
                    Errors = new List<string> { "The request must have a Content-Type header of 'application/x-www-form-urlencoded' or 'multipart/form-data'." },
                    StatusCode = 400,
                    Timestamp = DateTime.UtcNow
                };

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        }
    }
}
