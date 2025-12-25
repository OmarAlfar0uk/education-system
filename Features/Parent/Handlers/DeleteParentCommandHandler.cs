using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Parent.Commands;
using EduocationSystem.Shared.Responses;
using MediatR;

namespace EduocationSystem.Features.Parent.Handlers
{
    public class DeleteParentCommandHandler : IRequestHandler<DeleteParentCommand, ServiceResponse<bool>>
    {
        private readonly IGenericRepository<Domain.Entities.Parent> _parentRepo;
        private readonly IUnitOfWork _uow;

        public DeleteParentCommandHandler(
            IGenericRepository<Domain.Entities.Parent> parentRepo,
            IUnitOfWork uow)
        {
            _parentRepo = parentRepo;
            _uow = uow;
        }

        public async Task<ServiceResponse<bool>> Handle(
            DeleteParentCommand request,
            CancellationToken cancellationToken)
        {
            var parent = await _parentRepo.GetByIdAsync(request.ParentId);

            if (parent == null)
                return ServiceResponse<bool>.NotFoundResponse(
                    "Parent not found",
                    "ولي الأمر غير موجود"
                );

            _parentRepo.Delete(parent); // Soft delete
            await _uow.SaveChangesAsync();

            return ServiceResponse<bool>.SuccessResponse(
                true,
                "Parent deleted successfully",
                "تم حذف ولي الأمر بنجاح"
            );
        }
    }
}
