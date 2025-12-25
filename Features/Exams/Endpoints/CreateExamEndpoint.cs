using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Exams.Commands;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Endpoints
{
    public static class CreateExamEndpoint
    {
        public static void MapCreateExamEndpoint(this WebApplication app)
        {
            app.MapPost("/api/exams", async (HttpContext context, IMediator mediator) =>
            {
                try
                {
                    if (!context.Request.HasFormContentType)
                    {
                        return Results.Json(
                            ServiceResponse<int>.ErrorResponse(
                                "Content-Type must be 'multipart/form-data'",
                                "يجب أن يكون نوع المحتوى 'multipart/form-data'",
                                400
                            ),
                            statusCode: 400
                        );
                    }

                    var form = await context.Request.ReadFormAsync();

                    var title = form["Title"].ToString();
                    var description = form["Description"].ToString();
                    var iconUrl = form["IconUrl"].ToString();
                    var categoryIdStr = form["CategoryId"].ToString();
                    var startDateStr = form["StartDate"].ToString();
                    var endDateStr = form["EndDate"].ToString();
                    var durationStr = form["Duration"].ToString();

                    // Convert values safely
                    int.TryParse(categoryIdStr, out int categoryId);
                    DateTime.TryParse(startDateStr, out DateTime startDate);
                    DateTime.TryParse(endDateStr, out DateTime endDate);
                    int.TryParse(durationStr, out int duration);

                    var dto = new CreateExamDto
                    {
                        Title = title,
                        Description = description,
                        IconUrl = iconUrl,
                        CategoryId = categoryId,
                        StartDate = startDate,
                        EndDate = endDate,
                        Duration = duration
                    };

                    var result = await mediator.Send(new CreateExamCommand(dto));
                    return Results.Json(result, statusCode: result.StatusCode);
                }
                catch (Exception)
                {
                    return Results.Json(
                        ServiceResponse<int>.InternalServerErrorResponse(
                            "Invalid request format",
                            "تنسيق الطلب غير صالح"
                        ),
                        statusCode: 500
                    );
                }
            })
            .DisableAntiforgery()
            .RequireAuthorization("AdminOnly")
            .WithName("CreateExam")
            .WithTags("Exams")
            .Produces<ServiceResponse<int>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<int>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<int>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<int>>(StatusCodes.Status403Forbidden)
            .Produces<ServiceResponse<int>>(StatusCodes.Status500InternalServerError);
        }
    }
}
