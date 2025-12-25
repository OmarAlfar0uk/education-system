using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Categories.Commands;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Endpoints
{
    public static class CreateCategoryEndpoint
    {
        public static void MapCreateCategoryEndpoint(this WebApplication app)
        {
            app.MapPost("/api/categories", async (HttpContext context, IMediator mediator) =>
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
                    var icon = form.Files["Icon"];

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        return Results.Json(
                            ServiceResponse<int>.ErrorResponse(
                                "Title is required",
                                "العنوان مطلوب",
                                400
                            ),
                            statusCode: 400
                        );
                    }

                    if (icon == null || icon.Length == 0)
                    {
                        return Results.Json(
                            ServiceResponse<int>.ErrorResponse(
                                "Icon file is required",
                                "ملف الأيقونة مطلوب",
                                400
                            ),
                            statusCode: 400
                        );
                    }

                    var createCategoryDto = new createCategoryDTo
                    {
                        Title = title,
                        Icon = icon
                    };

                    var result = await mediator.Send(new CreateCategoryCommand(createCategoryDto));
                    return Results.Json(result, statusCode: result.StatusCode);
                }
                catch
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
.RequireAuthorization("AdminOnly")   // ⭐ هنا الرول
.WithName("CreateCategory")
.WithTags("Categories")
.Produces<ServiceResponse<int>>(StatusCodes.Status200OK)
.Produces<ServiceResponse<int>>(StatusCodes.Status400BadRequest)
.Produces<ServiceResponse<int>>(StatusCodes.Status401Unauthorized)
.ProducesValidationProblem();

        }
    }
}