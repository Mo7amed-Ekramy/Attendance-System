using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EF_LSM.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string UniversityId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Range(1, 10)]
        public int Level { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}