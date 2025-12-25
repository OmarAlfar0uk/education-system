using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Features.Parent.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Parent.Handlers
{
    public class GetParentByIdQueryHandler : IRequestHandler<GetParentByIdQuery, ServiceResponse<ParentDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Parent> _parentRepo;

        public GetParentByIdQueryHandler(IGenericRepository<Domain.Entities.Parent> parentRepo)
        {
            _parentRepo = parentRepo;
        }

        public async Task<ServiceResponse<ParentDto>> Handle(
            GetParentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var parent = await _parentRepo.GetAll()
                .Where(p => p.Id == request.Id && !EF.Property<bool>(p, "IsDeleted"))
                .Select(p => new ParentDto
                {
                    Id = p.Id,
                    FullName = p.User.FirstName + " " + p.User.LastName,
                    Email = p.User.Email!,
                    Phone = p.User.Phone
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (parent == null)
                return ServiceResponse<ParentDto>.NotFoundResponse(
                    "Parent not found", "ولي الأمر غير موجود");

            return ServiceResponse<ParentDto>.SuccessResponse(parent);
        }
    }
}