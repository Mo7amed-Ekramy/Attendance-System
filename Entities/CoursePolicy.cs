using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
{
    public class CoursePolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Range(0, 100)]
        public int SectionAttendanceMarks { get; set; }

        [Range(0, 100)]
        public int QuizMarks { get; set; }

        [Range(0, 100)]
        public int LectureAttendanceMarks { get; set; }

        [Range(0, 100)]
        public int AllowedAbsences { get; set; }

        [Range(0, 50)]
        public int BestQuizzesCount { get; set; }
    }
}