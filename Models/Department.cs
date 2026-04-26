using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC_PROJECT.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }   // AI, CS, SE, IS

        [Required]
        [StringLength(100)]
        public string Name { get; set; }   // Artificial Intelligence, Computer Science...

        public ICollection<Student> Students { get; set; } = new List<Student>();

        public ICollection<DepartmentSection> Sections { get; set; } = new List<DepartmentSection>();

        public ICollection<CourseDepartment> CourseDepartments { get; set; } = new List<CourseDepartment>();
    }
}