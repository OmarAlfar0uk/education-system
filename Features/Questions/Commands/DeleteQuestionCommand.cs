using MediatR;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Commands
{
    public record DeleteQuestionCommand(int QuestionId) : IRequest<ServiceResponse<bool>>;
}