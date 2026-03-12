using System;
using System.Collections.Generic;
using System.Text;

namespace EF_LSM.Entities
{
    public class QuizGrade
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal Mark { get; set; }
        public decimal PercentageScore { get; set; }

    }
}
