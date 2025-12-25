using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Queries
{
    public record GetExamDetailsQuery(int Id) : IRequest<ServiceResponse<UserExamDetailsDto>>;
}