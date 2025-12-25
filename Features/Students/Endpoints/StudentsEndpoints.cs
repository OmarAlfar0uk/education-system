using EduocationSystem.Features.Students.Commands;
using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Features.Students.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Endpoints
{
    public static class StudentsEndpoints
    {
        public static void MapStudentsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/v1/students")
                .WithTags("Students")
                .RequireAuthorization();


            group.MapPost("/", async (CreateStudentCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");


            group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetStudentByIdQuery(id));
                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOrStudent");



            group.MapGet("/", async (
            [AsParameters] PaginationParamsDto<StudentSortBy> pagination,
            IMediator mediator) =>
            {
                var result = await mediator.Send(new GetStudentsQuery(pagination));
                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");



            group.MapPut("/{id:int}", async (
            int id,
            UpdateStudentDto dto,
            IMediator mediator) =>
            {
                var result = await mediator.Send(new UpdateStudentCommand(id, dto));
                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOrStudent");


            //------------------------------------------------------------Studeindt-Parent-Linking------------------------------------------------------------//
            group.MapPost("/{studentId:int}/parents/{parentId:int}", async (
            int studentId,
            int parentId,
            IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new LinkParentToStudentCommand(parentId, studentId));

                return Results.Json(result, statusCode: result.StatusCode);
            }).RequireAuthorization("AdminOnly");



        }
    }
}