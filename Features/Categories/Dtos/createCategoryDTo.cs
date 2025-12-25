using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Features.Categories.Dtos
{
    public class createCategoryDTo
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(20, ErrorMessage = "Title cannot exceed 20 characters")]
        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "Only letters and spaces allowed")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Icon image is required")]
        public IFormFile Icon { get; set; } = null!;





    }
}
