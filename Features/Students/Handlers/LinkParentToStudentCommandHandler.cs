using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Students.Commands;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Students.Handlers
{
    public class LinkParentToStudentCommandHandler : IRequestHandler<LinkParentToStudentCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<ParentStudent> _repo;
        private readonly IUnitOfWork _uow;

        public LinkParentToStudentCommandHandler(
            IGenericRepository<ParentStudent> repo,
            IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
            LinkParentToStudentCommand request,
            CancellationToken cancellationToken)
        {
            var exists = await _repo.GetAll()
                .AnyAsync(ps =>
                    ps.ParentId == request.ParentId &&
                    ps.StudentId == request.StudentId,
                    cancellationToken);

            if (exists)
                return ServiceResponse<bool>.ConflictResponse(
                    "Parent already linked to this student",
                    "ولي الأمر مرتبط بالفعل بهذا الطالب");

            await _repo.AddAsync(new ParentStudent
            {
                ParentId = request.ParentId,
                StudentId = request.StudentId
            });

            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(true,
                "Parent linked to student successfully",
                "تم ربط ولي الأمر بالطالب بنجاح");
        }
    }
}