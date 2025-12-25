using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Parent.Commands;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Handlers
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IUnitOfWork _uow;

        public DeleteStudentCommandHandler(
            IGenericRepository<Student> studentRepo,
            IUnitOfWork uow)
        {
            _studentRepo = studentRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
            DeleteStudentCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _studentRepo.GetByIdAsync(request.StudentId);

            if (student == null)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Student not found",
                    "الطالب غير موجود"
                );

            _studentRepo.Delete(student); // ✅ Soft delete
            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Student deleted successfully",
                "تم حذف الطالب بنجاح"
            );
        }
    }
}
