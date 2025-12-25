using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Grade.Command
{
    public record UpdateGradeCommand(
       int GradeId,
       string AssessmentType,
       decimal Score
   ) : IRequest<ServiceResponse<bool>>;
}
