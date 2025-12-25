using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Categories.Commands;
using EduocationSystem.Features.Categories.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Categories.Endpoints
{
    public static class UpdateCategoryEndpoint
    {
        public static void MapUpdateCategoryEndpoint(this WebApplication app)
        {
            app.MapPut("/api/categories/{id}", async (HttpContext context, int id, IMediator mediator) =>
            {
                try
                {
                    // Check if the request has form content type
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

                    // Check if at least one field is provided
                    var hasTitle = !string.IsNullOrWhiteSpace(title);
                    var hasIcon = icon != null && icon.Length > 0;

                    if (!hasTitle && !hasIcon)
                    {
                        return Results.Json(
                            ServiceResponse<int>.ErrorResponse(
                                "At least one field (Title or Icon) must be provided for update",
                                "يجب تقديم حقل واحد على الأقل (العنوان أو الأيقونة) للتحديث",
                                400
                            ),
                            statusCode: 400
                        );
                    }

                    var updateCategoryDto = new UpdateCategoryDTo
                    {
                        Title = hasTitle ? title : string.Empty, // Only set if provided
                        Icon = hasIcon ? icon : null // Only set if provided
                    };

                    var result = await mediator.Send(new UpdateCategoryCommand(id, updateCategoryDto));
                    return Results.Json(result, statusCode: result.StatusCode);
                }
                catch (Exception ex)
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
            .WithName("UpdateCategory")
            .WithTags("Categories")
            .Produces<ServiceResponse<int>>(StatusCodes.Status200OK)
            .Produces<ServiceResponse<int>>(StatusCodes.Status400BadRequest)
            .Produces<ServiceResponse<int>>(StatusCodes.Status401Unauthorized)
            .Produces<ServiceResponse<int>>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
        }
    }
}