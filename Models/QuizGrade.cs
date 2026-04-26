using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_PROJECT.Models
{
    public class QuizGrade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Range(0, 100)]
        public decimal Mark { get; set; }

        [Range(0, 100)]
        public decimal PercentageScore { get; set; }

        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set; }

        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment Enrollment { get; set; }
    }
}