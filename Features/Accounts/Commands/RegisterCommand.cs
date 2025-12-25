using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using EduocationSystem.Domain;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Helpers;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Commands
{
    public record RegisterCommand(RegisterDto RegisterDto) : IRequest<ServiceResponse<ApplicationUser>>
    {
        public class Handler : IRequestHandler<RegisterCommand, ServiceResponse<ApplicationUser>>
        {
            private readonly JWT _jwtOptions;
            private readonly UserManager<ApplicationUser> userManager;
            private readonly ILogger<RegisterCommand> _logger;

            public Handler(UserManager<ApplicationUser> userManager, JWT jwtOptions, ILogger<RegisterCommand> logger)
            {
                _jwtOptions = jwtOptions;
                this.userManager = userManager;
                _logger = logger;
            }

            public async Task<ServiceResponse<ApplicationUser>> Handle(RegisterCommand request, CancellationToken cancellationToken)
            {
                // Check if email is already registered
                if (await userManager.FindByEmailAsync(request.RegisterDto.Email) is not null)
                {
                    return ServiceResponse<ApplicationUser>.ConflictResponse("Email is already registered");
                }

                // Check if username is already registered
                if (await userManager.FindByNameAsync(request.RegisterDto.UserName) is not null)
                {
                    return ServiceResponse<ApplicationUser>.ConflictResponse("Username is already registered");
                }

                var user = new ApplicationUser
                {
                    Email = request.RegisterDto.Email,
                    UserName = request.RegisterDto.UserName,
                    FirstName = request.RegisterDto.FirstName,
                    LastName = request.RegisterDto.LastName,
                    PhoneNumber = request.RegisterDto.Phone,
                    EmailConfirmed = false
                };

                var result = await userManager.CreateAsync(user, request.RegisterDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<ApplicationUser>.ValidationErrorResponse(
                        new Dictionary<string, List<string>> { { "Password", errors } },
                        "Registration failed"
                    );
                }

                // Add user to default role
                await userManager.AddToRoleAsync(user, "User");

                // Get the user ID for verification code (your original logic)
                var userId = user.Id;

                // Create verification code (your original, but not used here—pass to orchestrator)
                // var verificationCode = GenerateVerificationCode(); // Move to email command

                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return ServiceResponse<ApplicationUser>.SuccessResponse(
                    user,
                    "Registration completed successfully. Please check your email for verification code.",
                    "تمت العملية بنجاح تأكد من بريدك لكود التفعيل"
                );
            }
        }
    }
}