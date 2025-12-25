using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Commands
{
    public record SubmitExamCommand(int ExamId, SubmitExamDto SubmitExamDto) : IRequest<ServiceResponse<ExamResultDto>>;
}