using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Grade.Command
{
    public record AddGradeCommand(
         int EnrollmentId,
         string AssessmentType,
         decimal Score
     ) : IRequest<ServiceResponse<int>>;
}
