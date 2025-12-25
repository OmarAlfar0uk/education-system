using MediatR;
using EduocationSystem.Shared.Responses;
using EduocationSystem.Features.Exams.Dtos;

namespace EduocationSystem.Features.Exams.Commands
{
    public record DeleteExamCommand(DeleteExamDto DeleteExamDto) : IRequest<ServiceResponse<bool>>;
}
