using System;

namespace MVC_PROJECT.ViewModels.Student
{
    public class StudentQuizMarkItemViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public DateTime Date { get; set; }
        public decimal MaxMark { get; set; }
        public decimal Mark { get; set; }
        public decimal PercentageScore { get; set; }
    }
}
