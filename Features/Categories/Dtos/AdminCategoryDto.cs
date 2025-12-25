namespace EduocationSystem.Features.Categories.Dtos
{
    public class AdminCategoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
    }
}
