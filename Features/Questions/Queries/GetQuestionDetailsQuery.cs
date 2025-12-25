using MediatR;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Queries
{
    public record GetQuestionDetailsQuery(int QuestionId) : IRequest<ServiceResponse<QuestiondetailsDto>>;
}