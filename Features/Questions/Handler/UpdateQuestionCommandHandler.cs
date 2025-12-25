using MediatR;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Questions.Commands;
using EduocationSystem.Features.Questions.Dtos;
using EduocationSystem.Shared.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduocationSystem.Features.Questions.Handlers
{
    public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, ServiceResponse<QuestiondataDto>>
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Choice> _choiceRepository;
        private readonly IGenericRepository<Exam> _examRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateQuestionCommandHandler(
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

        public async Task<ServiceResponse<QuestiondataDto>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
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
                // Validate that at least one field is being updated
                var hasUpdates = HasUpdates(request.QuestionDto);
                if (!hasUpdates)
                {
                    return ServiceResponse<QuestiondataDto>.ErrorResponse(
                        "At least one field must be updated",
                        "يجب تحديث حقل واحد على الأقل",
                        400
                    );
                }

                // Get the existing question with choices
                var existingQuestion = await _questionRepository.GetAll()
                    .Include(q => q.Choices.Where(c => !c.IsDeleted))
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionId && !q.IsDeleted, cancellationToken);

                if (existingQuestion == null)
                {
                    return ServiceResponse<QuestiondataDto>.NotFoundResponse(
                        "Question not found or has been deleted",
                        "السؤال غير موجود أو تم حذفه"
                    );
                }

                // Validate exam exists if provided
                if (request.QuestionDto.ExamId.HasValue)
                {
                    var examExists = await _examRepository.GetAll()
                        .AnyAsync(e => e.Id == request.QuestionDto.ExamId.Value && !e.IsDeleted, cancellationToken);

                    if (!examExists)
                    {
                        return ServiceResponse<QuestiondataDto>.NotFoundResponse(
                            "Exam not found or has been deleted",
                            "الامتحان غير موجود أو تم حذفه"
                        );
                    }
                }

                // Validate choices if provided
                if (request.QuestionDto.Choices != null)
                {
                    var choicesValidationResult = ValidateChoices(request.QuestionDto);
                    if (!choicesValidationResult.IsValid)
                    {
                        return ServiceResponse<QuestiondataDto>.ErrorResponse(
                            choicesValidationResult.ErrorMessage!,
                            choicesValidationResult.ErrorMessageAr!,
                            400
                        );
                    }
                }

                // Update question fields if provided
                bool questionUpdated = false;

                if (!string.IsNullOrWhiteSpace(request.QuestionDto.Title))
                {
                    existingQuestion.Title = request.QuestionDto.Title.Trim();
                    questionUpdated = true;
                }

                if (request.QuestionDto.Type.HasValue)
                {
                    existingQuestion.Type = request.QuestionDto.Type.Value;
                    questionUpdated = true;
                }

                if (request.QuestionDto.ExamId.HasValue)
                {
                    existingQuestion.ExamId = request.QuestionDto.ExamId.Value;
                    questionUpdated = true;
                }

                if (questionUpdated)
                {
                    existingQuestion.UpdatedAt = DateTime.UtcNow;
                    _questionRepository.Update(existingQuestion);
                }

                // Handle choices updates if provided
                if (request.QuestionDto.Choices != null)
                {
                    await UpdateChoices(existingQuestion.Id, request.QuestionDto.Choices);
                }

                await _unitOfWork.SaveChangesAsync();

                // Reload the question with updated choices (no need to include Exam since we don't use ExamTitle)
                var updatedQuestion = await _questionRepository.GetAll()
                    .Include(q => q.Choices.Where(c => !c.IsDeleted))
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionId && !q.IsDeleted, cancellationToken);

                if (updatedQuestion == null)
                {
                    return ServiceResponse<QuestiondataDto>.NotFoundResponse(
                        "Question not found after update",
                        "لم يتم العثور على السؤال بعد التحديث"
                    );
                }

                // Prepare response using your simple DTO structure
                var responseDto = new QuestiondataDto
                {
                    Id = updatedQuestion.Id,
                    Title = updatedQuestion.Title,
                    Type = updatedQuestion.Type.ToString(),
                    ExamId = updatedQuestion.ExamId,
                    CreatedAt = updatedQuestion.CreatedAt,
                    UpdatedAt = updatedQuestion.UpdatedAt,
                    Choices = updatedQuestion.Choices.Select(c => new ChoicedataDto
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
                    "Question updated successfully",
                    "تم تحديث السؤال بنجاح"
                );
            }
            catch (Exception ex)
            {
                // Log the actual error
                Console.WriteLine($"ERROR updating question: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return ServiceResponse<QuestiondataDto>.InternalServerErrorResponse(
                    $"An error occurred while updating the question: {ex.Message}",
                    $"حدث خطأ أثناء تحديث السؤال: {ex.Message}"
                );
            }
        }

        private bool HasUpdates(UpdateQuestionDto questionDto)
        {
            return !string.IsNullOrWhiteSpace(questionDto.Title) ||
                   questionDto.Type.HasValue ||
                   questionDto.ExamId.HasValue ||
                   questionDto.Choices != null;
        }

        private async Task UpdateChoices(int questionId, List<UpdateChoiceDto> choiceDtos)
        {
            var existingChoices = await _choiceRepository.GetAll()
                .Where(c => c.QuestionId == questionId && !c.IsDeleted)
                .ToListAsync();

            var existingChoicesDict = existingChoices.ToDictionary(c => c.Id);

            foreach (var choiceDto in choiceDtos)
            {
                if (choiceDto.Id.HasValue && existingChoicesDict.TryGetValue(choiceDto.Id.Value, out var existingChoice))
                {
                    // Update existing choice
                    bool choiceUpdated = false;

                    if (!string.IsNullOrWhiteSpace(choiceDto.Text))
                    {
                        existingChoice.Text = choiceDto.Text.Trim();
                        choiceUpdated = true;
                    }

                    if (choiceDto.IsCorrect.HasValue)
                    {
                        existingChoice.IsCorrect = choiceDto.IsCorrect.Value;
                        choiceUpdated = true;
                    }

                    if (choiceUpdated)
                    {
                        existingChoice.UpdatedAt = DateTime.UtcNow;
                        _choiceRepository.Update(existingChoice);
                    }

                    // Remove from dictionary to track which choices were updated
                    existingChoicesDict.Remove(choiceDto.Id.Value);
                }
                else if (!choiceDto.Id.HasValue)
                {
                    // Add new choice (must have text and IsCorrect)
                    if (!string.IsNullOrWhiteSpace(choiceDto.Text) && choiceDto.IsCorrect.HasValue)
                    {
                        var newChoice = new Choice
                        {
                            Text = choiceDto.Text.Trim(),
                            IsCorrect = choiceDto.IsCorrect.Value,
                            QuestionId = questionId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _choiceRepository.AddAsync(newChoice);
                    }
                }
            }

            // Soft delete choices that weren't in the update request
            foreach (var choiceToDelete in existingChoicesDict.Values)
            {
                _choiceRepository.Delete(choiceToDelete);
            }
        }

        private (bool IsValid, string? ErrorMessage, string? ErrorMessageAr) ValidateChoices(UpdateQuestionDto questionDto)
        {
            if (questionDto.Choices == null || !questionDto.Choices.Any())
            {
                return (false, "At least one choice is required", "مطلوب اختيار واحد على الأقل");
            }

            // Check for duplicate choice texts
            var duplicateTexts = questionDto.Choices
                .Where(c => !string.IsNullOrWhiteSpace(c.Text))
                .GroupBy(c => c.Text!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTexts.Any())
            {
                return (false,
                    $"Duplicate choice texts are not allowed: {string.Join(", ", duplicateTexts)}",
                    $"لا يسمح بنصوص الاختيارات المكررة: {string.Join(", ", duplicateTexts)}");
            }

            // Validate based on question type (if provided)
            var questionType = questionDto.Type;
            if (questionType.HasValue)
            {
                var correctChoicesCount = questionDto.Choices.Count(c => c.IsCorrect == true);

                switch (questionType.Value)
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
                }
            }

            return (true, null, null);
        }
    }
}