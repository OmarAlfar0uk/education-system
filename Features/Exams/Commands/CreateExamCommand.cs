using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Commands
{
    public record CreateExamCommand(CreateExamDto CreateExamDto) : IRequest<ServiceResponse<int>>;

}


