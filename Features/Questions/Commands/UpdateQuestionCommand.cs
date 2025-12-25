using MediatR;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Commands
{
    public record UpdateQuestionCommand(int QuestionId, UpdateQuestionDto QuestionDto)
        : IRequest<ServiceResponse<QuestiondataDto>>;
}