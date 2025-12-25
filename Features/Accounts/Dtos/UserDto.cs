using System.Text.Json.Serialization;

namespace EduocationSystem.Features.Accounts.Dtos
{
    public class UserDto
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        //public DateTime Expiration { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public bool EmailConfirmed { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
    }
}
