using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Infrastructure.Service;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

            app.MapPost("/api/test-email", async (string email, IEmailQueueService queue) =>
            {
                var template = EmailTemplateHelper.LoadTemplate("Welcome.html");

                template = EmailTemplateHelper.ReplaceTokens(template, new()
    {
        {"name", "Test User"}
    });

                await queue.QueueAsync(new EmailQueue
                {
                    ToEmail = email,
                    Subject = "Test Email",
                    Body = template,
                    EmailType = EmailType.Notification
                });

                return Results.Ok("Queued!");
            }).RequireAuthorization("AdminOnly");



        }
    }
}
