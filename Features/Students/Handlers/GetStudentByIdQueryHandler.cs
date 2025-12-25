using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Students.Dtos;
using EduocationSystem.Features.Students.Queries;
using EduocationSystem.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduocationSystem.Features.Students.Handlers
{
    public class GetStudentByIdQueryHandler
    : IRequestHandler<GetStudentByIdQuery, ServiceResponse<StudentDto>>
    {
        private readonly IGenericRepository<Student> _studentRepo;

        public GetStudentByIdQueryHandler(IGenericRepository<Student> studentRepo)
        {
            _studentRepo = studentRepo;
        }

        public async Task<ServiceResponse<StudentDto>> Handle(
            GetStudentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var student = await _studentRepo.GetAll()
                .Where(s => s.Id == request.Id && !EF.Property<bool>(s, "IsDeleted"))
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email!,
                    Department = s.Department.Name,
                    Level = s.Level
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (student == null)
                return ServiceResponse<StudentDto>.NotFoundResponse(
                    "Student not found",
                    "الطالب غير موجود"
                );

            return ServiceResponse<StudentDto>.SuccessResponse(student);
        }
    }

}
