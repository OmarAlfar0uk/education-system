using MediatR;
using Microsoft.AspNetCore.Authorization;
using EduocationSystem.Features.Accounts.Commands;
using System.Net;

namespace EduocationSystem.Features.Accounts.Endpoints
{
    public static class TestEndpoint
    {
        public static void MapTestEndpoint(this WebApplication app)
        {
            app.MapGroup("/api")
               .RequireAuthorization()
               .MapGet("/gettest", () => "hello world !");

            //app.MapGet("s", async (IMediator mediator) =>
            //{
            //    await mediator.Send(new LogoutCommand("hello logout"));
            //});

        }
    }
}
