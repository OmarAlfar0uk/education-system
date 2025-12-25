using MediatR;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Commands
{
    public record StartExamAttemptCommand(int ExamId) : IRequest<ServiceResponse<List<QuestionDto>>>;
}