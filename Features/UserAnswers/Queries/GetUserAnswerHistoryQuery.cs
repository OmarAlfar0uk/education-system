using MediatR;
using EduocationSystem.Features.UserAnswers.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.UserAnswers.Queries
{
    public record GetUserAnswerHistoryQuery : IRequest<ServiceResponse<PagedUserAnswerHistoryDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SortBy { get; init; }
    }
}