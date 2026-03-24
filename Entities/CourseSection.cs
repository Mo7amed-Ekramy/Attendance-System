using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class CourseSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int DepartmentSectionId { get; set; }

        [Required]
        public int TAId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [ForeignKey(nameof(DepartmentSectionId))]
        public DepartmentSection DepartmentSection { get; set; }

        [ForeignKey(nameof(TAId))]
        public User TA { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}