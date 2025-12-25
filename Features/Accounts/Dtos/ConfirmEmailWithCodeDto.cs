namespace EduocationSystem.Features.Accounts.Dtos
{
    public class ConfirmEmailWithCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}