using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.ViewModels.Admin
{
    public class UpdateCourseDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20)]
        public string Code { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [StringLength(30)]
        public string Semester { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Range(1, 10, ErrorMessage = "Level must be between 1 and 10")]
        public int Level { get; set; }

        [Required(ErrorMessage = "Doctor is required")]
        public int DoctorId { get; set; }
    }
}
