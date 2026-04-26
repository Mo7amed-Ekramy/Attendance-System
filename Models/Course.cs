using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_PROJECT.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(30)]
        public string Semester { get; set; }

        [Required]
        [Range(1, 10)]
        public int Level { get; set; } = 3;

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        public CoursePolicy CoursePolicy { get; set; }

        public ICollection<CourseDepartment> CourseDepartments { get; set; } = new List<CourseDepartment>();

        public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }
}