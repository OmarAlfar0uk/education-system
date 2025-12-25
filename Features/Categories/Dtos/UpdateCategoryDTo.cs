using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Categories.Dtos
{
    public class UpdateCategoryDTo
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Title cannot exceed 20 characters")]
        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Only letters and spaces allowed")]
        public string Title { get; set; } = string.Empty;

        public IFormFile? Icon { get; set; } 
    }
}
