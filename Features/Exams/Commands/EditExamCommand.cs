using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Commands
{
    public record EditExamCommand(EditExamDto EditExamDto) : IRequest<ServiceResponse<bool>>;
}
