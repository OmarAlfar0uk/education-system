using MediatR;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Commands
{
    public record AddQuestionCommand(AddQuestionDto QuestionDto) : IRequest<ServiceResponse<QuestiondataDto>>;
}