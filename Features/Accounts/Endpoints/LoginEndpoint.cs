using MediatR;
using Microsoft.AspNetCore.Mvc;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class LoginEndpoint
    {
        public static void MapLoginEndpoint(this WebApplication app)
        {
            app.MapPost("/api/accounts/login", async (LoginReqDTO request, IMediator mediator) =>
            {
                var result = await mediator.Send(new Commands.LoginCommand(request));

                return result;
            })
            .WithName("Login")
            .WithTags("Accounts");
        }

    }
}
