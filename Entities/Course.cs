using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
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
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        public CoursePolicy CoursePolicy { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();

        public ICollection<CourseDepartment> CourseDepartments { get; set; } = new List<CourseDepartment>();
    }
}