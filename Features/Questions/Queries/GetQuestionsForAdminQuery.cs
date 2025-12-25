// GetQuestionsForAdminQuery.cs
using MediatR;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Questions.Queries
{
    public record GetQuestionsForAdminQuery(QuestionQueryParameters Parameters)
        : IRequest<ServiceResponse<PagedResult<AdminQuestionDto>>>;
}