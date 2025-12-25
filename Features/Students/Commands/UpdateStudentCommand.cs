using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Commands
{

    public record UpdateStudentCommand(int StudentId, UpdateStudentDto Dto)
        : IRequest<ServiceResponse<StudentDto>>;
}
