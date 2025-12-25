using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Parent.Commands;
using EduocationSystem.Features.Parent.Dtos;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Parent.Handlers
{
    public class CreateParentCommandHandler
       : IRequestHandler<CreateParentCommand, ServiceResponse<ParentDto>>
    {
        private readonly IGenericRepository<Domain.Entities.Parent> _parentRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;

        public CreateParentCommandHandler(
            IGenericRepository<Domain.Entities.Parent> parentRepo,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow)
        {
            _parentRepo = parentRepo;
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<ServiceResponse<ParentDto>> Handle(
            CreateParentCommand request,
            CancellationToken cancellationToken)
        {
            // ✅ Correct way to get user
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
                return ServiceResponse<ParentDto>.NotFoundResponse(
                    "User not found", "المستخدم غير موجود");

            // ✅ Check if parent already exists
            var exists = await _parentRepo.GetAll()
                .AnyAsync(p => p.UserId == request.UserId, cancellationToken);

            if (exists)
                return ServiceResponse<ParentDto>.ConflictResponse(
                    "Parent already exists", "ولي الأمر موجود بالفعل");

            var parent = new Domain.Entities.Parent
            {
                UserId = request.UserId
            };

            await _parentRepo.AddAsync(parent);
            await _uow.SaveChangesAsync();

            return ServiceResponse<ParentDto>.SuccessResponse(
                new ParentDto
                {
                    Id = parent.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email!,
                    Phone = user.Phone
                },
                "Parent created successfully",
                "تم إنشاء ولي الأمر بنجاح"
            );
        }
    }
}