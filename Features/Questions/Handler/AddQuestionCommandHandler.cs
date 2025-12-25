using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Questions.Handlers
{
    public class AddQuestionCommandHandler : IRequestHandler<AddQuestionCommand, ServiceResponse<QuestiondataDto>>
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public AddQuestionCommandHandler(
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Choice> choiceRepository,
            IGenericRepository<Exam> examRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _choiceRepository = choiceRepository;
            _examRepository = examRepository;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<QuestiondataDto>> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
        {
            // Check if user is authenticated and is Admin
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
            {
                return ServiceResponse<QuestiondataDto>.ForbiddenResponse(
                    "Access forbidden. Admin role required.",
                    "الوصول ممنوع. مطلوب دور المسؤول."
                );
            }

            if (request.QuestionDto == null)
            {
                return ServiceResponse<QuestiondataDto>.ErrorResponse(
                    "Question data is required",
                    "مطلوب بيانات السؤال",
                    400
                );
            }

            try
            {
                // Validate the question DTO structure first
                var dtoValidationResult = ValidateQuestionDto(request.QuestionDto);
                if (!dtoValidationResult.IsValid)
                {
                    return ServiceResponse<QuestiondataDto>.ErrorResponse(
                        dtoValidationResult.ErrorMessage!,
                        dtoValidationResult.ErrorMessageAr!,
                        400
                    );
                }

                // Validate exam exists
                var examExists = await _examRepository.GetAll()
                    .AnyAsync(e => e.Id == request.QuestionDto.ExamId && !e.IsDeleted, cancellationToken);

                if (!examExists)
                {
                    return ServiceResponse<QuestiondataDto>.NotFoundResponse(
                        "Exam not found or has been deleted",
                        "الامتحان غير موجود أو تم حذفه"
                    );
                }

                // Validate choices based on question type
                var choicesValidationResult = ValidateChoices(request.QuestionDto);
                if (!choicesValidationResult.IsValid)
                {
                    return ServiceResponse<QuestiondataDto>.ErrorResponse(
                        choicesValidationResult.ErrorMessage!,
                        choicesValidationResult.ErrorMessageAr!,
                        400
                    );
                }

                // REMOVED: await _unitOfWork.BeginTransactionAsync();

                // Create question
                var question = new Question
                {
                    Title = request.QuestionDto.Title.Trim(),
                    Type = request.QuestionDto.Type,
                    ExamId = request.QuestionDto.ExamId,
                    CreatedAt = DateTime.UtcNow
                };

                await _questionRepository.AddAsync(question);
                await _unitOfWork.SaveChangesAsync();

                // Create choices
                var choices = request.QuestionDto.Choices.Select(choiceDto => new Choice
                {
                    Text = choiceDto.Text.Trim(),
                    IsCorrect = choiceDto.IsCorrect,
                    QuestionId = question.Id,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _choiceRepository.AddRangeAsync(choices);
                await _unitOfWork.SaveChangesAsync();

                // REMOVED: await _unitOfWork.CommitTransactionAsync();

                // Get the choices with their actual IDs from the database
                var savedChoices = await _choiceRepository.GetAll()
                    .Where(c => c.QuestionId == question.Id)
                    .ToListAsync(cancellationToken);

                // Prepare response
                var responseDto = new QuestiondataDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Type = question.Type.ToString(),
                    ExamId = question.ExamId,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    Choices = savedChoices.Select(c => new ChoicedataDto
                    {
                        Id = c.Id,
                        Text = c.Text,
                        IsCorrect = c.IsCorrect,
                        QuestionId = c.QuestionId,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    }).ToList()
                };

                return ServiceResponse<QuestiondataDto>.SuccessResponse(
                    responseDto,
                    "Question created successfully",
                    "تم إنشاء السؤال بنجاح"
                );
            }
            catch (Exception ex)
            {
                // REMOVED: await _unitOfWork.RollbackTransactionAsync();
                // The middleware will handle rollback automatically

                // Log the actual error
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return ServiceResponse<QuestiondataDto>.InternalServerErrorResponse(
                    $"An error occurred while creating the question: {ex.Message}",
                    $"حدث خطأ أثناء إنشاء السؤال: {ex.Message}"
                );
            }
        }
        private (bool IsValid, string? ErrorMessage, string? ErrorMessageAr) ValidateQuestionDto(AddQuestionDto questionDto)
        {
            var errors = new List<string>();
            var errorsAr = new List<string>();

            // Validate Title
            if (string.IsNullOrWhiteSpace(questionDto.Title))
            {
                errors.Add("Question title is required");
                errorsAr.Add("عنوان السؤال مطلوب");
            }
            else if (questionDto.Title.Trim().Length > 500)
            {
                errors.Add("Question title cannot exceed 500 characters");
                errorsAr.Add("عنوان السؤال لا يمكن أن يتجاوز 500 حرف");
            }

            // Validate Type
            if (!Enum.IsDefined(typeof(Domain.Enums.QuestionType), questionDto.Type))
            {
                errors.Add("Invalid question type. Valid values: MultipleChoice (1) or multipleSelect (2)");
                errorsAr.Add("نوع السؤال غير صالح. القيم المسموحة: MultipleChoice (1) أو multipleSelect (2)");
            }

            // Validate ExamId
            if (questionDto.ExamId <= 0)
            {
                errors.Add("Exam ID must be a positive number");
                errorsAr.Add("معرف الامتحان يجب أن يكون رقم موجب");
            }

            // Validate Choices
            if (questionDto.Choices == null || !questionDto.Choices.Any())
            {
                errors.Add("At least one choice is required");
                errorsAr.Add("مطلوب اختيار واحد على الأقل");
            }
            else if (questionDto.Choices.Count < 2)
            {
                errors.Add("At least two choices are required");
                errorsAr.Add("مطلوب اختياران على الأقل");
            }
            else if (questionDto.Choices.Count > 10)
            {
                errors.Add("Cannot have more than 10 choices");
                errorsAr.Add("لا يمكن أن يكون هناك أكثر من 10 اختيارات");
            }
            else
            {
                // Validate each choice
                for (int i = 0; i < questionDto.Choices.Count; i++)
                {
                    var choice = questionDto.Choices[i];

                    if (string.IsNullOrWhiteSpace(choice.Text))
                    {
                        errors.Add($"Choice {i + 1}: Text is required");
                        errorsAr.Add($"الاختيار {i + 1}: النص مطلوب");
                    }
                    else if (choice.Text.Trim().Length > 500)
                    {
                        errors.Add($"Choice {i + 1}: Text cannot exceed 500 characters");
                        errorsAr.Add($"الاختيار {i + 1}: النص لا يمكن أن يتجاوز 500 حرف");
                    }
                }
            }

            if (errors.Any())
            {
                return (false, string.Join("; ", errors), string.Join("; ", errorsAr));
            }

            return (true, null, null);
        }

        private (bool IsValid, string? ErrorMessage, string? ErrorMessageAr) ValidateChoices(AddQuestionDto questionDto)
        {
            if (questionDto.Choices == null || !questionDto.Choices.Any())
                return (false, "Choices are required", "الاختيارات مطلوبة");

            // Check for duplicate choice texts
            var duplicateTexts = questionDto.Choices
                .GroupBy(c => c.Text.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTexts.Any())
            {
                return (false,
                    $"Duplicate choice texts are not allowed: {string.Join(", ", duplicateTexts)}",
                    $"لا يسمح بنصوص الاختيارات المكررة: {string.Join(", ", duplicateTexts)}");
            }

            // Validate based on question type
            var correctChoicesCount = questionDto.Choices.Count(c => c.IsCorrect);

            switch (questionDto.Type)
            {
                case Domain.Enums.QuestionType.MultipleChoice:
                    if (correctChoicesCount != 1)
                    {
                        return (false,
                            "Multiple choice questions must have exactly one correct answer",
                            "أسئلة الاختيار من متعدد يجب أن تحتوي على إجابة صحيحة واحدة بالضبط");
                    }
                    break;

                case Domain.Enums.QuestionType.multipleSelect:
                    if (correctChoicesCount < 1)
                    {
                        return (false,
                            "Multiple select questions must have at least one correct answer",
                            "أسئلة الاختيار المتعدد يجب أن تحتوي على إجابة صحيحة واحدة على الأقل");
                    }
                    break;

                default:
                    return (false, "Invalid question type", "نوع السؤال غير صالح");
            }

            return (true, null, null);
        }
    }
}