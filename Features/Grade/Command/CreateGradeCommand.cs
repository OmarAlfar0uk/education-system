using EduocationSystem.Features.Grade.DTOs;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Grade.Command
{
    public record CreateGradeCommand(GradeDto Dto)
       : IRequest<ServiceResponse<GradeDto>>;
}
