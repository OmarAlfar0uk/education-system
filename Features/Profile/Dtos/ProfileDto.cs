using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Profile.Dtos
{
    public class ProfileDto
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Image {  get; set; } = string.Empty;
    }
}
