using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Students.Queries
{

    public record GetStudentByIdQuery(int Id)
        : IRequest<ServiceResponse<StudentDto>>;
}
