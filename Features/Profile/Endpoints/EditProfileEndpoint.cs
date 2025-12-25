using MediatR;
using EduocationSystem.Features.Profile.Commands;
using EduocationSystem.Features.Profile.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Profile.Endpoints
{
    public static class UpdateProfileEndpoint
    {
        public static void MapUpdateProfileEndpoint(this WebApplication app)
        {
            app.MapPut("/api/profile", async (UpdateProfileDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new UpdateProfileCommand(request));
                return Results.Json(result, statusCode: result.StatusCode);
            })
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .WithTags("Profile");
        }
    }
}