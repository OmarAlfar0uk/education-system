using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Exams.Dtos;
using EduocationSystem.Features.Exams.Queries;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Exams.Handlers
{
    public class GetExamDetailsQueryHandler : IRequestHandler<GetExamDetailsQuery, ServiceResponse<UserExamDetailsDto>>
    {
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public GetExamDetailsQueryHandler(IGenericRepository<Exam> examRepository, IHttpContextAccessor httpContextAccessor)
        {

            _examRepository = examRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<UserExamDetailsDto>> Handle(GetExamDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return ServiceResponse<UserExamDetailsDto>.UnauthorizedResponse(
                        "Authentication required",
                        "مطلوب مصادقة"
                    );
                }

                var exam = await _examRepository.GetByIdAsync(request.Id);
                if (exam == null || exam.IsDeleted || !exam.IsActive)
                {
                    return ServiceResponse<UserExamDetailsDto>.NotFoundResponse(
                        "Exam not found",
                        "الامتحان غير موجود"
                    );
                }

                // Check if exam is available (within date range)
                if (DateTime.UtcNow < exam.StartDate || DateTime.UtcNow > exam.EndDate)
                {
                    return ServiceResponse<UserExamDetailsDto>.ErrorResponse(
                        "Exam is not available at this time",
                        "الامتحان غير متاح في هذا الوقت",
                        400
                    );
                }

                var examDto = new UserExamDetailsDto
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    IconUrl = exam.IconUrl,
                    CategoryName = exam.Category?.Title ?? "Unknown",
                    StartDate = exam.StartDate,
                    EndDate = exam.EndDate,
                    Duration = exam.Duration,
                    Description = exam.Description
                };

                return ServiceResponse<UserExamDetailsDto>.SuccessResponse(
                    examDto,
                    "Exam details retrieved successfully",
                    "تم استرجاع تفاصيل الامتحان بنجاح"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<UserExamDetailsDto>.InternalServerErrorResponse(
                    "An error occurred while retrieving exam details",
                    "حدث خطأ أثناء استرجاع تفاصيل الامتحان"
                );
            }
        }
    }
}