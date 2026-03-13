using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }

        [ForeignKey(nameof(SectionId))]
        public Section Section { get; set; }

        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

        public ICollection<QuizGrade> QuizGrades { get; set; } = new List<QuizGrade>();
    }
}