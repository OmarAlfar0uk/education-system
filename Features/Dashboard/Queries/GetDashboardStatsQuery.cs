using MediatR;
using EduocationSystem.Features.Dashboard.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Dashboard.Queries
{
    public record GetDashboardStatsQuery : IRequest<ServiceResponse<DashboardStatsDto>>;
}