using EduocationSystem.Features.Grade.DTOs;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Grade.Queries
{
    public record GetGradesByEnrollmentQuery(int EnrollmentId)
       : IRequest<ServiceResponse<List<GradeDto>>>;
}
