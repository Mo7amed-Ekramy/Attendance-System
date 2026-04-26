using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.ViewModels.Admin
{
    public class UpdateStudentDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "University ID is required")]
        [StringLength(30)]
        public string UniversityId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Range(1, 10, ErrorMessage = "Level must be between 1 and 10")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department section is required")]
        public int DepartmentSectionId { get; set; }

        public string? Password { get; set; }
    }
}
