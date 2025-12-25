using EduocationSystem.Features.Grade.Command;
using EduocationSystem.Features.Grade.Queries;
using MediatR;

namespace EduocationSystem.Features.Grade
{
    public static class GradesEndpoints
    {
        public static void MapGradesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/grades")
            .WithTags("Grades")
            .RequireAuthorization();


            group.MapPost("/", async (
            AddGradeCommand command,
            IMediator mediator) =>
                    {
                        var result = await mediator.Send(command);
                        return Results.Json(result, statusCode: result.StatusCode);
                    }) .RequireAuthorization("AdminOnly");


            group.MapPut("/{id:int}", async (
            int id,
            UpdateGradeRequest body,
            IMediator mediator) =>
                    {
                        var result = await mediator.Send(
                            new UpdateGradeCommand(
                                id,
                                body.AssessmentType,
                                body.Score));

                        return Results.Json(result, statusCode: result.StatusCode);
                    }).RequireAuthorization("AdminOnly");


            group.MapGet("/enrollment/{enrollmentId:int}", async (
             int enrollmentId,
             IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetGradesByEnrollmentQuery(enrollmentId));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOrStudent");

        }
    }

    public record UpdateGradeRequest(
        string AssessmentType,
        decimal Score
    );
}

