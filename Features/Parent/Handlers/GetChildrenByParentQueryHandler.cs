using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Features.Parent.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Parent.Handlers
{
    public class GetChildrenByParentQueryHandler : IRequestHandler<GetChildrenByParentQuery, ServiceResponse<List<ChildDto>>>
    {
        private readonly IGenericRepository<ParentStudent> _psRepo;

        public GetChildrenByParentQueryHandler(
            IGenericRepository<ParentStudent> psRepo)
        {
            _psRepo = psRepo;
        }

        public async Task<ServiceResponse<List<ChildDto>>> Handle(
            GetChildrenByParentQuery request,
            CancellationToken cancellationToken)
        {
            var children = await _psRepo.GetAll()
                .Where(ps => ps.ParentId == request.ParentId)
                .Select(ps => new ChildDto
                {
                    StudentId = ps.Student.Id,
                    FullName = ps.Student.User.FirstName + " " + ps.Student.User.LastName,
                    Department = ps.Student.Department.Name,
                    Level = ps.Student.Level
                })
                .ToListAsync(cancellationToken);

            return ServiceResponse<List<ChildDto>>.SuccessResponse(children);
        }
    }
}