using EduocationSystem.Features.Enrollment.Commands;
using EduocationSystem.Features.Enrollment.Queries;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EduocationSystem.Features.Enrollment
{
    public static class Endpoints
    {
        public static void MapEnrollmentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/enrollments")
             .WithTags("Enrollments")
             .RequireAuthorization();

            group.MapPost("/", async(
            EnrollStudentCommand command,
            IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOrStudent");

            group.MapGet("/{id:int}", async(
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetEnrollmentByIdQuery(id));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");

            group.MapGet("/student/{studentId:int}", async (
                int studentId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetEnrollmentsByStudentQuery(studentId));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOrStudent");

            group.MapGet("/course/{courseId:int}", async (
                int courseId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetEnrollmentsByCourseQuery(courseId));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");

            group.MapDelete("/{id:int}", async (
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new UnenrollStudentCommand(id));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");
        }
    }
}