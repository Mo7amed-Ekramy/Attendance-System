using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.ViewModels.Admin
{
    public class CreateSectionDto
    {
        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Department section is required")]
        public int DepartmentSectionId { get; set; }

        [Required(ErrorMessage = "TA is required")]
        public int TAId { get; set; }
    }
}
