using Microsoft.AspNetCore.Identity;
using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Enums;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using TechZone.Core.Entities;

namespace EduocationSystem.Shared.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {




            // 0) Roles & Admin & Users
            await SeedRolesAndUsersAsync(sp);

            // DbContext
            var ctx = sp.GetRequiredService<ApplicationDbContext>();

            // 1) Seed Categories
            await SeedCategoriesAsync(ctx);

            // 2) Seed Exams
            await SeedExamsAsync(ctx);

            // 3) Seed Questions & Choices
            await SeedQuestionsAndChoicesAsync(ctx);

            // 4) Seed Sample Exam Attempts
            await SeedExamAttemptsAsync(sp, ctx);
        }

        // ====== 0) Identity (Roles + Users) ======
        private static async Task SeedRolesAndUsersAsync(IServiceProvider sp)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣ Roles
            string[] roles = { "Admin", "Student", "Parent" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2️⃣ Super Admin
            var adminEmail = "admin@onlineexam.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    Phone = "+1234567890",
                    ImageUrl = "/uploads/admin-avatar.png",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 3️⃣ Regular Users → Students
            var users = new[]
            {
        new { UserName = "john.doe", Email = "john.doe@example.com", FirstName = "John", LastName = "Doe", Phone = "+1234567891" },
        new { UserName = "jane.smith", Email = "jane.smith@example.com", FirstName = "Jane", LastName = "Smith", Phone = "+1234567892" },
        new { UserName = "mike.wilson", Email = "mike.wilson@example.com", FirstName = "Mike", LastName = "Wilson", Phone = "+1234567893" },
        new { UserName = "sarah.jones", Email = "sarah.jones@example.com", FirstName = "Sarah", LastName = "Jones", Phone = "+1234567894" },
        new { UserName = "alex.brown", Email = "alex.brown@example.com", FirstName = "Alex", LastName = "Brown", Phone = "+1234567895" }
    };

            foreach (var userInfo in users)
            {
                var user = await userManager.FindByEmailAsync(userInfo.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = userInfo.UserName,
                        Email = userInfo.Email,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        Phone = userInfo.Phone,
                        ImageUrl = $"/uploads/{userInfo.UserName}-avatar.png",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Student");
                    }
                }
            }
        }


        // ====== 1) Seed Categories ======
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (context.Categories.Any()) return;

            var categories = new List<Category>
            {
                new Category { Title = "Mathematics", IconUrl = "/images/math-icon.png", Description = "Algebra, Geometry, Calculus, Statistics, and more mathematical disciplines" },
                new Category { Title = "Science", IconUrl = "/images/science-icon.png", Description = "Physics, Chemistry, Biology, Astronomy, and Environmental Science" },
                new Category { Title = "History", IconUrl = "/images/history-icon.png", Description = "World History, Ancient Civilizations, Modern Events, and Historical Figures" },
                new Category { Title = "Geography", IconUrl = "/images/geography-icon.png", Description = "Countries, Capitals, Landforms, Climate, and Cultural Geography" },
                new Category { Title = "English Language", IconUrl = "/images/english-icon.png", Description = "Grammar, Literature, Writing Skills, and Communication" },
                new Category { Title = "Computer Science", IconUrl = "/images/computer-icon.png", Description = "Programming, Algorithms, Data Structures, and Software Development" },
                new Category { Title = "Business", IconUrl = "/images/business-icon.png", Description = "Economics, Management, Marketing, Finance, and Entrepreneurship" },
                new Category { Title = "Arts & Music", IconUrl = "/images/arts-icon.png", Description = "Visual Arts, Music Theory, Art History, and Performance Arts" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // ====== 2) Seed Exams ======
        private static async Task SeedExamsAsync(ApplicationDbContext context)
        {
            if (context.Exams.Any()) return;

            var categories = context.Categories.ToList();
            var exams = new List<Exam>();

            foreach (var category in categories)
            {
                var categoryExams = GetExamsForCategory(category.Id, category.Title);
                exams.AddRange(categoryExams);
            }

            await context.Exams.AddRangeAsync(exams);
            await context.SaveChangesAsync();
        }

        // ====== 3) Seed Questions & Choices ======
        private static async Task SeedQuestionsAndChoicesAsync(ApplicationDbContext context)
        {
            if (context.Questions.Any()) return;

            var exams = context.Exams.ToList();

            // Step 1: Create and save questions first
            var questions = new List<Question>();
            foreach (var exam in exams)
            {
                var examQuestions = GetQuestionsForExam(exam.Id, exam.Title);
                questions.AddRange(examQuestions);
            }

            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync(); // This generates the Question IDs

            // Step 2: Now create choices with the actual Question IDs
            var choices = new List<Choice>();
            foreach (var question in questions)
            {
                var questionChoices = GetChoicesForQuestion(question.Id, question.Title, question.Type);
                choices.AddRange(questionChoices);
            }

            await context.Choices.AddRangeAsync(choices);
            await context.SaveChangesAsync();
        }

        // ====== 4) Seed Sample Exam Attempts ======
        private static async Task SeedExamAttemptsAsync(IServiceProvider sp, ApplicationDbContext context)
        {
            if (context.UserExamAttempts.Any()) return;

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var users = userManager.Users.Where(u => u.UserName != "admin").ToList();
            var exams = context.Exams.Take(3).ToList(); // Use only first 3 exams to avoid too much data
            var random = new Random();

            // Step 1: Create and save attempts first
            var attempts = new List<UserExamAttempt>();
            foreach (var user in users)
            {
                foreach (var exam in exams)
                {
                    var attempt = new UserExamAttempt
                    {
                        UserId = user.Id,
                        ExamId = exam.Id,
                        Score = random.Next(60, 100),
                        TotalQuestions = context.Questions.Count(q => q.ExamId == exam.Id),
                        AttemptDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        IsHighestScore = true,
                        FinishedAt = DateTime.UtcNow.AddMinutes(exam.Duration - random.Next(5, 20)),
                        AttemptNumber = 1
                    };
                    attempts.Add(attempt);
                }
            }

            await context.UserExamAttempts.AddRangeAsync(attempts);
            await context.SaveChangesAsync(); // This generates Attempt IDs

            // Step 2: Create and save user answers
            var userAnswers = new List<UserAnswer>();
            foreach (var attempt in attempts)
            {
                var questions = context.Questions.Where(q => q.ExamId == attempt.ExamId).ToList();

                foreach (var question in questions)
                {
                    var userAnswer = new UserAnswer
                    {
                        AttemptId = attempt.Id, // Now this ID exists in database
                        QuestionId = question.Id
                    };
                    userAnswers.Add(userAnswer);
                }
            }

            await context.userAnswers.AddRangeAsync(userAnswers);
            await context.SaveChangesAsync(); // (here the error) This generates UserAnswer IDs

            // Step 3: Create and save selected choices
            var selectedChoices = new List<UserSelectedChoice>();
            foreach (var userAnswer in userAnswers)
            {
                var question = context.Questions.First(q => q.Id == userAnswer.QuestionId);
                var questionChoices = context.Choices.Where(c => c.QuestionId == question.Id).ToList();

                if (question.Type == QuestionType.MultipleChoice)
                {
                    // Single choice selection
                    var selectedChoice = questionChoices[random.Next(questionChoices.Count)];
                    selectedChoices.Add(new UserSelectedChoice
                    {
                        UserAnswerId = userAnswer.Id, // Now this ID exists in database
                        ChoiceId = selectedChoice.Id
                    });
                }
                else if (question.Type == QuestionType.multipleSelect)
                {
                    // Multiple choice selection (1-2 choices)
                    var selectedCount = random.Next(1, Math.Min(3, questionChoices.Count));
                    var selected = questionChoices.OrderBy(x => random.Next()).Take(selectedCount);

                    foreach (var choice in selected)
                    {
                        selectedChoices.Add(new UserSelectedChoice
                        {
                            UserAnswerId = userAnswer.Id, // Now this ID exists in database
                            ChoiceId = choice.Id
                        });
                    }
                }
            }

            await context.UserSelectedChoices.AddRangeAsync(selectedChoices);
            await context.SaveChangesAsync();
        }

        private static List<Exam> GetExamsForCategory(int categoryId, string categoryTitle)
        {
            var now = DateTime.UtcNow;

            return categoryTitle switch
            {
                "Mathematics" => new List<Exam>
                {
                    new Exam { Title = "Algebra Fundamentals", IconUrl = "/images/algebra-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-30), EndDate = now.AddDays(60), Duration = 60, IsActive = true, Description = "Master basic algebraic concepts, equations, and problem-solving techniques" },
                    new Exam { Title = "Geometry Mastery", IconUrl = "/images/geometry-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-15), EndDate = now.AddDays(75), Duration = 75, IsActive = true, Description = "Advanced geometry concepts including theorems, proofs, and spatial reasoning" }
                },
                "Science" => new List<Exam>
                {
                    new Exam { Title = "Physics Principles", IconUrl = "/images/physics-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-25), EndDate = now.AddDays(65), Duration = 80, IsActive = true, Description = "Fundamental physics concepts, laws of motion, and energy principles" },
                    new Exam { Title = "Chemistry Essentials", IconUrl = "/images/chemistry-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-20), EndDate = now.AddDays(70), Duration = 70, IsActive = true, Description = "Periodic table, chemical reactions, bonding, and laboratory safety" }
                },
                "Computer Science" => new List<Exam>
                {
                    new Exam { Title = "Programming Fundamentals", IconUrl = "/images/programming-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-40), EndDate = now.AddDays(100), Duration = 120, IsActive = true, Description = "Basic programming concepts, algorithms, and problem-solving strategies" },
                    new Exam { Title = "Data Structures", IconUrl = "/images/datastructures-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-35), EndDate = now.AddDays(110), Duration = 100, IsActive = true, Description = "Arrays, linked lists, trees, graphs, and algorithm complexity" }
                },
                "Business" => new List<Exam>
                {
                    new Exam { Title = "Economics Principles", IconUrl = "/images/economics-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-10), EndDate = now.AddDays(70), Duration = 70, IsActive = true, Description = "Micro and macro economics fundamentals, market structures, and policies" }
                },
                "English Language" => new List<Exam>
                {
                    new Exam { Title = "Grammar Proficiency", IconUrl = "/images/grammar-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-20), EndDate = now.AddDays(80), Duration = 55, IsActive = true, Description = "Advanced English grammar, syntax, and sentence structure" }
                },
                _ => new List<Exam>
                {
                    new Exam { Title = $"{categoryTitle} Level 1", IconUrl = "/images/default-exam.png", CategoryId = categoryId, StartDate = now.AddDays(-10), EndDate = now.AddDays(60), Duration = 60, IsActive = true, Description = $"Basic concepts and fundamentals of {categoryTitle}" }
                }
            };
        }

        private static List<Question> GetQuestionsForExam(int examId, string examTitle)
        {
            var questions = new List<Question>();
            var random = new Random();

            // Generate 5-10 questions per exam (reduced for testing)
            var questionCount = random.Next(5, 11);

            for (int i = 1; i <= questionCount; i++)
            {
                var questionType = random.Next(0, 2) == 0 ? QuestionType.MultipleChoice : QuestionType.multipleSelect;
                var questionText = GetQuestionText(examTitle, i, questionType);

                questions.Add(new Question
                {
                    Title = questionText,
                    Type = questionType,
                    ExamId = examId
                });
            }

            return questions;
        }

        private static string GetQuestionText(string examTitle, int questionNumber, QuestionType type)
        {
            var baseQuestions = new[]
            {
                "Which of the following statements is correct?",
                "What is the primary concept being demonstrated?",
                "Which option best describes the fundamental principle?",
                "Select the most accurate description:",
                "Which answer provides the correct solution?",
                "Identify the valid statement from the options below:",
                "What would be the expected outcome in this scenario?",
                "Which approach is considered best practice?"
            };

            var random = new Random();
            var baseQuestion = baseQuestions[random.Next(baseQuestions.Length)];

            return type == QuestionType.multipleSelect ?
                $"{baseQuestion} (Select all that apply)" :
                baseQuestion;
        }

        private static List<Choice> GetChoicesForQuestion(int questionId, string questionTitle, QuestionType type)
        {
            var choices = new List<Choice>();
            var random = new Random();

            if (type == QuestionType.multipleSelect)
            {
                // For multiple select: 2 correct answers out of 5 choices
                var choiceTexts = new[]
                {
                    "This is a correct statement that applies to the question",
                    "Another valid point that should be selected",
                    "This option is incorrect and should not be chosen",
                    "While plausible, this answer is not entirely accurate",
                    "This represents a common misconception in the subject"
                };

                // Mark first two as correct, rest as incorrect
                for (int i = 0; i < 5; i++)
                {
                    choices.Add(new Choice
                    {
                        Text = choiceTexts[i],
                        IsCorrect = i < 2, // First two are correct
                        QuestionId = questionId
                    });
                }
            }
            else // MultipleChoice
            {
                // For multiple choice: 1 correct answer out of 4 choices
                var choiceTexts = new[]
                {
                    "The correct answer that solves the problem",
                    "A common mistake that seems reasonable but is wrong",
                    "An answer that addresses a different concept entirely",
                    "A partially correct but incomplete solution"
                };

                // Mark first one as correct
                for (int i = 0; i < 4; i++)
                {
                    choices.Add(new Choice
                    {
                        Text = choiceTexts[i],
                        IsCorrect = i == 0, // Only first is correct
                        QuestionId = questionId
                    });
                }
            }

            // Shuffle the choices to make it more realistic
            return choices.OrderBy(x => random.Next()).ToList();




        }
    }
}