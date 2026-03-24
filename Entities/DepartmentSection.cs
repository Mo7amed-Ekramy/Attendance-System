using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class DepartmentSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Number { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();

        public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }
}