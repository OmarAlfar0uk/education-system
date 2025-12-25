using EduocationSystem.Domain.Interfaces;

namespace EduocationSystem.Infrastructure.CurrentUserService
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?
                .Value;

        public bool IsInRole(string role) =>
            _httpContextAccessor.HttpContext?
                .User?
                .IsInRole(role) ?? false;
    }

}
