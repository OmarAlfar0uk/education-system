using MediatR;
using EduocationSystem.Features.UserAnswers.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.UserAnswers.Queries
{
    public record GetUserAnswerDetailQuery(int AttemptId) : IRequest<ServiceResponse<UserAnswerDetailDto>>;
}