using EduocationSystem.Features.Attendance.Commands;
using EduocationSystem.Features.Attendance.Queries;
using MediatR;

namespace EduocationSystem.Features.Attendance.Endpoints
{

    public static class AttendanceEndpoints
    {
        public static void MapAttendanceEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/attendance")
                .WithTags("Attendance")
                .RequireAuthorization();

            group.MapPost("/", async (
                MarkAttendanceCommand command,
                IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapPut("/{id:int}", async (
                int id,
                string status,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new UpdateAttendanceCommand(id, status));

                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/student/{studentId:int}", async (
                int studentId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetAttendanceByStudentQuery(studentId));

                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/week/{weekId:int}", async (
                int weekId,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetAttendanceByWeekQuery(weekId));

                return Results.Json(result, statusCode: result.StatusCode);
            });

            group.MapGet("/enrollment/{enrollmentId:int}", async (
            int enrollmentId,
            IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetAttendanceByEnrollmentQuery(enrollmentId));

                return Results.Json(result, statusCode: result.StatusCode);
            });

        }
    }
}