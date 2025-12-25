using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Commands
{
    public record CreateStudentCommand(CreateStudentDto Dto)
     : IRequest<ServiceResponse<StudentDto>>;
}
