using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_LSM.Entities
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
    }
}