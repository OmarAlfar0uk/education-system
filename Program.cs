using EduocationSystem.Domain;
using EduocationSystem.Domain.Entities;
using EduocationSystem.Domain.Interfaces;
using EduocationSystem.Features.Accounts.Endpoints;
using EduocationSystem.Features.Attendance.Endpoints;
using EduocationSystem.Features.Categories.Endpoints;
using EduocationSystem.Features.Dashboard.Endpoints;
using EduocationSystem.Features.Enrollment;
using EduocationSystem.Features.Exams.Endpoints;
using EduocationSystem.Features.Grade;
using EduocationSystem.Features.Notification;
using EduocationSystem.Features.Notification.Hubs;
using EduocationSystem.Features.Parent.Endpoints;
using EduocationSystem.Features.Profile.Endpoints;
using EduocationSystem.Features.Questions.Endpoints;
using EduocationSystem.Features.Students.Endpoints;
using EduocationSystem.Features.UserAnswers.Endpoints;
using EduocationSystem.Infrastructure.ApplicationDBContext;
using EduocationSystem.Infrastructure.Repositories;
using EduocationSystem.Infrastructure.Service;
using EduocationSystem.Infrastructure.Servies;
using EduocationSystem.Infrastructure.UnitOfWork;
using EduocationSystem.Middlewares;
using EduocationSystem.Shared.Data;
using EduocationSystem.Shared.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace EduocationSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Serilog setup (unchanged)

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "OnlineExam.Api")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate)
                .WriteTo.File("logs/OnlineExam-.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Starting OnlineExam API application");

            #endregion

            var builder = WebApplication.CreateBuilder(args);

            #region Services

            builder.Host.UseSerilog();
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
            // Services (unchanged)


            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));
            var JwtOption = builder.Configuration.GetSection("JWT").Get<JWT>();
            builder.Services.AddSingleton(JwtOption);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { /* unchanged */ })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("StudentOnly", policy =>
                    policy.RequireRole("Student"));

                options.AddPolicy("ParentOnly", policy =>
                    policy.RequireRole("Parent"));

                options.AddPolicy("AdminOrStudent", policy =>
                    policy.RequireRole("Admin", "Student"));

                options.AddPolicy("AdminOrParent", policy =>
                    policy.RequireRole("Admin", "Parent"));
            });


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Add this
            })


        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = JwtOption.Issuer,
                ValidAudience = JwtOption.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(JwtOption.Secretkey)),
                RoleClaimType = ClaimTypes.Role,    
                NameClaimType = ClaimTypes.Name,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"🔴 AUTH FAILED: {context.Exception.Message}");
                    Console.WriteLine($"🔴 Exception: {context.Exception}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"🟢 TOKEN VALIDATED");
                    Console.WriteLine($"🟢 User: {context.Principal.Identity.Name}");
                    Console.WriteLine($"🟢 IsAuthenticated: {context.Principal.Identity.IsAuthenticated}");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"🟡 CHALLENGE: {context.Error}");
                    Console.WriteLine($"🟡 Description: {context.ErrorDescription}");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    Console.WriteLine($"🔵 MESSAGE RECEIVED: {context.Token}");
                    return Task.CompletedTask;
                }
            };
        });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => { /* unchanged */ });
                options.EnableSensitiveDataLogging(false);
                options.EnableServiceProviderCaching();
                options.EnableDetailedErrors(builder.Environment.IsDevelopment());
                options.LogTo(message => Log.Debug("[EF] {Message}", message), LogLevel.Warning);
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMediatR(typeof(Program).Assembly);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
            builder.Services.AddHostedService<EmailBackgroundWorker>();
            builder.Services.AddScoped< NotificationService>();
            builder.Services.AddHostedService<EmailBackgroundWorker>();


            // UnitOfWork first
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add this with your other service registrations
            builder.Services.AddScoped<TransactionMiddleware>();

            // Dynamic generic repositories for BaseEntity subclasses
            var baseEntityAssembly = Assembly.GetAssembly(typeof(BaseEntity)) ?? Assembly.GetExecutingAssembly();  // Fallback if null
            var entityTypes = baseEntityAssembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
                .ToList();

            foreach (var entityType in entityTypes)
            {
                var interfaceType = typeof(IGenericRepository<>).MakeGenericType(entityType);
                var implementationType = typeof(GenericRepository<>).MakeGenericType(entityType);
                builder.Services.AddScoped(interfaceType, implementationType);
            }

            // Log for debugging (optional)
            Log.Information("Registered {Count} generic repositories for entities: {Entities}", entityTypes.Count, string.Join(", ", entityTypes.Select(t => t.Name)));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));





            #endregion


            var app = builder.Build();

            #region Build
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    Log.Information("📊 Starting database seeding...");


                    await DatabaseSeeder.SeedAsync(services);
                    Log.Information("🌱 Database seeding completed successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "❌ An error occurred while seeding the database");

                    if (app.Environment.IsDevelopment())
                    {
                        throw;
                    }

                    Log.Warning("⚠️ Application will continue without seeding");
                }
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();


            // Trust the dev certificate on startup
            if (app.Environment.IsDevelopment())
            {
                var cert = new HttpClient().GetAsync("https://localhost:5001").ContinueWith(task => { });
                app.UseDeveloperExceptionPage(); // Optional: for detailed errors
            }
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            // Add this BEFORE authentication
            app.Use(async (context, next) =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"[DEBUG] Auth Header: {authHeader}");
                Log.Information("Auth Header: {AuthHeader}", authHeader);
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<FormContentTypeValidationMiddleware>();
            app.UseMiddleware<TransactionMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>();
            app.UseMiddleware<ProfilingMiddleware>();

            app.MapControllers();

            #region Mapping Endpoints

            app.MapGet("/", () => "EduocationSystem API is running...");

            // ===================== AUTH =====================
            app.MapRegisterEndpoint();
            app.MapLoginEndpoint();
            app.MapConfirmEmailEndpoint();
            app.MapLogoutEndpoint();
            app.MapForgotPasswordEndpoint();
            app.MapResetPasswordEndpoint();
            app.MapResendVerificationCodeEndpoint();

            // ===================== PROFILE =====================
            app.MapProfileEndpoint();
            app.MapUpdateProfileEndpoint();

            // ===================== CATEGORY =====================
            app.MapGetUserCategoriesEndpoint();
            app.MapGetCategoriesForAdminEndpoint();
            app.MapGetCategoryByIdEndpoint();
            app.MapCreateCategoryEndpoint();
            app.MapUpdateCategoryEndpoint();
            app.MapDeleteCategoryEndpoint();

            // ===================== EXAMS =====================
            app.MapUserExamEndpoints();
            app.MapAdminExamEndpoints();
            app.MapGetExamByIDEndpoint();
            app.MapStartExamAttemptEndpoint();
            app.MapSubmitExamEndpoint();
            app.MapCreateExamEndpoint();
            app.MapEditExamEndpoint();
            app.MapDeleteExamEndpoint();

            // ===================== QUESTIONS =====================
            app.MapAdminQuestionEndpoint();
            app.MapAddQuestionEndpoint();
            app.MapUpdateQuestionEndpoint();
            app.MapDeleteQuestionEndpoint();
            app.MapGetQuestionDetailsEndpoint();

            // ===================== USER ANSWERS =====================
            app.MapGetAllUserAnswersEndpoints();
            app.MapGetDetailedUserAnswerEndpoint();

            // ===================== DASHBOARD =====================
            app.MapDashboardStatsEndpoints();
            app.MapMostActiveExamsEndpoints();
            app.MapMostActiveCategoriesEndpoints();

            // =====================================================
            // ===================== STUDENTS ======================
            app.MapStudentsEndpoints();

            // ===================== PARENTS ======================
           app.MapParentsEndpoints();

            // ===================== ENROLLMENTS ======================
            app.MapEnrollmentEndpoints();
            // ===================== WEEKS ======================


            // ===================== ATTENDANCE ======================
            app.MapAttendanceEndpoints();


            // ===================== GRADES ======================
            app.MapGradesEndpoints();

            // ===================== NOTIFICATIONS ======================
            app.MapNotificationsEndpoints();
            app.MapHub<NotificationHub>("/hubs/notifications");


            #endregion


            app.Use(async (ctx, next) =>
            {
                try { await next(); }
                catch (FluentValidation.ValidationException ex)
                {
                    var dict = ex.Errors
                        .GroupBy(e => e.PropertyName ?? "")
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    await Results.ValidationProblem(dict).ExecuteAsync(ctx);
                }
            });

            #endregion


            app.Run();

        }
    }
}