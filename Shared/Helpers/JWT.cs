namespace EduocationSystem.Shared.Helpers
{
    public class JWT
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secretkey { get; set; }
        public int ExpiryInMinutes { get; set; }

    }
}
